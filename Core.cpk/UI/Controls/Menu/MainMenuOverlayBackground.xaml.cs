namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu
{
    using System.Windows.Controls;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MainMenuOverlayBackground : BaseUserControl
    {
        private static MainMenuOverlayBackground instance;

        private static bool isHidden = true;

        public MainMenuOverlayBackground()
        {
        }

        public static bool IsHidden
        {
            get => isHidden;
            set
            {
                if (isHidden == value)
                {
                    return;
                }

                isHidden = value;

                if (isHidden)
                {
                    if (instance == null)
                    {
                        return;
                    }

                    Api.Client.UI.LayoutRootChildren.Remove(instance);
                }
                else
                {
                    instance = new MainMenuOverlayBackground();
                    Panel.SetZIndex(instance, -1);
                    Api.Client.UI.LayoutRootChildren.Insert(0, instance);
                }
            }
        }

        protected override void InitControl()
        {
        }
    }
}