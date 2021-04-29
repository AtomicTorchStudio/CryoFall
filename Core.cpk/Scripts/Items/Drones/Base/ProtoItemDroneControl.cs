namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDroneControl;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoItemDroneControl
        : ProtoItemWithDurability,
          IProtoItemDroneControl,
          IProtoItemWithCharacterAppearance
    {
        private const double DurabilityGranularityMultiplier = 100;

        /// <summary>
        /// The drones will not go automatically to cut trees with growth below 75%
        /// when Shift-Click to send several drones.
        /// </summary>
        private const double TreeGrowthProgressThreshold = 0.75;

        private uint durabilityMax;

        protected ProtoItemDroneControl()
        {
            var typeName = this.GetType().Name;

            this.CharacterTextureResource = new TextureResource(
                "Characters/Tools/" + typeName,
                isProvidesMagentaPixelPosition: true);
        }

        public override bool CanBeSelectedInVehicle => true;

        public TextureResource CharacterTextureResource { get; }

        public abstract TimeSpan DurabilityLifetime { get; }

        public sealed override uint DurabilityMax => this.durabilityMax;

        public override double GroundIconScale => 1.33;

        public override bool IsRepairable => true;

        public abstract byte MaxDronesToControl { get; }

        public override double ServerUpdateIntervalSeconds => 5;

        protected virtual string ActiveLightCharacterAnimationName => "Torch2";

        public virtual void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            // item in hand and animation disabled
            return;

            protoCharacterSkeleton.ClientSetupItemInHand(
                skeletonRenderer,
                "WeaponMelee",
                this.CharacterTextureResource);

            var isActive = true;
            this.ClientSetupSkeletonAnimation(isActive, item, character, skeletonRenderer, skeletonComponents);
        }

        public sealed override void ServerItemHotbarSelectionChanged(
            IItem item,
            ICharacter character,
            bool isSelected)
        {
            if (!isSelected)
            {
                CharacterDroneControlSystem.ServerRecallAllDrones(character);
            }
        }

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            var character = ClientCurrentCharacterHelper.Character;
            var characterTilePosition = character.TilePosition;
            var mouseTilePosition = Client.Input.MousePointedTilePosition;
            var dronesNumberToLaunch = Api.Client.Input.IsKeyHeld(InputKey.Shift, evenIfHandled: true)
                                           ? this.MaxDronesToControl
                                           : 1;

            using var tempExceptDrones = Api.Shared.GetTempList<IItem>();
            using var tempExceptTargets = Api.Shared.GetTempList<Vector2Ushort>();

            for (var index = 0; index < dronesNumberToLaunch; index++)
            {
                var showErrorNotification = index == 0;
                var itemDrone = CharacterDroneControlSystem.ClientSelectNextDrone(tempExceptDrones.AsList());
                if (itemDrone is null)
                {
                    if (CharacterDroneControlSystem.SharedIsMaxDronesToControlNumberExceeded(
                        character,
                        clientShowErrorNotification: showErrorNotification))
                    {
                        break;
                    }

                    if (showErrorNotification)
                    {
                        CannotInteractMessageDisplay.ClientOnCannotInteract(
                            character,
                            CharacterDroneControlSystem.Notification_ErrorNoDrones_Title,
                            isOutOfRange: false);
                    }

                    break;
                }

                tempExceptDrones.Add(itemDrone);
                Vector2Ushort targetPosition;

                if (index == 0)
                {
                    targetPosition = mouseTilePosition;
                    var targetObject = CharacterDroneControlSystem
                        .SharedGetCompatibleTarget(character,
                                                   mouseTilePosition,
                                                   out var hasIncompatibleTarget,
                                                   out var isPveActionForbidden);
                    if (targetObject is null)
                    {
                        if (showErrorNotification)
                        {
                            if (isPveActionForbidden)
                            {
                                PveSystem.ClientShowNotificationActionForbidden();
                            }

                            CannotInteractMessageDisplay.ClientOnCannotInteract(
                                character,
                                hasIncompatibleTarget
                                    ? CharacterDroneControlSystem.Notification_CannotMineThat
                                    : CharacterDroneControlSystem.Notification_NothingToMineThere,
                                isOutOfRange: false);
                        }

                        return false;
                    }

                    if (!WorldObjectClaimSystem.SharedIsAllowInteraction(character,
                                                                         targetObject,
                                                                         showClientNotification: showErrorNotification))
                    {
                        return false;
                    }

                    if (CharacterDroneControlSystem.SharedIsTargetAlreadyScheduledForAnyActiveDrone(
                        character,
                        mouseTilePosition,
                        logError: false))
                    {
                        // already scheduled a drone mining there...try find another target of the same type
                        targetPosition = TryGetNextTargetPosition();
                        if (targetPosition == default)
                        {
                            // no further targets
                            CannotInteractMessageDisplay.ClientOnCannotInteract(
                                character,
                                CharacterDroneControlSystem.Notification_DroneAlreadySent,
                                isOutOfRange: false);
                            return false;
                        }
                    }
                }
                else
                {
                    targetPosition = TryGetNextTargetPosition();
                    if (targetPosition == default)
                    {
                        // no further targets
                        break;
                    }
                }

                if (!CharacterDroneControlSystem.ClientTryStartDrone(itemDrone,
                                                                     targetPosition,
                                                                     showErrorNotification: showErrorNotification))
                {
                    break;
                }

                tempExceptTargets.Add(targetPosition);
            }

            CharacterDroneControlSystem.ClientSubmitStartDroneCommandsImmediately();

            // always return false as we don't want to play any device sounds
            return false;

            Vector2Ushort TryGetNextTargetPosition()
            {
                var targetObjectProto = CharacterDroneControlSystem
                                        .SharedGetCompatibleTarget(character,
                                                                   mouseTilePosition,
                                                                   out _,
                                                                   out _)?
                                        .ProtoWorldObject;

                if (targetObjectProto is null)
                {
                    return default;
                }

                var objectsNearby = Client.World.GetStaticWorldObjectsOfProto(
                    (IProtoStaticWorldObject)targetObjectProto);

                IStaticWorldObject selectedWorldObject = null;
                var selectedDistanceSqr = long.MaxValue;

                foreach (var worldObject in objectsNearby)
                {
                    var position = worldObject.TilePosition;
                    var distanceSqr = position.TileSqrDistanceTo(characterTilePosition);
                    if (distanceSqr >= selectedDistanceSqr)
                    {
                        // already found a closer candidate
                        continue;
                    }

                    if (!WorldObjectClaimSystem.SharedIsAllowInteraction(character,
                                                                         worldObject,
                                                                         showClientNotification: false))
                    {
                        continue;
                    }

                    if (worldObject.ProtoGameObject is IProtoObjectVegetation protoObjectVegetation
                        && protoObjectVegetation.SharedGetGrowthProgress(worldObject) < TreeGrowthProgressThreshold)
                    {
                        // not a full grown vegetation, ignore for auto-targeting
                        continue;
                    }

                    if (CharacterDroneControlSystem.SharedIsTargetAlreadyScheduledForAnyActiveDrone(
                            character,
                            position,
                            logError: false)
                        || tempExceptTargets.AsList().Contains(position))
                    {
                        continue;
                    }

                    selectedWorldObject = worldObject;
                    selectedDistanceSqr = distanceSqr;
                }

                return selectedWorldObject?.TilePosition ?? default;
            }
        }

        protected virtual void ClientSetupSkeletonAnimation(
            bool isActive,
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            if (isActive)
            {
                skeletonRenderer.RemoveAnimationTrack(trackIndex: AnimationTrackIndexes.Extra);
                skeletonRenderer.SetAnimation(
                    trackIndex: AnimationTrackIndexes.Extra,
                    animationName: this.ActiveLightCharacterAnimationName,
                    isLooped: true);
            }
            else if (skeletonRenderer.GetCurrentAnimationName(AnimationTrackIndexes.Extra) is not null)
            {
                // TODO: this is a hack - when an empty animation is added latest animation looping is breaking
                //skeletonRenderer.SetAnimationLoopMode(AnimationTrackIndexes.Extra, isLooped: false);
                skeletonRenderer.AddEmptyAnimation(AnimationTrackIndexes.Extra);
            }
        }

        protected override void ClientTooltipCreateControlsInternal(IItem item, List<UIElement> controls)
        {
            base.ClientTooltipCreateControlsInternal(item, controls);

            controls.Add(
                ItemPropertiesTooltipHelper.Create(CoreStrings.DroneControl_MaxSimultaneouslyControlledDrones,
                                                   this.MaxDronesToControl.ToString()));
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.DroneControl);
            hints.Add(ItemHints.DroneControlSendAllDrones);
        }

        protected sealed override void PrepareProtoItem()
        {
            base.PrepareProtoItem();

            var durability = this.DurabilityLifetime.TotalSeconds
                             * DurabilityGranularityMultiplier;
            Api.Assert(durability >= 0, "Durability lifetime seconds should be >= 0");
            Api.Assert(durability <= uint.MaxValue,
                       "Durability lifetime seconds should be <= " + uint.MaxValue / DurabilityGranularityMultiplier);
            this.durabilityMax = (uint)durability;

            this.PrepareProtoItemDroneControl();
        }

        protected virtual void PrepareProtoItemDroneControl()
        {
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            if (this.durabilityMax == 0)
            {
                return;
            }

            // let's reduce durability if there are any controlled drones
            var item = data.GameObject;
            var ownerCharacter = item.Container?.OwnerAsCharacter;

            if (ownerCharacter is null
                || !ReferenceEquals(item, ownerCharacter.SharedGetPlayerSelectedHotbarItem())
                || ownerCharacter.SharedGetCurrentControlledDronesNumber() == 0)
            {
                // the remote control's owner character is null or it doesn't control any drones
                return;
            }

            ItemDurabilitySystem.ServerModifyDurability(item,
                                                        -this.ServerUpdateIntervalSeconds
                                                        * DurabilityGranularityMultiplier,
                                                        roundUp: true);
        }
    }
}