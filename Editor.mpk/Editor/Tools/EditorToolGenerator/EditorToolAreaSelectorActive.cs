namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolGenerator
{
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class EditorToolAreaSelectorActive : BaseEditorActiveTool
    {
        private readonly IClientSceneObject sceneObject;

        public EditorToolAreaSelectorActive()
        {
            this.sceneObject = Api.Client.Scene.CreateSceneObject("Editor Generator Tool");
            var selectArea = this.sceneObject.AddComponent<ClientComponentEditorToolSelectLocationWithExtending>();
            this.LocationSettingsViewModel = new ViewModelLocationSettings(selectArea);
        }

        public ViewModelLocationSettings LocationSettingsViewModel { get; }

        public override void Dispose()
        {
            this.LocationSettingsViewModel.Dispose();
            this.sceneObject.Destroy();
        }
    }
}