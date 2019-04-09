namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ClientOptions;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public partial class MenuOptions : BaseUserControl
    {
        private List<ProtoOptionsCategory> allOptions;

        private TabControlCached tabControl;

        private ViewModelMenuOptions viewModel;

        public bool CheckCanHide(Action callbackOnHide)
        {
            return this.viewModel?.CheckCanHide(callbackOnHide)
                   ?? true;
        }

        public void SelectFirstTab()
        {
            if (this.viewModel != null)
            {
                this.viewModel.SelectedTab = (TabItem)this.tabControl.Items[0];
            }
        }

        protected override void InitControl()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.tabControl = this.GetByName<TabControlCached>("TabControl");
            var tabControlItems = this.tabControl.Items;

            // local helper method for getting options tab order
            IEnumerable<ProtoOptionsCategory> GetOptionsCategoryOrder(ProtoOptionsCategory tab)
            {
                if (tab.OrderAfterCategory != null)
                {
                    yield return tab.OrderAfterCategory;
                }
            }

            this.allOptions = Api.FindProtoEntities<ProtoOptionsCategory>()
                                 .OrderBy(o => o.ShortId)
                                 .TopologicalSort(GetOptionsCategoryOrder);

            var headerTemplate = this.GetResource<ControlTemplate>("HeaderTemplate");

            foreach (var protoOptionsCategory in this.allOptions)
            {
                var control = protoOptionsCategory.CreateControl();
                tabControlItems.Add(
                    new TabItem()
                    {
                        Header = new ContentControl()
                        {
                            Template = headerTemplate,
                            DataContext = new ViewModelOptionsCategory(protoOptionsCategory)
                        },
                        Content = control,
                        Tag = protoOptionsCategory,
                    });
            }

            this.tabControl.RefreshItems();
        }

        protected override void OnLoaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = this.viewModel = new ViewModelMenuOptions(this.allOptions);
            this.SelectFirstTab();
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }

    public class ViewModelOptionsCategory : BaseViewModel
    {
        private readonly ProtoOptionsCategory protoOptionsCategory;

        public ViewModelOptionsCategory(ProtoOptionsCategory protoOptionsCategory)
        {
            this.protoOptionsCategory = protoOptionsCategory;
        }

        public TextureBrush Icon => Api.Client.UI.GetTextureBrush(this.protoOptionsCategory.Icon);

        public string Name => this.protoOptionsCategory.Name;
    }
}