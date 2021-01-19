namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Teleport;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectTeleport : ProtoStaticWorldObject, IInteractableProtoWorldObject
    {
        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        // big and has large light source
        public override bool HasIncreasedScopeSize => true;

        public bool IsAutoEnterPrivateScopeOnInteraction => false;

        public override double ObstacleBlockDamageCoef => 1;

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override float StructurePointsMax => 0; // non-damageable

        // has light source
        public override BoundsInt ViewBoundsExpansion => new(minX: -3,
                                                             minY: -3,
                                                             maxX: 3,
                                                             maxY: 3);

        public virtual BaseUserControlWithWindow ClientOpenUI(IWorldObject worldObject)
        {
            var menu = Menu.Find<WindowWorldMap>();
            menu.OpenWindow();
            menu.UpdateLayout();
            menu.MapClickOverride = ClientMapClickOverride;

            foreach (var visualizer in menu.Visualizers)
            {
                visualizer.IsEnabled = visualizer switch
                {
                    ClientWorldMapTeleportsVisualizer        => false,
                    ClientWorldMapTradingTerminalsVisualizer => false,
                    _                                        => visualizer.IsEnabled
                };
            }

            var teleportsVisualizer = new ClientWorldMapTeleportsVisualizer(
                menu.WorldMapController,
                isActiveMode: true,
                currentTeleportLocation: worldObject.TilePosition);

            // temporarily add a new teleport visualizer (will be removed when the window is closed)
            menu.AddVisualizer(teleportsVisualizer);

            menu.EventWindowClosing += MenuOnEventWindowClosing;
            return menu;

            void MenuOnEventWindowClosing()
            {
                menu.EventWindowClosing -= MenuOnEventWindowClosing;
                menu.MapClickOverride = null;
                menu.RemoveVisualizer(teleportsVisualizer);

                WindowTeleportConfirmationDialog.Instance?.CloseWindow();
            }
        }

        /// <summary>
        /// Important: this method must be called when the player is standing in the teleport.
        /// Otherwise it will be unable to determine which vehicles could be teleported
        /// (it gets them from the player's view scope).
        /// </summary>
        public List<IDynamicWorldObject> ServerGetVehiclesToTeleport(
            IStaticWorldObject currentTeleport,
            ICharacter forCharacter)
        {
            var result = new List<IDynamicWorldObject>();
            var teleportBounds = currentTeleport.Bounds;

            using var tempObjects = Api.Shared.GetTempList<IWorldObject>();
            Server.World.GetWorldObjectsInView(forCharacter, tempObjects.AsList(), sortByDistance: false);

            foreach (var worldObject in tempObjects.AsList())
            {
                if (!(worldObject.ProtoGameObject is IProtoVehicle)
                    || !WorldObjectOwnersSystem.SharedIsOwner(forCharacter, worldObject))
                {
                    // not a vehicle or player doesn't own it
                    continue;
                }

                if (!teleportBounds.Contains(worldObject.PhysicsBody.Position))
                {
                    // not inside the teleport
                    continue;
                }

                if (worldObject.GetPublicState<VehiclePublicState>().PilotCharacter
                        is not null)
                {
                    // the vehicle has a pilot
                    continue;
                }

                // an owned vehicle found
                result.Add((IDynamicWorldObject)worldObject);
            }

            return result;
        }

        public virtual void ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);
            TeleportsSystem.ServerUnregisterTeleport(gameObject);
        }

        public virtual void ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        public sealed override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
            damageApplied = 0; // no damage
            return true;       // hit
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            // don't use base implementation
            //base.ClientInitialize(data);

            var renderer = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                textureResource: this.DefaultTexture);
            data.ClientState.Renderer = renderer;

            this.ClientSetupRenderer(renderer);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource($"StaticObjects/Misc/{thisType.Name}.png");
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            TeleportsSystem.ServerRegisterTeleport(data.GameObject);
        }

        private static void ClientMapClickOverride(Vector2Ushort worldPosition)
        {
            var visualInPointedPosition = Api.Client.UI.GetVisualInPointedPosition();
            var markTeleport = VisualTreeHelperExtension.FindParentOfType(visualInPointedPosition,
                                                                          typeof(WorldMapMarkTeleportActive))
                                   as WorldMapMarkTeleportActive;

            if (markTeleport is null)
            {
                // no teleport there
                return;
            }

            if (markTeleport.WorldPosition
                == InteractionCheckerSystem.ClientCurrentInteraction?.TilePosition)
            {
                // cannot teleport to the current teleport location 
                return;
            }

            WindowTeleportConfirmationDialog.ShowDialog(markTeleport.WorldPosition);
        }
    }
}