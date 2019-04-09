namespace AtomicTorch.CBND.CoreMod.ClientComponents.AmbientSound
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.DayNightSystem;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentAmbientSoundManager : ClientComponent
    {
        private const int AmbientSoundsScanRadius = SoundConstants.AmbientMaxDistance;

        private readonly Dictionary<AmbientSoundPreset, double> dictionarySoundsByDistanceSqr
            = new Dictionary<AmbientSoundPreset, double>();

        private readonly Dictionary<AmbientSoundPreset, ComponentAmbientSoundEmitter> emitters
            = new Dictionary<AmbientSoundPreset, ComponentAmbientSoundEmitter>();

        private readonly List<AmbientSoundPresetByDistance> tempList
            = new List<AmbientSoundPresetByDistance>();

        private ICharacter character;

        public ComponentAmbientSoundManager()
            : base(isLateUpdateEnabled: true)
        {
        }

        public override void LateUpdate(double deltaTime)
        {
            base.LateUpdate(deltaTime);

            var startTile = this.character.TilePosition;
            var clientWorld = Api.Client.World;
            var isDay = DayNightSystem.IsDay;

            this.dictionarySoundsByDistanceSqr.Clear();

            for (var x = -AmbientSoundsScanRadius; x <= AmbientSoundsScanRadius; x++)
            for (var y = -AmbientSoundsScanRadius; y <= AmbientSoundsScanRadius; y++)
            {
                var tX = startTile.X + x;
                var tY = startTile.Y + y;
                if (tX < 0
                    || tY < 0)
                {
                    continue;
                }

                var tilePosition = new Vector2Ushort((ushort)tX, (ushort)tY);
                var tile = clientWorld.GetTile(tilePosition, logOutOfBounds: false);
                if (!tile.IsValidTile)
                {
                    continue;
                }

                var protoTile = (ProtoTile)tile.ProtoTile;
                foreach (var ambientSoundPreset in protoTile.AmbientSoundProvider.ClientGetAmbientSound(tile, isDay))
                {
                    var distanceSqr = startTile.TileSqrDistanceTo(tilePosition);
                    if (!this.dictionarySoundsByDistanceSqr.TryGetValue(ambientSoundPreset, out var existingDistanceSqr)
                        || existingDistanceSqr > distanceSqr)
                    {
                        this.dictionarySoundsByDistanceSqr[ambientSoundPreset] = distanceSqr;
                    }
                }
            }

            // remove emitters which are no longer required
            this.emitters.ProcessAndRemoveByPair(
                pair =>
                {
                    if (this.dictionarySoundsByDistanceSqr.ContainsKey(pair.Key))
                    {
                        // don't remove
                        return false;
                    }

                    var emitter = pair.Value;
                    emitter.SetTargetVolume(0);
                    var needRemove = emitter.CurrentInterpolatedVolume <= 0.001;
                    return needRemove;
                },
                toRemove => toRemove.Value.Destroy());

            if (this.tempList.Count > 0)
            {
                this.tempList.Clear();
            }

            foreach (var pair in this.dictionarySoundsByDistanceSqr)
            {
                this.tempList.Add(new AmbientSoundPresetByDistance(pair.Key, pair.Value));
            }

            this.tempList.Sort(AmbientSoundPresetByDistance.Comparer);

            // add or update the emitters
            var suppression = 0.0;
            foreach (var pair in this.tempList)
            {
                var ambientSoundPreset = pair.AmbientSoundPreset;
                if (!this.emitters.TryGetValue(ambientSoundPreset, out var existingEmitter))
                {
                    existingEmitter = this.SceneObject.AddComponent<ComponentAmbientSoundEmitter>();
                    existingEmitter.Setup(ambientSoundPreset.SoundResource);
                    this.emitters[ambientSoundPreset] = existingEmitter;
                }

                var distance = Math.Sqrt(pair.DistanceSqr);
                var distanceCoef = distance / SoundConstants.AmbientMaxDistance;
                distanceCoef = 1 - MathHelper.Clamp(distanceCoef, 0, 1);

                var volume = distanceCoef * (1 - suppression);
                existingEmitter.SetTargetVolume(volume);

                suppression = Math.Max(suppression, distanceCoef * ambientSoundPreset.Suppression);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.dictionarySoundsByDistanceSqr.Clear();

            foreach (var emitter in this.emitters.Values)
            {
                emitter.Destroy();
            }

            this.emitters.Clear();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.character = Client.Characters.CurrentPlayerCharacter;
        }

        private struct AmbientSoundPresetByDistance : IComparable<AmbientSoundPresetByDistance>, IComparable
        {
            public static readonly Comparer<AmbientSoundPresetByDistance> Comparer
                = Comparer<AmbientSoundPresetByDistance>.Default;

            public readonly AmbientSoundPreset AmbientSoundPreset;

            public readonly double DistanceSqr;

            public AmbientSoundPresetByDistance(AmbientSoundPreset ambientSoundPreset, double distanceSqr)
            {
                this.AmbientSoundPreset = ambientSoundPreset;
                this.DistanceSqr = distanceSqr;
            }

            public static bool operator >(AmbientSoundPresetByDistance left, AmbientSoundPresetByDistance right)
            {
                return left.CompareTo(right) > 0;
            }

            public static bool operator >=(AmbientSoundPresetByDistance left, AmbientSoundPresetByDistance right)
            {
                return left.CompareTo(right) >= 0;
            }

            public static bool operator <(AmbientSoundPresetByDistance left, AmbientSoundPresetByDistance right)
            {
                return left.CompareTo(right) < 0;
            }

            public static bool operator <=(AmbientSoundPresetByDistance left, AmbientSoundPresetByDistance right)
            {
                return left.CompareTo(right) <= 0;
            }

            public int CompareTo(AmbientSoundPresetByDistance other)
            {
                return this.DistanceSqr.CompareTo(other.DistanceSqr);
            }

            public int CompareTo(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return 1;
                }

                if (!(obj is AmbientSoundPresetByDistance))
                {
                    throw new ArgumentException($"Object must be of type {nameof(AmbientSoundPresetByDistance)}");
                }

                return this.CompareTo((AmbientSoundPresetByDistance)obj);
            }
        }
    }
}