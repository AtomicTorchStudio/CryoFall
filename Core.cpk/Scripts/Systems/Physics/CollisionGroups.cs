namespace AtomicTorch.CBND.CoreMod.Systems.Physics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Data.Physics;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class CollisionGroups
    {
        private static CollisionGroup characterInteractionArea;

        private static CollisionGroup characterOrVehicle;

        private static CollisionGroup clickArea;

        private static CollisionGroup defaultGroup;

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

        public static CollisionGroup CharacterOrVehicle
        {
            get
            {
                InitializeIfNeeded();
                return characterOrVehicle;
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
            return collisionGroupId switch
            {
                CollisionGroupId.Default => defaultGroup,
                CollisionGroupId.HitboxMelee => hitboxMelee,
                CollisionGroupId.HitboxRanged => hitboxRanged,
                CollisionGroupId.ClickArea => clickArea,
                CollisionGroupId.InteractionArea => characterInteractionArea,
                _ => throw new ArgumentOutOfRangeException(nameof(collisionGroupId), collisionGroupId, null)
            };
        }

        public static CollisionGroupId GetCollisionGroupId(CollisionGroup collisionGroup)
        {
            InitializeIfNeeded();
            if (collisionGroup is null
                || ReferenceEquals(collisionGroup, defaultGroup)
                || ReferenceEquals(collisionGroup, characterOrVehicle))
            {
                return CollisionGroupId.Default;
            }

            if (ReferenceEquals(collisionGroup, hitboxMelee))
            {
                return CollisionGroupId.HitboxMelee;
            }

            if (ReferenceEquals(collisionGroup, hitboxRanged))
            {
                return CollisionGroupId.HitboxRanged;
            }

            if (ReferenceEquals(collisionGroup, clickArea))
            {
                return CollisionGroupId.ClickArea;
            }

            if (ReferenceEquals(collisionGroup, characterInteractionArea))
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

            if (PveSystem.SharedIsPve(true))
            {
                characterOrVehicle = new CollisionGroup("PvE character/vehicle", isSensor: false);
                //characterOrVehicle.SetCollidesWith(defaultGroup);
                defaultGroup.SetCollidesWith(characterOrVehicle);
                characterOrVehicle.SetCollidesWith(defaultGroup);
                // It doesn't collide with self!
                // In PvE, player players could pass through each other and vehicles.
                //characterOrVehicle.SetCollidesWith(characterOrVehicle);
            }
            else
            {
                characterOrVehicle = defaultGroup;
            }

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