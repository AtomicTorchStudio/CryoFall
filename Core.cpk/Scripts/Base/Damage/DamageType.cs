// ReSharper disable once CheckNamespace

namespace AtomicTorch.CBND.GameApi.Data.Weapons
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;

    public enum DamageType : byte
    {
        [Description(CoreStrings.DamageType_Impact)]
        Impact = 0,

        [Description(CoreStrings.DamageType_Kinetic)]
        Kinetic = 1,

        [Description(CoreStrings.DamageType_Explosion)]
        Explosion = 2,

        [Description(CoreStrings.DamageType_Heat)]
        Heat = 3,

        [Description(CoreStrings.DamageType_Cold)]
        Cold = 4,

        [Description(CoreStrings.DamageType_Chemical)]
        Chemical = 5,

        [Description(CoreStrings.DamageType_Radiation)]
        Radiation = 6,

        [Description(CoreStrings.DamageType_Psi)]
        Psi = 7
    }
}