namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class SharedProjectileDirectionHelper
    {
        /// <summary>
        /// Determines the direction a character needs to follow to intercept the target character
        /// based on the current target velocity and the current character expected velocity.
        /// </summary>
        public static Vector2D ServerCalculateInterceptDirectionToTargetCharacter(
            ICharacter currentCharacter,
            ICharacter targetCharacter,
            double currentCharacterExpectedSpeed)
        {
            var positionShooter = currentCharacter.Position;
            var positionTarget = targetCharacter.Position;
            var velocityTarget = ServerGetTargetVelocity(targetCharacter);
            return ServerCalculateInterceptDirectionToTargetCharacter(positionShooter,
                                                                      positionTarget,
                                                                      velocityTarget,
                                                                      currentCharacterExpectedSpeed);
        }

        public static Vector2D ServerCalculateInterceptDirectionToTargetCharacter(
            Vector2D positionShooter,
            Vector2D positionTarget,
            Vector2D velocityTarget,
            double projectileExpectedSpeed)
        {
            if (SharedCalculateInterceptPosition(positionShooter,
                                                 positionTarget,
                                                 velocityTarget,
                                                 projectileExpectedSpeed,
                                                 out var predictedTargetPosition))
            {
                return (predictedTargetPosition - positionShooter).Normalized;
            }

            // no solution - return just the direction to the target
            return (positionTarget - positionShooter).Normalized;
        }

        public static Vector2D ServerGetTargetVelocity(ICharacter targetCharacter)
        {
            var targetCharacterPhysicsBody = targetCharacter.PhysicsBody;
            if (targetCharacter.ProtoCharacter is PlayerCharacter
                && targetCharacter.SharedGetCurrentVehicle() is { } targetCharacterCurrentVehicle)
            {
                targetCharacterPhysicsBody = targetCharacterCurrentVehicle.PhysicsBody;
            }

            var targetVelocity = targetCharacterPhysicsBody.Velocity;
            return targetVelocity;
        }

        /// <summary>
        /// Based on https://github.com/NonlinearIdeas/NonRotatingShooter
        /// Article https://www.gamedev.net/tutorials/_/technical/math-and-physics/shooting-at-stuff-r3884/
        /// Basically, it's solving a quadratic equation to determine the result target position (interception position).
        /// </summary>
        public static bool SharedCalculateInterceptPosition(
            Vector2D positionShooter,
            Vector2D positionTarget,
            Vector2D velocityTarget,
            double speedProjectile,
            out Vector2D solution)
        {
            // This formulation uses the quadratic equation to solve
            // the intercept position.
            Vector2D R = positionTarget - positionShooter;
            double a = velocityTarget.X * velocityTarget.X
                       + velocityTarget.Y * velocityTarget.Y
                       - speedProjectile * speedProjectile;
            double b = 2 * (R.X * velocityTarget.X + R.Y * velocityTarget.Y);
            double c = R.X * R.X + R.Y * R.Y;
            double tBullet = 0;

            // If the target and the shooter have already collided, don't bother.
            if (R.LengthSquared < 2 * double.Epsilon)
            {
                solution = default;
                return false;
            }

            // If the squared velocity of the target and the bullet are the same, the equation
            // collapses to tBullet*b = -c.  If they are REALLY close to each other (float tol),
            // you could get some weirdness here.  Do some "is it close" checking?
            if (Math.Abs(a) < 2 * double.Epsilon)
            {
                // If the b value is 0, we can't get a solution.
                if (Math.Abs(b) < 2 * double.Epsilon)
                {
                    solution = default;
                    return false;
                }

                tBullet = -c / b;
            }
            else
            {
                // Calculate the discriminant to figure out how many solutions there are.
                double discriminant = b * b - 4 * a * c;
                if (discriminant < 0)
                {
                    // All solutions are complex.
                    solution = default;
                    return false;
                }

                if (discriminant > 0)
                {
                    // Two solutions.  Pick the smaller positive one.
                    // Calculate the quadratic.
                    double quad = Math.Sqrt(discriminant);
                    double tBullet1 = (-b + quad) / (2 * a);
                    double tBullet2 = (-b - quad) / (2 * a);
                    if ((tBullet1 < 0)
                        && (tBullet2 < 0))
                    {
                        // This would be really odd.
                        // Both times are negative.
                        solution = default;
                        return false;
                    }
                    else if (tBullet2 < 0
                             && tBullet1 >= 0)
                    {
                        // One negative, one positive.
                        tBullet = tBullet1;
                    }
                    else if (tBullet1 < 0
                             && tBullet2 >= 0)
                    {
                        // One negative, one positive.
                        tBullet = tBullet2;
                    }
                    else if (tBullet1 < tBullet2)
                    {
                        // First less than second
                        tBullet = tBullet1;
                    }
                    else
                    {
                        // Only choice left
                        tBullet = tBullet2;
                    }
                }
                else
                {
                    tBullet = -b / (2 * a);
                }
            }

            // If the time is negative, we can't get there from here.
            if (tBullet < 0)
            {
                solution = default;
                return false;
            }

            // Calculate the intercept position.
            solution = positionTarget + tBullet * velocityTarget;
            return true;
        }
    }
}