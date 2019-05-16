namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BootstrapperClientPostEffects : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            ClientPostEffectsManager.Initialize();

            //ClientPostEffectsManager.Add<BloomPostEffect>()
            //                        .Setup(BloomSettings.PresetSettings[0]);
            //
            //var blur = ClientPostEffectsManager.Add<BlurPostEffect>();
            //blur.RenderTextureDownsampling = 1;
            //blur.BlurAmountVertical = blur.BlurAmountHorizontal = 1;
            //
            //ClientPostEffectsManager.Add<PostEffectRadiation>()
            //                        .Intensity = 1;

            Client.Scene
                  .CreateSceneObject(nameof(ClientTimeOfDayVisualComponent))
                  .AddComponent<ClientTimeOfDayVisualComponent>()
                  .Initialize();
        }
    }
}