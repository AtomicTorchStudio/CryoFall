// ReSharper disable ImpureMethodCallOnReadonlyValueField

namespace AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.ColorGrading;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    /// <summary>
    /// This component handles visual change of time on the Client-side.
    /// </summary>
    public class ClientTimeOfDayVisualComponent : ClientComponent
    {
        public const double DayAmbientLightFraction = 1;

        public const double DuskDawnAmbientLightFraction = 0.6;

        private static readonly TextureResource3D TextureResourceColorGradingDay
            = new("FX/ColorGrading/Day", depth: 24, isTransparent: false);

        private static readonly TextureResource3D TextureResourceColorGradingDuskDawn
            = new("FX/ColorGrading/DuskDawn", depth: 24, isTransparent: false);

        private static readonly TextureResource3D TextureResourceColorGradingNight
            = new("FX/ColorGrading/Night", depth: 24, isTransparent: false);

        private ColorGradingPostEffect2LutWithLightmap colorGrading;

        /// <summary>
        /// Returns current fraction of day (0-1, where 1 means it's full day (noon)).
        /// </summary>
        public static double CurrentDayFraction { get; private set; }

        /// <summary>
        /// Returns current fraction of dusk or dawn (0-1, where 1 means it's full dusk or dawn).
        /// </summary>
        public static double CurrentDuskDawnFraction { get; private set; }

        /// <summary>
        /// Returns current fraction of night (0-1, where 1 means it's full night (midnight)).
        /// </summary>
        public static double CurrentNightFraction { get; private set; }

        /// <summary>
        /// Make brighter nights in PvP as it's too easy to workaround anyway
        /// (with monitor adjustments, GPU driver gamma, or a mod to change this value).
        /// </summary>
        public static double NightAmbientCurrentExtraLightFraction
            => PveSystem.ClientIsPve(false)
                   ? 0
                   : 0.15;

        public static double NightAmbientLightFraction
            => 0.15 + NightAmbientCurrentExtraLightFraction;

        public void Initialize()
        {
            this.colorGrading = ClientPostEffectsManager.Add<ColorGradingPostEffect2LutWithLightmap>();
            this.Update(0);
        }

        public override void Update(double deltaTime)
        {
            var timeOfDayHours = TimeOfDaySystem.CurrentTimeOfDayHours;

            var dayFraction = TimeOfDaySystem.IntervalDay.CalculateCurrentFraction(timeOfDayHours);
            var nightFraction = TimeOfDaySystem.IntervalNight.CalculateCurrentFraction(timeOfDayHours);
            var duskDawnFraction = Math.Max(
                TimeOfDaySystem.IntervalDusk.CalculateCurrentFraction(timeOfDayHours),
                TimeOfDaySystem.IntervalDawn.CalculateCurrentFraction(timeOfDayHours));

            if (dayFraction > 0
                && nightFraction > 0)
            {
                throw new Exception("Impossible - day and night fractions are both > 0. They must not overlap");
            }

            //// logging about the game time fractions
            //if (Client.CurrentGame.CurrentServerFrameNumber % 2 == 0)
            //{
            //    Logger.WriteDev(
            //        $"Current game time hours: {timeOfDayHours:F2}"
            //        + $"{Environment.NewLine}Current time-of-day fractions:"
            //        + $"{Environment.NewLine}Dusk/dawn: {duskDawnFraction:F2}"
            //        + $"{Environment.NewLine}Day: {dayFraction:F2}"
            //        + $"{Environment.NewLine}Night: {nightFraction:F2}");
            //}

            // normalize fractions
            var totalFraction = duskDawnFraction + dayFraction + nightFraction;
            dayFraction /= totalFraction;
            nightFraction /= totalFraction;
            duskDawnFraction /= totalFraction;

            ClientComponentLightingRenderer.AmbientLightFraction
                = (float)(duskDawnFraction * DuskDawnAmbientLightFraction
                          + dayFraction * DayAmbientLightFraction
                          + nightFraction * NightAmbientLightFraction);

            CurrentDuskDawnFraction = duskDawnFraction;
            CurrentDayFraction = dayFraction;
            CurrentNightFraction = nightFraction;

            var isDay = dayFraction > 0;
            this.colorGrading.Setup(
                TextureResourceColorGradingDuskDawn,
                isDay ? TextureResourceColorGradingDay : TextureResourceColorGradingNight,
                blendFactor: duskDawnFraction);
        }
    }
}