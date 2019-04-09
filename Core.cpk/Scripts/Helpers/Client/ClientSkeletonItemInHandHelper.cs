namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    internal static class ClientSkeletonItemInHandHelper
    {
        private const string SlotNameHandLeft = "HandLeft";

        private const string SlotNameWeapon = "Weapon";

        public static void Reset(IComponentSkeleton skeletonRenderer)
        {
            skeletonRenderer.SetAttachmentSprite(SlotNameWeapon, attachmentName: "WeaponMelee", textureResource: null);
            skeletonRenderer.SetAttachmentSprite(SlotNameWeapon, attachmentName: "WeaponRifle", textureResource: null);
            skeletonRenderer.SetAttachment(SlotNameWeapon, attachmentName: null);
        }

        public static void Setup(
            IComponentSkeleton skeletonRenderer,
            string attachmentName,
            TextureResource textureResource)
        {
            skeletonRenderer.SetAttachmentSprite(SlotNameWeapon, attachmentName, textureResource);
            skeletonRenderer.SetAttachment(SlotNameWeapon, attachmentName);

            // set left hand to use special sprite (representing holding of the item in hand)
            skeletonRenderer.SetAttachment(SlotNameHandLeft,               "HandLeft2");
            skeletonRenderer.SetAttachment(SlotNameHandLeft + "Equipment", "HandLeft2Equipment");
        }
    }
}