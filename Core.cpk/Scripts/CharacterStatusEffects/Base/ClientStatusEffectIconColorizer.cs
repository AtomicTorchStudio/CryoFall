namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public static class ClientStatusEffectIconColorizer
    {
        private static readonly Brush BrushBuffTier0
            = new SolidColorBrush(Color.FromRgb(0x11, 0x90, 0x33));

        private static readonly Brush BrushBuffTier1
            = new SolidColorBrush(Color.FromRgb(0x11, 0xB0, 0x44));

        private static readonly Brush BrushBuffTier2
            = new SolidColorBrush(Color.FromRgb(0x11, 0xCC, 0x55));

        private static readonly Brush BrushDebuffTier0
            = new SolidColorBrush(Color.FromRgb(0xEE, 0xCC, 0x55));

        private static readonly Brush BrushDebuffTier1
            = new SolidColorBrush(Color.FromRgb(0xEE, 0x88, 0x44));

        private static readonly Brush BrushDebuffTier2
            = new SolidColorBrush(Color.FromRgb(0xDD, 0x11, 0x33));

        private static readonly Brush BrushNeutral
            = new SolidColorBrush(Color.FromRgb(0x99, 0xDC, 0xFF));

        private static readonly Dictionary<CacheKey, ProceduralTexture> ColorizedIconsCache
            = new();

        private static readonly RenderingMaterial ColorizerRenderingMaterial
            = RenderingMaterial.Create(new EffectResource("Special/StatusEffectColorizer"));

        public static Brush GetBrush(StatusEffectKind kind, double intensity)
        {
            if (kind == StatusEffectKind.Neutral)
            {
                return BrushNeutral;
            }

            byte tier;
            if (intensity < 0.333)
            {
                tier = 0;
            }
            else if (intensity < 0.667)
            {
                tier = 1;
            }
            else
            {
                tier = 2;
            }

            if (kind == StatusEffectKind.Buff)
            {
                return tier switch
                {
                    0 => BrushBuffTier0,
                    1 => BrushBuffTier1,
                    2 => BrushBuffTier2
                };
            }

            return tier switch
            {
                0 => BrushDebuffTier0,
                1 => BrushDebuffTier1,
                2 => BrushDebuffTier2
            };
        }

        public static ITextureResource GetColorizedIcon(
            ITextureResource icon,
            StatusEffectKind kind,
            double intensity)
        {
            var brush = GetBrush(kind, intensity);
            var color = ((SolidColorBrush)brush).Color;

            var key = new CacheKey(icon, color);
            if (ColorizedIconsCache.TryGetValue(key, out var proceduralTexture))
            {
                return proceduralTexture;
            }

            proceduralTexture = new ProceduralTexture(
                name: icon + " colorized " + color,
                ProcedureTextureCallback,
                isTransparent: true,
                isUseCache: true,
                data: (icon, color),
                dependsOn: new[] { icon });

            ColorizedIconsCache[key] = proceduralTexture;
            return proceduralTexture;
        }

        private static async Task<ITextureResource> ProcedureTextureCallback(ProceduralTextureRequest request)
        {
            var renderingTag = request.TextureName;

            var (sourceTextureResource, color)
                = (ValueTuple<ITextureResource, Color>)request.Data;

            var rendering = Api.Client.Rendering;
            var textureSize = await rendering.GetTextureSize(sourceTextureResource);
            var textureWidth = textureSize.X;
            var textureHeight = textureSize.Y;

            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = rendering.CreateCamera(cameraObject,
                                                renderingTag: renderingTag,
                                                drawOrder: -10);

            var renderTexture = rendering.CreateRenderTexture(renderingTag, textureWidth, textureHeight);
            camera.RenderTarget = renderTexture;
            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.SetOrthographicProjection(textureWidth, textureHeight);

            var spriteRenderer = rendering.CreateSpriteRenderer(cameraObject,
                                                                sourceTextureResource,
                                                                renderingTag: renderingTag,
                                                                // draw down
                                                                spritePivotPoint: (0, 1));
            spriteRenderer.Color = color;
            spriteRenderer.RenderingMaterial = ColorizerRenderingMaterial;

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: false,
                                                                     customName: renderingTag);
            renderTexture.Dispose();
            request.ThrowIfCancelled();

            return generatedTexture;
        }

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            public readonly Color Color;

            public readonly ITextureResource Icon;

            public CacheKey(ITextureResource icon, Color color)
            {
                this.Icon = icon;
                this.Color = color;
            }

            public bool Equals(CacheKey other)
            {
                return this.Color.Equals(other.Color)
                       && Equals(this.Icon, other.Icon);
            }

            public override bool Equals(object obj)
            {
                return obj is CacheKey other && this.Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.Color, this.Icon);
            }
        }
    }
}