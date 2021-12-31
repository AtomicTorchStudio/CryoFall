namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowSignPicture : BaseViewModel
    {
        private readonly ObjectSignPublicState publicState;

        private readonly IStaticWorldObject worldObjectSign;

        public ViewModelWindowSignPicture(
            IStaticWorldObject worldObjectSign)
        {
            this.worldObjectSign = worldObjectSign;
            this.publicState = worldObjectSign.GetPublicState<ObjectSignPublicState>();

            this.publicState.ClientSubscribe(
                _ => _.Text,
                newText => this.Refresh(),
                this);

            this.Images = SharedSignPictureHelper.AllImagesFileNames
                                                 .OrderBy(fileName => int.TryParse(fileName, out var result)
                                                                          ? result
                                                                          : 0)
                                                 .Select(f => new SignPictureData(f))
                                                 .ToArray();
            this.Refresh();
        }

        public Action CloseWindowCallback { get; set; }

        public BaseCommand CommandSave => new ActionCommand(this.ExecuteCommandSave);

        public SignPictureData[] Images { get; }

        public SignPictureData SelectedImage { get; set; }

        private void ExecuteCommandSave()
        {
            var protoSign = (IProtoObjectSign)this.worldObjectSign.ProtoStaticWorldObject;
            protoSign.ClientSetSignText(this.worldObjectSign, this.SelectedImage.FilePath);
            this.CloseWindowCallback.Invoke();
        }

        private void Refresh()
        {
            this.SelectedImage = this.Images.FirstOrDefault(
                i => i.FilePath == this.publicState.Text);
        }

        public readonly struct SignPictureData
        {
            public SignPictureData(string filePath)
            {
                this.FilePath = filePath;
            }

            public string FilePath { get; }

            public Brush Icon => Client.UI.GetTextureBrush(
                SharedSignPictureHelper.GetTextureResource(this.FilePath));
        }
    }
}