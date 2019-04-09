namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class QuestRequirement<TState> : IQuestRequirement
        where TState : QuestRequirementState, new()
    {
        private readonly Dictionary<ICharacter, ServerCharacterActiveQuestRequirement> activeContexts
            = new Dictionary<ICharacter, ServerCharacterActiveQuestRequirement>();

        private readonly string description;

        private IProtoQuest quest;

        protected QuestRequirement(string description)
        {
            this.description = description;
        }

        public string Description => this.description
                                     ?? this.AutoDescription;

        public abstract bool IsReversible { get; }

        public IProtoQuest Quest
        {
            get => this.quest;
            set
            {
                if (this.quest != null)
                {
                    throw new Exception("Quest object is already associated with this requirement");
                }

                this.quest = value;
            }
        }

        protected virtual string AutoDescription { get; }

        public QuestRequirementState CreateState()
        {
            return new TState();
        }

        public bool IsStateTypeMatch(QuestRequirementState state)
        {
            return state is TState;
        }

        public void ServerRefreshIsSatisfied(ICharacter character, QuestRequirementState state)
        {
            if (state.IsSatisfied
                && !this.IsReversible)
            {
                // was already completed - don't change
                return;
            }

            if (!character.IsOnline)
            {
                // don't refresh for offline characters
                return;
            }

            var wasSatisfied = state.IsSatisfied;
            state.IsSatisfied = this.ServerIsSatisfied(character, (TState)state);
            if (wasSatisfied == state.IsSatisfied)
            {
                return;
            }

            if (!wasSatisfied)
            {
                Api.Logger.Important("Requirement satisfied completely: " + this, character);
            }
            else
            {
                Api.Logger.Important("Reversible requirement became unsatisfied: " + this, character);
            }
        }

        public void ServerRegisterOrUnregisterContext(ServerCharacterActiveQuestRequirement context)
        {
            if (context.IsActive)
            {
                this.activeContexts.Add(context.Character, context);
                if (this.activeContexts.Count == 1)
                {
                    this.SetTriggerActive(isActive: true);
                }
            }
            else
            {
                this.activeContexts.Remove(context.Character);
                if (this.activeContexts.Count == 0)
                {
                    this.SetTriggerActive(isActive: false);
                }
            }
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }

        protected ServerCharacterActiveQuestRequirement GetActiveContext(ICharacter character, out TState state)
        {
            if (this.activeContexts.TryGetValue(character, out var result))
            {
                state = (TState)result.QuestRequirementState;
                return result;
            }

            state = default;
            return null;
        }

        protected abstract bool ServerIsSatisfied(ICharacter character, TState state);

        protected void ServerRefreshAllActiveContexts()
        {
            if (this.activeContexts.Count == 0)
            {
                return;
            }

            using (var tempList =
                Api.Shared.GetTempList<KeyValuePair<ICharacter, ServerCharacterActiveQuestRequirement>>())
            {
                // make a copy of the list (as it could be modified during the enumeration)
                // and process it
                tempList.AddRange(this.activeContexts);
                foreach (var pair in tempList)
                {
                    pair.Value.Refresh();
                }
            }
        }

        protected virtual void SetTriggerActive(bool isActive)
        {
        }
    }
}