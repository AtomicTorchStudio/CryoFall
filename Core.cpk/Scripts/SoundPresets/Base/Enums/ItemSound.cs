namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum ItemSound
    {
        /// <summary>
        /// Pick up item in inventory or from the ground.
        /// </summary>
        Pick,

        /// <summary>
        /// Put item back into inventory or on the ground.
        /// </summary>
        Drop,

        /// <summary>
        /// Use item from quick slot. Such as eating food or using medkit.
        /// </summary>
        Use,

        /// <summary>
        /// Equipping items into the character slots. Applicable for items such as armor, devices and a few others.
        /// </summary>
        Equip,

        /// <summary>
        /// Unequipping items from the character slots. Applicable for items such as armor, devices and a few others.
        /// </summary>
        Unequip,

        /// <summary>
        /// Item selected in hotbar.
        /// </summary>
        Select,

        /// <summary>
        /// Item deselected in hotbar.
        /// </summary>
        Deselect,

        /// <summary>
        /// Item deselected in hotbar.
        /// </summary>
        DeselectOnEmpty,

        /// <summary>
        /// Item cannot be selected.
        /// </summary>
        CannotSelect,

        /// <summary>
        /// Item refilled/recharged.
        /// </summary>
        Refill,

        /// <summary>
        /// Item broken (and removed/destroyed).
        /// </summary>
        Broken,

        /// <summary>
        /// Selected item loop.
        /// </summary>
        Idle
    }
}