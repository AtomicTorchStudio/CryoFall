namespace AtomicTorch.CBND.CoreMod.Tiles
{
    public enum DecalHidingSetting
    {
        /// <summary>
        /// Never hide the decal.
        /// </summary>
        Never,

        /// <summary>
        /// Hide the decal only if there is a structure of floor object.
        /// </summary>
        StructureOrFloorObject,

        /// <summary>
        /// Hide the decal if there are any object in the cells under the decal.
        /// </summary>
        AnyObject
    }
}