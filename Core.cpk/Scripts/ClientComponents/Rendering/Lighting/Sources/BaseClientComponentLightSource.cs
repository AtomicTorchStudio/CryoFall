namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class BaseClientComponentLightSource : ClientComponent
    {
        public const string DefaultLightRenderingTag = ClientLighting.RenderingTag;

        protected static readonly IRenderingClientService Rendering = Client.Rendering;

        private Color color;

        private bool isDirty;

        private Size2F logicalSize;

        private double opacity = 1;

        private Vector2D positionOffset = (0.5, 0);

        private Size2F renderingSize = 1;

        private Vector2D spritePivotPoint = (0.5, 0.5);

        protected BaseClientComponentLightSource()
            : base(isLateUpdateEnabled: true)
        {
        }

        public Color Color
        {
            get => this.color;
            set
            {
                if (this.color.Equals(value))
                {
                    return;
                }

                this.color = value;
                this.SetDirty();
            }
        }

        public double LogicalLightRadiusSqr { get; private set; }

        public Size2F LogicalSize
        {
            get => this.logicalSize;
            set
            {
                if (this.logicalSize == value)
                {
                    return;
                }

                this.logicalSize = value;
                var logicalLightRadius = Math.Max(this.logicalSize.X, this.logicalSize.Y)
                                         / 2.5;
                this.LogicalLightRadiusSqr = logicalLightRadius * logicalLightRadius;
            }
        }

        public double Opacity
        {
            get => this.opacity;
            set
            {
                if (this.opacity == value)
                {
                    return;
                }

                this.opacity = value;
                this.SetDirty();
            }
        }

        public Vector2D PositionOffset
        {
            get => this.positionOffset;
            set
            {
                if (this.positionOffset.Equals(value))
                {
                    return;
                }

                this.positionOffset = value;
                this.SetDirty();
            }
        }

        public Size2F RenderingSize
        {
            get => this.renderingSize;
            set
            {
                if (this.renderingSize.Equals(value))
                {
                    return;
                }

                this.renderingSize = value;
                this.SetDirty();
            }
        }

        public Vector2D SpritePivotPoint
        {
            get => this.spritePivotPoint;
            set
            {
                if (this.spritePivotPoint.Equals(value))
                {
                    return;
                }

                this.spritePivotPoint = value;
                this.SetDirty();
            }
        }

        protected Color LightColorPremultipliedAndWithOpacity
        {
            get
            {
                var a = (double)this.Color.A * this.Opacity;
                return Color.FromArgb(
                    (byte)a,
                    (byte)(this.Color.R * a / byte.MaxValue),
                    (byte)(this.Color.G * a / byte.MaxValue),
                    (byte)(this.Color.B * a / byte.MaxValue));
            }
        }

        public sealed override void LateUpdate(double deltaTime)
        {
            if (this.isDirty)
            {
                this.SetProperties();
            }

            this.LateUpdateLight(deltaTime);
        }

        public sealed override void Update(double deltaTime)
        {
            this.UpdateLight(deltaTime);
        }

        protected virtual void LateUpdateLight(double deltaTime)
        {
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ClientLightSourceManager.Unregister(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ClientLightSourceManager.Register(this);
        }

        protected void SetDirty()
        {
            this.isDirty = true;
        }

        protected abstract void SetProperties();

        protected virtual void UpdateLight(double deltaTime)
        {
        }
    }
}