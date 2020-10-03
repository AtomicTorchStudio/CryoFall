namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ClientComponentAutoDisplayConstructionSiteStructurePointsBar : ClientComponent
    {
        private static readonly IInputClientService InputClientService = Api.Client.Input;

        private IComponentAttachedControl componentAttachedUIElement;

        private ObjectStructurePointsData data;

        private ConstructionSiteStructurePointsBarControl structurePointsBarControl;

        public void Setup(
            IStaticWorldObject staticWorldObject,
            float structurePointsMax)
        {
            this.data = new ObjectStructurePointsData(staticWorldObject,
                                                      structurePointsMax);
        }

        public override void Update(double deltaTime)
        {
            this.RefreshBarControl();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.RemoveAttachedControl();
        }

        private bool CheckIsBarShouldBeVisible()
        {
            if (ClientInputManager.IsButtonHeld(GameButton.DisplayLandClaim)
                || InputClientService.IsKeyHeld(InputKey.Alt, evenIfHandled: true))
            {
                return true;
            }

            return ClientComponentObjectInteractionHelper.MouseOverObject
                   == this.data.WorldObject;
        }

        private void RefreshBarControl()
        {
            var isBarVisible = this.CheckIsBarShouldBeVisible();
            if (!isBarVisible)
            {
                this.RemoveAttachedControl();
                return;
            }

            if (this.componentAttachedUIElement is not null)
            {
                return;
            }

            var protoWorldObject = this.data.ProtoWorldObject;

            this.structurePointsBarControl = ControlsCache<ConstructionSiteStructurePointsBarControl>.Instance.Pop();
            this.structurePointsBarControl.ObjectStructurePointsData = this.data;

            var offset = protoWorldObject.SharedGetObjectCenterWorldOffset(
                this.SceneObject.AttachedWorldObject);

            this.componentAttachedUIElement = Api.Client.UI.AttachControl(
                this.SceneObject,
                this.structurePointsBarControl,
                positionOffset: offset + (0, 0.85),
                isFocusable: false);
        }

        private void RemoveAttachedControl()
        {
            if (this.componentAttachedUIElement is null)
            {
                return;
            }

            this.componentAttachedUIElement.Destroy();
            this.componentAttachedUIElement = null;
            ControlsCache<ConstructionSiteStructurePointsBarControl>.Instance.Push(this.structurePointsBarControl);
            this.structurePointsBarControl = null;
        }
    }
}