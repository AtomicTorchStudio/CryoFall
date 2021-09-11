namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using System;
    using AtomicTorch.CBND.CoreMod.Achievements;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectLaunchpadStage5 : ProtoObjectLaunchpad
    {
        public const double RocketLaunchAnimationTotalDuration = 12;

        private const double FireRendererTextureCutOffset = 1.5;

        private const double Mast1MaxWorldOffsetX = 0.8;

        private const double Mast2MaxWorldOffsetX = 0.7;

        private const double MastAnimationDuration = 4;

        private const double RocketLaunchAnimationStartDelay = 3;

        private const double RocketLaunchFinalOffsetY = 24;

        private static readonly Vector2Ushort FireRendererTextureCutSize = (327, 962);

        private static readonly Vector2Ushort Mast1TextureSize = (295, 143);

        private static readonly Vector2Ushort Mast2TextureSize = (261, 139);

        private static readonly Vector2D[] RocketFireOffsets =
        {
            new(-36 / 256.0, -732 / 256.0),
            new(174 / 256.0, -802 / 256.0),
            new(378 / 256.0, -732 / 256.0)
        };

        private static readonly Vector2Ushort RocketTextureCutSize = (689, 1194);

        private static readonly TextureAtlasResource TextureResourceRocketFireAnimation
            = new("FX/RocketFireAnimation",
                  columns: 8,
                  rows: 1,
                  isTransparent: true);

        public override string Description => GetProtoEntity<ObjectLaunchpadStage1>().Description;

        public override string Name => "Launchpad — Stage 5";

        public static bool SharedCanResetLaunchpad(IStaticWorldObject objectLaunchpad)
        {
            var launchTime = GetPublicState(objectLaunchpad).LaunchServerFrameTime;
            if (launchTime <= 0)
            {
                // not yet launched
                return false;
            }

            // ensure the launch animation has completed
            var serverTime = IsServer
                                 ? Server.Game.FrameTime
                                 : Client.CurrentGame.ServerFrameTimeApproximated;

            return serverTime
                   > launchTime + RocketLaunchAnimationTotalDuration;
        }

        public void ClientLaunchRocket(IStaticWorldObject objectLaunchpad)
        {
            this.VerifyGameObject(objectLaunchpad);
            var character = ClientCurrentCharacterHelper.Character;
            if (!this.SharedCanInteract(character, objectLaunchpad, writeToLog: true))
            {
                return;
            }

            if (GetPublicState(objectLaunchpad).LaunchServerFrameTime > 0)
            {
                // already launched
                return;
            }

            Logger.Important("Launching the rocket: " + objectLaunchpad);
            this.CallServer(_ => _.ServerRemote_LaunchRocket(objectLaunchpad));
        }

        public void ClientResetLaunchpad(IStaticWorldObject objectLaunchpad)
        {
            this.VerifyGameObject(objectLaunchpad);
            var character = ClientCurrentCharacterHelper.Character;
            if (!this.SharedCanInteract(character, objectLaunchpad, writeToLog: true))
            {
                return;
            }

            if (!SharedCanResetLaunchpad(objectLaunchpad))
            {
                return;
            }

            Logger.Important("Resetting the launchpad: " + objectLaunchpad);
            this.CallServer(_ => _.ServerRemote_ResetLaunchpad(objectLaunchpad));
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var worldObject = data.GameObject;

            var rocketRenderer = this.ClientAddRenderer(worldObject, TextureStage5,     TextureStage5DockedOffset);
            var mast1Renderer = this.ClientAddRenderer(worldObject,  TextureTowerMast1, TextureTowerMast1Offset);
            var mast2Renderer = this.ClientAddRenderer(worldObject,  TextureTowerMast2, TextureTowerMast2Offset);
            mast1Renderer.DrawOrder = mast2Renderer.DrawOrder = DrawOrder.Default + 1;
            this.ClientAddRenderer(worldObject, TextureTower, TextureTowerOffset);

            data.PublicState.ClientSubscribe(_ => _.LaunchServerFrameTime,
                                             _ =>
                                             {
                                                 if (InteractionCheckerSystem.ClientCurrentInteraction == worldObject)
                                                 {
                                                     InteractionCheckerSystem.SharedAbortCurrentInteraction(
                                                         ClientCurrentCharacterHelper.Character);
                                                 }

                                                 RefreshLaunchState();
                                             },
                                             data.ClientState);

            RefreshLaunchState();

            void RefreshLaunchState()
            {
                worldObject.ClientSceneObject
                           .FindComponent<ComponentRocketLaunchAnimation>()
                           ?.Destroy();

                rocketRenderer.CustomTextureBounds = new BoundsUshort(offset: (0, 0),
                                                                      size: RocketTextureCutSize);

                rocketRenderer.IsEnabled = true;
                mast1Renderer.IsEnabled = true;
                mast2Renderer.IsEnabled = true;

                var launchTime = GetPublicState(worldObject).LaunchServerFrameTime;
                if (launchTime <= 0)
                {
                    // the rocket is not yet launched
                    return;
                }

                var timePassedSinceLaunch = Client.CurrentGame.ServerFrameTimeApproximated - launchTime;
                if (timePassedSinceLaunch >= RocketLaunchAnimationTotalDuration)
                {
                    // the launch animation has already completed
                    rocketRenderer.IsEnabled = false;
                    mast1Renderer.IsEnabled = false;
                    mast2Renderer.IsEnabled = false;
                    return;
                }

                // setup the launch animation
                worldObject.ClientSceneObject
                           .AddComponent<ComponentRocketLaunchAnimation>()
                           .Setup(rocketRenderer,
                                  mast1Renderer,
                                  mast2Renderer,
                                  timePassedSinceLaunch);
            }
        }

        protected override void PrepareLaunchpadConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade)
        {
            // build is not allowed - it's an upgrade from previous level
            build.IsAllowed = false;

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.VeryLong;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 4);
            repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 2);
        }

        protected override void PrepareLaunchpadTasks(LaunchpadTasksList tasksList)
        {
            // there is only a Launch button
        }

        [RemoteCallSettings(timeInterval: 1)]
        private void ServerRemote_LaunchRocket(IStaticWorldObject objectLaunchpad)
        {
            this.VerifyGameObject(objectLaunchpad);
            var character = ServerRemoteContext.Character;
            if (!this.SharedCanInteract(character, objectLaunchpad, writeToLog: true))
            {
                return;
            }

            var publicState = GetPublicState(objectLaunchpad);
            if (publicState.LaunchServerFrameTime > 0)
            {
                // already launched
                return;
            }

            Logger.Important("Launching the rocket: " + objectLaunchpad, character);
            publicState.LaunchServerFrameTime = Server.Game.FrameTime;
            publicState.LaunchedByPlayerName = character.Name;

            character.SharedGetAchievements()
                     .ServerTryAddAchievement(Api.GetProtoEntity<AchievementCompleteTheGame>(),
                                              isUnlocked: true);
        }

        [RemoteCallSettings(timeInterval: 1)]
        private void ServerRemote_ResetLaunchpad(IStaticWorldObject objectLaunchpad)
        {
            this.VerifyGameObject(objectLaunchpad);
            var character = ServerRemoteContext.Character;
            if (!this.SharedCanInteract(character, objectLaunchpad, writeToLog: true))
            {
                return;
            }

            if (!SharedCanResetLaunchpad(objectLaunchpad))
            {
                return;
            }

            Logger.Important("Resetting the launchpad: " + objectLaunchpad, character);
            var tilePosition = objectLaunchpad.TilePosition;
            Server.World.DestroyObject(objectLaunchpad);
            Server.World.CreateStaticWorldObject<ObjectLaunchpadStage1>(tilePosition);
        }

        public class ComponentRocketLaunchAnimation : ClientComponent
        {
            private Vector2D defaultMast1RendererPositionOffset;

            private Vector2D defaultMast2RendererPositionOffset;

            private double defaultRocketRendererDrawOrderOffsetY;

            private Vector2D defaultRocketRendererPositionOffset;

            private ClientComponentSpriteSheetAnimator[] fireAnimators;

            private double lastProgress;

            private IComponentSpriteRenderer mast1Renderer;

            private IComponentSpriteRenderer mast2Renderer;

            private IComponentSpriteRenderer rocketRenderer;

            private double time;

            private double durationRocketAnimation => RocketLaunchAnimationTotalDuration;

            public void Setup(
                IComponentSpriteRenderer rocketRenderer,
                IComponentSpriteRenderer mast1Renderer,
                IComponentSpriteRenderer mast2Renderer,
                double timePassedSinceLaunch)
            {
                this.rocketRenderer = rocketRenderer;
                this.time = -RocketLaunchAnimationStartDelay + timePassedSinceLaunch;
                this.mast1Renderer = mast1Renderer;
                this.mast2Renderer = mast2Renderer;

                this.defaultRocketRendererPositionOffset = rocketRenderer.PositionOffset;
                this.defaultRocketRendererDrawOrderOffsetY = rocketRenderer.DrawOrderOffsetY;

                this.defaultMast1RendererPositionOffset = mast1Renderer.PositionOffset;
                this.defaultMast2RendererPositionOffset = mast2Renderer.PositionOffset;

                var sceneObject = rocketRenderer.SceneObject;

                this.fireAnimators = new ClientComponentSpriteSheetAnimator[RocketFireOffsets.Length];
                for (var index = 0; index < this.fireAnimators.Length; index++)
                {
                    var fireRenderer = Client.Rendering.CreateSpriteRenderer(sceneObject,
                                                                             TextureResourceRocketFireAnimation);

                    this.fireAnimators[index] = sceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
                    this.fireAnimators[index].Setup(fireRenderer,
                                                    ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                                                        TextureResourceRocketFireAnimation),
                                                    isLooped: true,
                                                    frameDurationSeconds: 1 / 30.0,
                                                    randomizeInitialFrame: true);
                }

                // toggle rocket renderer to ensure it's drawn on front
                rocketRenderer.IsEnabled = false;
                rocketRenderer.IsEnabled = true;

                this.Update(0);
            }

            public override void Update(double deltaTime)
            {
                this.time += deltaTime;
                if (this.time > this.durationRocketAnimation)
                {
                    this.Destroy();
                    return;
                }

                var progress = this.time / this.durationRocketAnimation;
                if (this.time < 0)
                {
                    progress = 0;
                }

                // apply sine easy-in 1 - cos((x * PI) / 2)
                progress = ApplySineEasyIn(progress);
                this.lastProgress = progress;

                var rocketOffsetY = progress * RocketLaunchFinalOffsetY;

                this.rocketRenderer.PositionOffset = this.defaultRocketRendererPositionOffset + (0, rocketOffsetY);
                this.rocketRenderer.DrawOrderOffsetY = this.defaultRocketRendererDrawOrderOffsetY - rocketOffsetY;
                this.rocketRenderer.CustomTextureBounds = new BoundsUshort(
                    Vector2Ushort.Zero,
                    (RocketTextureCutSize.X,
                     (ushort)(RocketTextureCutSize.Y + rocketOffsetY * ScriptingConstants.TileSizeRealPixels)));

                for (var index = 0; index < this.fireAnimators.Length; index++)
                {
                    var offset = RocketFireOffsets[index];
                    var fireRenderer = this.fireAnimators[index].SpriteRenderer;
                    fireRenderer.PositionOffset = this.rocketRenderer.PositionOffset + offset;
                    fireRenderer.DrawOrderOffsetY = this.rocketRenderer.DrawOrderOffsetY - offset.Y;
                    var cutProgress = MathHelper.Clamp((rocketOffsetY + offset.Y + FireRendererTextureCutOffset)
                                                       / (FireRendererTextureCutSize.Y / 256.0),
                                                       0,
                                                       1);

                    fireRenderer.CustomTextureBounds = new BoundsUshort(
                        Vector2Ushort.Zero,
                        (FireRendererTextureCutSize.X,
                         (ushort)(FireRendererTextureCutSize.Y * cutProgress)));
                }

                var mastProgress = (this.time + RocketLaunchAnimationStartDelay) / MastAnimationDuration;
                mastProgress = MathHelper.Clamp(mastProgress, 0, 1);
                mastProgress = ApplySineEasyIn(mastProgress);

                this.mast1Renderer.PositionOffset
                    = this.defaultMast1RendererPositionOffset
                      + (mastProgress * Mast1MaxWorldOffsetX, 0);

                this.mast2Renderer.PositionOffset
                    = this.defaultMast2RendererPositionOffset
                      + (mastProgress * Mast2MaxWorldOffsetX, 0);

                this.mast1Renderer.CustomTextureBounds = new BoundsUshort(
                    Vector2Ushort.Zero,
                    ((ushort)(Mast1TextureSize.X
                              - Mast1MaxWorldOffsetX * mastProgress * ScriptingConstants.TileSizeRealPixels),
                     Mast1TextureSize.Y));

                this.mast2Renderer.CustomTextureBounds = new BoundsUshort(
                    Vector2Ushort.Zero,
                    ((ushort)(Mast2TextureSize.X
                              - Mast2MaxWorldOffsetX * mastProgress * ScriptingConstants.TileSizeRealPixels),
                     Mast2TextureSize.Y));
            }

            protected override void OnDisable()
            {
                this.rocketRenderer.IsEnabled = false;
                this.mast1Renderer.IsEnabled = false;
                this.mast2Renderer.IsEnabled = false;

                foreach (var fireAnimator in this.fireAnimators)
                {
                    fireAnimator.SpriteRenderer.Destroy();
                    fireAnimator.Destroy();
                }

                this.fireAnimators = null;
            }

            protected override void OnEnable()
            {
                this.lastProgress = 0;
                this.ClientAddShakes();
            }

            private static double ApplySineEasyIn(double x)
            {
                if (x is <= 0 or >= 1)
                {
                    return x;
                }

                return 1 - Math.Cos((x * MathConstants.PI) / 2);
            }

            private void ClientAddShakes()
            {
                const float shakesInterval = 0.5f,
                            shakesDuration = 1f,
                            shakesDistanceMin = 0.2f,
                            shakesDistanceMax = 0.25f;

                var intensity = 0.3;

                intensity *= Math.Max(0, 1 - this.lastProgress * 2.0);

                if (intensity > 0)
                {
                    ClientComponentCameraScreenShakes.AddRandomShakes(
                        duration: shakesDuration,
                        worldDistanceMin: (float)(intensity * -shakesDistanceMin),
                        worldDistanceMax: (float)(intensity * shakesDistanceMax));
                }

                if (this.IsEnabled)
                {
                    ClientTimersSystem.AddAction(shakesInterval, this.ClientAddShakes);
                }
            }
        }
    }
}