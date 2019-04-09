namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    public enum ObjectSound
    {
        /// <summary>
        /// Used for certain objects that require to play sound immediately at activation.
        /// </summary>
        InteractStart,

        /// <summary>
        /// Looped sound played during the interaction with the world object.
        /// </summary>
        InteractProcess,

        /// <summary>
        /// If interaction with the object is successful, some objects may need to play an appropriate sound at that event.
        /// </summary>
        InteractSuccess,

        /// <summary>
        /// If interaction was unsuccessful. For example an object is out of order or access denied (object belongs to another
        /// player), etc.
        /// </summary>
        InteractFail,

        /// <summary>
        /// Player is too far away to interact with the world object.
        /// </summary>
        InteractOutOfRange,

        /// <summary>
        /// Some objects may have a state where they "do something", this state may be denotes by special sound. Example of that is
        /// a burning campfire or furnace.
        /// </summary>
        Active,

        /// <summary>
        /// Object blueprint placed or built without the blueprint.
        /// </summary>
        Place
    }
}