namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class SharedCharacterFaceStylesProvider
    {
        public const string HairColorRootFolderPath = ContentPaths.Textures + "Characters/HairColors/";

        public const string SkinTonesRootFolderPath = ContentPaths.Textures + "Characters/SkinTones/";

        private const string FacesRootFolderPath = ContentPaths.Textures + "Characters/Faces/";

        private const string HairRootFolderPath = ContentPaths.Textures + "Characters/Hair/";

        private static readonly Dictionary<string, SharedCharacterFaceStylesProvider> Providers =
            new Dictionary<string, SharedCharacterFaceStylesProvider>();

        private readonly string facesFolderPath;

        private readonly string hairFolderPath;

        private IReadOnlyDictionary<string, FaceFolder> allFaceFolders;

        private IList<string> allHairColorFiles;

        private IList<string> allHairFolders;

        private IList<string> allSkinToneFiles;

        private bool isInitialized;

        private SharedCharacterFaceStylesProvider(string subpath)
        {
            this.facesFolderPath = FacesRootFolderPath + subpath;
            this.hairFolderPath = HairRootFolderPath + subpath;
        }

        public string FacesFolderPath => this.facesFolderPath;

        public string HairFolderPath => this.hairFolderPath;

        public static SharedCharacterFaceStylesProvider GetForGender(bool isMale)
        {
            var subpath = "Human/" + (isMale ? "Male" : "Female") + "/";
            if (Providers.TryGetValue(subpath, out var provider))
            {
                return provider;
            }

            Providers[subpath] = provider = new SharedCharacterFaceStylesProvider(subpath);
            return provider;
        }

        public static string GetSkinToneFilePath(string skinToneId)
        {
            return SkinTonesRootFolderPath + $"{skinToneId}" + ".png";
        }

        public CharacterHumanFaceStyle GenerateRandomFace()
        {
            this.EnsureInitialized();
            var face = this.allFaceFolders.Values.TakeByRandom();
            var topId = face.TopIds.TakeByRandom();
            var bottomId = face.BottomIds.TakeByRandom();
            var hairStyle = this.allHairFolders.TakeByRandom();
            var skinToneId = this.allSkinToneFiles.TakeByRandom();
            var hairColorId = this.allHairColorFiles.TakeByRandom();

            return new CharacterHumanFaceStyle(face.Id,
                                               topId,
                                               bottomId,
                                               hairStyle,
                                               skinToneId,
                                               hairColorId);
        }

        public FaceFolder GetFace(string faceId)
        {
            this.EnsureInitialized();
            return this.allFaceFolders[faceId];
        }

        public FaceFolder GetFace(ushort faceIndex)
        {
            this.EnsureInitialized();
            return this.allFaceFolders.Values.FirstOrDefault(f => f.Index == faceIndex);
        }

        public ushort GetFacesCount()
        {
            this.EnsureInitialized();
            return (ushort)this.allFaceFolders.Count;
        }

        public string GetHairColorByIndex(ushort skinToneId)
        {
            this.EnsureInitialized();
            return this.allHairColorFiles[skinToneId];
        }

        public ushort GetHairColorCount()
        {
            this.EnsureInitialized();
            return (ushort)this.allHairColorFiles.Count;
        }

        public ushort GetHairColorIndex(string skinToneName)
        {
            this.EnsureInitialized();
            return (ushort)this.allHairColorFiles.IndexOf(skinToneName);
        }

        public ushort GetHairCount()
        {
            this.EnsureInitialized();
            return (ushort)this.allHairFolders.Count;
        }

        public string GetHairStyleByIndex(ushort hairId)
        {
            this.EnsureInitialized();
            return this.allHairFolders[hairId];
        }

        public ushort GetHairStyleIndex(string hairStyleName)
        {
            this.EnsureInitialized();
            return (ushort)this.allHairFolders.IndexOf(hairStyleName);
        }

        public string GetSkinToneByIndex(ushort skinToneId)
        {
            this.EnsureInitialized();
            return this.allSkinToneFiles[skinToneId];
        }

        public ushort GetSkinToneCount()
        {
            this.EnsureInitialized();
            return (ushort)this.allSkinToneFiles.Count;
        }

        public ushort GetSkinToneIndex(string skinToneName)
        {
            this.EnsureInitialized();
            return (ushort)this.allSkinToneFiles.IndexOf(skinToneName);
        }

        private void EnsureInitialized()
        {
            if (this.isInitialized)
            {
                return;
            }

            this.isInitialized = true;

            var sharedApi = Api.Shared;
            this.allHairFolders = sharedApi.GetFolderNamesInFolder(this.hairFolderPath).OrderBy(f => f).ToList();
            this.allHairFolders.Insert(0, null);

            var allFacesFolders = sharedApi.GetFolderNamesInFolder(this.facesFolderPath).OrderBy(f => f).ToList();
            if (allFacesFolders.Count == 0)
            {
                throw new Exception("There must be at least one face variant at " + this.facesFolderPath);
            }

            {
                using var tempList = sharedApi.GetFilePathsInFolder(SkinTonesRootFolderPath,
                                                                    includeSubfolders: false,
                                                                    withoutExtensions: true,
                                                                    stripFolderPathFromFilePaths: true);
                this.allSkinToneFiles = tempList.AsList().ToList();
                this.allSkinToneFiles.Insert(0, null);
            }

            {
                using var tempList = sharedApi.GetFilePathsInFolder(HairColorRootFolderPath,
                                                                    includeSubfolders: false,
                                                                    withoutExtensions: true,
                                                                    stripFolderPathFromFilePaths: true);
                this.allHairColorFiles = tempList.AsList().ToList();
                this.allHairColorFiles.Insert(0, null);
            }

            var allFacePresets = new Dictionary<string, FaceFolder>();
            for (var index = 0; index < allFacesFolders.Count; index++)
            {
                var faceFolderName = allFacesFolders[index];
                var faceFolderPath = this.facesFolderPath + faceFolderName;

                using var availableFiles = sharedApi.GetFilePathsInFolder(
                    faceFolderPath,
                    includeSubfolders: false,
                    stripFolderPathFromFilePaths: true,
                    withoutExtensions: true);

                // TODO: rewrite to avoid LINQ
                var topIds = availableFiles.AsList()
                                           .Where(_ => _.StartsWith("FrontTop"))
                                           .Select(_ => _.Substring("FrontTop".Length))
                                           .ToArray();

                var bottomIds = availableFiles.AsList()
                                              .Where(_ => _.StartsWith("FrontBottom"))
                                              .Select(_ => _.Substring("FrontBottom".Length))
                                              .ToArray();

                if (topIds.Length == 0)
                {
                    throw new Exception("There are must be at least one top face part at " + faceFolderPath);
                }

                if (bottomIds.Length == 0)
                {
                    throw new Exception("There are must be at least one bottom face path " + faceFolderPath);
                }

                allFacePresets[faceFolderName] = new FaceFolder(faceFolderName, (ushort)index, topIds, bottomIds);
            }

            this.allFaceFolders = allFacePresets;
        }

        public class FaceFolder
        {
            public readonly string[] BottomIds;

            public readonly string Id;

            public readonly ushort Index;

            public readonly string[] TopIds;

            public FaceFolder(string id, ushort index, string[] topIds, string[] bottomIds)
            {
                this.Id = id;
                this.Index = index;
                this.TopIds = topIds;
                this.BottomIds = bottomIds;
            }
        }
    }
}