namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IProtoCharacterCore : IProtoCharacter
    {
        IReadOnlyStatsDictionary ProtoCharacterDefaultEffects { get; }

        double StatDefaultHealthMax { get; }

        ObjectMaterial SharedGetObjectMaterialForCharacter(ICharacter character);

        Vector2D SharedGetWeaponFireWorldPosition(ICharacter character, bool isMeleeWeapon);
    }
}