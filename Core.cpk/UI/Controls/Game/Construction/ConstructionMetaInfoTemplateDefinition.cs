namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction
{
    using System;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;

    public class ConstructionMetaInfoTemplateDefinition
    {
        public ConstructionMetaInfoTemplateDefinition(
            Func<IProtoObjectStructure, bool> condition,
            ControlTemplate template)
        {
            this.Condition = condition;
            this.Template = template;
        }

        public Func<IProtoObjectStructure, bool> Condition { get; }

        public ControlTemplate Template { get; }
    }
}