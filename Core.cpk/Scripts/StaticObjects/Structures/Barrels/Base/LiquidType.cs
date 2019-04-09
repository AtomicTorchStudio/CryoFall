namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels
{
    public enum LiquidType : byte
    {
        /// <summary>
        /// Water.
        /// </summary>
        Water = 0,

        /// <summary>
        /// Raw unprocessed petroleum oil which could be refined to produce fuels and mineral oils.
        /// </summary>
        Petroleum = 1,

        /// <summary>
        /// Gasoline is commonly used as liquid fuel. It's obtained as a result of raw petroleum oil refining.
        /// </summary>
        Gasoline = 2,

        /// <summary>
        /// Mineral oil is commonly used as lubricant. It's obtained as a result of raw petroleum oil refining.
        /// </summary>
        MineralOil = 3,
    }
}