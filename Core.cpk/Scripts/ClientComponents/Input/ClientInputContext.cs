namespace AtomicTorch.CBND.CoreMod.ClientComponents.Input
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data;
    using AtomicTorch.GameEngine.Common.DataStructures;

    public class ClientInputContext
    {
        private static readonly List<ClientInputContext> currentContexts;

        private readonly List<ButtonCallback> buttonDownCallbacks
            = new List<ButtonCallback>(capacity: 0);

        private Action callbackHandleAll;

        private bool isCallbacksValidated;

        private bool isStarted;

        static ClientInputContext()
        {
            currentContexts = new List<ClientInputContext>(capacity: 16);
            CurrentContexts = new FreezableListWrapper<ClientInputContext>(currentContexts);
        }

        private ClientInputContext(string name)
        {
            this.Name = name;
            this.Start();
        }

        public static IFreezableListWrapper<ClientInputContext> CurrentContexts { get; }

        public string Name { get; }

        public static ClientInputContext Start(string name)
        {
            return new ClientInputContext(name);
        }

        public ClientInputContext HandleAll(Action callback)
        {
            if (this.callbackHandleAll != null)
            {
                throw new Exception(nameof(this.HandleAll) + " callback is already assigned");
            }

            this.callbackHandleAll = callback;
            return this;
        }

        public ClientInputContext HandleButtonDown<TButton>(
            TButton button,
            Action buttonCallback,
            bool evenIfHandled = false)
            where TButton : struct, Enum
        {
            this.buttonDownCallbacks.Add(
                new ButtonCallback(
                    WrappedButton<TButton>.GetWrappedButton(button),
                    buttonCallback,
                    evenIfHandled));
            return this;
        }

        public void Stop()
        {
            if (!this.isStarted)
            {
                return;
            }

            this.isStarted = false;
            currentContexts.Remove(this);
        }

        internal void Update()
        {
            this.ValidateCallbacks();

            if (this.buttonDownCallbacks.Count > 0
                // if there is no key rebinding window
                && ViewModelRebindKeyWindow.Instance == null)
            {
                foreach (var pair in this.buttonDownCallbacks)
                {
                    var button = pair.WrappedButton;
                    if (!ClientInputManager.IsButtonDown(button, evenIfHandled: pair.EvenIfHandled))
                    {
                        continue;
                    }

                    // button pressed - consume and execute the callback
                    ClientInputManager.ConsumeButton(button);
                    pair.Callback();
                }
            }

            this.callbackHandleAll?.Invoke();
        }

        private void Start()
        {
            if (this.isStarted)
            {
                throw new Exception("The input context " + this.Name + " is already listening");
            }

            this.isStarted = true;
            currentContexts.Add(this);
        }

        private void ValidateCallbacks()
        {
            if (this.isCallbacksValidated)
            {
                return;
            }

            if (this.buttonDownCallbacks.Count == 0
                && this.callbackHandleAll == null)
            {
                this.Stop();
                throw new Exception(
                    "You didn't registered any callbacks for input context "
                    + this.Name
                    + " (stopping this input context from processing).");
            }

            this.isCallbacksValidated = true;
        }

        private struct ButtonCallback
        {
            public readonly Action Callback;

            public readonly bool EvenIfHandled;

            public readonly IWrappedButton WrappedButton;

            public ButtonCallback(IWrappedButton wrappedButton, Action callback, bool evenIfHandled)
            {
                this.WrappedButton = wrappedButton;
                this.Callback = callback;
                this.EvenIfHandled = evenIfHandled;
            }
        }
    }
}