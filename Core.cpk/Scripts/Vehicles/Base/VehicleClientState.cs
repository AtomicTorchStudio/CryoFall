namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class VehicleClientState : BaseClientState
    {
        public IComponentSpriteRenderer RendererShadow { get; set; }

        public IComponentSkeleton SkeletonRenderer { get; set; }

        public IComponentSpriteRenderer SpriteRenderer { get; set; }

        public IDisposable UIElementsHolder { get; set; }
    }
}