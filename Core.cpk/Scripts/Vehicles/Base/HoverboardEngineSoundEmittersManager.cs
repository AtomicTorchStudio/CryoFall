namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class HoverboardEngineSoundEmittersManager
    {
        private static readonly Dictionary<uint, ComponentHoverboardEngineSoundEmitter> VehicleSoundEmitters
            = new Dictionary<uint, ComponentHoverboardEngineSoundEmitter>();

        public static ComponentHoverboardEngineSoundEmitter CreateSoundEmitter(
            IDynamicWorldObject vehicle,
            SoundResource engineSoundResource,
            double engineSoundVolume)
        {
            if (VehicleSoundEmitters.TryGetValue(vehicle.Id, out var component))
            {
                component.Setup(vehicle,
                                engineSoundResource,
                                volume: engineSoundVolume);
                return component;
            }

            var sceneObject = Api.Client.Scene.CreateSceneObject(
                $"HoverboardSoundEmitter: {vehicle.ProtoGameObject.Id} Id={vehicle.Id}");
            component = sceneObject.AddComponent<ComponentHoverboardEngineSoundEmitter>();
            component.Setup(vehicle,
                            engineSoundResource,
                            volume: engineSoundVolume);

            VehicleSoundEmitters[vehicle.Id] = component;
            return component;
        }

        public static void OnSoundEmitterComponentVehicleDestroyed(ComponentHoverboardEngineSoundEmitter component)
        {
            if (VehicleSoundEmitters.TryGetValue(component.Vehicle.Id, out var currentComponent)
                && currentComponent == component)
            {
                VehicleSoundEmitters.Remove(component.Vehicle.Id);
            }

            if (!component.SceneObject.IsDestroyed)
            {
                component.SceneObject.Destroy();
            }
        }
    }
}