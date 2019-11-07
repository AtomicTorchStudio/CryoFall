namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.FeaturesSlideshow.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelFeaturesSlideshowEntry : BaseViewModel
    {
        public ViewModelFeaturesSlideshowEntry(
            string title,
            string description,
            ITextureResource textureResource,
            bool isAutoDisposeFields = true) : base(isAutoDisposeFields)
        {
            this.Title = title;
            this.Description = description;
            this.Image = Api.Client.UI.GetTextureBrush(textureResource);
        }

        public string Description { get; }

        public Brush Image { get; }

        public string Title { get; }
    }
}