namespace AtomicTorch.CBND.CoreMod.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class WindowsManager
    {
        public const int WindowsOffset = 100;

        public static readonly IReadOnlyList<GameWindow> OpenedWindows;

        // ReSharper disable once InconsistentNaming
        private static readonly List<GameWindow> openedWindows;

        private static GameWindowBackgroundOverlay backgroundOverlay;

        static WindowsManager()
        {
            openedWindows = new List<GameWindow>();
            OpenedWindows = openedWindows;
        }

        public static GameWindow LastOpenedWindow
        {
            get
            {
                var count = openedWindows.Count;
                return count > 0 ? openedWindows[count - 1] : null;
            }
        }

        public static int OpenedWindowsCount => openedWindows.Count;

        public static void BringToFront(GameWindow window)
        {
            var parent = window.LinkedParent
                         ?? window.Parent
                         ?? window.TemplatedParent ?? LogicalTreeHelper.GetParent(window);

            var windowZIndex = CalculateLastZIndex(window);
            window.CurrentZIndex = windowZIndex;

            Panel.SetZIndex((UIElement)parent, windowZIndex);
            Api.Logger.Important(
                string.Format("ZIndex for window {0} set to {1}", window.WindowName, windowZIndex));

            UpdateOverlayZIndex();
        }

        public static void OnWindowStateChanged()
        {
            UpdateOverlayZIndex();
        }

        public static void RegisterWindow(GameWindow window)
        {
            if (window.IsDestroyed)
            {
                throw new Exception("Window is destroyed: " + window);
            }

            if (openedWindows.Contains(window))
            {
                return;
            }

            openedWindows.Add(window);
            BringToFront(window);
            window.Open();

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            window.CloseByEscapeKeyInputContext
                = ClientInputContext.Start("Close window")
                                    .HandleAll(
                                        () =>
                                        {
                                            const GameButton button = GameButton.CancelOrClose;
                                            if (ClientInputManager.IsButtonDown(button)
                                                && window.CloseByEscapeKey)
                                            {
                                                window.Close(DialogResult.Cancel);
                                                ClientInputManager.ConsumeButton(button);
                                            }
                                        });
        }

        public static void UnregisterWindow(GameWindow window)
        {
            if (!openedWindows.Remove(window))
            {
                return;
            }

            var lastOpenedWindow = LastOpenedWindow;
            if (lastOpenedWindow != null)
            {
                UpdateOverlayZIndex();
            }
            else
            {
                TurnOffOverlay();
            }

            var closeByEscapeKeyInputContext = window.CloseByEscapeKeyInputContext;
            if (closeByEscapeKeyInputContext != null)
            {
                closeByEscapeKeyInputContext.Stop();
                window.CloseByEscapeKeyInputContext = null;
            }

            if (window.IsCached)
            {
                return;
            }

            // destroy window completely
            var layoutRootChildren = Api.Client.UI.LayoutRootChildren;
            if (window.Parent is UIElement parent
                && layoutRootChildren != null
                && layoutRootChildren.Contains(parent))
            {
                layoutRootChildren.Remove(parent);
                Api.Logger.Important("Window completely destroyed: " + window);
                window.IsDestroyed = true;
            }
        }

        private static int CalculateLastZIndex(GameWindow window)
        {
            return WindowsOffset + openedWindows.Count * 2 + window.ZIndexOffset;
        }

        private static void TurnOffOverlay()
        {
            if (backgroundOverlay != null)
            {
                backgroundOverlay.IsBackgroundEnabled = false;
            }
        }

        private static void UpdateOverlayZIndex()
        {
            if (openedWindows.Count == 0
                || openedWindows.All(w => w.State != GameWindowState.Opened
                                          && w.State != GameWindowState.Opening))
            {
                TurnOffOverlay();
                return;
            }

            var maxWindowZIndex = openedWindows.Max(w => w.CurrentZIndex);
            if (backgroundOverlay == null)
            {
                backgroundOverlay = new GameWindowBackgroundOverlay();
                Api.Client.UI.LayoutRootChildren.Add(backgroundOverlay);
            }

            Panel.SetZIndex(backgroundOverlay, maxWindowZIndex - 1);
            backgroundOverlay.IsBackgroundEnabled = true;
        }
    }
}