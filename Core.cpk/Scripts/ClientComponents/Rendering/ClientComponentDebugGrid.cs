namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentDebugGrid : ClientComponent
    {
        private static ClientComponentDebugGrid instance;

        private bool isDrawing;

        public static event Action IsDrawingChanged;

        public static ClientComponentDebugGrid Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Client.Scene.CreateSceneObject("Debug Grid")
                                     .AddComponent<ClientComponentDebugGrid>();
                }

                return instance;
            }
        }

        public bool IsDrawing
        {
            get => this.isDrawing;
            set
            {
                if (this.isDrawing == value)
                {
                    return;
                }

                Client.Rendering.IsDebugGridEnabled = this.isDrawing = value;

                IsDrawingChanged?.Invoke();
            }
        }

        public void Refresh()
        {
            // do not remove this method - it's used to create an instance during bootstrapping
        }

        public override void Update(double deltaTime)
        {
            this.IsDrawing = Client.Rendering.IsDebugGridEnabled;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.isDrawing = Client.Rendering.IsDebugGridEnabled;
        }
    }
}