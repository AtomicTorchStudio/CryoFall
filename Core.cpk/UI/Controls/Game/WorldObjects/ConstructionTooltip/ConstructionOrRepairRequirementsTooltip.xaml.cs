namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ConstructionTooltip
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ConstructionTooltip.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ConstructionOrRepairRequirementsTooltip : BaseUserControl
    {
        public static readonly DependencyProperty TextBrushProperty
            = DependencyProperty.Register(nameof(TextBrush),
                                          typeof(Brush),
                                          typeof(ConstructionOrRepairRequirementsTooltip),
                                          new PropertyMetadata(default(Brush)));

        private ConstructionSitePublicState constructionSitePublicState;

        private StaticObjectPublicState objectToRepairPublicState;

        private BaseViewModelConstructionRequirementsTooltip viewModel;

        static ConstructionOrRepairRequirementsTooltip()
        {
        }

        public Brush TextBrush
        {
            get => (Brush)this.GetValue(TextBrushProperty);
            set => this.SetValue(TextBrushProperty, value);
        }

        public IStaticWorldObject WorldObject { get; private set; }

        public static IComponentAttachedControl CreateAndAttach(IStaticWorldObject worldObject)
        {
            var control = new ConstructionOrRepairRequirementsTooltip();
            control.WorldObject = worldObject;

            if (worldObject.ProtoStaticWorldObject is ProtoObjectConstructionSite)
            {
                // construction
                control.Setup(
                    constructionSitePublicState: ProtoObjectConstructionSite.GetPublicState(worldObject));
            }
            else
            {
                // repair
                control.Setup(
                    objectToRepairPublicState: worldObject.GetPublicState<StaticObjectPublicState>());
            }

            var centerOffset = worldObject.ProtoStaticWorldObject.SharedGetObjectCenterWorldOffset(worldObject);

            return Api.Client.UI.AttachControl(
                worldObject,
                control,
                positionOffset: centerOffset + (0, -0.6),
                isFocusable: true);
        }

        public void Setup(StaticObjectPublicState objectToRepairPublicState)
        {
            this.objectToRepairPublicState = objectToRepairPublicState;
        }

        public void Setup(ConstructionSitePublicState constructionSitePublicState)
        {
            this.constructionSitePublicState = constructionSitePublicState;
        }

        protected override void OnLoaded()
        {
            if (this.constructionSitePublicState != null)
            {
                this.viewModel = new ViewModelConstructionBuildRequirementsTooltip(
                    this.constructionSitePublicState);
            }
            else
            {
                this.viewModel = new ViewModelConstructionRepairRequirementsTooltip(
                    this.objectToRepairPublicState);
            }

            this.DataContext = this.viewModel;

            ClientUpdateHelper.UpdateCallback += this.Update;
            this.Update();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;

            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        private void Update()
        {
            var canInteract = this.WorldObject.ProtoWorldObject.SharedIsInsideCharacterInteractionArea(
                Api.Client.Characters.CurrentPlayerCharacter,
                this.WorldObject,
                writeToLog: false);

            this.TextBrush = canInteract
                                 ? InteractionTooltip.TextBrushCanInteract
                                 : InteractionTooltip.TextBrushCannotInteract;
        }
    }
}