namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;

    public abstract class BasePostEffect
    {
        protected static readonly IRenderingClientService Rendering = Api.Client.Rendering;

        private Action callbackRegister;

        private Action callbackUnregister;

        private PostEffectsOrder? customOrder;

        private bool isEnabled;

        public virtual PostEffectsOrder DefaultOrder => PostEffectsOrder.Default;

        public abstract bool IsCanRender { get; }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }

                this.isEnabled = value;

                if (this.isEnabled)
                {
                    this.OnEnable();
                    this.callbackRegister?.Invoke();
                }
                else
                {
                    this.OnDisable();
                    this.callbackUnregister?.Invoke();
                }
            }
        }

        public PostEffectsOrder Order
        {
            get => this.customOrder ?? this.DefaultOrder;
            set
            {
                if (this.Order == value)
                {
                    return;
                }

                this.customOrder = value;

                if (this.IsEnabled)
                {
                    // need to sort the post effects
                    this.SortAllPostEffects();
                }
            }
        }

        public void Destroy()
        {
            this.IsEnabled = false;
        }

        /// <summary>
        /// Render post effect source to destination render targets.
        /// Please note that the destination render target must be cleared manually as it might contain garbage.
        /// </summary>
        public abstract void Render(IRenderTarget2D source, IRenderTarget2D destination);

        public override string ToString()
        {
            return string.Format("{0} ({1})",
                                 this.GetType().Name.Replace("PostEffect", string.Empty),
                                 this.GetType().FullName);
        }

        internal void SetupManager(Action callbackRegister, Action callbackUnregister)
        {
            if (this.callbackRegister != null)
            {
                throw new Exception("Already setup: " + this);
            }

            this.callbackRegister = callbackRegister;
            this.callbackUnregister = callbackUnregister;
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnEnable()
        {
        }

        private void SortAllPostEffects()
        {
            if (!this.isEnabled)
            {
                return;
            }

            this.callbackUnregister?.Invoke();
            this.callbackRegister?.Invoke();
        }
    }
}