namespace AtomicTorch.CBND.CoreMod.ClientComponents.Input
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public enum GameButton
    {
        // Cancel or close menu/window (usually Escape)
        [Description("Cancel or close")]
        [ButtonInfo(InputKey.Escape)]
        CancelOrClose,

        [Description("Move up")]
        [ButtonInfo(InputKey.W)]
        MoveUp,

        [Description("Move left")]
        [ButtonInfo(InputKey.A)]
        MoveLeft,

        [Description("Move down")]
        [ButtonInfo(InputKey.S)]
        MoveDown,

        [Description("Move right")]
        [ButtonInfo(InputKey.D)]
        MoveRight,

        // Run temporary, while the key is held
        [Description("Run (held)")]
        [ButtonInfo(InputKey.Shift)]
        RunTemporary,

        // Switches between run/walk mode
        [Description("Run (toggle)")]
        [ButtonInfo] // no default key binding
        RunToggle,

        // Use the current item (from the active hotbar slot)
        [Description("Action—use item")]
        [ButtonInfo(InputKey.MouseLeftButton)]
        ActionUseCurrentItem,

        // Interaction with the world object under the mouse cursor
        [Description("Action—interact")]
        [ButtonInfo(InputKey.MouseRightButton)]
        ActionInteract,

        [Description("Enter/Exit vehicle")]
        [ButtonInfo(InputKey.Q)]
        VehicleEnterExit,

        [Description("Reload item")]
        [ButtonInfo(InputKey.R)]
        ItemReload,

        // Switch item mode (for example, switch/alternate the ammo type for current weapon)
        [Description(@"Switch item mode
  [br]/switch ammo")]
        [ButtonInfo(InputKey.B)]
        ItemSwitchMode,

        // Toggle currently equipped helmet light (if you have equipped a helmet with the light source)
        [Description("Helmet light toggle")]
        [ButtonInfo(InputKey.F)]
        HeadEquipmentLightToggle,

        [Description("Slot #1")]
        [ButtonInfo(InputKey.D1)]
        HotbarSelectSlot1,

        [Description("Slot #2")]
        [ButtonInfo(InputKey.D2)]
        HotbarSelectSlot2,

        [Description("Slot #3")]
        [ButtonInfo(InputKey.D3)]
        HotbarSelectSlot3,

        [Description("Slot #4")]
        [ButtonInfo(InputKey.D4)]
        HotbarSelectSlot4,

        [Description("Slot #5")]
        [ButtonInfo(InputKey.D5)]
        HotbarSelectSlot5,

        [Description("Slot #6")]
        [ButtonInfo(InputKey.D6)]
        HotbarSelectSlot6,

        [Description("Slot #7")]
        [ButtonInfo(InputKey.D7)]
        HotbarSelectSlot7,

        [Description("Slot #8")]
        [ButtonInfo(InputKey.D8)]
        HotbarSelectSlot8,

        [Description("Slot #9")]
        [ButtonInfo(InputKey.D9)]
        HotbarSelectSlot9,

        [Description("Slot #10")]
        [ButtonInfo(InputKey.D0)]
        HotbarSelectSlot10,

        [Description("Camera zoom in")]
        [ButtonInfo(InputKey.PageDown, InputKey.OemPlus)]
        CameraZoomIn,

        [Description("Camera zoom out")]
        [ButtonInfo(InputKey.PageUp, InputKey.OemMinus)]
        CameraZoomOut,

        [Description("Chat")]
        [ButtonInfo(InputKey.Enter)]
        OpenChat,

        [Description(CoreStrings.HUDButtonsBar_MenuTitle_Equipment)]
        [ButtonInfo(InputKey.E)]
        InventoryMenu,

        [Description(CoreStrings.HUDButtonsBar_MenuTitle_Crafting)]
        [ButtonInfo(InputKey.C)]
        CraftingMenu,

        [Description(CoreStrings.HUDButtonsBar_MenuTitle_Map)]
        [ButtonInfo(InputKey.M)]
        MapMenu,

        [Description(CoreStrings.HUDButtonsBar_MenuTitle_Construction)]
        [ButtonInfo(InputKey.Tab)]
        ConstructionMenu,

        [Description(CoreStrings.HUDButtonsBar_MenuTitle_Skills)]
        [ButtonInfo(InputKey.K)]
        SkillsMenu,

        [Description(CoreStrings.HUDButtonsBar_MenuTitle_Technologies)]
        [ButtonInfo(InputKey.G)]
        TechnologiesMenu,

        [Description(CoreStrings.HUDButtonsBar_MenuTitle_Social)]
        [ButtonInfo(InputKey.H)]
        SocialMenu,

        [Description(CoreStrings.HUDButtonsBar_MenuTitle_Quests)]
        [ButtonInfo(InputKey.J)]
        QuestsMenu,

        [Description(CoreStrings.HUDButtonsBar_MenuTitle_Politics)]
        [ButtonInfo(InputKey.P)]
        PoliticsMenu,

        [Description("Developer console")]
        [ButtonInfo(InputKey.OemTilde, InputKey.CircumflexAccent)]
        ToggleDeveloperConsole,

        // Toggle debug tools overlay
        [Description("Debug tools")]
        [ButtonInfo(InputKey.F5)]
        ToggleDebugToolsOverlay,

        [Description("Toggle fullscreen")]
        [ButtonInfo(InputKey.F11)]
        ToggleFullscreen,

        // (See the screenshot settings in the General options tab)
        [Description("Capture screenshot")]
        [ButtonInfo(InputKey.F4, InputKey.F12)]
        CaptureScreenshot,

        // Point on an items container and press this button to sort its content
        [Description("Sort container")]
        [ButtonInfo(InputKey.MouseScrollButton, InputKey.Z)]
        ContainerSort,

        [Description("Take all items")]
        [ButtonInfo(InputKey.R, Category = CoreStrings.OptionsInput_ContainerMenuCategory)]
        ContainerTakeAll,

        [Description("Match items down")]
        [ButtonInfo(InputKey.T, Category = CoreStrings.OptionsInput_ContainerMenuCategory)]
        ContainerMoveItemsMatchDown,

        [Description("Match items up")]
        [ButtonInfo(InputKey.Y, Category = CoreStrings.OptionsInput_ContainerMenuCategory)]
        ContainerMoveItemsMatchUp,

        // Hold to display the land claim zones and healthbars for all damaged objects on the screen (or you can also just hold the Alt key).
        [Description("Display land claim[br]and health bars")]
        [ButtonInfo(InputKey.L)]
        DisplayLandClaim,
    }
}