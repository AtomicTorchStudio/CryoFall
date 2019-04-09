namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies
{
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class TechTreeControl : BaseUserControl
    {
        public static readonly DependencyProperty TechGroupProperty =
            DependencyProperty.Register(
                nameof(TechGroup),
                typeof(ViewModelTechGroup),
                typeof(TechTreeControl),
                new PropertyMetadata(null, TechGroupPropertyChanged));

        private PanningPanel panningPanel;

        private ViewModelTechTreeControl viewModel;

        public TechTreeControl()
        {
        }

        public ViewModelTechGroup TechGroup
        {
            get => (ViewModelTechGroup)this.GetValue(TechGroupProperty);
            set => this.SetValue(TechGroupProperty, value);
        }

        protected override void InitControl()
        {
            this.panningPanel = this.GetByName<PanningPanel>("PanningPanel");
            var nodeSize = this.GetResource<double>("NodeSize");
            var nodeMargin = this.GetResource<Thickness>("NodeMargin");
            var nodeVerticalSpacing = this.GetResource<double>("NodeVerticalSpacing");
            var nodeLinkArrowHeight = this.GetResource<double>("NodeLinkArrowHeight");

            this.viewModel =
                new ViewModelTechTreeControl(
                    nodeSize,
                    nodeMargin.Left + nodeMargin.Right,
                    nodeVerticalSpacing,
                    nodeLinkArrowHeight);

            this.panningPanel.DataContext = this.viewModel;
        }

        private static void TechGroupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TechTreeControl)d).OnTechGroupChanged();
        }

        private void OnTechGroupChanged()
        {
            this.UpdateTechGroup();
        }

        private void UpdateTechGroup()
        {
            if (this.viewModel == null)
            {
                return;
            }

            this.viewModel.TechGroup = this.TechGroup?.TechGroup;
            if (this.viewModel.TechGroup == null)
            {
                return;
            }

            this.panningPanel.UpdateLayout();
            this.panningPanel.Refresh();

            // find first level nodes
            var firstLevelNodes = new List<ViewModelTechNode>(capacity: 8);
            foreach (var viewModelTechNode in this.viewModel.Nodes)
            {
                if (viewModelTechNode.HierarchyLevel > 0)
                {
                    break;
                }

                firstLevelNodes.Add(viewModelTechNode);
            }

            double centerX;
            if (firstLevelNodes.Count == 0)
            {
                centerX = 0;
            }
            else
            {
                // take middle node position as center
                centerX = firstLevelNodes[(firstLevelNodes.Count - 1) / 2].PositionX;
                // add half node offset
                centerX += this.viewModel.NodeWidth / 2;
            }

            this.panningPanel.CenterOnPoint((centerX, 0));
        }
    }
}