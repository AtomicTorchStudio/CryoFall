namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ConstructionMetaInfoControl : BaseUserControl
    {
        public static readonly DependencyProperty EntriesProperty
            = DependencyProperty.Register(nameof(Entries),
                                          typeof(List<ControlTemplate>),
                                          typeof(ConstructionMetaInfoControl),
                                          new PropertyMetadata(default(List<ControlTemplate>)));

        public static readonly List<ConstructionMetaInfoTemplateDefinition> Templates = new();

        public static readonly DependencyProperty ProtoObjectStructureProperty
            = DependencyProperty.Register(nameof(ProtoObjectStructure),
                                          typeof(IProtoObjectStructure),
                                          typeof(ConstructionMetaInfoControl),
                                          new PropertyMetadata(default(IProtoObjectStructure),
                                                               DependencyPropertyChanged));

        private Grid layoutRoot;

        private ViewModelStructure viewModel;

        public List<ControlTemplate> Entries
        {
            get => (List<ControlTemplate>)this.GetValue(EntriesProperty);
            set => this.SetValue(EntriesProperty, value);
        }

        public IProtoObjectStructure ProtoObjectStructure
        {
            get => (IProtoObjectStructure)this.GetValue(ProtoObjectStructureProperty);
            set => this.SetValue(ProtoObjectStructureProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.DestroyViewModel();
        }

        private static void DependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ConstructionMetaInfoControl)d).Refresh();
        }

        private void DestroyViewModel()
        {
            if (this.viewModel is null)
            {
                return;
            }

            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private void Refresh()
        {
            if (!this.isLoaded)
            {
                return;
            }

            this.DestroyViewModel();
            this.layoutRoot.DataContext = this.viewModel = new ViewModelStructure(this.ProtoObjectStructure);

            ConstructionMetaInfoTemplatesBootstrapper.InitIfNecessary();

            var protoStructure = this.ProtoObjectStructure;
            var result = new List<ControlTemplate>();

            if (protoStructure is not null)
            {
                foreach (var entry in Templates)
                {
                    if (entry.Condition(protoStructure))
                    {
                        result.Add(entry.Template);
                    }
                }
            }

            this.Entries = result;
        }
    }
}