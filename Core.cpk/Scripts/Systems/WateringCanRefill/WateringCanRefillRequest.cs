namespace AtomicTorch.CBND.CoreMod.Systems.WateringCanRefill
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class WateringCanRefillRequest : ItemWorldActionRequest
    {
        public WateringCanRefillRequest(
            ICharacter character,
            IWorldObject worldObject,
            IItem item,
            IReadOnlyList<IItem> itemsToConsumeForRefill)
            : base(character, worldObject, item)
        {
            this.ItemsToConsumeForRefill = itemsToConsumeForRefill;
        }

        public IReadOnlyList<IItem> ItemsToConsumeForRefill { get; }

        public override bool Equals(IActionRequest other)
        {
            if (!base.Equals(other))
            {
                return false;
            }

            var otherThisType = (WateringCanRefillRequest)other;
            if (this.ItemsToConsumeForRefill == null
                && otherThisType.ItemsToConsumeForRefill == null)
            {
                return true;
            }

            if (this.ItemsToConsumeForRefill != null
                && otherThisType.ItemsToConsumeForRefill != null)
            {
                return this.ItemsToConsumeForRefill
                           .SequenceEqual(otherThisType.ItemsToConsumeForRefill);
            }

            return false;
        }
    }
}