namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public static class ProtoVehicleHelper
    {
        private static List<IProtoVehicle> allVehicles;

        public static IReadOnlyList<IProtoVehicle> AllVehicles
        {
            get
            {
                if (allVehicles == null)
                {
                    var list = Api.FindProtoEntities<IProtoVehicle>();
                    list.RemoveAll(r => r.ListedInTechNodes.Count == 0);
                    list.SortBy(r => r.Id);
                    allVehicles = list;
                }

                return allVehicles;
            }
        }
    }
}