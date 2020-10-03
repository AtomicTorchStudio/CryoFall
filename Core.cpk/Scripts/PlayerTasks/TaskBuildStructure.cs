namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
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
            bool isSharedWithPartyMembers)
            : base(list, count, description)
        {
            this.IsSharedWithPartyMembers = isSharedWithPartyMembers;
        }

        public override bool IsReversible => false;

        public bool IsSharedWithPartyMembers { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ListNames);

        public static TaskBuildStructure Require<TProtoStructure>(
            ushort count = 1,
            string description = null,
            bool isSharedWithPartyMembers = true)
            where TProtoStructure : class, IProtoObjectStructure
        {
            var list = Api.FindProtoEntities<TProtoStructure>();
            return new TaskBuildStructure(list, count, description, isSharedWithPartyMembers);
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

            if (!this.IsSharedWithPartyMembers
                || !(this.TaskTarget is IProtoQuest))
            {
                return false;
            }

            // check whether any of the other party members has this requirement satisfied
            var partyMembers = PartySystem.ServerGetPartyMembersReadOnly(character);
            if (partyMembers.Count <= 1)
            {
                // no party or single-player party
                return false;
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

            return false;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                ConstructionPlacementSystem.ServerStructureBuilt += this.StructureBuiltHandler;
                PartySystem.ServerCharacterJoinedOrLeftParty += this.ServerCharacterJoinedOrLeftPartyHandler;
            }
            else
            {
                ConstructionPlacementSystem.ServerStructureBuilt -= this.StructureBuiltHandler;
                PartySystem.ServerCharacterJoinedOrLeftParty -= this.ServerCharacterJoinedOrLeftPartyHandler;
            }
        }

        private void RefreshContextForPartyMembers(ICharacter character)
        {
            if (!this.IsSharedWithPartyMembers)
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

                if (partyMember is null)
                {
                    continue;
                }

                var context = this.GetActiveContext(partyMember, out _);
                context?.Refresh();
            }
        }

        private void ServerCharacterJoinedOrLeftPartyHandler(
            ICharacter character,
            ILogicObject party,
            bool isJoined)
        {
            if (PartySystem.ServerGetParty(character) is null)
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
            }
        }
    }
}