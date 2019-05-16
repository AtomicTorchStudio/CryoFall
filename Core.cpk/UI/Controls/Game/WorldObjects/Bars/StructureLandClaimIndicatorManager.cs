namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    internal static class StructureLandClaimIndicatorManager
    {
        private static IComponentAttachedControl componentStructureLandClaimIndicator;

        private static StructureLandClaimIndicator structureLandClaimIndicator;

        public static void ClientObserving(IStaticWorldObject worldObject, bool isObserving)
        {
            if (isObserving)
            {
                structureLandClaimIndicator = ControlsCache<StructureLandClaimIndicator>.Instance.Pop();
                structureLandClaimIndicator.Setup(
                    isClaimed: LandClaimSystem.SharedIsLandClaimedByAnyone(worldObject.Bounds));

                var offset = worldObject.ProtoStaticWorldObject.SharedGetObjectCenterWorldOffset(worldObject);
                componentStructureLandClaimIndicator = Api.Client.UI.AttachControl(
                    worldObject,
                    structureLandClaimIndicator,
                    positionOffset: offset + (0, 0.3),
                    isFocusable: false);
            }
            else
            {
                componentStructureLandClaimIndicator.Destroy();
                componentStructureLandClaimIndicator = null;
                ControlsCache<StructureLandClaimIndicator>.Instance.Push(structureLandClaimIndicator);
                structureLandClaimIndicator = null;
            }
        }
    }
}