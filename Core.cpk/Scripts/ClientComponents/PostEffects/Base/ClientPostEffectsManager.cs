namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Extensions;

    public static class ClientPostEffectsManager
    {
        private static readonly List<BasePostEffect> ActivePostEffects = new List<BasePostEffect>();

        private static readonly IClientStorage SessionStorage;

        private static bool isPostEffectsEnabled;

        private static bool isPostEffectsRenderingNow;

        static ClientPostEffectsManager()
        {
            SessionStorage = Api.Client.Storage.GetSessionStorage(
                $"{nameof(ClientPostEffectsManager)}.{nameof(IsPostEffectsEnabled)}");
            if (!SessionStorage.TryLoad(out isPostEffectsEnabled)
                && !Api.IsEditor)
            {
                // post-effects are enabled by default in non-Editor
                isPostEffectsEnabled = true;
            }
        }

        public static event Action IsPostEffectsEnabledChanged;

        public static bool IsPostEffectsEnabled
        {
            get => isPostEffectsEnabled;
            set
            {
                if (isPostEffectsEnabled == value)
                {
                    return;
                }

                isPostEffectsEnabled = value;
                Api.Logger.Important($"Post-effects is {(value ? "enabled" : "disabled")} now");
                SessionStorage.Save(isPostEffectsEnabled);
                IsPostEffectsEnabledChanged?.Invoke();
            }
        }

        public static TPostEffect Add<TPostEffect>()
            where TPostEffect : BasePostEffect, new()
        {
            var effect = new TPostEffect();
            effect.SetupManager(
                callbackRegister: () => Register(effect),
                callbackUnregister: () => Unregister(effect));
            effect.IsEnabled = true;
            return effect;
        }

        public static TPostEffect Get<TPostEffect>()
            where TPostEffect : BasePostEffect, new()
        {
            foreach (var effect in ActivePostEffects)
            {
                if (effect is TPostEffect found)
                {
                    return found;
                }
            }

            return null;
        }

        public static TPostEffect GetOrAdd<TPostEffect>()
            where TPostEffect : BasePostEffect, new()
        {
            return Get<TPostEffect>()
                   ?? Add<TPostEffect>();
        }

        public static void Initialize()
        {
            Api.ValidateIsClient();
            Api.Client.Rendering.PostEffectsRendering += PostEffectsRenderingHandler;
        }

        private static void PostEffectsRenderingHandler()
        {
            var hasAtLeastOnePostEffect = false;
            if (IsPostEffectsEnabled)
            {
                // check if there is at least one post-effect which could be rendered
                foreach (var effect in ActivePostEffects)
                {
                    if (effect.IsCanRender)
                    {
                        hasAtLeastOnePostEffect = true;
                        break;
                    }
                }
            }

            if (!hasAtLeastOnePostEffect)
            {
                return;
            }

            var device = Api.Client.Rendering.GraphicsDevice;
            var source = device.ComposerFramebuffer;

            try
            {
                isPostEffectsRenderingNow = true;

                using (var tempDestination = Api.Client.Rendering.GetTempRenderTexture(
                    source.Width,
                    source.Height))
                {
                    // we will swap these render targets during post-effects rendering
                    var rtA = source;
                    var rtB = (IRenderTarget2D)tempDestination;

                    for (var index = 0; index < ActivePostEffects.Count; index++)
                    {
                        var effect = ActivePostEffects[index];
                        try
                        {
                            if (!effect.IsCanRender)
                            {
                                continue;
                            }

                            effect.Render(rtA, rtB);
                            // swap for the next post-effect
                            var temp = rtA;
                            rtA = rtB;
                            rtB = temp;
                        }
                        catch (Exception ex)
                        {
                            ActivePostEffects.Remove(effect);
                            Api.Logger.Exception(ex, "Problem with post-effect rendering: " + effect);
                        }
                    }

                    if (rtA == tempDestination)
                    {
                        // we have rtA as destination and rtB as source
                        // need to blit result to source
                        device.SetRenderTarget(source);
                        device.Blit(tempDestination, blendState: BlendMode.Opaque);
                    }
                }
            }
            finally
            {
                isPostEffectsRenderingNow = false;
            }
        }

        private static void Register(BasePostEffect postEffect)
        {
            if (ActivePostEffects.Contains(postEffect))
            {
                throw new Exception("Already registered post effect: " + postEffect);
            }

            ActivePostEffects.Add(postEffect);
            SortPostEffects();
        }

        private static void SortPostEffects()
        {
            ActivePostEffects.SortBy(e => (ushort)e.Order);
        }

        private static void Unregister(BasePostEffect postEffect)
        {
            if (isPostEffectsRenderingNow)
            {
                throw new Exception("Post-effect "
                                    + postEffect
                                    + " cannot be removed right now as the game currently rendering the post effects!");
            }

            if (!ActivePostEffects.Remove(postEffect))
            {
                throw new Exception("Don't have registered post effect: " + postEffect);
            }

            SortPostEffects();
        }
    }
}