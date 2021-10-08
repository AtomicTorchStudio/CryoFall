namespace AtomicTorch.CBND.CoreMod.Systems.NewGamePlus
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.CharacterOrigins;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterCreation;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterStyle;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.CharacterCreation;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    /// <summary>
    /// "New Game +" is a special option available when the server owner has completed the game on the local server.
    /// The client saves the character data and progress. Then it requests the server to create a new world and
    /// awaits connection. Then calls the server to transfer the data to the new character.
    /// </summary>
    public class NewGamePlusSystem : ProtoSystem<NewGamePlusSystem>
    {
        private static readonly IClientStorage ClientStoredCharacterData;

        static NewGamePlusSystem()
        {
            if (IsServer)
            {
                return;
            }

            ClientStoredCharacterData = Api.Client.Storage.GetSessionStorage(
                nameof(NewGamePlus) + ".Settings");
            ClientStoredCharacterData.RegisterType(typeof(CharacterProgressData));
            ClientStoredCharacterData.RegisterType(typeof(CharacterHumanFaceStyle));
            BootstrapperClientGame.InitCallback += ClientInitCallbackHandler;
        }

        public static void ClientStartNewGamePlus()
        {
            if (!ClientCurrentCharacterHelper.PrivateState.IsEscapedOnRocket)
            {
                throw new Exception("Not escaped");
            }

            var currentServerAddress = Api.Client.CurrentGame.ServerInfo.ServerAddress;
            if (!currentServerAddress.IsLocalServer)
            {
                throw new Exception("Not a local server");
            }

            var newSlotId = Api.Client.LocalServer.GetNextFreeSlotId();

            SaveCharacterProgress(newSlotId);

            // copy current rates
            var serverRatesConfig = Api.Client.Core.CreateNewServerRatesConfig();
            foreach (var rate in RatesManager.Rates)
            {
                rate.SharedApplyToConfig(serverRatesConfig, rate.SharedAbstractValue);
            }

            // create new world (and connect to it)
            Logger.Important("Starting New Game +, new save slot ID=" + newSlotId);

            Api.Client.LocalServer.CreateNewWorld(newSlotId,
                                                  Api.Client.LocalServer.GetSaveName(
                                                      currentServerAddress.LocalServerSlotId)
                                                  + " (New Game +)",
                                                  Array.Empty<string>(),
                                                  serverRatesConfig);
        }

        private static async void ClientInitCallbackHandler(ICharacter character)
        {
            if (Client.CurrentGame.ConnectionState != ConnectionState.Connected)
            {
                return;
            }

            if (!ClientStoredCharacterData.TryLoad(out CharacterProgressData data))
            {
                // no data to restore
                return;
            }

            ClientStoredCharacterData.Clear();

            var serverAddress = Client.CurrentGame.ServerInfo.ServerAddress;
            if (!serverAddress.IsLocalServer
                || serverAddress.LocalServerSlotId != data.SlotId)
            {
                // the client has connected to something else, it seems
                return;
            }

            await Instance.CallServer(_ => _.ServerRemote_RestoreCharacterData(data));
            MenuCharacterCreation.HideIfCharacterCreated();
        }

        private static void SaveCharacterProgress(ushort slotId)
        {
            var data = new CharacterProgressData(ClientCurrentCharacterHelper.Character, slotId);
            ClientStoredCharacterData.Save(data);
            Logger.Important("Saved character progress: slotId=" + slotId);
        }

        [RemoteCallSettings(timeInterval: 10)]
        private bool ServerRemote_RestoreCharacterData(CharacterProgressData data)
        {
            var character = ServerRemoteContext.Character;
            if (!Server.Core.IsLocalServer
                || !Server.Core.IsLocalServerHoster(character))
            {
                throw new Exception("The player is not the local server hoster: " + character);
            }

            Logger.Important("New Game +: Restoring old character progress");

            // restore style
            CharacterStyleSystem.ServerSetStyle(character, data.FaceStyle, data.IsMale);
            CharacterCreationSystem.ServerSetOrigin(character, data.ProtoCharacterOrigin);

            // ensure character spawned
            CharacterCreationSystem.ServerSpawnIfReady(character);

            // restore finished quests
            var quests = character.SharedGetQuests();
            quests.ServerReset();
            foreach (var quest in data.FinishedQuests)
            {
                QuestsSystem.ServerCompleteQuest(quests,
                                                 quest,
                                                 ignoreRequirements: true,
                                                 provideReward: false);
            }

            // restore technologies
            var technologies = character.SharedGetTechnologies();
            technologies.ServerSetLearningPoints(data.LearningPoints);

            foreach (var techGroup in data.TechGroups)
            {
                technologies.ServerAddGroup(techGroup);
            }

            foreach (var techNode in data.TechNodes)
            {
                technologies.ServerAddNode(techNode);
            }

            // restore skills
            var skills = character.SharedGetSkills();
            skills.ServerReset();
            foreach (var (skill, experience) in data.Skills)
            {
                skills.ServerDebugSetSkill(skill, experience);
            }

            Logger.Important("New Game +: Finished restoring old character data");
            return true;
        }

        private class CharacterProgressData : IRemoteCallParameter
        {
            public readonly CharacterHumanFaceStyle FaceStyle;

            public readonly bool IsMale;

            public readonly uint LearningPoints;

            private readonly IProtoEntity[] finishedQuests;

            private readonly IProtoEntity protoCharacterOrigin;

            private readonly (IProtoEntity skill, byte level)[] skills;

            private readonly IProtoEntity[] techGroups;

            private readonly IProtoEntity[] techNodes;

            public CharacterProgressData(ICharacter character, ushort slotId)
            {
                this.SlotId = slotId;

                var publicState = PlayerCharacter.GetPublicState(character);
                this.FaceStyle = publicState.FaceStyle;
                this.IsMale = publicState.IsMale;
                this.protoCharacterOrigin = PlayerCharacter.GetPrivateState(character).Origin;

                var technologies = character.SharedGetTechnologies();
                this.LearningPoints = technologies.LearningPoints;
                this.techNodes = technologies.Nodes.Cast<IProtoEntity>().ToArray();
                this.techGroups = technologies.Groups.Cast<IProtoEntity>().ToArray();

                this.skills = character.SharedGetSkills()
                                       .Skills
                                       .Select(s => ((IProtoEntity)s.Key, s.Value.Level))
                                       .ToArray();

                this.finishedQuests = character.SharedGetQuests()
                                               .Quests
                                               .Where(q => q.IsCompleted)
                                               .Select(q => (IProtoEntity)q.Quest)
                                               .ToArray();
            }

            public IProtoQuest[] FinishedQuests => this.finishedQuests.Cast<IProtoQuest>().ToArray();

            public ProtoCharacterOrigin ProtoCharacterOrigin => (ProtoCharacterOrigin)this.protoCharacterOrigin;

            public (IProtoSkill, byte level)[] Skills
                => this.skills.Select(s => ((IProtoSkill)s.skill, s.level))
                       .ToArray();

            public ushort SlotId { get; }

            public TechGroup[] TechGroups => this.techGroups.Cast<TechGroup>().ToArray();

            public TechNode[] TechNodes => this.techNodes.Cast<TechNode>().ToArray();
        }
    }
}