namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;

    public enum LiquidType : byte
    {
        /// <summary>
        /// Water.
        /// </summary>
        [Description(CoreStrings.TitleLiquidWater)]
        Water = 0,

        /// <summary>
        /// Raw unprocessed petroleum oil which could be refined to produce fuels and mineral oils.
        /// </summary>
        [Description(CoreStrings.TitleLiquidPetroleum)]
        Petroleum = 1,

        /// <summary>
        /// Gasoline is commonly used as liquid fuel. It's obtained as a result of raw petroleum oil refining.
        /// </summary>
        [Description(CoreStrings.TitleLiquidGasoline)]
        Gasoline = 2,

        /// <summary>
        /// Mineral oil is commonly used as lubricant. It's obtained as a result of raw petroleum oil refining.
        /// </summary>
        [Description(CoreStrings.TitleLiquidMineralOil)]
        MineralOil = 3,
    }
}