namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Launchpad;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectLaunchpad
        : ProtoObjectStructure
          <ObjectLaunchpadPrivateState,
              ObjectLaunchpadPublicState,
              StaticObjectClientState>,
          IInteractableProtoWorldObject
    {
        protected const double RendererDrawOrderOffsetY = 2.5;

        public override bool HasIncreasedScopeSize => true;

        public override string InteractionTooltipText => InteractionTooltipTexts.Interact;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override bool IsRelocatable => false;

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override float StructurePointsMax => 10000;

        public IReadOnlyList<LaunchpadTask> TasksList { get; private set; }

        protected static ITextureResource TextureBase { get; }
            = new TextureResource("StaticObjects/Structures/Misc/ObjectLaunchpad/Base.png");

        protected static ITextureResource TextureFront { get; }
            = new TextureResource("StaticObjects/Structures/Misc/ObjectLaunchpad/Front.png");

        protected static ITextureResource TextureStage2 { get; }
            = new TextureResource("StaticObjects/Structures/Misc/ObjectLaunchpad/Stage2_422x142.png");

        protected static Vector2D TextureStage2Offset => (422 / 256.0, 142 / 256.0);

        protected static ITextureResource TextureStage3 { get; }
            = new TextureResource("StaticObjects/Structures/Misc/ObjectLaunchpad/Stage3_422x142.png");

        protected static Vector2D TextureStage3Offset => (422 / 256.0, 142 / 256.0);

        protected static ITextureResource TextureStage4 { get; }
            = new TextureResource("StaticObjects/Structures/Misc/ObjectLaunchpad/Stage4_422x142.png");

        protected static Vector2D TextureStage4Offset => (422 / 256.0, 142 / 256.0);

        protected static ITextureResource TextureStage5 { get; }
            = new TextureResource("StaticObjects/Structures/Misc/ObjectLaunchpad/Stage5_422x396.png");

        protected static Vector2D TextureStage5DockedOffset => (422 / 256.0, (142 - 535) / 256.0);

        protected static Vector2D TextureStage5LaunchedOffset => (422 / 256.0, 396 / 256.0);

        protected static ITextureResource TextureTower { get; }
            = new TextureResource("StaticObjects/Structures/Misc/ObjectLaunchpad/Tower_1248x447.png");

        protected static ITextureResource TextureTowerMast1 { get; }
            = new TextureResource("StaticObjects/Structures/Misc/ObjectLaunchpad/TowerMast1_965x988.png");

        protected static Vector2D TextureTowerMast1Offset => (965 / 256.0, 988 / 256.0);

        protected static ITextureResource TextureTowerMast2 { get; }
            = new TextureResource("StaticObjects/Structures/Misc/ObjectLaunchpad/TowerMast2_1018x563.png");

        protected static Vector2D TextureTowerMast2Offset => (1018 / 256.0, 563 / 256.0);

        protected static Vector2D TextureTowerOffset => (1248 / 256.0, 447 / 256.0);

        public void ClientCompleteTask(IStaticWorldObject worldObject, int taskIndex)
        {
            if (this.SharedValidateCanCompleteTask(worldObject,
                                                   taskIndex,
                                                   ClientCurrentCharacterHelper.Character))
            {
                this.CallServer(_ => _.ServerRemote_CompleteTask(worldObject, taskIndex));
            }
        }

        public BaseUserControlWithWindow ClientOpenUI(IWorldObject worldObject)
        {
            return WindowLaunchpad.Open((IStaticWorldObject)worldObject);
        }

        public void ClientUpgradeToNextStage(IStaticWorldObject objectLaunchpad)
        {
            if (this.SharedValidateCanUpgradeToNextStage(objectLaunchpad,
                                                         ClientCurrentCharacterHelper.Character,
                                                         out _))
            {
                this.CallServer(_ => _.ServerRemote_UpgradeToNextStage(objectLaunchpad));
            }
        }

        public void ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        public void ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            if (LandClaimSystem.SharedIsObjectInsideOwnedOrFreeArea(worldObject,
                                                                    character,
                                                                    requireFactionPermission: true))
            {
                return true;
            }

            if (writeToLog)
            {
                if (IsServer)
                {
                    LandClaimSystem.ServerNotifyCannotInteractNotOwner(character, worldObject);
                }
                else
                {
                    LandClaimSystem.ClientCannotInteractNotOwner(worldObject);
                }
            }

            return false;
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return base.SharedGetObjectCenterWorldOffset(worldObject) + (0, 1.33);
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            var staticWorldObject = (IStaticWorldObject)worldObject;
            return this.ClientOpenUI(staticWorldObject);
        }

        protected IComponentSpriteRenderer ClientAddRenderer(
            IStaticWorldObject worldObject,
            ITextureResource textureResource,
            Vector2D positionOffset)
        {
            var renderer = Client.Rendering.CreateSpriteRenderer(worldObject,
                                                                 textureResource);
            this.ClientSetupRenderer(renderer);
            renderer.PositionOffset += positionOffset;
            renderer.DrawOrderOffsetY -= positionOffset.Y;
            renderer.RenderingMaterial = RenderingMaterials.DefaultDrawEffectWithTransparentBorder;
            return renderer;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var frontRenderer = this.ClientAddRenderer(data.GameObject, TextureFront, Vector2D.Zero);
            frontRenderer.DrawOrderOffsetY -= 0.01; // draw on front

            ClientPreloadTextures();
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = RendererDrawOrderOffsetY;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("######",
                         "######",
                         "######",
                         "######");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryOther>();
            this.PrepareLaunchpadConstructionConfig(build, repair, upgrade);
            if (upgrade.Entries.Count > 1
                || (upgrade.Entries.Count == 1
                    && upgrade.Entries[0].RequiredItems.Count > 0))
            {
                throw new Exception("There must be max 1 upgrade entry and there must be no required items");
            }

            var tasksList = new LaunchpadTasksList();
            this.PrepareLaunchpadTasks(tasksList);
            this.TasksList = tasksList.AsReadOnly();
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return TextureBase;
        }

        protected abstract void PrepareLaunchpadConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade);

        protected abstract void PrepareLaunchpadTasks(LaunchpadTasksList tasksList);

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            data.PrivateState.ServerInitialize((byte)this.TasksList.Count);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                // horizontal rectangle
                .AddShapeRectangle((5.9, 1.3),
                                   offset: (0.05, 1.0))
                // vertical rectangle
                .AddShapeRectangle((4.0, 3.0),
                                   offset: (1.0, 0.15))
                // bottom-left corner
                .AddShapeCircle(radius: 1.0,
                                center: (1.05, 1.15))
                // bottom-right corner
                .AddShapeCircle(radius: 1.0,
                                center: (4.95, 1.15))
                // top-left corner
                .AddShapeCircle(radius: 1.0,
                                center: (1.05, 2.15))
                // top-right corner
                .AddShapeCircle(radius: 1.0,
                                center: (4.95, 2.15))
                // hitbox melee
                .AddShapeRectangle((5.9, 2.0),
                                   offset: (0.05, 1.3),
                                   group: CollisionGroups.HitboxMelee)
                // hitbox ranged
                .AddShapeRectangle((5.9, 1.5),
                                   offset: (0.05, 1.8),
                                   group: CollisionGroups.HitboxRanged)
                // interaction area
                .AddShapeRectangle((5.6, 2.7),
                                   offset: (0.2, 0.25),
                                   group: CollisionGroups.ClickArea)
                .AddShapeRectangle((2.4, 3),
                                   offset: (1.8, 1.5),
                                   group: CollisionGroups.ClickArea);
        }

        private static void ClientPreloadTextures()
        {
            foreach (var texture in new[]
            {
                TextureBase, TextureFront, TextureStage2, TextureStage3,
                TextureStage4, TextureStage5, TextureTower,
                TextureTowerMast1, TextureTowerMast2
            })
            {
                Client.Rendering.PreloadTextureAsync(texture);
            }
        }

        private void ServerRemote_CompleteTask(IStaticWorldObject objectLaunchpad, int taskIndex)
        {
            this.VerifyGameObject(objectLaunchpad);
            var character = ServerRemoteContext.Character;

            if (!this.SharedCanInteract(character, objectLaunchpad, writeToLog: true))
            {
                return;
            }

            if (!this.SharedValidateCanCompleteTask(objectLaunchpad, taskIndex, character))
            {
                return;
            }

            // consume items
            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                Logger.Important(character + " is in creative mode - no items deduction on task completion.");
            }
            else
            {
                InputItemsHelper.ServerDestroyItems(character, this.TasksList[taskIndex].InputItems);
            }

            var privateState = GetPrivateState(objectLaunchpad);
            var taskCompletionState = privateState.TaskCompletionState;

            taskCompletionState = taskCompletionState.ToArray();
            taskCompletionState[taskIndex] = true;
            privateState.TaskCompletionState = taskCompletionState;

            Logger.Important(objectLaunchpad + " task completed: #" + taskIndex, character);
        }

        private void ServerRemote_UpgradeToNextStage(IStaticWorldObject objectLaunchpad)
        {
            this.VerifyGameObject(objectLaunchpad);
            var character = ServerRemoteContext.Character;

            if (!this.SharedValidateCanUpgradeToNextStage(objectLaunchpad,
                                                          character,
                                                          out var upgradeStructure))
            {
                return;
            }

            // upgrade (it will destroy an existing structure and place new in its place)
            var tilePosition = objectLaunchpad.TilePosition;

            // destroy old structure
            Server.World.DestroyObject(objectLaunchpad);

            // create new structure
            var upgradedObject = Server.World.CreateStaticWorldObject(upgradeStructure, tilePosition);

            // notify client (to play a sound)
            ConstructionPlacementSystem.Instance.ServerNotifyOnStructurePlacedOrRelocated(
                upgradedObject,
                character);

            Logger.Important(objectLaunchpad + " upgraded to " + upgradedObject, character);
        }

        private bool SharedValidateCanCompleteTask(
            IStaticWorldObject worldObject,
            int taskIndex,
            ICharacter character)
        {
            var privateState = GetPrivateState(worldObject);

            var taskCompletionState = privateState.TaskCompletionState;
            if (taskCompletionState[taskIndex])
            {
                Logger.Warning("Task already completed", character);
                return false;
            }

            if (InputItemsHelper.SharedPlayerHasRequiredItems(character,
                                                              this.TasksList[taskIndex].InputItems,
                                                              noCheckInCreativeMode: true))
            {
                return true;
            }

            Logger.Warning("Not enough items to finish the task", character);

            if (IsClient)
            {
                NotificationSystem.ClientShowNotification(
                    ConstructionSystem.NotificationNotEnoughItems_Message,
                    color: NotificationColor.Bad,
                    icon: this.Icon);
            }

            return false;
        }

        private bool SharedValidateCanUpgradeToNextStage(
            IStaticWorldObject objectLaunchpad,
            ICharacter character,
            out IProtoObjectStructure upgradeStructure)
        {
            if (!this.SharedCanInteract(character, objectLaunchpad, writeToLog: true))
            {
                // cannot interact
                upgradeStructure = null;
                return false;
            }

            upgradeStructure = this.ConfigUpgrade.Entries[0].ProtoStructure;
            if (upgradeStructure is null)
            {
                // no upgrade exists
                return false;
            }

            if (!upgradeStructure.ListedInTechNodes.Any(
                    techNode => character.SharedGetTechnologies().SharedIsNodeUnlocked(techNode))
                && !CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                // has not yet researched the relevant technology
                return false;
            }

            var privateState = GetPrivateState(objectLaunchpad);
            var taskCompletionState = privateState.TaskCompletionState;
            if (!taskCompletionState.All(flag => flag))
            {
                Logger.Warning(character + " has not completed all the tasks yet - cannot upgrade: " + objectLaunchpad);
                return false;
            }

            // all tasks completed
            return true;
        }

        public readonly struct LaunchpadTask
        {
            public LaunchpadTask(string name, ITextureResource icon, IReadOnlyList<ProtoItemWithCount> inputItems)
            {
                this.Name = name;
                this.Icon = icon;
                this.InputItems = inputItems;
            }

            public ITextureResource Icon { get; }

            public IReadOnlyList<ProtoItemWithCount> InputItems { get; }

            public string Name { get; }
        }

        public class LaunchpadTasksList
        {
            private readonly List<LaunchpadTask> list = new();

            public LaunchpadTasksList AddTask(string name, ITextureResource icon, InputItems inputItems)
            {
                this.list.Add(new LaunchpadTask(name, icon, inputItems.AsReadOnly()));
                return this;
            }

            public IReadOnlyList<LaunchpadTask> AsReadOnly()
            {
                return this.list.ToArray();
            }
        }
    }
}