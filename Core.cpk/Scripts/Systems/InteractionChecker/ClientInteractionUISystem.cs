namespace AtomicTorch.CBND.CoreMod.Systems.InteractionChecker
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientInteractionUISystem
    {
        private static readonly Dictionary<IWorldObject, Token> RegisteredWindows
            = new();

        public static void OnServerForceFinishInteraction(IWorldObject worldObject)
        {
            Api.Logger.Info($"Server informed that the object interaction with {worldObject} is finished");
            InteractionCheckerSystem.ClientOnServerForceFinishInteraction(worldObject);

            if (!RegisteredWindows.TryGetValue(worldObject, out var token))
            {
                return;
            }

            Unregister(worldObject);

            if (!token.Menu.TryGetTarget(out var menu))
            {
                return;
            }

            Api.Logger.Warning(
                "Server forced close of the interaction menu: " + menu + " for " + worldObject);
            menu.CloseWindow();
        }

        public static void Register(
            IWorldObject worldObject,
            BaseUserControlWithWindow menu,
            Action onMenuClosedByClient)
        {
            Unregister(worldObject);

            Action handler = () =>
                             {
                                 if (Unregister(worldObject))
                                 {
                                     onMenuClosedByClient();
                                 }
                             };

            menu.EventWindowClosing += handler;
            RegisteredWindows[worldObject] = new Token(menu, handler);
        }

        private static bool Unregister(IWorldObject worldObject)
        {
            if (!RegisteredWindows.TryGetValue(worldObject, out var token))
            {
                return false;
            }

            RegisteredWindows.Remove(worldObject);

            if (token.Menu.TryGetTarget(out var menu))
            {
                menu.EventWindowClosing -= token.Handler;
            }

            return true;
        }

        private readonly struct Token
        {
            public readonly Action Handler;

            public readonly WeakReference<BaseUserControlWithWindow> Menu;

            public Token(BaseUserControlWithWindow menu, Action handler)
            {
                this.Menu = new WeakReference<BaseUserControlWithWindow>(menu);
                this.Handler = handler;
            }
        }
    }
}