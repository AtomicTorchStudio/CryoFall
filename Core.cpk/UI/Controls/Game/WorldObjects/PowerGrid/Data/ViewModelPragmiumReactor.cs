namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items.Reactor;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelPragmiumReactor : BaseViewModel
    {
        private static readonly Brush PsiBrushGray = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99));

        private static readonly Brush PsiBrushOrange = Api.Client.UI.GetApplicationResource<Brush>("BrushColor6");

        private static readonly Brush PsiBrushRed = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed6");

        private static readonly Brush StatusBrushOffline
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorAlt6"); // blue

        private static readonly Brush StatusBrushOperational
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorGreen6"); // green

        private static readonly Brush StatusBrushStartingUp
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColor6"); // orange

        private static readonly Brush StatusBrushShuttingDown = StatusBrushStartingUp; // also orange

        private readonly byte reactorIndex;

        private readonly ObjectGeneratorPragmiumReactorPrivateState reactorPrivateState;

        private readonly IStaticWorldObject worldObjectGenerator;

        private bool isGlowUp = true;

        private PragmiumReactorStatsData statsData;

        public ViewModelPragmiumReactor(
            IStaticWorldObject worldObjectGenerator,
            byte reactorIndex,
            ObjectGeneratorPragmiumReactorPrivateState reactorPrivateState)
        {
            this.reactorPrivateState = reactorPrivateState;
            this.worldObjectGenerator = worldObjectGenerator;
            this.reactorIndex = reactorIndex;
            if (reactorPrivateState is null)
            {
                return;
            }

            this.HasReactor = true;

            this.ViewModelItemsContainerExchange
                = new ViewModelItemsContainerExchange(reactorPrivateState.ItemsContainer)
                {
                    IsContainerTitleVisible = false,
                    IsManagementButtonsVisible = false
                };

            this.reactorPrivateState.ClientSubscribe(_ => _.IsEnabled,
                                                     _ => this.NotifyPropertyChanged(
                                                         nameof(this.ToggleReactorButtonText)),
                                                     this);

            this.reactorPrivateState.ClientSubscribe(_ => _.ActivationProgressPercents,
                                                     _ => this.RefreshActivity(),
                                                     this);

            this.reactorPrivateState.ClientSubscribe(_ => _.Stats,
                                                     _ => this.RefreshStatsAndActivity(),
                                                     this);
            this.RefreshStatsAndActivity();

            ClientUpdateHelper.UpdateCallback += this.ClientUpdateHelperOnUpdateCallback;

            if (this.reactorPrivateState.ActivationProgressPercents > 0)
            {
                this.ItemsContainerGlowEffectOpacity = 1.0;
            }

            this.RefreshStatusTimeRemainsText();
        }

        public IReadOnlyList<ProtoItemWithCount> BuildRequiredItems
            => this.ProtoGenerator
                   .BuildAdditionalReactorRequiredItems;

        public BaseCommand CommandBuildReactor
            => new ActionCommand(this.ExecuteCommandBuildReactor);

        public BaseCommand CommandToggleReactor
            => new ActionCommand(this.ExecuteCommandToggleReactor);

        public ushort EfficiencyPercentsCurrent
            => (ushort)(this.EfficiencyPercentsMax
                        * (this.reactorPrivateState?.ActivationProgressPercents / 100.0
                           ?? 0));

        public ushort EfficiencyPercentsMax
            => this.reactorPrivateState?.Stats.EfficiencyPercent ?? 100;

        public ushort EfficiencyPercentsMiddle => (ushort)(this.EfficiencyPercentsMax / 2);

        public ushort FuelLifetimePercent => this.statsData.FuelLifetimePercent;

        public bool HasReactor { get; }

        public double ItemsContainerGlowEffectOpacity { get; private set; }

        public string OutputValueCurrent
        {
            get
            {
                var activationProgress = this.GetActivationProgress();
                var maxOutput = this.statsData.OutputValue;
                var currentOutput = activationProgress * maxOutput;
                return $"{currentOutput:0.0} {CoreStrings.EnergyUnitPerSecondAbbreviation}";
            }
        }

        public string OutputValueMaxText
            => $"{this.statsData.OutputValue:0.0} {CoreStrings.EnergyUnitPerSecondAbbreviation}";

        public double PsiEmissionLevelCurrent
        {
            get
            {
                if (this.reactorPrivateState is null)
                {
                    return 0;
                }

                return this.ProtoGenerator.SharedGetPsiEmissionLevelCurrent(this.reactorPrivateState);
            }
        }

        public double PsiEmissionLevelMax => this.statsData.PsiEmissionLevel;

        public Brush PsiEmissionLevelMaxBrush
        {
            get
            {
                var emissionFraction = this.PsiEmissionLevelMax
                                       / Api.GetProtoEntity<ItemReactorFuelRod>().PsiEmissionLevel;
                return emissionFraction switch
                {
                    <= 2 => PsiBrushGray,
                    <= 4 => PsiBrushOrange,
                    _    => PsiBrushRed
                };
            }
        }

        public string ReactorName
            => string.Format(ViewModelWindowGeneratorPragmium.ReactorNameFormat, this.reactorIndex + 1);

        public Brush StatusBrush
        {
            get
            {
                if (this.reactorPrivateState is null)
                {
                    return null;
                }

                return this.reactorPrivateState.ActivationProgressPercents switch
                {
                    0                                         => StatusBrushOffline,
                    100                                       => StatusBrushOperational,
                    _ when this.reactorPrivateState.IsEnabled => StatusBrushStartingUp,
                    _                                         => StatusBrushShuttingDown
                };
            }
        }

        public bool StatusIsStartingUpShuttingDown
            => this.reactorPrivateState is not null
               && this.reactorPrivateState.ActivationProgressPercents > 0
               && this.reactorPrivateState.ActivationProgressPercents < 100;

        public string StatusText
        {
            get
            {
                if (this.reactorPrivateState is null)
                {
                    return null;
                }

                return this.reactorPrivateState.ActivationProgressPercents switch
                {
                    0   => CoreStrings.WindowGeneratorPragmium_ReactorState_Offline,
                    100 => CoreStrings.WindowGeneratorPragmium_ReactorState_Operational,
                    _ when this.reactorPrivateState.IsEnabled
                        => CoreStrings.WindowGeneratorPragmium_ReactorState_StartingUp,
                    _ => CoreStrings.WindowGeneratorPragmium_ReactorState_ShuttingDown
                };
            }
        }

        public string StatusTimeRemainsText
        {
            get
            {
                if (this.reactorPrivateState is null)
                {
                    return null;
                }

                var progress = this.reactorPrivateState.ActivationProgressPercents / 100.0;
                var isEnabled = this.reactorPrivateState.IsEnabled;

                var protoGenerator = this.ProtoGenerator;
                var duration = isEnabled
                                   ? protoGenerator.StartupDuration
                                   : protoGenerator.ShutdownDuration;

                if (isEnabled)
                {
                    progress = 1 - progress;
                }

                var timeRemains = TimeSpan.FromSeconds(progress
                                                       * duration
                                                       * this.reactorPrivateState.Stats.StartupShutdownTimePercent
                                                       / 100.0);
                var sb = new StringBuilder();
                if (timeRemains.Days > 0)
                {
                    sb.Append(timeRemains.Days)
                      .Append(':');
                }

                if (sb.Length > 0
                    || timeRemains.Hours > 0)
                {
                    sb.Append(timeRemains.Hours)
                      .Append(':');
                }

                sb.AppendFormat("{0:00}", timeRemains.Minutes)
                  .Append(':')
                  .AppendFormat("{0:00}", timeRemains.Seconds);

                return sb.ToString();
            }
        }

        public string ToggleReactorButtonText
            => this.reactorPrivateState?.IsEnabled ?? false
                   ? CoreStrings.WindowGeneratorPragmium_StopReactor
                   : CoreStrings.WindowGeneratorPragmium_StartReactor;

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }

        private ProtoObjectGeneratorPragmium ProtoGenerator
            => ((ProtoObjectGeneratorPragmium)this.worldObjectGenerator.ProtoGameObject);

        protected override void DisposeViewModel()
        {
            ClientUpdateHelper.UpdateCallback -= this.ClientUpdateHelperOnUpdateCallback;
            base.DisposeViewModel();
        }

        private void ClientUpdateHelperOnUpdateCallback()
        {
            const double glowSpeed = 0.75;

            var opacity = this.ItemsContainerGlowEffectOpacity;
            var delta = Api.Client.Core.DeltaTime * glowSpeed;
            var isActive = this.reactorPrivateState.ActivationProgressPercents > 0;

            if (!isActive)
            {
                this.isGlowUp = false;
            }

            if (this.isGlowUp)
            {
                opacity += delta;
                if (opacity >= 1)
                {
                    opacity = 1;
                    this.isGlowUp = false;
                }
            }
            else
            {
                opacity -= delta;

                var minOpacity = isActive ? 0.6 : 0;
                if (opacity <= minOpacity)
                {
                    opacity = minOpacity;
                    this.isGlowUp = true;
                }
            }

            this.ItemsContainerGlowEffectOpacity = opacity;
        }

        private void ExecuteCommandBuildReactor()
        {
            this.ProtoGenerator
                .ClientBuildReactor(this.worldObjectGenerator, this.reactorIndex);
        }

        private void ExecuteCommandToggleReactor()
        {
            this.ProtoGenerator
                .ClientSetReactorMode(this.worldObjectGenerator,
                                      this.reactorIndex,
                                      isEnabled: !this.reactorPrivateState.IsEnabled);
        }

        private double GetActivationProgress()
        {
            return (this.reactorPrivateState?.ActivationProgressPercents ?? 0) / 100.0;
        }

        private void RefreshActivity()
        {
            this.NotifyPropertyChanged(nameof(this.OutputValueCurrent));
            this.NotifyPropertyChanged(nameof(this.EfficiencyPercentsCurrent));
            this.NotifyPropertyChanged(nameof(this.PsiEmissionLevelCurrent));
            this.NotifyPropertyChanged(nameof(this.StatusBrush));
            this.NotifyPropertyChanged(nameof(this.StatusText));
            this.NotifyPropertyChanged(nameof(this.StatusIsStartingUpShuttingDown));
            this.NotifyPropertyChanged(nameof(this.StatusTimeRemainsText));
        }

        private void RefreshStatsAndActivity()
        {
            this.statsData = this.reactorPrivateState.Stats;
            this.NotifyPropertyChanged(nameof(this.FuelLifetimePercent));
            this.NotifyPropertyChanged(nameof(this.PsiEmissionLevelMax));
            this.NotifyPropertyChanged(nameof(this.PsiEmissionLevelMaxBrush));
            this.NotifyPropertyChanged(nameof(this.EfficiencyPercentsMiddle));
            this.NotifyPropertyChanged(nameof(this.EfficiencyPercentsMax));
            this.NotifyPropertyChanged(nameof(this.OutputValueCurrent));
            this.NotifyPropertyChanged(nameof(this.OutputValueMaxText));
            this.RefreshActivity();
        }

        private void RefreshStatusTimeRemainsText()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.StatusTimeRemainsText));
            ClientTimersSystem.AddAction(
                delaySeconds: 0.5,
                this.RefreshStatusTimeRemainsText);
        }
    }
}