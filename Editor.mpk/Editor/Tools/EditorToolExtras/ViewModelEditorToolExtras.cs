namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras
{
    using AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.ItemsBrowser;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.NoiseView;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelEditorToolExtras : BaseViewModel
    {
        public BaseCommand CommandOpenItemsBrowserWindow { get; }
            = new ActionCommand(
                () => Api.Client.UI.LayoutRootChildren
                         .Add(new WindowEditorItemsBrowserWindow()));

        public BaseCommand CommandOpenNoiseComposerWindow { get; }
            = new ActionCommand(
                () => Api.Client.UI.LayoutRootChildren
                         .Add(new WindowEditorNoiseComposer()));

        public BaseCommand CommandOpenNoiseForGroundWindow { get; }
            = new ActionCommand(
                () => Api.Client.UI.LayoutRootChildren
                         .Add(new WindowEditorNoiseForGround()));
    }
}