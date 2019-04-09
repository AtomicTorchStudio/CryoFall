namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class ClientSkeletonAttachmentsLoader
    {
        private static readonly ISharedApi SharedApi = Api.Shared;

        public static void CreateSlotAttachmentsForTexturesFolder(
            string fullPath,
            string skeletonName,
            string filterSubfolderPath,
            List<string> filePaths,
            List<SkeletonSlotAttachment> slotAttachments)
        {
            foreach (var path in filePaths)
            {
                // ensure that the attachment path is located in the filterSubfolderPath
                if (!path.StartsWith(filterSubfolderPath, StringComparison.Ordinal)
                    || path.IndexOf('/', filterSubfolderPath.Length) >= 0)
                {
                    continue;
                }

                // get attachment name without extension
                var attachmentName = SharedApi.GetFileNameWithoutExtension(path);

                // all cloth will be drawn over default attachments - in "equipment" attachments
                attachmentName += "Equipment";

                slotAttachments.Add(
                    new SkeletonSlotAttachment(
                        skeletonName,
                        attachmentName,
                        new TextureResource(
                            fullPath + path,
                            isProvidesMagentaPixelPosition: true)));
            }
        }

        public static void SetAttachments(
            IComponentSkeleton skeletonRenderer,
            IReadOnlyList<SkeletonSlotAttachment> slotAttachments)
        {
            foreach (var skeleton in skeletonRenderer.AllSkeletons)
            {
                var skeletonName = skeleton.SkeletonName;
                foreach (var slotAttachment in slotAttachments)
                {
                    if (slotAttachment.SkeletonName != null
                        && !slotAttachment.SkeletonName.Equals(skeletonName, StringComparison.Ordinal))
                    {
                        // this attachment for different skeleton
                        continue;
                    }

                    if (slotAttachment.AttachmentName.StartsWith("Head", StringComparison.Ordinal))
                    {
                        // Head attachments are processed by ClientCharacterHeadSpriteComposer
                        // when generating the head sprite. They're not assigned directly to skeleton.
                        continue;
                    }

                    skeletonRenderer.SetAttachmentSprite(
                        skeleton,
                        // auto find slot
                        slotName: null,
                        attachmentName: slotAttachment.AttachmentName,
                        textureResource: slotAttachment.TextureResource);
                }
            }
        }
    }
}