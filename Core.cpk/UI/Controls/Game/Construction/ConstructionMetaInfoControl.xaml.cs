namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ConstructionMetaInfoControl : BaseUserControl
    {
        public static readonly DependencyProperty EntriesProperty =
            DependencyProperty.Register(nameof(Entries),
                                        typeof(List<ControlTemplate>),
                                        typeof(ConstructionMetaInfoControl),
                                        new PropertyMetadata(default(List<ControlTemplate>)));

        public static List<ConstructionMetaInfoTemplateDefinition> Templates
            = new List<ConstructionMetaInfoTemplateDefinition>();

        public static readonly DependencyProperty ProtoObjectStructureProperty =
            DependencyProperty.Register(nameof(ProtoObjectStructure),
                                        typeof(IProtoObjectStructure),
                                        typeof(ConstructionMetaInfoControl),
                                        new PropertyMetadata(default(IProtoObjectStructure),
                                                             DependencyPropertyChanged));

        public ConstructionMetaInfoControl()
        {
        }

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

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        private static void DependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ConstructionMetaInfoControl)d).Refresh();
        }

        private void Refresh()
        {
            if (!this.isLoaded)
            {
                return;
            }

            ConstructionMetaInfoTemplatesBootstrapper.InitIfNecessary();

            var protoStructure = this.ProtoObjectStructure;
            var result = new List<ControlTemplate>();

            if (protoStructure != null)
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