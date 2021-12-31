namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.SkinsBrowser.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowEditorSkinsBrowser : BaseViewModel
    {
        public ViewModelWindowEditorSkinsBrowser(Action closeCallback)
        {
            this.CommandClose = new ActionCommand(closeCallback);

            var list = ClientContainerSortHelper.SortItemPrototypes(
                                                    Api.FindProtoEntities<IProtoItem>().ToList())
                                                .Cast<IProtoItemWithSkinData>()
                                                .ToList()
                                                .Where(i => i.IsSkin 
                                                            || i.IsSkinnable && i.Skins.Count > 0)
                                                .Select(i => new ViewItemSkinWithIcon(i))
                                                .ToList();
            list.SortBy(g => (ushort)g.ProtoItem.SkinId);
            var grouped = list.GroupBy(g => g.ProtoItem.BaseProtoItem ?? g.ProtoItem).ToList();
            grouped.SortBy(g => (ushort)g.FirstOrDefault(g => g.ProtoItem.IsSkin).ProtoItem.SkinId);
            this.AllItemsList = grouped.SelectMany(g => g).ToList();
        }

        public List<ViewItemSkinWithIcon> AllItemsList { get; }

        public BaseCommand CommandClose { get; }

        public readonly struct ViewItemSkinWithIcon
        {
            public ViewItemSkinWithIcon(IProtoItemWithSkinData protoItem)
            {
                this.ProtoItem = protoItem;
            }

            public string BaseItemName => this.ProtoItem.IsSkin
                                              ? this.ProtoItem.BaseProtoItem.Name
                                              : this.ProtoItem.Name;

            public Brush Icon => Api.Client.UI.GetTextureBrush(this.ProtoItem?.Icon);

            public IProtoItemWithSkinData ProtoItem { get; }

            public string SkinName => this.ProtoItem.IsSkin
                                          ? this.ProtoItem.Name
                                          : CoreStrings.Skin_Default;
        }
    }
}