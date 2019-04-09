namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting
{
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

        private double opacity = 1;

        private Vector2D positionOffset = (0.5, 0);

        private Size2F size = 1;

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

        public Size2F Size
        {
            get => this.size;
            set
            {
                if (this.size.Equals(value))
                {
                    return;
                }

                this.size = value;
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