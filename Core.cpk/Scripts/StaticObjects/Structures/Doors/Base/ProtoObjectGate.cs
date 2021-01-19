namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectGate
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectDoor
            <TPrivateState,
                TPublicState,
                TClientState>
        where TPrivateState : ObjectDoorPrivateState, new()
        where TPublicState : ObjectDoorPublicState, new()
        where TClientState : ObjectDoorClientState, new()
    {
        public override int DoorSizeTiles => 2;

        public override bool IsHeavyVehicleCanPass => true;

        public sealed override bool? IsHorizontalDoorOnly => this.IsHorizontalGate;

        protected abstract bool IsHorizontalGate { get; }

        protected sealed override void CreateLayout(StaticObjectLayout layout)
        {
            if (this.IsHorizontalGate)
            {
                layout.Setup("##");
            }
            else
            {
                layout.Setup("#",
                             "#");
            }
        }

        // the gates are double size so we need to increase the check bounds
        protected override void PrepareOpeningBounds(out BoundsDouble horizontal, out BoundsDouble vertical)
        {
            var distance = 1.6;
            var distanceLarge = distance + 0.5;
            horizontal = new BoundsDouble(minX: -distanceLarge,
                                          minY: -distance,
                                          maxX: distanceLarge,
                                          maxY: distance);

            vertical = new BoundsDouble(minX: -distance,
                                        minY: -distanceLarge,
                                        maxX: distance,
                                        maxY: distanceLarge);
        }

        protected override BoundsDouble SharedGetDoorOpeningBounds(IStaticWorldObject worldObject)
        {
            var isHorizontalDoor = GetPublicState(worldObject).IsHorizontalDoor;
            var objectOpeningBounds = this.SharedGetDoorOpenBounds(isHorizontalDoor);
            var offset = isHorizontalDoor
                             ? new Vector2D(1.0, 0.1)
                             : new Vector2D(0.5, 1.0);

            var tilePosition = worldObject.TilePosition;
            var boundsOffset = objectOpeningBounds.Offset;
            return new BoundsDouble(
                new Vector2D(
                    boundsOffset.X + offset.X + tilePosition.X,
                    boundsOffset.Y + offset.Y + tilePosition.Y),
                objectOpeningBounds.Size);
        }
    }

    public abstract class ProtoObjectGate
        : ProtoObjectGate<
            ObjectDoorPrivateState,
            ObjectDoorPublicState,
            ObjectDoorClientState>
    {
    }
}