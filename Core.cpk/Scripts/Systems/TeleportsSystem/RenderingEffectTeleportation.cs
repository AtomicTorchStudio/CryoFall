namespace AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class RenderingEffectTeleportation : RenderingEffect
    {
        private readonly IGraphicsDevice device;

        private readonly EffectInstance effect;

        private double progress;

        public RenderingEffectTeleportation()
        {
            this.device = Api.Client.Rendering.GraphicsDevice;
            this.effect = EffectInstance.Create(new EffectResource("Special/Teleportation"));
            this.effect.Parameters
                .Set("NoiseTexture",
                     new TextureResource("FX/Noise", isLinearSpace: true, qualityOffset: -100));
        }

        public override bool IsReady => this.effect.IsReady;

        public double Progress
        {
            get => this.progress;
            set
            {
                if (this.progress == value)
                {
                    return;
                }

                this.progress = value;
                this.effect.Parameters.Set("Progress", (float)this.progress);
            }
        }

        public override void Draw(
            IRenderTarget2D source,
            IRenderTarget2D destination,
            IGraphicsDevice graphicsDevice)
        {
            this.device.SetRenderTarget(destination);
            this.device.Clear(Color.FromArgb(0, 0, 0, 0));

            this.effect.Parameters
                .Set("NoiseTextureUvScale", new Vector2F(source.Width, source.Height) / 256.0f);

            graphicsDevice.DrawTexture(
                source,
                source.Width,
                source.Height,
                effectInstance: this.effect,
                blendState: BlendMode.AdditivePremultiplied);
        }

        public override Vector2Int GetTargetTextureSize(IRenderTarget2D source)
        {
            return (source.Width, source.Height);
        }

        public class RenderingEffectTeleportationTestDummy : RenderingEffectTeleportation
        {
            private bool isTeleportationIn;

            public RenderingEffectTeleportationTestDummy()
            {
                ClientUpdateHelper.UpdateCallback += this.Update;
            }

            private void Update()
            {
                var sign = this.isTeleportationIn ? 1 : -1;
                this.Progress += sign * Api.Client.Core.DeltaTime * 0.5;

                if (this.isTeleportationIn
                    && this.Progress > 1)
                {
                    this.isTeleportationIn = false;
                    this.Progress = 1;
                }
                else if (!this.isTeleportationIn
                         && this.Progress < 0)
                {
                    this.isTeleportationIn = true;
                    this.Progress = 0;
                }
            }
        }
    }
}