namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectWallClientState : StaticObjectClientState
    {
        public IList<IComponentSpriteRenderer> RenderersObjectOverlay { get; set; }
    }
}