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

            switch (data)
            {
                case CircleShapeRemoteData circleData:
                    return new CircleShape(
                        circleData.Center,
                        circleData.Radius,
                        collisionGroup);

                case RectangleShapeRemoteData rectangleData:
                    return new RectangleShape(
                        rectangleData.Position,
                        rectangleData.Size,
                        collisionGroup);

                case LineShapeRemoteData lineData:
                    return new LineShape(
                        lineData.BasePosition,
                        lineData.Direction,
                        collisionGroup);

                case LineSegmentShapeRemoteData lineSegmentData:
                    return new LineSegmentShape(
                        lineSegmentData.Point1,
                        lineSegmentData.Point2,
                        collisionGroup);

                case PointShapeRemoteData pointData:
                    return new PointShape(
                        pointData.Point,
                        collisionGroup);

                default:
                    throw new ArgumentOutOfRangeException("Unknown shape type: " + data.GetType().Name);
            }
        }

        public static BasePhysicsShapeRemoteData Wrap(IPhysicsShape shape)
        {
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                    return new CircleShapeRemoteData((CircleShape)shape);

                case ShapeType.Rectangle:
                    return new RectangleShapeRemoteData((RectangleShape)shape);

                case ShapeType.Line:
                    return new LineShapeRemoteData((LineShape)shape);

                case ShapeType.LineSegment:
                    return new LineSegmentShapeRemoteData((LineSegmentShape)shape);

                case ShapeType.Point:
                    return new PointShapeRemoteData((PointShape)shape);

                default:
                    throw new ArgumentOutOfRangeException("Unknown shape type: " + shape.GetType().Name);
            }
        }
    }
}