namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectDecorationToilet : ProtoObjectDecoration
    {
        private const int UseIntervalSeconds = 3;

        private double clientLastInteractTime;

        public override string Description =>
            "Make sure to put this in a separate and well-ventilated room for maximum privacy.";

        public override string Name => "Toilet";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 1000;

        private static ReadOnlySoundResourceSet SoundSet
            => new SoundResourceSet()
               .Add("Objects/Structures/ObjectDecorationToilet/Action_")
               .ToReadOnly();

        protected override void ClientInteractFinish(ClientObjectData data)
        {
            var time = Client.Core.ClientRealTime;
            if (time - this.clientLastInteractTime < UseIntervalSeconds)
            {
                return;
            }

            this.clientLastInteractTime = time;

            var randomSound = ClientGetRandomSound(out var soundIndex);
            Client.Audio.PlayOneShot(randomSound);

            this.CallServer(_ => _.ServerRemote_ObjectUse(data.GameObject, soundIndex));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (0.5, 0.2);
            renderer.DrawOrderOffsetY = 0.25;
        }

        protected override void PrepareDecorationConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemClay>(count: 10);
            build.AddStageRequiredItem<ItemCement>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemClay>(count: 10);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.5, 0.4), offset: (0.25, 0.3))
                .AddShapeRectangle((0.5, 0.4), offset: (0.25, 0.9), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.5, 0.3), offset: (0.25, 1.0), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((0.5, 0.4), offset: (0.25, 0.9), group: CollisionGroups.ClickArea);
        }

        private static SoundResource ClientGetRandomSound(out int index)
        {
            var soundSet = SoundSet;
            var soundResource = soundSet.GetSound();
            index = soundSet.IndexOf(soundResource);
            return soundResource;
        }

        private void ClientRemote_OnObjectUseByOtherPlayer(IStaticWorldObject worldObject, int index)
        {
            var soundResource = SoundSet.GetSoundAtIndex(index);
            if (soundResource is not null)
            {
                Client.Audio.PlayOneShot(soundResource, worldObject);
            }
        }

        [RemoteCallSettings(timeInterval: UseIntervalSeconds, clientMaxSendQueueSize: 1)]
        private void ServerRemote_ObjectUse(IStaticWorldObject worldObject, int soundIndex)
        {
            var character = ServerRemoteContext.Character;
            if (!worldObject.ProtoStaticWorldObject.SharedCanInteract(character,
                                                                      worldObject,
                                                                      writeToLog: true))
            {
                return;
            }

            using var observers = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(worldObject, observers);
            observers.Remove(character);

            this.CallClient(observers.AsList(),
                            _ => _.ClientRemote_OnObjectUseByOtherPlayer(worldObject, soundIndex));
        }
    }
}