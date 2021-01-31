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

        private static IComponentAttachedControl tooltipDeconstruct;

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

            var selectedProtoItem = ClientHotbarSelectedItemManager.SelectedItem?.ProtoItem;
            if (selectedProtoItem is IProtoItemToolToolbox
                || selectedProtoItem is IProtoItemToolCrowbar)
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

                tooltipDeconstruct?.Destroy();
                tooltipDeconstruct = null;

                currentTooltipsWorldObject = worldObject;
            }

            if (protoStructure is null)
            {
                return;
            }

            // process structure repair tooltip
            var isBuildTooltipRequired
                = selectedProtoItem is IProtoItemToolToolbox
                  && protoStructure.ClientIsConstructionOrRepairRequirementsTooltipShouldBeDisplayed(worldObject);

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
            var canRelocate = selectedProtoItem is IProtoItemToolToolbox
                              && !ConstructionRelocationSystem.IsInObjectPlacementMode
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

            // process structure deconstruction tooltip
            var canDeconstruct = selectedProtoItem is IProtoItemToolCrowbar;
            if (tooltipDeconstruct is null)
            {
                if (canDeconstruct)
                {
                    tooltipDeconstruct = ConstructionDeconstructTooltip.CreateAndAttach(worldObject);
                }
            }
            else if (!canDeconstruct)
            {
                tooltipDeconstruct.Destroy();
                tooltipDeconstruct = null;
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