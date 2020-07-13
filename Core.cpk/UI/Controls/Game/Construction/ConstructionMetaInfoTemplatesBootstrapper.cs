namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction
{
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public static class ConstructionMetaInfoTemplatesBootstrapper
    {
        private static bool isInitialized;

        public static void InitIfNecessary()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            var templates = ConstructionMetaInfoControl.Templates;
            var xamlTemplates =
                Api.Client.UI.LoadResourceDictionary(
                    "UI/Controls/Game/Construction/ConstructionMetaInfoTemplates.xaml");

            // add some default templates
            templates.Add(
                new ConstructionMetaInfoTemplateDefinition(
                    condition: protoStructure => !protoStructure.IsRelocatable,
                    template: GetTemplate("TemplateUnmovable")));

            templates.Add(
                new ConstructionMetaInfoTemplateDefinition(
                    condition: protoStructure => protoStructure.Category is StructureCategoryDecorations,
                    template: GetTemplate("TemplateDecorative")));

            templates.Add(
                new ConstructionMetaInfoTemplateDefinition(
                    condition: protoStructure => protoStructure is IProtoObjectElectricityConsumer protoConsumer
                                                 && protoConsumer.ElectricityConsumptionPerSecondWhenActive > 0,
                    template: GetTemplate("TemplatePowerConsumer")));

            templates.Add(
                new ConstructionMetaInfoTemplateDefinition(
                    condition: protoStructure => protoStructure is IProtoObjectElectricityProducer,
                    template: GetTemplate("TemplatePowerProducer")));

            templates.Add(
                new ConstructionMetaInfoTemplateDefinition(
                    condition: protoStructure => protoStructure is IProtoObjectElectricityStorage,
                    template: GetTemplate("TemplatePowerStorage")));

            ControlTemplate GetTemplate(string name)
                => xamlTemplates.GetResource<ControlTemplate>(name);
        }
    }
}