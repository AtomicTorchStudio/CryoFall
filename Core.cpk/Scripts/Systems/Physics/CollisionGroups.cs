namespace AtomicTorch.CBND.CoreMod.Systems.Physics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Data.Physics;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class CollisionGroups
    {
        private static CollisionGroup characterInteractionArea;

        private static CollisionGroup clickArea;

        private static CollisionGroup defaultGroup;

        private static CollisionGroup defaultWithCollisionToInteractionArea;

        private static CollisionGroup hitboxMelee;

        private static CollisionGroup hitboxRanged;

        private static bool isInitialized;

        public static CollisionGroup CharacterInteractionArea
        {
            get
            {
                InitializeIfNeeded();
                return characterInteractionArea;
            }
        }

        public static CollisionGroup ClickArea
        {
            get
            {
                InitializeIfNeeded();
                return clickArea;
            }
        }

        public static CollisionGroup Default
        {
            get
            {
                InitializeIfNeeded();
                return defaultGroup;
            }
        }

        /// <summary>
        /// Special group for collision detection between interaction areas and default collision group.
        /// </summary>
        public static CollisionGroup DefaultWithCollisionToInteractionArea
        {
            get
            {
                InitializeIfNeeded();
                return defaultWithCollisionToInteractionArea;
            }
        }

        public static CollisionGroup HitboxMelee
        {
            get
            {
                InitializeIfNeeded();
                return hitboxMelee;
            }
        }

        public static CollisionGroup HitboxRanged
        {
            get
            {
                InitializeIfNeeded();
                return hitboxRanged;
            }
        }

        public static CollisionGroup GetCollisionGroup(CollisionGroupId collisionGroupId)
        {
            InitializeIfNeeded();
            switch (collisionGroupId)
            {
                case CollisionGroupId.Default:
                    return defaultGroup;
                case CollisionGroupId.HitboxMelee:
                    return hitboxMelee;
                case CollisionGroupId.HitboxRanged:
                    return hitboxRanged;
                case CollisionGroupId.ClickArea:
                    return clickArea;
                case CollisionGroupId.InteractionArea:
                    return characterInteractionArea;
                default:
                    throw new ArgumentOutOfRangeException(nameof(collisionGroupId), collisionGroupId, null);
            }
        }

        public static CollisionGroupId GetCollisionGroupId(CollisionGroup collisionGroup)
        {
            InitializeIfNeeded();
            if (collisionGroup == null
                || collisionGroup == defaultGroup)
            {
                return CollisionGroupId.Default;
            }

            if (collisionGroup == hitboxMelee)
            {
                return CollisionGroupId.HitboxMelee;
            }

            if (collisionGroup == hitboxRanged)
            {
                return CollisionGroupId.HitboxRanged;
            }

            if (collisionGroup == clickArea)
            {
                return CollisionGroupId.ClickArea;
            }

            if (collisionGroup == characterInteractionArea)
            {
                return CollisionGroupId.InteractionArea;
            }

            throw new ArgumentOutOfRangeException(nameof(collisionGroup), collisionGroup, null);
        }

        private static void Initialize()
        {
            if (isInitialized)
            {
                throw new Exception("Already initialized");
            }

            isInitialized = true;

            defaultGroup = CollisionGroup.GetDefault();
            defaultGroup.SetCollidesWith(defaultGroup);

            hitboxMelee = new CollisionGroup("Hitbox Melee", isSensor: true);
            //hitboxMelee.SetCollidesWith(defaultGroup);
            hitboxMelee.SetCollidesWith(HitboxMelee);

            hitboxRanged = new CollisionGroup("Hitbox Ranged", isSensor: true);
            //hitboxRanged.SetCollidesWith(defaultGroup);
            hitboxRanged.SetCollidesWith(HitboxRanged);

            characterInteractionArea = new CollisionGroup("Interaction Area", isSensor: true);

            clickArea = new CollisionGroup("Click Area", isSensor: true);
            clickArea.SetCollidesWith(clickArea);
            clickArea.SetCollidesWith(characterInteractionArea);

            defaultWithCollisionToInteractionArea = new CollisionGroup(
                "Default with collision with the interaction area",
                isSensor: true);
            defaultWithCollisionToInteractionArea.SetCollidesWith(characterInteractionArea);
            defaultWithCollisionToInteractionArea.SetCollidesWith(defaultGroup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InitializeIfNeeded()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }
    }
}