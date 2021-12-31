namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    /// <summary>
    /// Technically, all item prototypes are implementing this interface.
    /// </summary>
    public interface IProtoItemWithSkinData : IProtoItem
    {
        IProtoItem BaseProtoItem { get; }

        bool IsModSkin { get; }

        bool IsSkin { get; }

        bool IsSkinnable { get; }

        SkinId SkinId { get; }

        IReadOnlyList<IProtoItem> Skins { get; }

        bool IsSkinOrVariant(IProtoItem protoItem);

        void PrepareProtoItemLinkSkin(IProtoItem protoItem);
    }
}