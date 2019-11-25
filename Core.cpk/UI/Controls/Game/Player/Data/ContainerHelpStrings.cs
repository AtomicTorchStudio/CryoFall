namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class ContainerHelpStrings
    {
        public const string AltLeftClickOnItem_Description =
            @"Use/eat the pointed item.
              [br](only for items that can be used/eaten)";

        public const string AltLeftClickOnItem_Title =
            "Alt + Left click on an item";

        public const string AltRightClickOnItem_Description =
            @"Drop the pointed item stack to the ground.
              [br](also hold Ctrl to drop only a single item from the stack)";

        public const string AltRightClickOnItem_Title =
            "Alt + Right click on an item";

        public const string CtrlLeftOrRightClick_Description =
            "Move a single item from the pointed stack to another container.";

        public const string CtrlLeftOrRightClick_Title =
            "Ctrl + Left or Right click on items";

        public const string LeftClickEmptyHand_Description =
            "Take that item or items.";

        public const string LeftClickEmptyHand_Title =
            "Left click with empty hand on an item";

        public const string LeftClickFullHand_Description =
            @"If slot is empty—put the item or items there.
              [br]If slot is not empty—combine or swap items.";

        public const string LeftClickFullHand_Title =
            "Left click with a full hand on a slot";

        public const string MiddleClick_Description =
            "Sort inventory (can be used on any pointed items grid).";

        public const string MiddleClick_Title =
            "Middle (scroll wheel) click";

        public const string RightClickEmptyHand_Description =
            "Take half of those items.";

        public const string RightClickEmptyHand_Title =
            "Right click with empty hand on items";

        public const string RightClickFullHand_Description =
            "If slot is empty or contains the same item type—[br]move a single item from hand to slot.";

        public const string RightClickFullHand_Title =
            "Right click with a full hand on a slot";

        public const string ShiftLeftClickOnItems_Description =
            "Move all stacks of the pointed item type to another container.";

        public const string ShiftLeftClickOnItems_Title =
            "Shift + Left click on items";

        public const string ShiftRightClickOnItems_Description =
            "Move only the pointed items stack to another container.";

        public const string ShiftRightClickOnItems_Title =
            "Shift + Right click on items";

        public const string TitleItemSlotActions =
            "Item slot actions";
    }
}