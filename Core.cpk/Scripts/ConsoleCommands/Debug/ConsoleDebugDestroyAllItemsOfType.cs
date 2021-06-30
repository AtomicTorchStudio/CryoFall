namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleDebugDestroyAllItemsOfType : BaseConsoleCommand
    {
        public override string Description =>
            "Finds and destroy all items of specified type from all containers and inventories.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.destroyItemsEverywhere";

        public string Execute(IProtoItem protoItem)
        {
            using var tempListContainers = Api.Shared.GetTempList<IItemsContainer>();
            using var tempListItems = Api.Shared.GetTempList<IItem>();
            var containers = tempListContainers.AsList();
            var items = tempListItems.AsList();
            ulong totalDestroyedCount = 0;

            foreach (var protoItemsContainer in Api.FindProtoEntities<IProtoItemsContainer>())
            {
                containers.Clear();
                protoItemsContainer.GetAllGameObjects(containers);

                foreach (var container in containers)
                {
                    items.Clear();
                    items.AddRange(container.Items);

                    foreach (var item in items)
                    {
                        if (item.ProtoItem != protoItem)
                        {
                            continue;
                        }

                        totalDestroyedCount += item.Count;
                        Server.Items.DestroyItem(item);
                    }
                }
            }

            return $"Destroyed {totalDestroyedCount} item(s).";
        }
    }
}