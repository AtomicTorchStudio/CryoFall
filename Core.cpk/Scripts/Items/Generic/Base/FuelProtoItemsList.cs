namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class FuelProtoItemsList : List<IProtoItemFuel>
    {
        public void Add<TFuelType>()
            where TFuelType : IProtoItemFuel, new()
        {
            this.Add(Api.GetProtoEntity<TFuelType>());
        }

        public void AddAll<TFuelType>()
            where TFuelType : class, IProtoItemFuel
        {
            var protos = Api.FindProtoEntities<TFuelType>();
            if (protos.Count == 0)
            {
                throw new Exception("Cannot find proto classes implementing " + typeof(TFuelType).Name);
            }

            this.AddRange(protos);
        }
    }
}