namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolMap
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelEditorToolMapSettings : BaseViewModel
    {
        public ViewModelEditorToolMapSettings(
            BaseCommand newWorld,
            BaseCommand openWorld,
            BaseCommand saveWorldAs,
            BaseCommand saveWorld,
            BaseCommand commandLoadSavegame,
            BaseCommand commandSaveSavegame)
        {
            this.CommandNewWorld = newWorld;
            this.CommandOpenWorld = openWorld;
            this.CommandSaveWorldAs = saveWorldAs;
            this.CommandSaveWorld = saveWorld;
            this.CommandLoadSavegame = commandLoadSavegame;
            this.CommandSaveSavegame = commandSaveSavegame;
        }

        public ViewModelEditorToolMapSettings()
        {
        }

        public BaseCommand CommandLoadSavegame { get; }

        public BaseCommand CommandNewWorld { get; }

        public BaseCommand CommandOpenWorld { get; }

        public BaseCommand CommandSaveSavegame { get; }

        public BaseCommand CommandSaveWorld { get; }

        public BaseCommand CommandSaveWorldAs { get; }
    }
}