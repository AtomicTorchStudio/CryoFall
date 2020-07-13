namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications
{
    using System;

    public interface IHudNotificationControl
    {
        bool IsAutoHide { get; }

        bool IsHiding { get; }

        void Hide(bool quick);

        void HideAfterDelay(double delaySeconds);

        bool IsSame(IHudNotificationControl other);

        void SetupAutoHideChecker(Func<bool> checker);
    }
}