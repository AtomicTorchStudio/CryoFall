namespace AtomicTorch.CBND.CoreMod.Editor
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Editor.UI;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public enum EditorButton
    {
        [Description("Toggle editor mode")]
        [ButtonInfo(InputKey.F8, Category = EditorStrings.Editor)]
        ToggleEditorMode,

        // Editor: saves world, game and server state to a separate quick slot
        [Description("Quick save state")]
        [ButtonInfo(InputKey.F2, Category = EditorStrings.Editor)]
        MakeQuickSavegame,

        // Editor: loads server state from the quick slot
        [Description("Quick load state")]
        [ButtonInfo(InputKey.F3, Category = EditorStrings.Editor)]
        LoadQuickSavegame,

        // Editor: delete selected object(s)
        [Description("Delete selected")]
        [ButtonInfo(InputKey.Delete, Category = EditorStrings.Editor)]
        EditorDeleteSelectedObjects,
    }
}