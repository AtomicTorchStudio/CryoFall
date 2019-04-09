namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectSignPublicState : StaticObjectPublicState
    {
        // in case of a picture sign this is considered to be a texture path inside 
        // Content/Textures/StaticObjects/Structures/Signs/Pictures
        [SyncToClient]
        public string Text { get; set; }
    }
}