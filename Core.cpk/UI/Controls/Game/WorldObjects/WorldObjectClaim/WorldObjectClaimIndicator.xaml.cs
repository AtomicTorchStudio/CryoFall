namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.WorldObjectClaim
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldObjectClaimIndicator : BaseUserControl
    {
        public WorldObjectClaimIndicator(IWorldObject taggedWorldObject)
        {
            this.TaggedWorldObject = taggedWorldObject;
        }

        public IWorldObject TaggedWorldObject { get; }

        public static IComponentAttachedControl AttachTo(IWorldObject taggedWorldObject)
        {
            return Api.Client.UI.AttachControl(
                taggedWorldObject,
                new WorldObjectClaimIndicator(taggedWorldObject),
                taggedWorldObject.ProtoWorldObject.SharedGetObjectCenterWorldOffset(
                    taggedWorldObject),
                isFocusable: false);
        }
    }
}