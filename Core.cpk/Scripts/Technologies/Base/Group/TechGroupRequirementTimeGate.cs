namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TechGroupRequirementTimeGate : BaseTechGroupRequirement
    {
        public const string DescriptionFormat = "Unavailable for {0} after server wipe (please wait {1}).";

        public TechGroupRequirementTimeGate(double durationSeconds)
        {
            this.DurationSeconds = durationSeconds;
        }

        public double DurationSeconds { get; }

        public double CalculateTimeRemains()
        {
            var serverTimeSinceWipe = Api.IsServer
                                          ? Api.Server.Game.SecondsSinceWorldCreation
                                          : Api.Client.CurrentGame.SecondsSinceWorldCreation;

            return this.DurationSeconds - serverTimeSinceWipe;
        }

        public override BaseViewModelTechGroupRequirement CreateViewModel()
        {
            return new ViewModelTechGroupRequirementTimeGate(this);
        }

        protected override bool IsSatisfied(CharacterContext context, out string errorMessage)
        {
            var deltaTime = this.CalculateTimeRemains();
            var isSatisfied = deltaTime <= 0;
            if (isSatisfied)
            {
                errorMessage = null;
                return true;
            }

            errorMessage = string.Format(DescriptionFormat,
                                         ClientTimeFormatHelper.FormatTimeDuration(
                                             TimeSpan.FromSeconds(this.DurationSeconds),
                                             trimRemainder: true),
                                         ClientTimeFormatHelper.FormatTimeDuration(deltaTime));
            return false;
        }
    }
}