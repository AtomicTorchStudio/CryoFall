namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.ItemsBrowser.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowEditorItemsBrowserWindow : BaseViewModel
    {
        public ViewModelWindowEditorItemsBrowserWindow(Action closeCallback)
        {
            this.CommandClose = new ActionCommand(closeCallback);

            this.AllItemsList = ClientContainerSortHelper.SortItemPrototypes(
                                                             Api.FindProtoEntities<IProtoItem>().ToList())
                                                         .ToList()
                                                         .Where(i => i.Icon != null)
                                                         .Select(i => new ViewItemWithIcon(i))
                                                         .ToList();

            this.SelectedProtoItemViewModel = this.AllItemsList.FirstOrDefault();
        }

        public List<ViewItemWithIcon> AllItemsList { get; }

        public BaseCommand CommandClose { get; }

        public BaseCommand CommandSpawnSelectedItem
            => new ActionCommand(() =>
                                 {
                                     var protoItem = this.SelectedProtoItemViewModel.ProtoItem;
                                     var command = "/addItem " + protoItem.ShortId;
                                     if (protoItem.IsStackable)
                                     {
                                         command += " " + protoItem.MaxItemsPerStack;
                                     }

                                     ConsoleCommandsSystem.SharedExecuteConsoleCommand(command);
                                 });

        public ViewItemWithIcon SelectedProtoItemViewModel { get; set; }
    }
}