namespace AtomicTorch.CBND.CoreMod.Characters.Turrets
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoCharacterTurret : IProtoCharacter
    {
        ITextureResource Icon { get; }

        ProtoItemWeaponTurret ProtoItemWeaponTurret { get; }
    }
}