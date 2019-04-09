namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementBuildStructure : QuestRequirementWithList<IProtoObjectStructure>
    {
        public const string DescriptionFormat = "Build: {0}";

        public RequirementBuildStructure(
            IReadOnlyList<IProtoObjectStructure> list,
            ushort count,
            string description)
            : base(list, count, description)
        {
        }

        public override bool IsReversible => false;

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ListNames);

        public static RequirementBuildStructure Require<TProtoStructure>(
            ushort count = 1,
            string description = null)
            where TProtoStructure : class, IProtoObjectStructure
        {
            var list = Api.FindProtoEntities<TProtoStructure>();
            return new RequirementBuildStructure(list, count, description);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementStateWithCount state)
        {
            if (base.ServerIsSatisfied(character, state))
            {
                return true;
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

                if (partyMember == null)
                {
                    continue;
                }

                if (partyMember.SharedGetQuests()
                               .SharedHasCompletedRequirement(this))
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

                if (partyMember == null)
                {
                    continue;
                }

                var context = this.GetActiveContext(partyMember, out _);
                context?.Refresh();
            }
        }

        private void ServerCharacterJoinedOrLeftPartyHandler(ICharacter character)
        {
            var context = this.GetActiveContext(character, out _);
            context?.Refresh();

            this.RefreshContextForPartyMembers(character);
        }

        private void StructureBuiltHandler(ICharacter character, IStaticWorldObject structure)
        {
            var context = this.GetActiveContext(character, out var state);
            if (context == null)
            {
                return;
            }

            if (!this.List.Contains(structure.ProtoStaticWorldObject))
            {
                return;
            }

            var isSatisfied = state.IsSatisfied;
            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            context.Refresh();

            if (state.IsSatisfied != isSatisfied)
            {
                this.RefreshContextForPartyMembers(character);
            }
        }
    }
}