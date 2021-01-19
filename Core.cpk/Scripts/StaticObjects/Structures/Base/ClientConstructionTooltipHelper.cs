namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ConstructionTooltip;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public static class ClientConstructionTooltipHelper
    {
        private static IStaticWorldObject currentTooltipsWorldObject;

        private static IComponentAttachedControl tooltipBuildOrRepair;

        private static IComponentAttachedControl tooltipRelocate;

        private static void Update()
        {
            if (LoadingSplashScreenManager.Instance.CurrentState
                != LoadingSplashScreenState.Hidden)
            {
                return;
            }

            IStaticWorldObject worldObject = null;
            IProtoObjectStructure protoStructure = null;

            if (ClientHotbarSelectedItemManager.SelectedItem?.ProtoItem is IProtoItemToolToolbox)
            {
                worldObject = ClientComponentObjectInteractionHelper.MouseOverObject as IStaticWorldObject;
                protoStructure = worldObject?.ProtoGameObject as IProtoObjectStructure;
            }

            if (currentTooltipsWorldObject != worldObject)
            {
                tooltipBuildOrRepair?.Destroy();
                tooltipBuildOrRepair = null;

                tooltipRelocate?.Destroy();
                tooltipRelocate = null;

                currentTooltipsWorldObject = worldObject;
            }

            if (protoStructure is null)
            {
                return;
            }

            // process structure repair tooltip
            var isBuildTooltipRequired
                = protoStructure.ClientIsConstructionOrRepairRequirementsTooltipShouldBeDisplayed(worldObject);

            if (tooltipBuildOrRepair is null)
            {
                if (isBuildTooltipRequired)
                {
                    tooltipBuildOrRepair = ConstructionOrRepairRequirementsTooltip.CreateAndAttach(worldObject);
                }
            }
            else if (!isBuildTooltipRequired)
            {
                tooltipBuildOrRepair.Destroy();
                tooltipBuildOrRepair = null;
            }

            // process structure relocation tooltip
            var canRelocate = !ConstructionRelocationSystem.IsInObjectPlacementMode
                              && ConstructionRelocationSystem.SharedIsRelocatable(worldObject);
            if (tooltipRelocate is null)
            {
                if (canRelocate)
                {
                    tooltipRelocate = ConstructionRelocationTooltip.CreateAndAttach(worldObject);
                }
            }
            else if (!canRelocate)
            {
                tooltipRelocate.Destroy();
                tooltipRelocate = null;
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                ClientUpdateHelper.UpdateCallback += Update;
            }
        }
    }
}