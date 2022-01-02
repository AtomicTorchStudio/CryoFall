namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelSkinEarningProgressControl : BaseViewModel
    {
        private const double AnimationStartDelay = 0.35;

        private double progressAnimationDelayRemains;

        public ViewModelSkinEarningProgressControl()
        {
            Client.Microtransactions.SkinsDataReceived += this.SkinsDataReceivedHandler;
            ClientUpdateHelper.UpdateCallback += this.Update;
            this.Reset();
        }

        public bool CanClaimEarnedSkin
            => this.DaysToUnlockRemains == 0
               && this.DaysToUnlockRequired > 0;

        public BaseCommand CommandClaimEarnedSkin
            => new ActionCommand(ExecuteCommandClaimEarnedSkin);

        public int DaysToUnlockPassed { get; private set; }

        public int DaysToUnlockRemains => this.DaysToUnlockRequired - this.DaysToUnlockPassed;

        public int DaysToUnlockRequired { get; private set; }

        /// <summary>
        /// Skin earning progress is displayed only if there are skins to earn.
        /// </summary>
        public bool IsVisible => this.DaysToUnlockRequired > 0;

        public double ProgressBarValueCurrent { get; set; }

        public double ProgressBarValueMax => 100;

        public string ProgressText
            => string.Format(CoreStrings.Skins_FreeSkinProgress_Format,
                             this.DaysToUnlockRemains);

        public int TicksCount => this.DaysToUnlockRequired;

        public IReadOnlyList<int> TicksEnumeration => Enumerable.Range(0,
                                                                       count: Math.Max(this.TicksCount - 1, 1))
                                                                .ToList();

        public void ResetProgressBar()
        {
            this.ProgressBarValueCurrent = 0;
            this.progressAnimationDelayRemains = AnimationStartDelay;
            this.NotifyPropertyChanged(nameof(this.TicksCount));
            this.NotifyPropertyChanged(nameof(this.TicksEnumeration));
        }

        protected override void DisposeViewModel()
        {
            Client.Microtransactions.SkinsDataReceived -= this.SkinsDataReceivedHandler;
            ClientUpdateHelper.UpdateCallback -= this.Update;
            base.DisposeViewModel();
        }

        private static void ExecuteCommandClaimEarnedSkin()
        {
            Client.Microtransactions.ClaimEarnedSkin();
            SkinsMenuOverlay.IsDisplayed = true;
        }

        private void Reset()
        {
            this.DaysToUnlockPassed = Client.Microtransactions.DaysToUnlockEarnedSkinPassed;
            this.DaysToUnlockRequired = Client.Microtransactions.DaysToUnlockEarnedSkinRequired;
            this.NotifyPropertyChanged(nameof(this.DaysToUnlockRemains));
            this.NotifyPropertyChanged(nameof(this.ProgressText));
            this.NotifyPropertyChanged(nameof(this.CanClaimEarnedSkin));
            this.NotifyPropertyChanged(nameof(this.IsVisible));
            this.ResetProgressBar();
        }

        private void SkinsDataReceivedHandler()
        {
            this.Reset();
        }

        /// <summary>
        /// Animate the progress bar so it's filled smoothly and slowly.
        /// </summary>
        private void Update()
        {
            var deltaTime = Api.Client.Core.DeltaTime;

            if (this.progressAnimationDelayRemains > 0)
            {
                this.progressAnimationDelayRemains -= deltaTime;
                if (this.progressAnimationDelayRemains > 0)
                {
                    return;
                }
            }

            var maxValue = this.ProgressBarValueMax;
            var value = this.ProgressBarValueCurrent;
            var actualValue = maxValue
                              * this.DaysToUnlockPassed
                              / (double)this.DaysToUnlockRequired;

            var differenceFraction = Math.Abs(actualValue - value) / maxValue;

            if (differenceFraction < 0.001)
            {
                // difference is too small
                value = actualValue;
            }
            else
            {
                // interpolate
                value = MathHelper.LerpWithDeltaTime(value,
                                                     actualValue,
                                                     deltaTime,
                                                     rate: 10);
            }

            this.ProgressBarValueCurrent = value;
        }
    }
}