namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Structures;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class SharedFactionEmblemProvider
    {
        private const string BackgroundFolderPath = ContentPaths.Textures + "FactionEmblems/Background/";

        private const string ForegroundFolderPath = ContentPaths.Textures + "FactionEmblems/Foreground/";

        private const string ShapeMaskFolderPath = ContentPaths.Textures + "FactionEmblems/ShapeMask/";

        public static readonly IReadOnlyCollection<ColorRgb> SupportedColorsBackground
            = new HashSet<ColorRgb>()
            {
                0x7b000f, 0xa03882, 0x7738c2, 0x35119f,
                0x1a2382, 0x43beb7, 0x1f5c5c, 0x337b2d,
                0xaeaeb4, 0x4f4d54, 0x10354f, 0x4f2b10,
                0x784c2b, 0xe2710c, 0xcc3329, 0xc40018
            };

        public static readonly IReadOnlyCollection<ColorRgb> SupportedColorsForeground
            = new HashSet<ColorRgb>()
            {
                0xab485d, 0xc082af, 0xaf87d7, 0x7354b4,
                0x5163b2, 0x90d7d3, 0x558383, 0x71a26d,
                0xfcfbff, 0x848387, 0x517cb2, 0x71573d,
                0x9e836b, 0xe0a45b, 0xdb8079, 0xce395d
            };

        private static IList<string> allBackgroundFiles;

        private static IList<string> allForegroundFiles;

        private static IList<string> allShapeMaskFiles;

        private static bool isInitialized;

        public static FactionEmblem GenerateRandomEmblem()
        {
            EnsureInitialized();

            var foregroundId = allForegroundFiles.TakeByRandom();
            var backgroundId = allBackgroundFiles.TakeByRandom();
            var shapeMaskId = allShapeMaskFiles.TakeByRandom();

            // select two different colors
            ColorRgb colorForeground, colorBackground1, colorBackground2;
            do
            {
                colorForeground = SupportedColorsForeground.TakeByRandom();
                colorBackground1 = SupportedColorsBackground.TakeByRandom();
                colorBackground2 = SupportedColorsBackground.TakeByRandom();
            }
            while (colorForeground.Equals(colorBackground1)
                   || colorForeground.Equals(colorBackground2)
                   || colorBackground1.Equals(colorBackground2));

            return new FactionEmblem(foregroundId: foregroundId,
                                     backgroundId: backgroundId,
                                     shapeMaskId: shapeMaskId,
                                     colorForeground: colorForeground,
                                     colorBackground1: colorBackground1,
                                     colorBackground2: colorBackground2);
        }

        public static ushort GetBackgroundCount()
        {
            EnsureInitialized();
            return (ushort)allBackgroundFiles.Count;
        }

        public static string GetBackgroundFilePath(string backgroundId)
        {
            return BackgroundFolderPath + $"{backgroundId}" + ".png";
        }

        public static string GetBackgroundStyleByIndex(ushort backgroundId)
        {
            EnsureInitialized();
            return allBackgroundFiles[backgroundId];
        }

        public static ushort GetBackgroundStyleIndex(string backgroundStyleName)
        {
            EnsureInitialized();
            return (ushort)allBackgroundFiles.IndexOf(backgroundStyleName);
        }

        public static ushort GetForegroundCount()
        {
            EnsureInitialized();
            return (ushort)allForegroundFiles.Count;
        }

        public static string GetForegroundFilePath(string foregroundId)
        {
            return ForegroundFolderPath + $"{foregroundId}" + ".png";
        }

        public static string GetForegroundStyleByIndex(ushort foregroundId)
        {
            EnsureInitialized();
            return allForegroundFiles[foregroundId];
        }

        public static ushort GetForegroundStyleIndex(string foregroundStyleName)
        {
            EnsureInitialized();
            return (ushort)allForegroundFiles.IndexOf(foregroundStyleName);
        }

        public static ushort GetShapeMaskCount()
        {
            EnsureInitialized();
            return (ushort)allShapeMaskFiles.Count;
        }

        public static string GetShapeMaskFilePath(string shapeMaskId)
        {
            return ShapeMaskFolderPath + $"{shapeMaskId}" + ".png";
        }

        public static string GetShapeMaskStyleByIndex(ushort shapeMaskId)
        {
            EnsureInitialized();
            return allShapeMaskFiles[shapeMaskId];
        }

        public static ushort GetShapeMaskStyleIndex(string shapeMaskStyleName)
        {
            EnsureInitialized();
            return (ushort)allShapeMaskFiles.IndexOf(shapeMaskStyleName);
        }

        public static bool SharedIsValidEmblem(FactionEmblem emblem)
        {
            EnsureInitialized();

            return allForegroundFiles.Contains(emblem.ForegroundId)
                   && allBackgroundFiles.Contains(emblem.BackgroundId)
                   && allShapeMaskFiles.Contains(emblem.ShapeMaskId)
                   && SupportedColorsForeground.Contains(emblem.ColorForeground)
                   && SupportedColorsBackground.Contains(emblem.ColorBackground1)
                   && SupportedColorsBackground.Contains(emblem.ColorBackground2);
        }

        private static void EnsureInitialized()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            var sharedApi = Api.Shared;

            allBackgroundFiles = GetFileNames(BackgroundFolderPath);
            allShapeMaskFiles = GetFileNames(ShapeMaskFolderPath);
            allForegroundFiles = GetFileNames(ForegroundFolderPath);

            List<string> GetFileNames(string folderPath)
            {
                using var tempFilePaths = sharedApi.GetFilePathsInFolder(folderPath,
                                                                         includeSubfolders: false,
                                                                         stripFolderPathFromFilePaths: true,
                                                                         withoutExtensions: true);
                var result = new List<string>(tempFilePaths.AsList());
                if (result.Count == 0)
                {
                    throw new Exception("No faction emblem files found in: " + folderPath);
                }

                result.Sort(StringComparer.Ordinal);
                return result;
            }
        }
    }
}