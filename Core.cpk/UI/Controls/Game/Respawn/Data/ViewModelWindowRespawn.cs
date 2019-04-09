namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Respawn.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Core;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterRespawn;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowRespawn : BaseViewModel
    {
        private double cooldownSecondsRemains;

        public ViewModelWindowRespawn()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.RefreshCanRespawnAtBed();

            ClientComponentUpdateHelper.UpdateCallback += this.Update;
        }

        public bool CanRespawnAtBed { get; set; }

        public BaseCommand CommandRespawnAtBed => new ActionCommand(
            CharacterRespawnSystem.Instance.ClientRequestRespawnAtBed);

        public BaseCommand CommandRespawnInWorld => new ActionCommand(
            CharacterRespawnSystem.Instance.ClientRequestRespawnInWorld);

        public BaseCommand CommandRespawnNearBed => new ActionCommand(
            CharacterRespawnSystem.Instance.ClientRequestRespawnNearBed);

        public bool HasBed { get; private set; }

        public string TextCooldownSecondsRemains { get; private set; }

        public Visibility VisibilityBedCooldownTimer { get; set; } = Visibility.Collapsed;

        private double CooldownSecondsRemains
        {
            get => this.cooldownSecondsRemains;
            set
            {
                this.cooldownSecondsRemains = value;
                this.TextCooldownSecondsRemains =
                    ClientTimeFormatHelper.FormatTimeDuration(this.cooldownSecondsRemains);
                this.Refresh();
            }
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            ClientComponentUpdateHelper.UpdateCallback -= this.Update;
        }

        private void Refresh()
        {
            this.CanRespawnAtBed = this.HasBed
                                   && this.CooldownSecondsRemains <= 0;

            this.VisibilityBedCooldownTimer = this.HasBed
                                              && this.CooldownSecondsRemains > 0
                                                  ? Visibility.Visible
                                                  : Visibility.Collapsed;
        }

        private async void RefreshCanRespawnAtBed()
        {
            var result = await CharacterRespawnSystem.Instance.ClientGetHasBedAsync();
            this.HasBed = result.Item1;
            this.CooldownSecondsRemains = result.Item2;
        }

        private void Update()
        {
            if (this.HasBed
                && this.CooldownSecondsRemains > 0)
            {
                this.CooldownSecondsRemains -= Client.Core.DeltaTime;
            }
        }
    }
}