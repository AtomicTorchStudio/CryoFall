namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectWithGroundSoundMaterial : IProtoStaticWorldObject
    {
        GroundSoundMaterial GroundSoundMaterial { get; }
    }
}