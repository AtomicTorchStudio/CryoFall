namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowPowerThresholdsConfiguration : BaseViewModel
    {
        private readonly Action callbackCancel;

        private readonly Action callbackSave;

        private readonly IStaticWorldObject worldObject;

        private byte shutdownPercent;

        private byte startupPercent;

        public ViewModelWindowPowerThresholdsConfiguration(
            IStaticWorldObject worldObject,
            Action callbackSave,
            Action callbackCancel)
        {
            this.worldObject = worldObject;
            this.callbackCancel = callbackCancel;
            this.callbackSave = callbackSave;
            this.IsElectricityProducer = worldObject.ProtoGameObject is IProtoObjectElectricityProducer;

            var privateState = worldObject.GetPrivateState<IObjectElectricityStructurePrivateState>();
            this.ApplyPreset(privateState.ElectricityThresholds);
        }

        public BaseCommand CommandCancel => new ActionCommand(this.ExecuteCommandCancel);

        public BaseCommand CommandReset => new ActionCommand(this.ExecuteCommandReset);

        public BaseCommand CommandSave => new ActionCommand(this.ExecuteCommandSave);

        public bool IsElectricityProducer { get; }

        public byte ShutdownPercent
        {
            get => this.shutdownPercent;
            set
            {
                if (this.shutdownPercent == value)
                {
                    return;
                }

                this.shutdownPercent = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShutdownPercentText));

                this.RefreshLimits();
            }
        }

        public string ShutdownPercentText
            => ElectricityThresholdsPreset.FormatShutdownThreshold(
                this.worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer,
                this.shutdownPercent);

        public byte StartupPercent
        {
            get => this.startupPercent;
            set
            {
                if (this.startupPercent == value)
                {
                    return;
                }

                this.startupPercent = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.StartupPercentText));

                this.RefreshLimits();
            }
        }

        public string StartupPercentText
            => ElectricityThresholdsPreset.FormatStartupThreshold(
                this.worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer,
                this.startupPercent);

        private void ApplyPreset(ElectricityThresholdsPreset thresholds)
        {
            this.startupPercent = thresholds.StartupPercent;
            this.shutdownPercent = thresholds.ShutdownPercent;
            this.RefreshLimits();
        }

        private void ExecuteCommandCancel()
        {
            this.callbackCancel();
        }

        private void ExecuteCommandReset()
        {
            var thresholds = this.worldObject.ProtoGameObject switch
            {
                IProtoObjectElectricityProducer protoProducer => protoProducer.DefaultGenerationElectricityThresholds,
                IProtoObjectElectricityConsumer protoConsumer => protoConsumer.DefaultConsumerElectricityThresholds,
                _                                             => throw new InvalidOperationException()
            };

            this.ApplyPreset(thresholds);

            this.NotifyPropertyChanged(nameof(this.StartupPercent));
            this.NotifyPropertyChanged(nameof(this.StartupPercentText));
            this.NotifyPropertyChanged(nameof(this.ShutdownPercent));
            this.NotifyPropertyChanged(nameof(this.ShutdownPercentText));
        }

        private void ExecuteCommandSave()
        {
            this.callbackSave();
        }

        private void RefreshLimits()
        {
            var thresholds = new ElectricityThresholdsPreset(this.startupPercent, this.shutdownPercent);
            thresholds = thresholds.Normalize(this.IsElectricityProducer);
            this.StartupPercent = thresholds.StartupPercent;
            this.ShutdownPercent = thresholds.ShutdownPercent;
        }
    }
}