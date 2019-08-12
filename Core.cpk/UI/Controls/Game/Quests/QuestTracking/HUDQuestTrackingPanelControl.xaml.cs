namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Quests.QuestTracking
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDQuestTrackingPanelControl : BaseUserControl
    {
        private Border innerBorder;

        private bool? isHidden;

        private NetworkSyncList<PlayerCharacterQuests.CharacterQuestEntry> questsList;

        private Dictionary<PlayerCharacterQuests.CharacterQuestEntry, HUDQuestTrackingEntryControl> registeredEntries;

        private UIElementCollection stackPanelChildren;

        private StateSubscriptionStorage stateSubscriptionStorage;

        private Storyboard storyboardHide;

        private Storyboard storyboardHighlight;

        private bool storyboardHighlightIsPlaying;

        private Storyboard storyboardShow;

        protected override void InitControl()
        {
            this.innerBorder = this.GetByName<Border>("InnerBorder");
            this.stackPanelChildren = this.GetByName<StackPanel>("StackPanel").Children;
            this.storyboardShow = this.GetResource<Storyboard>("StoryboardShow");
            this.storyboardHide = this.GetResource<Storyboard>("StoryboardHide");
            this.storyboardHighlight = this.GetResource<Storyboard>("StoryboardHighlight");
        }

        protected override void OnLoaded()
        {
            this.registeredEntries
                = new Dictionary<PlayerCharacterQuests.CharacterQuestEntry, HUDQuestTrackingEntryControl>();
            this.stateSubscriptionStorage = new StateSubscriptionStorage();

            ClientUpdateHelper.UpdateCallback += this.Update;
            this.questsList = ClientCurrentCharacterHelper.PrivateState.Quests.Quests;
            this.questsList.ClientElementInserted += this.QuestsListElementInserted;
            this.questsList.ClientElementRemoved += this.QuestsListElementRemoved;

            foreach (var questEntry in this.questsList)
            {
                this.Register(questEntry);
            }
        }

        protected override void OnUnloaded()
        {
            ClientUpdateHelper.UpdateCallback -= this.Update;
            this.questsList.ClientElementInserted -= this.QuestsListElementInserted;
            this.questsList.ClientElementRemoved -= this.QuestsListElementRemoved;
            this.questsList = null;

            this.stateSubscriptionStorage.Dispose();
            this.stateSubscriptionStorage = null;

            this.registeredEntries.Clear();
            this.stackPanelChildren.Clear();
        }

        private bool IsRequiresHighlight()
        {
            if (this.isHidden ?? true)
            {
                return false;
            }

            if (WindowsManager.OpenedWindowsCount > 0)
            {
                return false;
            }

            foreach (var questEntry in this.questsList)
            {
                if (questEntry.IsNew
                    || (questEntry.AreAllRequirementsSatisfied
                        && !questEntry.IsCompleted))
                {
                    return true;
                }
            }

            return false;
        }

        private void QuestsListElementInserted(
            NetworkSyncList<PlayerCharacterQuests.CharacterQuestEntry> source,
            int index,
            PlayerCharacterQuests.CharacterQuestEntry value)
        {
            this.Register(value);
        }

        private void QuestsListElementRemoved(
            NetworkSyncList<PlayerCharacterQuests.CharacterQuestEntry> source,
            int index,
            PlayerCharacterQuests.CharacterQuestEntry removedvalue)
        {
            foreach (var stackPanelChild in this.stackPanelChildren)
            {
                var control = (HUDQuestTrackingEntryControl)stackPanelChild;
                if (control.QuestEntry == removedvalue)
                {
                    control.Hide(quick: false);
                    return;
                }
            }
        }

        private void RefreshAllEntries()
        {
            foreach (var pair in this.registeredEntries.ToList())
            {
                var questEntry = pair.Key;
                var control = pair.Value;
                if (questEntry.IsCompleted
                    && control != null)
                {
                    control.Hide(quick: false);
                    this.registeredEntries[questEntry] = null;
                }
                else if (!questEntry.IsCompleted
                         && control == null)
                {
                    this.registeredEntries.Remove(questEntry);
                    // register again
                    this.Register(questEntry);
                }
            }
        }

        private void Register(PlayerCharacterQuests.CharacterQuestEntry questEntry)
        {
            if (this.registeredEntries.ContainsKey(questEntry))
            {
                return;
            }

            questEntry.ClientSubscribe(_ => _.IsCompleted,
                                       _ => this.RefreshAllEntries(),
                                       this.stateSubscriptionStorage);

            if (questEntry.IsCompleted)
            {
                this.registeredEntries[questEntry] = null;
                return;
            }

            var control = HUDQuestTrackingEntryControl.Create(questEntry);
            this.registeredEntries[questEntry] = control;
            this.stackPanelChildren.Add(control);
        }

        private void Update()
        {
            this.UpdateHightlight();

            var shouldBeHidden = this.stackPanelChildren.Count == 0;
            if (this.isHidden.HasValue
                && this.isHidden == shouldBeHidden)
            {
                return;
            }

            this.isHidden = shouldBeHidden;
            if (shouldBeHidden)
            {
                this.storyboardShow.Stop();
                this.storyboardHide.Begin();
            }
            else
            {
                this.storyboardHide.Stop();
                this.storyboardShow.Begin();
            }
        }

        private void UpdateHightlight()
        {
            if (!this.IsRequiresHighlight())
            {
                if (this.storyboardHighlightIsPlaying)
                {
                    this.storyboardHighlightIsPlaying = false;
                    this.storyboardHighlight.Stop(this.innerBorder);
                }

                return;
            }

            if (this.storyboardHighlightIsPlaying)
            {
                return;
            }

            this.storyboardHighlightIsPlaying = true;
            this.storyboardHighlight.Begin(this.innerBorder, isControllable: true);
        }
    }
}