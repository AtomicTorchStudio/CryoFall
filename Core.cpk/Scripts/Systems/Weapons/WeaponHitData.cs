namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public readonly struct WeaponHitData : IRemoteCallParameter
    {
        public readonly ObjectMaterial FallbackObjectMaterial;

        public readonly IProtoWorldObject FallbackProtoWorldObject;

        public readonly Vector2Ushort FallbackTilePosition;

        public readonly Vector2F HitPoint;

        public readonly IWorldObject WorldObject;

        public WeaponHitData(IWorldObject worldObject, Vector2F hitPoint)
        {
            this.WorldObject = worldObject;
            this.FallbackProtoWorldObject = worldObject.ProtoWorldObject;
            this.FallbackTilePosition = worldObject.TilePosition;
            this.HitPoint = hitPoint;

            if (worldObject is ICharacter character)
            {
                this.FallbackObjectMaterial = ((IProtoCharacterCore)character.ProtoCharacter)
                    .SharedGetObjectMaterialForCharacter(character);
            }
            else if (worldObject.ProtoWorldObject is IProtoWorldObjectWithSoundPresets protoWorldObjectWithSoundPresets)
            {
                this.FallbackObjectMaterial = protoWorldObjectWithSoundPresets
                    .ObjectMaterial;
            }
            else
            {
                this.FallbackObjectMaterial = ObjectMaterial.HardTissues;
            }
        }
    }
}