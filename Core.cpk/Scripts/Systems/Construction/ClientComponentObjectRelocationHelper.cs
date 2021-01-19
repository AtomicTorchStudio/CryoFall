namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// This component will draw an overlay over the building that is selected for relocation.
    /// </summary>
    public class ClientComponentObjectRelocationHelper : ClientComponent
    {
        private static readonly Lazy<RenderingMaterial> RenderingMaterial = new(
            () =>
            {
                var material = GameApi.ServicesClient.Components.RenderingMaterial
                                      .Create(new EffectResource("ConstructionPlacement"));
                material.EffectParameters
                        .Set("ColorAdd",      new Vector4(0.3f, 0.5f, 0.7f, 0))
                        .Set("ColorMultiply", new Vector4(1,    1,    1,    0.5f));
                return material;
            });

        private IClientSceneObject sceneObjectForComponents;

        public void Setup(IStaticWorldObject objectStructure)
        {
            this.sceneObjectForComponents?.Destroy();
            this.sceneObjectForComponents = Client.Scene.CreateSceneObject(
                $"Scene object for {nameof(ClientComponentObjectRelocationHelper)} components");

            this.sceneObjectForComponents
                .AddComponent<SceneObjectPositionSynchronizer>()
                .Setup(objectStructure.ClientSceneObject);

            // use blueprint renderer to render the overlay
            var overlay = new ClientBlueprintRenderer(this.sceneObjectForComponents,
                                                      isConstructionSite: false);

            objectStructure.ProtoStaticWorldObject.ClientSetupBlueprint(
                objectStructure.OccupiedTile,
                overlay);

            overlay.SpriteRenderer.RenderingMaterial = RenderingMaterial.Value;

            // remove all attached UI components that were added during blueprint setup
            // (remove the unnecessary placement guides)
            var sceneObject = overlay.SceneObject;
            var components = sceneObject.FindComponents<IComponentAttachedControl>();
            foreach (var component in components)
            {
                sceneObject.DestroyComponent(component);
            }
        }

        protected override void OnDisable()
        {
            this.sceneObjectForComponents?.Destroy();
            this.sceneObjectForComponents = null;
        }
    }
}