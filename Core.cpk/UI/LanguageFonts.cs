namespace AtomicTorch.CBND.CoreMod.UI
{
    using AtomicTorch.CBND.GameApi;

    public static class LanguageFonts
    {
        // this is a font name, do not translate it
        public const string FontCondensed =
            "/UI/Fonts/#Roboto Condensed, /UI/Fonts/#Source Han Sans, /UI/Fonts/#Twemoji Mozilla";

        // this is a font name, do not translate it
        public const string FontDefault = "/UI/Fonts/#Roboto, /UI/Fonts/#Source Han Sans, /UI/Fonts/#Twemoji Mozilla";

        public const string FontDigits = FontDigitsBase + ", " + FontCondensed;

        // this is a font name, do not translate it
        public const string FontPlayerDialogue =
            "/UI/Fonts/#Amatic SC, /UI/Fonts/#Source Han Sans, /UI/Fonts/#Twemoji Mozilla";

        [NotLocalizable]
        private const string FontDigitsBase = "/UI/Fonts/#Oswald";
    }
}