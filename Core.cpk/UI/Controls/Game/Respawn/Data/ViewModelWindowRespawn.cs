namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Respawn.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterRespawn;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowRespawn : BaseViewModel
    {
        private readonly Action callbackRefreshHeght;

        private double cooldownSecondsRemains;

        public ViewModelWindowRespawn(Action callbackRefreshHeght)
        {
            this.callbackRefreshHeght = callbackRefreshHeght;
            if (IsDesignTime)
            {
                return;
            }

            this.RefreshCanRespawnAtBed();
            this.RefreshDamageSources();
            this.RefreshMessage();

            ClientUpdateHelper.UpdateCallback += this.Update;
            PveSystem.ClientIsPvEChanged += this.RefreshMessage;

            // Disable respawn buttons for 5 seconds.
            // So player will not click too early
            // and will pay attention to the respawn reasons.
            this.IsRespawnButtonsEnabled = false;
            ClientTimersSystem.AddAction(
                delaySeconds: 4,
                () => this.IsRespawnButtonsEnabled = true);
        }

        public bool CanRespawnAtBed { get; set; }

        public BaseCommand CommandRespawnAtBed => new ActionCommand(
            CharacterRespawnSystem.Instance.ClientRequestRespawnAtBed);

        public BaseCommand CommandRespawnInWorld => new ActionCommand(
            CharacterRespawnSystem.Instance.ClientRequestRespawnInWorld);

        public BaseCommand CommandRespawnNearBed => new ActionCommand(
            CharacterRespawnSystem.Instance.ClientRequestRespawnNearBed);

        public IList<ViewModelDamageSource> DamageSourcesList { get; private set; }

        public bool HasBed { get; private set; }

        public bool IsDespawned { get; private set; }

        public bool IsNewbiePvPdeath { get; private set; }

        public bool IsRegularDeath { get; private set; }

        public bool IsRespawnButtonsEnabled { get; private set; }

        public string Message { get; private set; } = CoreStrings.WindowRespawn_Message;

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
            ClientUpdateHelper.UpdateCallback -= this.Update;
            PveSystem.ClientIsPvEChanged -= this.RefreshMessage;
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

        private async void RefreshDamageSources()
        {
            var serverResult = await CharacterDamageTrackingSystem.ClientGetDamageTrackingStatsAsync();
            if (serverResult == null)
            {
                this.DamageSourcesList = null;
                this.callbackRefreshHeght();
                return;
            }

            var list = new List<ViewModelDamageSource>(capacity: serverResult.Count);
            var index = 0;
            var accumulatedPercent = 0;
            while (index < serverResult.Count)
            {
                var entry = serverResult[index];
                if (entry.Fraction < 0.05
                    && list.Count >= 1)
                {
                    // too low percent - will add to "Other" percent
                    break;
                }

                var roundedPercent = (int)Math.Round(entry.Fraction * 100,
                                                     MidpointRounding.AwayFromZero);
                if (roundedPercent < 1)
                {
                    roundedPercent = 1;
                }

                list.Add(new ViewModelDamageSource(entry.ProtoEntity,
                                                   entry.Name,
                                                   roundedPercent));
                accumulatedPercent += roundedPercent;
                index++;
            }

            if (accumulatedPercent < 100)
            {
                // add "Other" percent
                list.Add(new ViewModelDamageSource(null,
                                                   null,
                                                   100 - accumulatedPercent));
            }

            this.DamageSourcesList = list;
            this.callbackRefreshHeght();
        }

        // should be called only when the damage sources list is processed
        private async void RefreshMessage()
        {
            if (CharacterDespawnSystem.ClientIsDespawned)
            {
                this.IsDespawned = true;
                this.Message = CoreStrings.WindowRespawn_MessageDespawned;
                return;
            }

            this.IsDespawned = false;
            if (PveSystem.ClientIsPve(logErrorIfDataIsNotYetAvailable: false))
            {
                this.Message = CoreStrings.WindowRespawn_MessagePvE;
                this.IsRegularDeath = true;
                return;
            }

            var isNewbiePvPdeath = await NewbieProtectionSystem.ClientGetLatestDeathIsNewbiePvP();
            this.IsNewbiePvPdeath = isNewbiePvPdeath;
            this.IsRegularDeath = !isNewbiePvPdeath;

            this.Message = isNewbiePvPdeath
                               ? CoreStrings.WindowRespawn_MessageWithNewbieProtection
                               : CoreStrings.WindowRespawn_Message;
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