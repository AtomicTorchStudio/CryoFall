namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.GameApi.Resources;

    internal static class MuzzleFlashAtlases
    {
        /// <summary>
        /// Used for laser weapons.
        /// </summary>
        public static readonly TextureAtlasResource AtlasLaser
            = new TextureAtlasResource(
                "FX/MuzzleFlash/AtlasLaser",
                columns: 6,
                rows: 2,
                isTransparent: true);

        /// <summary>
        /// Used for modern military firearms such as machine pistols and rifles.
        /// </summary>
        public static readonly TextureAtlasResource AtlasNoSmokeLarge
            = new TextureAtlasResource(
                "FX/MuzzleFlash/AtlasNoSmokeLarge",
                columns: 6,
                rows: 2,
                isTransparent: true);

        /// <summary>
        /// Used for modern small arms such as handgun.
        /// </summary>
        public static readonly TextureAtlasResource AtlasNoSmokeSmall
            = new TextureAtlasResource(
                "FX/MuzzleFlash/AtlasNoSmokeSmall",
                columns: 6,
                rows: 2,
                isTransparent: true);

        /// <summary>
        /// Used for modern firearms with wider bore diameter, such as shotguns.
        /// </summary>
        public static readonly TextureAtlasResource AtlasNoSmokeWide
            = new TextureAtlasResource(
                "FX/MuzzleFlash/AtlasNoSmokeWide",
                columns: 6,
                rows: 2,
                isTransparent: true);

        /// <summary>
        /// Used for plasma weapons.
        /// </summary>
        public static readonly TextureAtlasResource AtlasPlasma
            = new TextureAtlasResource(
                "FX/MuzzleFlash/AtlasPlasma",
                columns: 6,
                rows: 2,
                isTransparent: true);

        /// <summary>
        /// Used for primitive rifles such as musket.
        /// </summary>
        public static readonly TextureAtlasResource AtlasSmokeLarge
            = new TextureAtlasResource(
                "FX/MuzzleFlash/AtlasSmokeLarge",
                columns: 6,
                rows: 2,
                isTransparent: true);

        /// <summary>
        /// Used for primitive pistols such as flintlock pistol.
        /// </summary>
        public static readonly TextureAtlasResource AtlasSmokeSmall1
            = new TextureAtlasResource(
                "FX/MuzzleFlash/AtlasSmokeSmall1",
                columns: 6,
                rows: 2,
                isTransparent: true);

        /// <summary>
        /// Used for black powder pistols such as luger and revolver.
        /// </summary>
        public static readonly TextureAtlasResource AtlasSmokeSmall2
            = new TextureAtlasResource(
                "FX/MuzzleFlash/AtlasSmokeSmall2",
                columns: 6,
                rows: 2,
                isTransparent: true);
    }
}