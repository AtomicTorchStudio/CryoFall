namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectDoorClientState : StaticObjectClientState
    {
        public IComponentSpriteRenderer DoorBaseRenderer { get; set; }

        public IComponentSpriteRenderer DoorVerticalFrontPartRenderer { get; set; }

        public bool IsOpened { get; set; }

        public ClientComponentDoorSpriteSheetAnimator SpriteAnimator { get; set; }
    }
}