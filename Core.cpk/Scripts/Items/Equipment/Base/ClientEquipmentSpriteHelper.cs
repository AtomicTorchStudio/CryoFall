namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Extensions;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class ClientEquipmentSpriteHelper
    {
        public static void CollectSlotAttachments(
            List<FilePathsList> sources,
            string typeName,
            bool requireEquipmentTextures,
            out List<SkeletonSlotAttachment> slotAttachmentsMale,
            out List<SkeletonSlotAttachment> slotAttachmentsFemale)
        {
            slotAttachmentsMale = CollectSlotAttachments(
                sources,
                typeName,
                isMale: true,
                requireEquipmentTextures: requireEquipmentTextures);

            slotAttachmentsFemale = CollectSlotAttachments(
                sources,
                typeName,
                isMale: false,
                requireEquipmentTextures: requireEquipmentTextures);

            using var originalSlotAttachmentsFemale = Api.Shared.WrapInTempList(slotAttachmentsFemale);

            // populate missing female attachments from male skeleton
            foreach (var requiredAttachment in slotAttachmentsMale)
            {
                var isFound = false;
                foreach (var existingAttachment in originalSlotAttachmentsFemale)
                {
                    if (existingAttachment.AttachmentName == requiredAttachment.AttachmentName)
                    {
                        isFound = true;
                        break;
                    }
                }

                if (isFound)
                {
                    continue;
                }

                // remove "Male"
                var skeletonName = "Female" + requiredAttachment.SkeletonName.Remove(0, 4);
                slotAttachmentsFemale.Add(
                    new SkeletonSlotAttachment(
                        skeletonName,
                        requiredAttachment.AttachmentName,
                        requiredAttachment.TextureResource));
            }
        }

        public static ITempList<FilePathsList> CollectSpriteFilePaths(List<string> sourcePaths)
        {
            var result = Api.Shared.GetTempList<FilePathsList>();
            var resultList = result.AsList();
            resultList.Capacity = sourcePaths.Count;

            foreach (var path in sourcePaths)
            {
                var sourcePath = path + "/";
                var tempFiles = Api.Shared.GetFilePathsInFolder(
                    ContentPaths.Textures + sourcePath,
                    includeSubfolders: true,
                    stripFolderPathFromFilePaths: true,
                    withoutExtensions: false);

                resultList.Add(new FilePathsList(sourcePath, tempFiles));
            }

            return result;
        }

        private static List<SkeletonSlotAttachment> CollectSlotAttachments(
            List<FilePathsList> sources,
            string typeName,
            bool isMale,
            bool requireEquipmentTextures)
        {
            var result = new List<SkeletonSlotAttachment>();
            var gender = isMale ? "Male" : "Female";
            var skeletonNameFront = gender + "Front";
            var skeletonNameBack = gender + "Back";

            foreach (var pair in sources)
            {
                var fullPath = pair.SourceFolderPath;
                var filePaths = pair.FilesInFolder.AsList();
                if (filePaths.Count == 0)
                {
                    continue;
                }

                // collect unisex uni-view sprites
                ClientSkeletonAttachmentsLoader.CreateSlotAttachmentsForTexturesFolder(
                    fullPath,
                    skeletonName: null,
                    filterSubfolderPath: string.Empty,
                    filePaths: filePaths,
                    slotAttachments: result);

                // collect unisex front-view sprites
                ClientSkeletonAttachmentsLoader.CreateSlotAttachmentsForTexturesFolder(
                    fullPath,
                    skeletonName: skeletonNameFront,
                    filterSubfolderPath: "Front/",
                    filePaths: filePaths,
                    slotAttachments: result);

                // collect gender-specific sprites
                ClientSkeletonAttachmentsLoader.CreateSlotAttachmentsForTexturesFolder(
                    fullPath,
                    skeletonName: skeletonNameFront,
                    filterSubfolderPath: "Front/" + gender + "/",
                    filePaths: filePaths,
                    slotAttachments: result);

                // collect unisex back-view sprites
                ClientSkeletonAttachmentsLoader.CreateSlotAttachmentsForTexturesFolder(
                    fullPath,
                    skeletonName: skeletonNameBack,
                    filterSubfolderPath: "Back/",
                    filePaths: filePaths,
                    slotAttachments: result);

                // collect gender-specific back-view sprites
                ClientSkeletonAttachmentsLoader.CreateSlotAttachmentsForTexturesFolder(
                    fullPath,
                    skeletonName: skeletonNameBack,
                    filterSubfolderPath: "Back/" + gender + "/",
                    filePaths: filePaths,
                    slotAttachments: result);
            }

            CopyAttachmentIfNotAvailable(
                result,
                fromSkeletonName: skeletonNameFront,
                fromAttachmentName: "HandLeftEquipment",
                toSkeletonName: skeletonNameBack,
                toAttachmentName: "HandLeftFrontEquipment");

            if (requireEquipmentTextures && result.Count == 0)
            {
                var message =
                    $"There are no attachment sprites for: {typeName} (sex: {gender}). Checked at folders:{Environment.NewLine}"
                    + sources.Select(p => ContentPaths.Textures + p.SourceFolderPath)
                             .GetJoinedString(Environment.NewLine);

                if (isMale)
                {
                    Api.Logger.Error(message);
                }
                else
                {
                    // TODO: this also should be a error - but currently we don't have female sprites for all equipment
                    Api.Logger.Warning(message);
                }
            }

            return result;
        }

        private static void CopyAttachmentIfNotAvailable(
            List<SkeletonSlotAttachment> result,
            string fromSkeletonName,
            string fromAttachmentName,
            string toSkeletonName,
            string toAttachmentName)
        {
            foreach (var slotAttachment in result)
            {
                if (slotAttachment.AttachmentName.Equals(toAttachmentName, StringComparison.Ordinal)
                    && toSkeletonName.Equals(slotAttachment.SkeletonName, StringComparison.Ordinal))
                {
                    // such attachment already exists - no need to copy
                    return;
                }
            }

            foreach (var slotAttachment in result)
            {
                if (slotAttachment.AttachmentName.Equals(fromAttachmentName, StringComparison.Ordinal)
                    && fromSkeletonName.Equals(slotAttachment.SkeletonName, StringComparison.Ordinal))
                {
                    // copy this attachment
                    result.Add(
                        new SkeletonSlotAttachment(
                            toSkeletonName,
                            toAttachmentName,
                            slotAttachment.TextureResource));
                    return;
                }
            }
        }

        public struct FilePathsList
        {
            public readonly ITempList<string> FilesInFolder;

            public readonly string SourceFolderPath;

            public FilePathsList(string sourceFolderPath, ITempList<string> filesInFolder)
            {
                this.SourceFolderPath = sourceFolderPath;
                this.FilesInFolder = filesInFolder;

                foreach (var path in filesInFolder)
                {
                    if (!path.EndsWith(".png"))
                    {
                        throw new Exception(
                            "All loadable attachments must be a PNG-files: wrong file - " + path);
                    }
                }
            }
        }
    }
}