namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs
{
    using System;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectBombResonance : ProtoObjectExplosive
    {
        // damage radius to dynamic objects (like characters)
        private const double DamageRadiusDynamicObjectsOnly = 2.1;

        // damage radius to deliver full damage to static objects
        private const int DamageRadiusFullDamage = 4;

        // max damage radius to deliver damage to static objects
        // (see method ServerCalculateDamageCoefByDistance)
        private const int DamageRadiusMax = 7;

        protected static readonly Lazy<SolidColorBrush> ExplosionBlueprintBorderBrushRed
            = new(() => new SolidColorBrush(Color.FromArgb(0x99, 0xEE, 0x00, 0x00)));

        protected static readonly Lazy<SolidColorBrush> ExplosionBlueprintBorderBrushYellow
            = new(() => new SolidColorBrush(Color.FromArgb(0x99, 0xEE, 0x99, 0x00)));

        protected static readonly Lazy<SolidColorBrush> ExplosionBlueprintFillBrushRed
            = new(() => new SolidColorBrush(Color.FromArgb(0x22, 0xEE, 0x00, 0x00)));

        protected static readonly Lazy<SolidColorBrush> ExplosionBlueprintFillBrushYellow
            = new(() => new SolidColorBrush(Color.FromArgb(0x22, 0xEE, 0x99, 0x00)));

        public override double DamageRadius => DamageRadiusMax;

        public override bool IsActivatesRaidModeForLandClaim => true;

        public override string Name => "Resonance bomb";

        public override double ServerCalculateDamageCoefByDistanceForDynamicObjects(double distance)
        {
            var distanceThreshold = 1;
            if (distance <= distanceThreshold)
            {
                return 1;
            }

            distance -= distanceThreshold;
            distance = Math.Max(0, distance);

            var maxDistance = DamageRadiusDynamicObjectsOnly;
            maxDistance -= distanceThreshold;
            maxDistance = Math.Max(0, maxDistance);

            return 1 - Math.Min(distance / maxDistance, 1);
        }

        public override double ServerCalculateDamageCoefByDistanceForStaticObjects(double distance)
        {
            var tileDistance = (int)distance;
            if (tileDistance <= DamageRadiusFullDamage)
            {
                return 1; // full damage
            }

            switch (tileDistance)
            {
                case 5:
                    return 0.7; // 70%
                case 6:
                    return 0.5; // 50%;
                case 7:
                    return 0.3; // 30%;

                // no damage beyond this point
                default:
                    return 0;
            }
        }

        public override void ServerExecuteExplosion(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            WeaponFinalCache weaponFinalCache)
        {
            WeaponExplosionSystem.ServerProcessExplosionBomberman(
                positionEpicenter: positionEpicenter,
                physicsSpace: physicsSpace,
                damageDistanceFullDamage: DamageRadiusFullDamage,
                damageDistanceMax: DamageRadiusMax,
                damageDistanceDynamicObjectsOnly: DamageRadiusDynamicObjectsOnly,
                weaponFinalCache: weaponFinalCache,
                callbackCalculateDamageCoefByDistanceForStaticObjects:
                this.ServerCalculateDamageCoefByDistanceForStaticObjects,
                callbackCalculateDamageCoefByDistanceForDynamicObjects:
                this.ServerCalculateDamageCoefByDistanceForDynamicObjects);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            data.ClientState.Renderer.DrawOrderOffsetY = 0.355;
        }

        protected override void ClientOnObjectDestroyed(Vector2D position)
        {
            //base.ClientOnObjectDestroyed(position);
            Logger.Important(this + " exploded at " + position);

            var explosionPresetNode = ExplosionPresets.PragmiumResonanceBomb_NodeClientOnly;
            var positionEpicenter = position + this.Layout.Center;
            ProcessExplosionDirection(-1, 0);  // left
            ProcessExplosionDirection(0,  1);  // top
            ProcessExplosionDirection(1,  0);  // right
            ProcessExplosionDirection(0,  -1); // bottom

            SharedExplosionHelper.ClientExplode(position: position + this.Layout.Center,
                                                this.ExplosionPreset,
                                                this.VolumeExplosion);

            void ProcessExplosionDirection(int xOffset, int yOffset)
            {
                foreach (var (_, offsetIndex) in
                    WeaponExplosionSystem.SharedEnumerateExplosionBombermanDirectionTilesWithTargets(
                        positionEpicenter: positionEpicenter,
                        damageDistanceFullDamage: DamageRadiusFullDamage,
                        damageDistanceMax: DamageRadiusMax,
                        Api.Client.World,
                        xOffset,
                        yOffset))
                {
                    ClientTimersSystem.AddAction(
                        delaySeconds: 0.1 * offsetIndex, // please note the offsetIndex is starting with 1
                        () => SharedExplosionHelper.ClientExplode(
                            position: positionEpicenter + (offsetIndex * xOffset, offsetIndex * yOffset),
                            explosionPresetNode,
                            volume: 0));
                }
            }
        }

        protected override void ClientSetupExplosionBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            ProcessExplosionDirection(-1, 0);  // left
            ProcessExplosionDirection(0,  1);  // top
            ProcessExplosionDirection(1,  0);  // right
            ProcessExplosionDirection(0,  -1); // bottom

            void ProcessExplosionDirection(int xOffset, int yOffset)
            {
                foreach (var (_, offsetIndex) in
                    WeaponExplosionSystem.SharedEnumerateExplosionBombermanDirectionTilesWithTargets(
                        positionEpicenter: tile.Position.ToVector2D(),
                        damageDistanceFullDamage: DamageRadiusFullDamage,
                        damageDistanceMax: DamageRadiusMax,
                        Api.Client.World,
                        xOffset,
                        yOffset))
                {
                    var rectangle = new Rectangle()
                    {
                        Width = 1 * ScriptingConstants.TileSizeVirtualPixels - 10,
                        Height = 1 * ScriptingConstants.TileSizeVirtualPixels - 10,
                        StrokeThickness = 4
                    };

                    if (offsetIndex <= DamageRadiusFullDamage)
                    {
                        rectangle.Stroke = ExplosionBlueprintBorderBrushRed.Value;
                        rectangle.Fill = ExplosionBlueprintFillBrushRed.Value;
                    }
                    else
                    {
                        switch (offsetIndex)
                        {
                            case 5:
                            case 6:
                            case 7:
                                rectangle.Stroke = ExplosionBlueprintBorderBrushYellow.Value;
                                rectangle.Fill = ExplosionBlueprintFillBrushYellow.Value;
                                break;

                            default:
                                throw new Exception("Should be impossible");
                        }
                    }

                    // workaround for NoesisGUI
                    rectangle.SetValue(Shape.StrokeDashArrayProperty, "2,1");

                    Api.Client.UI.AttachControl(
                        blueprint.SceneObject,
                        positionOffset: this.Layout.Center
                                        + (offsetIndex * xOffset,
                                           offsetIndex * yOffset),
                        uiElement: rectangle,
                        isFocusable: false,
                        isScaleWithCameraZoom: true);
                }
            }
        }

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 100;
            armorPiercingCoef = 0.5;
            finalDamageMultiplier = 1;
            damageDistribution.Set(DamageType.Kinetic, 1);
        }

        protected override void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            // PLEASE NOTE: while the damage is the same as the Modern bomb (T3)
            // it has a totally different damage propagation algorithm.
            // Resonance bombs explode like in bomberman in a cross-shaped pattern (damaging only walls and doors).
            // Four closest tiles to the bomb are damaged with the 100% damage, then if there is no free space
            // it could continue propagating the damage up to 7 tiles total (but the damage is reduced for every
            // next tile after the fourth). It's extremely effective against multilayered walls.
            damageValue = 12000;
            defencePenetrationCoef = 0.5;
        }

        protected override void PrepareProtoObjectExplosive(out ExplosionPreset explosionPresets)
        {
            explosionPresets = ExplosionPresets.PragmiumResonanceBomb_Center;
        }
    }
}