namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    /// <summary>
    /// Guides are the lines you see in the world on the cell borders.
    /// </summary>
    public class ClientDebugGuidesManager
    {
        private readonly IClientStorage sessionStorage;

        private bool isGuidesEnabled;

        private ClientDebugGuidesManager()
        {
            this.sessionStorage = Api.Client.Storage.GetSessionStorage(
                $"{nameof(ClientDebugGuidesManager)}.{nameof(this.IsGuidesEnabled)}");
            this.sessionStorage.TryLoad(out this.isGuidesEnabled);
        }

        public static event Action IsDrawingChanged;

        public static ClientDebugGuidesManager Instance { get; } = new();

        public bool IsGuidesEnabled
        {
            get => this.isGuidesEnabled;
            set
            {
                if (this.isGuidesEnabled == value)
                {
                    return;
                }

                this.isGuidesEnabled = value;
                this.sessionStorage.Save(this.isGuidesEnabled);
                ClientComponentGuidesRenderer.Refresh();
                IsDrawingChanged?.Invoke();
            }
        }

        public void Refresh()
        {
            ClientComponentGuidesRenderer.Refresh();
        }
    }
}