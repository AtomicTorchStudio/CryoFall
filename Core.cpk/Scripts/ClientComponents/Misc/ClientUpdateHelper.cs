// ReSharper disable once CheckNamespace

namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public static class ClientUpdateHelper
    {
        static ClientUpdateHelper()
        {
            Api.Client.Scene
               .CreateSceneObject("Update helper")
               .AddComponent<Component>();
        }

        public static event Action UpdateCallback;

        private class Component : ClientComponent
        {
            public override void Update(double deltaTime)
            {
                Api.SafeInvoke(UpdateCallback);
            }
        }
    }
}