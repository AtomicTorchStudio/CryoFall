namespace AtomicTorch.CBND.CoreMod.Systems.ItemFuelRefill
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemFuelRefillRequest : ItemActionRequest
    {
        public readonly IReadOnlyList<IItem> ItemsToConsumeForRefill;

        public ItemFuelRefillRequest(
            ICharacter character,
            IItem item,
            IReadOnlyList<IItem> itemsToConsumeForRefill)
            : base(character, item)
        {
            this.ItemsToConsumeForRefill = itemsToConsumeForRefill;
        }

        public override bool Equals(IActionRequest other)
        {
            return base.Equals(other)
                   && this.ItemsToConsumeForRefill.SequenceEqual(
                       ((ItemFuelRefillRequest)other).ItemsToConsumeForRefill);
        }
    }
}