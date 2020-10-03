namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.GameApi.Resources;

    public static class ProtoItemEquipmentHeadHelper
    {
        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public static void ClientFindDefaultHeadSprites(
            IReadOnlyList<SkeletonSlotAttachment> slotAttachments,
            SkeletonResource skeletonResource,
            bool isFrontFace,
            out string helmetFront,
            out string helmetBehind,
            string headEquipmentName = "HeadEquipment")
        {
            helmetFront = null;
            helmetBehind = null;

            var skeletonName = skeletonResource?.SkeletonName;
            foreach (var slot in slotAttachments)
            {
                if (slot.SkeletonName is not null
                    && slot.SkeletonName != skeletonName)
                {
                    continue;
                }

                if (slot.AttachmentName == headEquipmentName)
                {
                    helmetFront = slot.TextureResource.FullPath;
                }
                else if (isFrontFace
                         && slot.AttachmentName == "HeadBehindEquipment")
                {
                    helmetBehind = slot.TextureResource.FullPath;
                }
            }
        }
    }
}