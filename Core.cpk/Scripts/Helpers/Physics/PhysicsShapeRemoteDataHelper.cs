namespace AtomicTorch.CBND.CoreMod.Helpers.Physics
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;

    public static class PhysicsShapeRemoteDataHelper
    {
        public static IPhysicsShape Unwrap(BasePhysicsShapeRemoteData data)
        {
            var collisionGroup = CollisionGroups.GetCollisionGroup(data.CollisionGroupId);

            return data switch
            {
                CircleShapeRemoteData circleData
                    => new CircleShape(circleData.Center,
                                       circleData.Radius,
                                       collisionGroup),
                RectangleShapeRemoteData rectangleData
                    => new RectangleShape(rectangleData.Position,
                                          rectangleData.Size,
                                          collisionGroup),

                LineShapeRemoteData lineData
                    => new LineShape(lineData.BasePosition,
                                     lineData.Direction,
                                     collisionGroup),

                LineSegmentShapeRemoteData lineSegmentData
                    => new LineSegmentShape(lineSegmentData.Point1,
                                            lineSegmentData.Point2,
                                            collisionGroup),

                PointShapeRemoteData pointData
                    => new PointShape(pointData.Point,
                                      collisionGroup),

                _ => throw new ArgumentOutOfRangeException("Unknown shape type: " + data.GetType().Name)
            };
        }

        public static BasePhysicsShapeRemoteData Wrap(IPhysicsShape shape)
        {
            return shape.ShapeType switch
            {
                ShapeType.Circle => new CircleShapeRemoteData((CircleShape)shape),
                ShapeType.Rectangle => new RectangleShapeRemoteData((RectangleShape)shape),
                ShapeType.Line => new LineShapeRemoteData((LineShape)shape),
                ShapeType.LineSegment => new LineSegmentShapeRemoteData((LineSegmentShape)shape),
                ShapeType.Point => new PointShapeRemoteData((PointShape)shape),
                _ => throw new ArgumentOutOfRangeException("Unknown shape type: " + shape.GetType().Name)
            };
        }
    }
}