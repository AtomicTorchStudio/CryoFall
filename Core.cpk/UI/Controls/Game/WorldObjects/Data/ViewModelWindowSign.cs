namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowSign : BaseViewModel
    {
        private readonly IStaticWorldObject worldObjectSign;

        public ViewModelWindowSign(
            IStaticWorldObject worldObjectSign)
        {
            this.worldObjectSign = worldObjectSign;
            var publicState = worldObjectSign.GetPublicState<ObjectSignPublicState>();

            publicState.ClientSubscribe(
                _ => _.Text,
                newText => this.SignText = newText,
                this);

            this.SignText = publicState.Text;
        }

        public Action CloseWindowCallback { get; set; }

        public BaseCommand CommandSave => new ActionCommand(this.ExecuteCommandSave);

        public string SignText { get; set; }

        private void ExecuteCommandSave()
        {
            var protoSign = (IProtoObjectSign)this.worldObjectSign.ProtoStaticWorldObject;
            protoSign.ClientSetSignText(this.worldObjectSign, this.SignText);
            this.CloseWindowCallback.Invoke();
        }
    }
}