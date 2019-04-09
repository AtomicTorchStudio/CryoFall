namespace AtomicTorch.CBND.CoreMod.ClientComponents.Core
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentUpdateHelper : ClientComponent
    {
        private static ClientComponentUpdateHelper instance;

        public static event Action UpdateCallback;

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            UpdateCallback?.Invoke();
        }

        protected override void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        protected override void OnEnable()
        {
            instance = this;
        }
    }
}