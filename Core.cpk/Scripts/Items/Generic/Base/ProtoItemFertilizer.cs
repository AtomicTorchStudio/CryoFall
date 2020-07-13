namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoItemFertilizer
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemGeneric
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemFertilizer
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public const string CannotApplyErrorTitle =
            "Cannot apply fertilizer here";

        public const string NotificationFertilizerAlreadyApplied =
            "Fertilizer is already applied";

        public const string NotificationPlantHarvestReady =
            "The plant is ready to be harvested. No point in adding fertilizer.";

        // ReSharper disable once StaticMemberInGenericType
        private static readonly IWorldService WorldService =
            IsClient ? (IWorldService)Client.World : Server.World;

        public abstract string FertilizerShortDescription { get; }

        public abstract double PlantGrowthSpeedMultiplier { get; }

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
                var isWatered = await this.CallServer(_ => _.ServerRemote_ApplyFertilizer(tilePosition, data.Item));
                if (isWatered)
                {
                    // play sound if success
                    this.SoundPresetItem.PlaySound(ItemSound.Use);
                }
            }
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.FertilizerApplication);
        }

        private static GetPlantResult SharedGetAvailablePlantAt(Vector2Ushort tilePosition, ICharacter character)
        {
            var tile = WorldService.GetTile(tilePosition);
            var objectPlant = tile.StaticObjects.FirstOrDefault(so => so.ProtoStaticWorldObject is IProtoObjectPlant);
            if (objectPlant == null)
            {
                return GetPlantResult.Fail(CannotApplyErrorTitle, "Can apply only on plants.");
            }

            if (!objectPlant.ProtoStaticWorldObject
                            .SharedCanInteract(
                                character,
                                objectPlant,
                                writeToLog: false))
            {
                return GetPlantResult.Fail("Too far!", "Come closer to apply fertilizer.");
            }

            return GetPlantResult.Success(objectPlant);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_CannotApplyAlreadyApplied()
        {
            NotificationSystem.ClientShowNotification(
                CannotApplyErrorTitle,
                NotificationFertilizerAlreadyApplied,
                NotificationColor.Bad,
                this.Icon);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_CannotApplyLastHarvestOrRotten()
        {
            NotificationSystem.ClientShowNotification(
                CannotApplyErrorTitle,
                NotificationPlantHarvestReady,
                NotificationColor.Bad,
                this.Icon);
        }

        private bool ServerRemote_ApplyFertilizer(Vector2Ushort tilePosition, IItem item)
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
            var plantPrivateState = objectPlant.GetPrivateState<PlantPrivateState>();

            // validate if can apply fertilizer
            if (plantPrivateState.AppliedFertilizerProto != null)
            {
                // already applied
                this.CallClient(character, _ => _.ClientRemote_CannotApplyAlreadyApplied());
                return false;
            }

            var plantPublicState = objectPlant.GetPublicState<PlantPublicState>();
            if ((plantPrivateState.ProducedHarvestsCount == protoPlant.NumberOfHarvests
                 && protoPlant.NumberOfHarvests > 0)
                || plantPublicState.IsSpoiled)
            {
                // no need to apply
                this.CallClient(character, _ => _.ClientRemote_CannotApplyLastHarvestOrRotten());
                return false;
            }

            // apply fertilizer
            ServerItemUseObserver.NotifyItemUsed(character, item);
            Server.Items.SetCount(item, item.Count - 1);
            plantPrivateState.AppliedFertilizerProto = this;
            protoPlant.ServerRefreshCurrentGrowthDuration(objectPlant);
            character.ServerAddSkillExperience<SkillFarming>(SkillFarming.ExperienceForFertilizing);

            // notify client
            var publicState = objectPlant.GetPublicState<PlantPublicState>();
            if (!publicState.IsFertilized)
            {
                publicState.IsFertilized = true;
            }
            else
            {
                // ensure the client will request update data
                publicState.ServerForceIsFertilizedSync();
            }

            Logger.Important($"Fertilizer applied: {this} to {objectPlant}");
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

    public abstract class ProtoItemFertilizer
        : ProtoItemFertilizer
            <EmptyPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}