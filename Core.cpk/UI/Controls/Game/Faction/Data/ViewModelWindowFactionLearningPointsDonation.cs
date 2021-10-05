namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using JetBrains.Annotations;

    public class ViewModelWindowFactionLearningPointsDonation : BaseViewModel
    {
        private readonly Action callbackCloseWindow;

        private readonly byte toLevel;

        private ushort learningPointsToDonateSelected;

        public ViewModelWindowFactionLearningPointsDonation([NotNull] Action callbackCloseWindow)
        {
            this.callbackCloseWindow = callbackCloseWindow;

            var faction = FactionSystem.ClientCurrentFaction;
            var factionPublicState = Faction.GetPublicState(faction);
            var factionPrivateState = Faction.GetPrivateState(faction);

            this.toLevel = (byte)(factionPublicState.Level + 1);
            this.UpdateCostLearningPoints = FactionConstants.SharedGetFactionUpgradeCost(
                toLevel: this.toLevel);

            this.FactionAccumulatedLearningPointsForUpgrade = factionPrivateState.AccumulatedLearningPointsForUpgrade;

            this.LearningPointsToDonateMax
                = (ushort)Math.Max(0,
                                   this.UpdateCostLearningPoints
                                   - factionPrivateState.AccumulatedLearningPointsForUpgrade);

            this.PlayerAvailableLearningPoints = ClientComponentTechnologiesWatcher.CurrentTechnologies.LearningPoints;
            this.LearningPointsToDonateMax = (ushort)Math.Min(this.LearningPointsToDonateMax,
                                                              this.PlayerAvailableLearningPoints);

            this.LearningPointsToDonateSelected = this.LearningPointsToDonateMax;

            if (this.LearningPointsToDonateMax == 0
                && this.PlayerAvailableLearningPoints > 0)
            {
                // no need to donate anything as the required amount of LP is already present
                FactionSystem.ClientUpgradeFactionLevel(learningPointsDonation: 0,
                                                        this.toLevel);
                callbackCloseWindow();
            }
        }

        public bool CanDonate => this.LearningPointsToDonateSelected > 0;

        public BaseCommand CommandCancel
            => new ActionCommand(this.ExecuteCommandCancel);

        public BaseCommand CommandDonate
            => new ActionCommand(this.ExecuteCommandDonate);

        public ushort FactionAccumulatedLearningPointsForUpgrade { get; }

        public ushort LearningPointsToDonateMax { get; }

        public ushort LearningPointsToDonateSelected
        {
            get => this.learningPointsToDonateSelected;
            set
            {
                if (this.LearningPointsToDonateSelected == value)
                {
                    return;
                }

                this.learningPointsToDonateSelected = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.CanDonate));
            }
        }

        public uint PlayerAvailableLearningPoints { get; }

        public ushort UpdateCostLearningPoints { get; }

        private void ExecuteCommandCancel()
        {
            this.callbackCloseWindow.Invoke();
        }

        private void ExecuteCommandDonate()
        {
            this.callbackCloseWindow.Invoke();
            FactionSystem.ClientUpgradeFactionLevel(this.LearningPointsToDonateSelected, this.toLevel);
        }
    }
}