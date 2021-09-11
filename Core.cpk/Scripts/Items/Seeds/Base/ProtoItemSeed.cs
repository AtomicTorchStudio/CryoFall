namespace AtomicTorch.CBND.CoreMod.Items.Seeds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Item prototype for plant seed items.
    /// </summary>
    public abstract class ProtoItemSeed
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItem
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemSeed
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        // part of a message "Requires: farm plot, plant pot"
        public const string DescriptionAcceptedPlacementLocation = "Requires";

        private IConstructionTileRequirementsReadOnly tileRequirementsPlantPlacement;

        public IReadOnlyList<IProtoObjectFarm> AllowedToPlaceAtFarmObjects { get; private set; }

        /// <summary>
        /// By default max stack size is "Medium".
        /// </summary>
        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public IProtoObjectVegetation ObjectPlantProto { get; private set; }

        public void ClientPlaceAt(IItem itemSeed, Vector2Ushort tilePosition)
        {
            this.SoundPresetItem.PlaySound(ItemSound.Use);
            this.CallServer(_ => _.ServerRemote_PlaceAt(itemSeed, tilePosition));
        }

        public void SharedIsValidPlacementPosition(
            Vector2Ushort tilePosition,
            ICharacter character,
            bool logErrors,
            out bool canPlace,
            out bool isTooFar,
            out string errorMessage)
        {
            canPlace = this.tileRequirementsPlantPlacement.Check(this.ObjectPlantProto,
                                                                 tilePosition,
                                                                 character,
                                                                 errorMessage: out errorMessage,
                                                                 logErrors);
            if (!canPlace)
            {
                isTooFar = false;
                return;
            }

            isTooFar = this.SharedIsTooFarToPlace(tilePosition,
                                                  character,
                                                  logErrors: logErrors);
        }

        protected static IProtoObjectVegetation GetPlant<TProtoVegetation>()
            where TProtoVegetation : class, IProtoObjectVegetation, new()
        {
            return GetProtoEntity<TProtoVegetation>();
        }

        protected static IProtoObjectFarm GetPlot<TProtoPlot>()
            where TProtoPlot : class, IProtoObjectFarm, new()
        {
            return GetProtoEntity<TProtoPlot>();
        }

        protected override void ClientItemHotbarSelectionChanged(ClientHotbarItemData data)
        {
            ClientSeedPlacerHelper.Setup(data.Item, data.IsSelected);
        }

        protected override string GenerateIconPath()
        {
            return "Items/Seeds/" + this.GetType().Name;
        }

        protected sealed override void PrepareProtoItem()
        {
            base.PrepareProtoItem();
            var allowedToPlaceAt = new List<IProtoObjectFarm>();
            this.PrepareProtoItemSeed(out var plantProto, allowedToPlaceAt);

            this.ObjectPlantProto = plantProto;
            this.AllowedToPlaceAtFarmObjects = allowedToPlaceAt.Distinct().ToList();

            this.tileRequirementsPlantPlacement = this.PrepareTileRequirements()
                                                  ?? throw new Exception("No tile requirements provided");
        }

        protected abstract void PrepareProtoItemSeed(
            out IProtoObjectVegetation objectPlantProto,
            List<IProtoObjectFarm> allowedToPlaceAt);

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemSeed;
        }

        protected virtual ConstructionTileRequirements PrepareTileRequirements()
        {
            if (this.AllowedToPlaceAtFarmObjects.Count == 0)
            {
                throw new Exception(
                    "During " + nameof(this.PrepareProtoItemSeed) + " no allowed farm objects were provided");
            }

            return new ConstructionTileRequirements()
                   .Add( // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                       DescriptionAcceptedPlacementLocation
                       + ":[br]"
                       + this.AllowedToPlaceAtFarmObjects.Select(s => s.Name).GetJoinedString(),
                       context =>
                       {
                           // check that the tile contains only the farm plot object and/or floor
                           var tileStaticObjects = context.Tile.StaticObjects;
                           if (tileStaticObjects.Count < 1)
                           {
                               return false;
                           }

                           var isAllowed = false;
                           foreach (var staticWorldObject in tileStaticObjects)
                           {
                               var protoStaticWorldObject = staticWorldObject.ProtoStaticWorldObject;
                               if (this.AllowedToPlaceAtFarmObjects
                                       .Contains(protoStaticWorldObject))
                               {
                                   isAllowed = true;
                               }
                               else // not allowed farm object
                               {
                                   if (protoStaticWorldObject.Kind == StaticObjectKind.Floor)
                                   {
                                       // allow floor
                                       continue;
                                   }

                                   return false;
                               }
                           }

                           return isAllowed;
                       })
                   .Add(ConstructionTileRequirements.ValidatorClientOnlyNoCurrentPlayer)
                   .Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyDynamic)
                   .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                   .Add(LandClaimSystem.ValidatorIsOwnedOrFreeLandNoFactionPermissionsRequired);
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_CannotPlaceTooFar()
        {
            NotificationSystem.ClientShowNotification(CoreStrings.Notification_CannotPlaceThere_Title,
                                                      CoreStrings.Notification_TooFar,
                                                      NotificationColor.Bad,
                                                      this.ObjectPlantProto.Icon);
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered, timeInterval: 0.2)]
        private void ServerRemote_PlaceAt(IItem item, Vector2Ushort tilePosition)
        {
            var character = ServerRemoteContext.Character;
            this.ServerValidateItemForRemoteCall(item, character);

            this.SharedIsValidPlacementPosition(
                tilePosition,
                character,
                logErrors: true,
                canPlace: out var canPlace,
                isTooFar: out var isTooFar,
                errorMessage: out _);
            if (!canPlace || isTooFar)
            {
                return;
            }

            var plantObject = Server.World.CreateStaticWorldObject(this.ObjectPlantProto, tilePosition);
            if (this.ObjectPlantProto is IProtoObjectPlant protoFarmPlant)
            {
                protoFarmPlant.ServerSetBonusForCharacter(plantObject, character);
            }

            Logger.Important($"{character} has placed plant {plantObject} from seed {item}");

            this.ServerNotifyItemUsed(character, item);
            // decrease item count
            Server.Items.SetCount(item, (ushort)(item.Count - 1));

            character.ServerAddSkillExperience<SkillFarming>(SkillFarming.ExperienceForSeedPlanting);

            // restore structure points and reset decay for the farm(s) in the tile where the seed was planted
            foreach (var tileObject in plantObject.OccupiedTile.StaticObjects)
            {
                if (tileObject.ProtoStaticWorldObject is not IProtoObjectFarm protoFarm)
                {
                    continue;
                }

                tileObject.GetPublicState<StaticObjectPublicState>()
                          .StructurePointsCurrent = protoFarm.SharedGetStructurePointsMax(tileObject);
            }
        }

        private bool SharedIsTooFarToPlace(Vector2Ushort tilePosition, ICharacter character, bool logErrors)
        {
            if (character.TilePosition.TileDistanceTo(tilePosition)
                <= ClientSeedPlacerHelper.MaxSeedPlacementDistance)
            {
                return false;
            }

            if (!logErrors)
            {
                return true;
            }

            if (IsClient)
            {
                this.ClientRemote_CannotPlaceTooFar();
            }
            else
            {
                this.CallClient(character, _ => _.ClientRemote_CannotPlaceTooFar());
            }

            return true;
        }
    }

    /// <summary>
    /// Item prototype for plant seed items.
    /// </summary>
    public abstract class ProtoItemSeed
        : ProtoItemSeed
            <EmptyPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}