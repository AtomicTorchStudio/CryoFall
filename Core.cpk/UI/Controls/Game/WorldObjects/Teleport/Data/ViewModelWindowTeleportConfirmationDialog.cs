namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Teleport.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelWindowTeleportConfirmationDialog : BaseViewModel
    {
        private readonly CharacterCurrentStats characterCurrentStats;

        private readonly TaskCompletionSource<bool> taskWaitingForUnfriendlyPlayersCheck = new();

        private bool isRequestedTeleportation;

        public ViewModelWindowTeleportConfirmationDialog(Vector2Ushort teleportWorldPosition)
        {
            this.TeleportWorldPosition = teleportWorldPosition;
            this.characterCurrentStats = PlayerCharacter.GetPublicState(ClientCurrentCharacterHelper.Character)
                                                        .CurrentStats;
            this.characterCurrentStats.ClientSubscribe(_ => _.HealthCurrent,
                                                       _ =>
                                                       {
                                                           this.NotifyPropertyChanged(nameof(this.BloodCostText));
                                                           this.NotifyPropertyChanged(nameof(this.HasEnoughBlood));
                                                       },
                                                       this);

            ClientCurrentCharacterContainersHelper.ContainersItemsReset += this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged +=
                this.ItemAddedOrRemovedOrCountChangedHandler;

            if (PveSystem.ClientIsPve(true))
            {
                this.taskWaitingForUnfriendlyPlayersCheck.SetResult(true);
                return;
            }

            // For PvP: check whether there are any players camping this teleport.
            TeleportsSystem.ClientIsTeleportHasUnfriendlyPlayersNearby(teleportWorldPosition)
                           .ContinueWith(hasPlayersNearby =>
                                         {
                                             if (!this.IsDisposed)
                                             {
                                                 this.HasUnfriendlyPlayersNearby = hasPlayersNearby.Result;
                                                 this.taskWaitingForUnfriendlyPlayersCheck.SetResult(true);
                                             }
                                         },
                                         TaskContinuationOptions.ExecuteSynchronously);
        }

        public string BloodCostText
            => string.Format("{0} {1}",
                             TeleportsSystem.SharedCalculateTeleportationBloodCost(
                                 ClientCurrentCharacterHelper.Character),
                             CoreStrings.HealthPointsAbbreviation);

        public BaseCommand CommandTeleportPayWithBlood
            => new ActionCommand(
                () => this.ExecuteCommandTeleport(payWithItem: false));

        public BaseCommand CommandTeleportPayWithItem
            => new ActionCommand(
                () => this.ExecuteCommandTeleport(payWithItem: true));

        public bool HasEnoughBlood
            => this.characterCurrentStats.HealthCurrent
               > TeleportsSystem.SharedCalculateTeleportationBloodCost(this.characterCurrentStats);

        public bool HasOptionalItem
            => TeleportsSystem.SharedHasOptionalRequiredItem(ClientCurrentCharacterHelper.Character);

        public bool HasUnfriendlyPlayersNearby { get; private set; }

        public IReadOnlyList<ProtoItemWithCount> OptionalInputItems
            => new[]
            {
                new ProtoItemWithCount(
                    TeleportsSystem.OptionalTeleportationItemProto,
                    count: 1)
            };

        public Vector2Ushort TeleportWorldPosition { get; }

        protected override void DisposeViewModel()
        {
            ClientCurrentCharacterContainersHelper.ContainersItemsReset -= this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged -=
                this.ItemAddedOrRemovedOrCountChangedHandler;
            this.taskWaitingForUnfriendlyPlayersCheck.TrySetCanceled();
            base.DisposeViewModel();
        }

        private void ContainersItemsResetHandler()
        {
            this.NotifyPropertyChanged(nameof(this.HasOptionalItem));
        }

        private async void ExecuteCommandTeleport(bool payWithItem)
        {
            if (this.isRequestedTeleportation)
            {
                return;
            }

            this.isRequestedTeleportation = true;
            await this.taskWaitingForUnfriendlyPlayersCheck.Task;
            this.isRequestedTeleportation = false;

            if (this.IsDisposed)
            {
                return;
            }

            if (!this.HasUnfriendlyPlayersNearby)
            {
                TeleportsSystem.ClientTeleport(this.TeleportWorldPosition, payWithItem);
                return;
            }

            // ask for confirmation
            var textBlock = DialogWindow.CreateTextElement(CoreStrings.Teleport_OtherPlayersNearby, TextAlignment.Left);
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Foreground = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed6");

            DialogWindow.ShowDialog(
                title: CoreStrings.QuestionAreYouSure,
                content: textBlock,
                okAction: () => TeleportsSystem.ClientTeleport(this.TeleportWorldPosition, payWithItem),
                okText: CoreStrings.Button_OK,
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        private void ItemAddedOrRemovedOrCountChangedHandler(IItem obj)
        {
            this.NotifyPropertyChanged(nameof(this.HasOptionalItem));
        }
    }
}