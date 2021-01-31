namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskBuildStructure : BasePlayerTaskWithListAndCount<IProtoObjectStructure>
    {
        public const string DescriptionFormat = "Build: {0}";

        public TaskBuildStructure(
            IReadOnlyList<IProtoObjectStructure> list,
            ushort count,
            string description,
            bool isSharedWithPartyAndFactionMembers)
            : base(list, count, description)
        {
            this.IsSharedWithPartyAndFactionMembers = isSharedWithPartyAndFactionMembers;
        }

        public override bool IsReversible => false;

        public bool IsSharedWithPartyAndFactionMembers { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ListNames);

        public static TaskBuildStructure Require<TProtoStructure>(
            ushort count = 1,
            string description = null,
            bool isSharedWithPartyAndFactionMembers = true)
            where TProtoStructure : class, IProtoObjectStructure
        {
            var list = Api.FindProtoEntities<TProtoStructure>();
            return new TaskBuildStructure(list, count, description, isSharedWithPartyAndFactionMembers);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.List.Count == 1
                       ? this.List[0].Icon
                       : null;
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskStateWithCount state)
        {
            if (base.ServerIsCompleted(character, state))
            {
                return true;
            }

            if (!this.IsSharedWithPartyAndFactionMembers
                || !(this.TaskTarget is IProtoQuest))
            {
                return false;
            }

            // check whether any of the other party members has this requirement satisfied
            var currentCharacterName = character.Name;
            var partyMembers = PartySystem.ServerGetPartyMembersReadOnly(character);
            if (partyMembers.Count > 1)
            {
                foreach (var partyMemberName in partyMembers)
                {
                    if (partyMemberName == currentCharacterName)
                    {
                        continue;
                    }

                    var partyMember = Api.Server.Characters
                                         .GetPlayerCharacter(partyMemberName);

                    if (partyMember is null)
                    {
                        continue;
                    }

                    if (partyMember.SharedGetQuests()
                                   .SharedHasCompletedTask(this))
                    {
                        // party member has satisfied this requirement
                        return true;
                    }
                }
            }

            // check whether any of the other faction members has this requirement satisfied
            if (FactionSystem.ServerGetFaction(character) is { } faction
                && FactionSystem.SharedGetFactionKind(faction) != FactionKind.Public)
            {
                var factionMembers = FactionSystem.ServerGetFactionMembersReadOnly(faction);
                if (factionMembers.Count > 1)
                {
                    foreach (var factionMemberEntry in factionMembers)
                    {
                        var factionMemberName = factionMemberEntry.Name;
                        if (factionMemberName == currentCharacterName)
                        {
                            continue;
                        }

                        var factionMember = Api.Server.Characters
                                               .GetPlayerCharacter(factionMemberName);

                        if (factionMember is null)
                        {
                            continue;
                        }

                        if (factionMember.SharedGetQuests()
                                         .SharedHasCompletedTask(this))
                        {
                            // faction member has satisfied this requirement
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                ConstructionPlacementSystem.ServerStructureBuilt += this.StructureBuiltHandler;
                PartySystem.ServerCharacterJoinedOrLeftParty += this.ServerCharacterJoinedOrLeftPartyHandler;
                FactionSystem.ServerCharacterJoinedOrLeftFaction += this.ServerCharacterJoinedOrLeftFactionHandler;
            }
            else
            {
                ConstructionPlacementSystem.ServerStructureBuilt -= this.StructureBuiltHandler;
                PartySystem.ServerCharacterJoinedOrLeftParty -= this.ServerCharacterJoinedOrLeftPartyHandler;
                FactionSystem.ServerCharacterJoinedOrLeftFaction -= this.ServerCharacterJoinedOrLeftFactionHandler;
            }
        }

        private void RefreshContextForFactionMembers(ICharacter character)
        {
            if (!this.IsSharedWithPartyAndFactionMembers)
            {
                return;
            }

            var factionMembers = FactionSystem.ServerGetFactionMembersReadOnly(character);
            if (factionMembers.Count <= 1)
            {
                // no party or single-player party
                return;
            }

            var currentCharacterName = character.Name;
            foreach (var factionMemberEntry in factionMembers)
            {
                var factionMemberName = factionMemberEntry.Name;
                if (factionMemberName == currentCharacterName)
                {
                    continue;
                }

                var factionMember = Api.Server.Characters
                                       .GetPlayerCharacter(factionMemberName);

                if (factionMember is not null)
                {
                    var context = this.GetActiveContext(factionMember, out _);
                    context?.Refresh();
                }
            }
        }

        private void RefreshContextForPartyMembers(ICharacter character)
        {
            if (!this.IsSharedWithPartyAndFactionMembers)
            {
                return;
            }

            var partyMembers = PartySystem.ServerGetPartyMembersReadOnly(character);
            if (partyMembers.Count <= 1)
            {
                // no party or single-player party
                return;
            }

            var currentCharacterName = character.Name;
            foreach (var partyMemberName in partyMembers)
            {
                if (partyMemberName == currentCharacterName)
                {
                    continue;
                }

                var partyMember = Api.Server.Characters
                                     .GetPlayerCharacter(partyMemberName);

                if (partyMember is not null)
                {
                    var context = this.GetActiveContext(partyMember, out _);
                    context?.Refresh();
                }
            }
        }

        private void ServerCharacterJoinedOrLeftFactionHandler(
            ICharacter character,
            ILogicObject faction,
            bool isJoined)
        {
            if (!isJoined
                || FactionSystem.SharedGetFactionKind(faction) == FactionKind.Public)
            {
                return;
            }

            var context = this.GetActiveContext(character, out _);
            context?.Refresh();

            this.RefreshContextForFactionMembers(character);
        }

        private void ServerCharacterJoinedOrLeftPartyHandler(
            ICharacter character,
            ILogicObject party,
            bool isJoined)
        {
            if (!isJoined)
            {
                return;
            }

            var context = this.GetActiveContext(character, out _);
            context?.Refresh();

            this.RefreshContextForPartyMembers(character);
        }

        private void StructureBuiltHandler(ICharacter character, IStaticWorldObject structure)
        {
            var context = this.GetActiveContext(character, out var state);
            if (context is null)
            {
                return;
            }

            if (!this.List.Contains(structure.ProtoStaticWorldObject))
            {
                return;
            }

            var isSatisfied = state.IsCompleted;
            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            context.Refresh();

            if (state.IsCompleted != isSatisfied)
            {
                this.RefreshContextForPartyMembers(character);
                this.RefreshContextForFactionMembers(character);
            }
        }
    }
}