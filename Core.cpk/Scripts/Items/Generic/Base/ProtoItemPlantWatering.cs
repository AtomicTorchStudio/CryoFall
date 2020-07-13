namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Watering;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoItemPlantWatering
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemGeneric
            <TPrivateState,
                TPublicState,
                TClientState>
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        // Cannot apply Hygroscopic granules (or similar item) on object as it could be applied only to plants.
        public const string CanApplyOnlyOnPlants = "Can apply only on plants.";

        // Cannot apply Hygroscopic granules (or similar item) on an object.
        public const string CannotApplyErrorTitle = "Cannot apply here";

        // ReSharper disable once StaticMemberInGenericType
        private static readonly IWorldService WorldService =
            IsClient ? (IWorldService)Client.World : Server.World;

        public abstract double WateringDuration { get; }

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            var tilePosition = Client.Input.MousePointedTilePosition;
            var result = SharedGetAvailablePlantAt(tilePosition, ClientCurrentCharacterHelper.Character);
            if (!result.IsSuccess)
            {
                NotificationSystem.ClientShowNotification(
                    result.ErrorTitle,
                    result.ErrorMessage,
                    NotificationColor.Bad,
                    this.Icon);
                return false;
            }

            CallServerAsync();
            return false; // don't play sound instantly

            async void CallServerAsync()
            {
                var isWatered = await this.CallServer(_ => _.ServerRemote_ApplyWatering(tilePosition, data.Item));
                if (isWatered)
                {
                    // play sound if success
                    this.SoundPresetItem.PlaySound(ItemSound.Use);
                }
            }
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric.Clone()
                                    .Replace(ItemSound.Use, "Items/Tools/WateringCan/Use");
        }

        private static GetPlantResult SharedGetAvailablePlantAt(Vector2Ushort tilePosition, ICharacter character)
        {
            var tile = WorldService.GetTile(tilePosition);
            var objectPlant = tile.StaticObjects.FirstOrDefault(so => so.ProtoStaticWorldObject is IProtoObjectPlant);
            if (objectPlant == null)
            {
                return GetPlantResult.Fail(CannotApplyErrorTitle, CanApplyOnlyOnPlants);
            }

            if (!objectPlant.ProtoStaticWorldObject
                            .SharedCanInteract(
                                character,
                                objectPlant,
                                writeToLog: false))
            {
                return GetPlantResult.Fail(CoreStrings.Notification_TooFar, null);
            }

            return GetPlantResult.Success(objectPlant);
        }

        private bool ServerRemote_ApplyWatering(Vector2Ushort tilePosition, IItem item)
        {
            var character = ServerRemoteContext.Character;
            this.ServerValidateItemForRemoteCall(item, character);

            var result = SharedGetAvailablePlantAt(tilePosition, character);
            if (!result.IsSuccess)
            {
                Logger.Warning(result.ErrorTitle + ": " + result.ErrorMessage, character);
                return false;
            }

            var objectPlant = result.ObjectPlant;
            var protoPlant = (IProtoObjectPlant)objectPlant.ProtoStaticWorldObject;

            if (!WateringSystem.ServerIsWateringRequired(objectPlant,
                                                         character,
                                                         this,
                                                         protoPlant,
                                                         this.WateringDuration))
            {
                // no watering required (an error notification is sent during the check)
                return false;
            }

            // apply watering
            ServerItemUseObserver.NotifyItemUsed(character, item);
            Server.Items.SetCount(item, item.Count - 1);
            protoPlant.ServerOnWatered(objectPlant,
                                       wateringDuration: this.WateringDuration,
                                       byCharacter: character);

            // notify client
            Logger.Important($"Watering applied: {this} to {objectPlant}");
            return true;
        }

        private struct GetPlantResult : IRemoteCallParameter
        {
            public readonly string ErrorMessage;

            public readonly string ErrorTitle;

            public readonly bool IsSuccess;

            public readonly IStaticWorldObject ObjectPlant;

            private GetPlantResult(string errorTitle, string errorMessage) : this()
            {
                this.IsSuccess = false;
                this.ErrorTitle = errorTitle;
                this.ErrorMessage = errorMessage;
            }

            private GetPlantResult(IStaticWorldObject objectPlant) : this()
            {
                this.IsSuccess = true;
                this.ObjectPlant = objectPlant;
            }

            public static GetPlantResult Fail(string errorTitle, string errorMessage)
            {
                return new GetPlantResult(errorTitle, errorMessage);
            }

            public static GetPlantResult Success(IStaticWorldObject objectPlant)
            {
                return new GetPlantResult(objectPlant);
            }
        }
    }

    public abstract class ProtoItemPlantWatering
        : ProtoItemPlantWatering
            <EmptyPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}