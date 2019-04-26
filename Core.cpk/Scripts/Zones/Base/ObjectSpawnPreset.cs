namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ObjectSpawnPreset
    {
        private const int MaxPadding = 2 * ProtoZoneSpawnScript.SpawnZoneAreaSize;

        public static readonly ObjectSpawnPreset Empty = new ObjectSpawnPreset();

        private ListDictionary<ObjectSpawnPreset, double> customObjectPadding;

        private ArrayWithWeights<IProtoSpawnableObject> frozenArray;

        private IReadOnlyDictionary<ObjectSpawnPreset, double> frozenCustomObjectPadding;

        private List<ValueWithWeight<IProtoSpawnableObject>> list
            = new List<ValueWithWeight<IProtoSpawnableObject>>(capacity: 1);

        /// <summary>
        /// Please do not use this directly! Use SpawnList.CreatePreset() instead.
        /// </summary>
        internal ObjectSpawnPreset(
            double interval,
            double padding,
            double iterationLimitMinFraction,
            double iterationLimitMaxFraction,
            bool isRestrictionPreset,
            bool useSectorDensity = true,
            int paddingToLandClaimAreas = 0)
        {
            this.PresetUseSectorDensity = useSectorDensity;
            this.PaddingToLandClaimAreas = paddingToLandClaimAreas;
            ValidateArgs(isRestrictionPreset, interval, padding, iterationLimitMinFraction, iterationLimitMaxFraction);

            if (!isRestrictionPreset)
            {
                this.Density = 1d / (interval * interval);
            }
            else
            {
                this.Density = 0;
            }

            this.Padding = padding;

            this.IterationLimitFractionRange = new RangeDouble(
                iterationLimitMinFraction,
                iterationLimitMaxFraction);
            this.customObjectPadding = new ListDictionary<ObjectSpawnPreset, double>();
        }

        // this empty construction is used only for the singleton Empty preset
        private ObjectSpawnPreset()
        {
        }

        public Func<IPhysicsSpace, Vector2Ushort, bool> CustomCanSpawnCheckCallback { get; private set; }

        public IReadOnlyDictionary<ObjectSpawnPreset, double> CustomObjectPadding
        {
            get
            {
                if (this.frozenCustomObjectPadding != null)
                {
                    return this.frozenCustomObjectPadding;
                }

                this.VerifyIsFrozen();
                return null;
            }
        }

        public double Density { get; }

        public bool IsContainsOnlyStaticObjects { get; private set; }

        public RangeDouble IterationLimitFractionRange { get; }

        public double Padding { get; }

        public int PaddingToLandClaimAreas { get; }

        public bool PresetUseSectorDensity { get; }

        //public bool UseSectorDensity { get; }

        /// <summary>
        /// Includes/adds a proto entity (or entities) to the spawn preset.
        /// </summary>
        /// <typeparam name="TProtoSpawnableObject">
        /// Specify an entity class or an interface. It must be inherited from
        /// <see cref="IProtoSpawnableObject" /> (all items and static objects are inherited from it).
        /// </typeparam>
        /// <param name="weight">
        /// (Optional) defines weight in this preset. Higher value means a higher chance of spawning relative to other objects in
        /// this preset.
        /// </param>
        /// <returns>This instance (chainable).</returns>
        public ObjectSpawnPreset Add<TProtoSpawnableObject>(double weight = 1)
            where TProtoSpawnableObject : class, IProtoSpawnableObject
        {
            if (weight <= 0)
            {
                throw new Exception("Frequency must be >= 0");
            }

            var objectTypes = Api.FindProtoEntities<TProtoSpawnableObject>();
            if (objectTypes.Count == 0)
            {
                throw new Exception("No entities found for: " + typeof(TProtoSpawnableObject).Name);
            }

            this.list.AddRange(
                objectTypes
                    .Cast<IProtoSpawnableObject>()
                    .Select(spawnableObject
                                => new ValueWithWeight<IProtoSpawnableObject>(spawnableObject,
                                                                              weight)));
            return this;
        }

        /// <summary>
        /// Includes/adds an exact proto entity instance to the spawn preset. Inheritors of this proto entity type will be not
        /// included.
        /// </summary>
        /// <typeparam name="TProtoSpawnableObject">
        /// Specify an exact entity class.
        /// </typeparam>
        /// <param name="weight">
        /// (Optional) defines weight in this preset. Higher value means a higher chance of spawning relative to other objects in
        /// this preset.
        /// </param>
        /// <returns>This instance (chainable).</returns>
        public ObjectSpawnPreset AddExact<TProtoSpawnableObject>(double weight = 1)
            where TProtoSpawnableObject : class, IProtoSpawnableObject, new()
        {
            if (weight <= 0)
            {
                throw new Exception("Frequency must be >= 0");
            }

            var protoEntity = Api.GetProtoEntity<TProtoSpawnableObject>();

            this.list.Add(new ValueWithWeight<IProtoSpawnableObject>(protoEntity, weight));
            return this;
        }

        public bool Contains(IProtoSpawnableObject protoStaticObject)
        {
            this.MakeReadOnly();
            return this.frozenArray.Contains(protoStaticObject,
                                             EqualityComparer<IProtoSpawnableObject>.Default);
        }

        /// <summary>
        /// Get random object proto from this spawn preset.
        /// </summary>
        public IProtoSpawnableObject GetRandomObjectProto()
        {
            try
            {
                return this.frozenArray.GetSingleRandomElement();
            }
            catch
            {
                this.VerifyIsFrozen();
                return null;
            }
        }

        public void MakeReadOnly()
        {
            if (this.frozenArray != null)
            {
                // already made frozen
                return;
            }

            var count = this.list.Count;
            if (count == 0)
            {
                throw new Exception("There is a spawn list without entries defined");
            }

            // let's calculate frozen array - see frozenArray description
            this.frozenArray = new ArrayWithWeights<IProtoSpawnableObject>(this.list);
            this.list = null;

            this.frozenCustomObjectPadding = this.customObjectPadding.ToReadOnlyListDictionary();
            this.customObjectPadding = null;

            var isContainsOnlyStaticObjects = true;
            foreach (var entry in this.frozenArray)
            {
                if (!(entry.Value is IProtoStaticWorldObject))
                {
                    isContainsOnlyStaticObjects = false;
                }
            }

            this.IsContainsOnlyStaticObjects = isContainsOnlyStaticObjects;
        }

        public string PrintEntries()
        {
            this.VerifyIsFrozen();

            var sb = new StringBuilder("{");

            var isNotFirst = false;
            foreach (var entry in this.frozenArray)
            {
                if (isNotFirst)
                {
                    sb.Append(", ");
                }
                else
                {
                    isNotFirst = true;
                }

                sb.AppendFormat(
                    "{0} (P={1:0.#}%)",
                    entry.Value.GetType().Name,
                    entry.Probability * 100);
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// Exclude added proto entities which are implementing the specified type.
        /// </summary>
        /// <typeparam name="TProtoSpawnableObject">Type of proto spawnable object (can be interface or base class) to exclude.</typeparam>
        /// <returns>This instance (chainable).</returns>
        public ObjectSpawnPreset Remove<TProtoSpawnableObject>()
            where TProtoSpawnableObject : IProtoSpawnableObject
        {
            if (this.list == null)
            {
                throw new Exception("Cannot exclude types - there is only one type added in the preset.");
            }

            this.list.RemoveAll(_ => _.Value is TProtoSpawnableObject);
            if (this.list.Count == 0)
            {
                throw new Exception("Objects list at spawn preset become empty after Exclude() call");
            }

            return this;
        }

        public ObjectSpawnPreset SetCustomCanSpawnCheckCallback(
            Func<IPhysicsSpace, Vector2Ushort, bool> customCanSpawnCheckCallback)
        {
            this.VerifyIsNotFrozen();
            this.CustomCanSpawnCheckCallback = customCanSpawnCheckCallback;
            return this;
        }

        /// <summary>
        /// Set custom padding (minimal distance between objects) with another preset. Applied in both directions, so no need to do
        /// the same for another preset.
        /// </summary>
        /// <param name="preset">Another spawn preset.</param>
        /// <param name="padding">Defines the minimal distance to the objects of this and another spawn presets.</param>
        /// <returns>This instance (chainable).</returns>
        public ObjectSpawnPreset SetCustomPaddingWith(ObjectSpawnPreset preset, double padding)
        {
            this.InternalSetCustomMinSpawnDistance(preset, padding, recursive: true);
            return this;
        }

        /// <summary>
        /// Set custom padding (minimal distance between objects) for objects of this preset.
        /// </summary>
        /// <param name="padding">Defines the minimal distance between the object types of this spawn preset.</param>
        /// <returns>This instance (chainable).</returns>
        public ObjectSpawnPreset SetCustomPaddingWithSelf(double padding)
        {
            return this.SetCustomPaddingWith(this, padding);
        }

        public override string ToString()
        {
            return "Spawn preset: " + this.PrintEntries();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateArgs(
            bool isRestrictionPreset,
            double interval,
            double padding,
            double iterationLimitMinFraction,
            double iterationLimitMaxFraction)
        {
            if (isRestrictionPreset)
            {
                return;
            }

            if (interval <= 0)
            {
                throw new ArgumentException("interval must be > 0", nameof(interval));
            }

            if (padding <= 0)
            {
                throw new ArgumentException("padding must be > 0", nameof(padding));
            }

            if (iterationLimitMinFraction > iterationLimitMaxFraction)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(iterationLimitMinFraction),
                    "Min fraction value must be <= max value");
            }

            if (iterationLimitMinFraction < 0
                || iterationLimitMinFraction > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(iterationLimitMinFraction), "Value must be in 0-1 range");
            }

            if (iterationLimitMaxFraction < 0
                || iterationLimitMaxFraction > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(iterationLimitMaxFraction), "Value must be in 0-1 range");
            }

            ValidatePadding(padding);
        }

        private static void ValidatePadding(double padding)
        {
            if (padding >= MaxPadding)
            {
                Api.Logger.Error(
                    $"Padding with another object must not exceed {MaxPadding}. Padding value in the spawn list is defined as: {padding}");
            }
        }

        private void InternalSetCustomMinSpawnDistance(ObjectSpawnPreset preset, double padding, bool recursive)
        {
            ValidatePadding(padding);

            if (this.customObjectPadding.TryGetValue(preset, out var storedDistance)
                && storedDistance == padding)
            {
                // already registered
                return;
            }

            this.customObjectPadding[preset] = padding;

            if (recursive)
            {
                preset.InternalSetCustomMinSpawnDistance(this, padding, recursive: false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void VerifyIsFrozen()
        {
            if (this.frozenArray == null)
            {
                throw new Exception("Object spawn preset must be frozen in order to use this method");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void VerifyIsNotFrozen()
        {
            if (this.frozenArray != null)
            {
                throw new Exception("Object spawn preset must be NOT frozen in order to use this method");
            }
        }
    }
}