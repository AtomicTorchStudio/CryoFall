namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class ItemNeuralRecombinator : ProtoItemMedical
    {
        public const string Message_CannotBeUsedOften_Format =
            @"Please note: this item cannot be used often.
              [br]If you use it now, you will be unable to use it again for {0}.";

        public const string Message_UsedRecently_Format =
            @"You have used this item recently.
              [br]Please wait {0} until you can use this item again.";

        private static readonly double ServerCooldownSeconds
            = TimeSpan.FromDays(4).TotalSeconds;

        private static Dictionary<ICharacter, double> serverLastItemUseTimeByCharacter;

        public override double CooldownDuration => MedicineCooldownDuration.None;

        public override string Description =>
            "Completely clears all knowledge of all technologies upon use and returns all spent learning points so you can redistribute them. Cannot be used often.";

        public override ushort MaxItemsPerStack => ItemStackSize.Small;

        public override double MedicalToxicity => 0;

        public override string Name => "Neural recombinator";

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            this.ClientShowDialog(data);
            return false;
        }

        protected override void PrepareProtoItemMedical()
        {
            base.PrepareProtoItemMedical();

            if (IsClient)
            {
                return;
            }

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            const string databaseKeyPlayerDamageSourceStats = "LastItemUseTimeByCharacter";

            if (Server.Database.TryGet(nameof(ItemNeuralRecombinator),
                                       databaseKeyPlayerDamageSourceStats,
                                       out serverLastItemUseTimeByCharacter))
            {
                return;
            }

            serverLastItemUseTimeByCharacter = new Dictionary<ICharacter, double>();
            Server.Database.Set(nameof(ItemNeuralRecombinator),
                                databaseKeyPlayerDamageSourceStats,
                                serverLastItemUseTimeByCharacter);
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            var technologies = character.SharedGetTechnologies();
            technologies.ServerResetTechTreeAndRefundLearningPoints();
            technologies.IsTechTreeChanged = false;

            serverLastItemUseTimeByCharacter[character] = Server.Game.FrameTime;
        }

        protected override bool SharedCanUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            var technologies = character.SharedGetTechnologies();
            if (technologies.Nodes.Count == 0)
            {
                return false;
            }

            if (IsServer
                && ServerCalculateCooldownRemainsSeconds(character) > 0)
            {
                // used recently
                if (IsServer)
                {
                    Logger.Warning(this.ShortId + " was used recently and under cooldown", character);
                }

                return false;
            }

            return true;
        }

        private static double ServerCalculateCooldownRemainsSeconds(ICharacter character)
        {
            if (!serverLastItemUseTimeByCharacter.TryGetValue(character, out var lastUseTime))
            {
                return 0;
            }

            return Math.Max(0, lastUseTime + ServerCooldownSeconds - Server.Game.FrameTime);
        }

        private async void ClientShowDialog(ClientItemData data)
        {
            var (cooldownDuration, cooldownRemainsSeconds)
                = await this.CallServer(_ => _.ServerRemote_GetCooldownRemainsSeconds());

            if (cooldownRemainsSeconds > 0)
            {
                // under active cooldown
                NotificationSystem.ClientShowNotification(
                    this.Name,
                    string.Format(Message_UsedRecently_Format,
                                  ClientTimeFormatHelper.FormatTimeDuration(
                                      TimeSpan.FromSeconds(
                                          Math.Max(1, cooldownRemainsSeconds)),
                                      trimRemainder: true)),
                    NotificationColor.Bad,
                    this.Icon);
            }
            else // if no active cooldown
            {
                var messageText = string.Format(Message_CannotBeUsedOften_Format,
                                                ClientTimeFormatHelper.FormatTimeDuration(
                                                    TimeSpan.FromSeconds(cooldownDuration),
                                                    trimRemainder: true));

                DialogWindow.ShowDialog(
                    string.Format(CoreStrings.Dialog_AreYouSureWantToUse_Format, this.Name),
                    messageText,
                    okAction: () =>
                              {
                                  if (base.ClientItemUseFinish(data))
                                  {
                                      this.SoundPresetItem.PlaySound(ItemSound.Use);
                                  }
                              },
                    cancelAction: () => { },
                    okText: this.ItemUseCaption,
                    cancelText: CoreStrings.Button_Cancel,
                    focusOnCancelButton: true);
            }
        }

        [RemoteCallSettings(timeInterval: 1)]
        private (double cooldownDuration, double cooldownRemains) ServerRemote_GetCooldownRemainsSeconds()
        {
            return (ServerCooldownSeconds,
                    ServerCalculateCooldownRemainsSeconds(ServerRemoteContext.Character));
        }
    }
}