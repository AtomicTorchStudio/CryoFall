namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Lights;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class ClientCharacterEquipmentHelper
    {
        // we disallow naked humans by default (some default cloth will be put on a naked character)
        private const bool IsAllowNakedHumans = false;

        private static readonly Lazy<PlaceholderAttachments> GenericPantsAttachments
            = new Lazy<PlaceholderAttachments>(() => GetAttachments("GenericPants"));

        private static readonly Lazy<PlaceholderAttachments[]> GenericShirtAttachments
            = new Lazy<PlaceholderAttachments[]>(
                // find all T-shirt folders and get attachments for every of them
                () => Api.Shared.GetFolderNamesInFolder(
                             ContentPaths.Textures + "Characters/Equipment/")
                         .Where(fn => fn.StartsWith("GenericTshirt", StringComparison.Ordinal))
                         .Select(name => GetAttachments(name))
                         .ToArray());

        public static void ClientRebuildAppearance(
            ICharacter character,
            BaseCharacterClientState clientState,
            ICharacterPublicState publicState,
            IItem selectedItem)
        {
            character.ProtoCharacter.SharedGetSkeletonProto(
                character,
                out var newProtoSkeleton,
                out var skeletonScaleMultiplier);

            var isSkeletonChanged = newProtoSkeleton != clientState.CurrentProtoSkeleton;
            clientState.CurrentProtoSkeleton = (ProtoCharacterSkeleton)newProtoSkeleton;
            clientState.CurrentProtoSkeletonScaleMultiplier = skeletonScaleMultiplier;

            var skeletonRenderer = clientState.SkeletonRenderer;
            var skeletonComponents = clientState.SkeletonComponents;

            if (skeletonComponents.Count > 0)
            {
                foreach (var comp in skeletonComponents)
                {
                    try
                    {
                        comp.Destroy();
                    }
                    catch (Exception ex)
                    {
                        Api.Logger.Exception(ex);
                    }
                }

                skeletonComponents.Clear();
            }

            var isNewSkeleton = false;

            if (skeletonRenderer == null
                || isSkeletonChanged)
            {
                skeletonRenderer?.Destroy();
                if (newProtoSkeleton == null
                    || newProtoSkeleton.SkeletonResourceFront == null)
                {
                    return;
                }

                var sceneObject = Api.Client.Scene.GetSceneObject(character);
                var scale = clientState.CurrentProtoSkeleton.WorldScale
                            * clientState.CurrentProtoSkeletonScaleMultiplier;

                // create new skeleton renderer
                skeletonRenderer = CreateCharacterSkeleton(sceneObject, newProtoSkeleton, scale);

                clientState.SkeletonRenderer = skeletonRenderer;
                isNewSkeleton = true;
                //Api.Logger.Write("Skeleton created for " + character);
            }

            if (clientState.LastSelectedItem != selectedItem)
            {
                clientState.LastSelectedItem = selectedItem;
                if (!isNewSkeleton)
                {
                    // cleanup skeleton
                    skeletonRenderer.ResetSkeleton();
                }
            }

            var containerEquipment = (publicState as ICharacterPublicStateWithEquipment)
                ?.ContainerEquipment;

            if (containerEquipment != null)
            {
                SetupSkeletonEquipmentForCharacter(
                    character,
                    containerEquipment,
                    skeletonRenderer,
                    clientState.CurrentProtoSkeleton,
                    skeletonComponents);
            }

            if (selectedItem != null)
            {
                var activeProtoItem = selectedItem.ProtoGameObject as IProtoItemWithCharacterAppearance;
                activeProtoItem?.ClientSetupSkeleton(selectedItem, character, skeletonRenderer, skeletonComponents);
            }

            if (character.IsCurrentClientCharacter
                && character.ProtoCharacter is PlayerCharacter
                && !skeletonComponents.Any(c => c is ClientComponentLightInSkeleton))
            {
                // this is current client character and it doesn't have light in hand
                // create a faint light source (see called method comments)
                var lightSource = PlayerCharacter.ClientCreateDefaultLightSource(character);
                if (lightSource != null)
                {
                    skeletonComponents.Add(lightSource);
                }
            }
        }

        public static void ClientRefreshEquipment(
            ICharacter character,
            BaseCharacterClientState clientState,
            ICharacterPublicStateWithEquipment publicState)
        {
            var selectedItem = character.IsCurrentClientCharacter
                                   ? ClientHotbarSelectedItemManager.SelectedItem
                                   : publicState.SelectedHotbarItem;

            var containerEquipment = publicState.ContainerEquipment;
            if (clientState.LastEquipmentContainerHash == containerEquipment.StateHash
                && clientState.SkeletonRenderer != null
                && clientState.LastSelectedItem == selectedItem)
            {
                return;
            }

            clientState.LastEquipmentContainerHash = containerEquipment.StateHash;
            ClientRebuildAppearance(
                character,
                clientState,
                publicState,
                selectedItem);
        }

        public static IComponentSkeleton CreateCharacterSkeleton(
            IClientSceneObject sceneObject,
            IProtoCharacterSkeleton protoCharacterSkeleton,
            double worldScale,
            sbyte spriteQualityOffset = 0)
        {
            if (protoCharacterSkeleton.SkeletonResourceFront == null)
            {
                return null;
            }

            var skeletonResources = new List<SkeletonResource>();
            skeletonResources.Add(protoCharacterSkeleton.SkeletonResourceFront);
            if (protoCharacterSkeleton.SkeletonResourceBack != null)
            {
                skeletonResources.Add(protoCharacterSkeleton.SkeletonResourceBack);
            }

            var skeleton = Api.Client.Rendering.CreateSkeletonRenderer(
                sceneObject,
                skeletonResources: skeletonResources,
                defaultSkeleton: protoCharacterSkeleton.SkeletonResourceFront,
                defaultLoopedAnimationName: "Idle",
                positionOffset: (0, 0),
                worldScale: worldScale,
                speedMultiplier: protoCharacterSkeleton.SpeedMultiplier,
                spriteQualityOffset: spriteQualityOffset);

            protoCharacterSkeleton.OnSkeletonCreated(skeleton);
            return skeleton;
        }

        public static void SetupSkeletonEquipmentForCharacter(
            ICharacter character,
            IItemsContainer containerEquipment,
            IComponentSkeleton skeletonRenderer,
            ProtoCharacterSkeleton skeleton,
            List<IClientComponent> skeletonComponents)
        {
            if (!(skeleton is SkeletonHumanMale)
                && !(skeleton is SkeletonHumanFemale))
            {
                // not a human
                return;
            }

            skeletonRenderer.ResetAttachments();

            bool isMale;
            CharacterHumanFaceStyle faceStyle;
            if (character.ProtoCharacter is PlayerCharacter)
            {
                var pubicState = PlayerCharacter.GetPublicState(character);
                faceStyle = pubicState.FaceStyle;
                isMale = pubicState.IsMale;

                if (isMale && !(skeleton is SkeletonHumanMale)
                    || !isMale && !(skeleton is SkeletonHumanFemale))
                {
                    throw new Exception(
                        $"Skeleton don\'t match the gender of the player\'s character: isMale={isMale}, {skeleton}");
                }
            }
            else
            {
                // for NPC it will generate a random face
                isMale = true;
                faceStyle = SharedCharacterFaceStylesProvider.GetForGender(isMale).GenerateRandomFace();
            }

            // setup equipment items
            var equipmentItems = containerEquipment.GetItemsOfProto<IProtoItemEquipment>().ToList();

            if (!IsAllowNakedHumans)
            {
                if (!equipmentItems.Any(i => i.ProtoGameObject is IProtoItemEquipmentLegs))
                {
                    // no lower cloth - apply generic one
                    ClientSkeletonAttachmentsLoader.SetAttachments(
                        skeletonRenderer,
                        isMale
                            ? GenericPantsAttachments.Value.SlotAttachmentsMale
                            : GenericPantsAttachments.Value.SlotAttachmentsFemale);
                }

                if (!equipmentItems.Any(i => i.ProtoGameObject is IProtoItemEquipmentChest))
                {
                    // no upper cloth - apply generic one (based on character Id)
                    var allShirts = GenericShirtAttachments.Value;
                    var selectedShirtIndex = character.Id % allShirts.Length;
                    var shirt = allShirts[(int)selectedShirtIndex];
                    ClientSkeletonAttachmentsLoader.SetAttachments(
                        skeletonRenderer,
                        isMale
                            ? shirt.SlotAttachmentsMale
                            : shirt.SlotAttachmentsFemale);
                }
            }

            IItem headEquipment = null;
            foreach (var item in equipmentItems)
            {
                var proto = (IProtoItemEquipment)item.ProtoGameObject;
                proto.ClientSetupSkeleton(item, character, skeletonRenderer, skeletonComponents);

                if (item.ProtoItem is IProtoItemEquipmentHead
                    && headEquipment == null)
                {
                    headEquipment = item;
                }
            }

            // generate head sprites for human players
            const string slotName = "Head", attachmentName = "Head";
            skeletonRenderer.SetAttachmentSprite(
                skeleton.SkeletonResourceFront,
                slotName,
                attachmentName,
                new ProceduralTexture(
                    "Head Front CharacterID=" + character.Id,
                    proceduralTextureRequest =>
                        ClientCharacterHeadSpriteComposer.GenerateHeadSprite(
                            new CharacterHeadSpriteData(faceStyle, headEquipment, skeleton.SkeletonResourceFront),
                            proceduralTextureRequest,
                            isMale,
                            isFrontFace: true,
                            spriteQualityOffset: skeletonRenderer.SpriteQualityOffset),
                    isTransparent: true,
                    isUseCache: false));

            skeletonRenderer.SetAttachmentSprite(
                skeleton.SkeletonResourceBack,
                slotName,
                attachmentName,
                new ProceduralTexture(
                    "Head Back CharacterID=" + character.Id,
                    proceduralTextureRequest =>
                        ClientCharacterHeadSpriteComposer.GenerateHeadSprite(
                            new CharacterHeadSpriteData(faceStyle, headEquipment, skeleton.SkeletonResourceBack),
                            proceduralTextureRequest,
                            isMale,
                            isFrontFace: false,
                            spriteQualityOffset: skeletonRenderer.SpriteQualityOffset),
                    isTransparent: true,
                    isUseCache: false));

            ClientResetWeaponAttachments(skeletonRenderer);
        }

        private static void ClientResetWeaponAttachments(IComponentSkeleton skeletonRenderer)
        {
            ClientSkeletonItemInHandHelper.Reset(skeletonRenderer);
        }

        private static PlaceholderAttachments GetAttachments(
            string name,
            bool requireEquipmentTextures = true)
        {
            using var tempSourcePaths = Api.Shared.WrapObjectInTempList("Characters/Equipment/" + name);
            using var tempSpritePaths = ClientEquipmentSpriteHelper.CollectSpriteFilePaths(tempSourcePaths.AsList());

            ClientEquipmentSpriteHelper.CollectSlotAttachments(
                tempSpritePaths.AsList(),
                typeName: name,
                requireEquipmentTextures: requireEquipmentTextures,
                out var slotAttachmentsMale,
                out var slotAttachmentsFemale);

            foreach (var spriteFilePath in tempSpritePaths)
            {
                spriteFilePath.FilesInFolder.Dispose();
            }

            return new PlaceholderAttachments(slotAttachmentsMale, slotAttachmentsFemale);
        }

        [NotPersistent]
        private readonly struct PlaceholderAttachments
        {
            public readonly List<SkeletonSlotAttachment> SlotAttachmentsFemale;

            public readonly List<SkeletonSlotAttachment> SlotAttachmentsMale;

            public PlaceholderAttachments(
                List<SkeletonSlotAttachment> slotAttachmentsMale,
                List<SkeletonSlotAttachment> slotAttachmentsFemale)
            {
                this.SlotAttachmentsMale = slotAttachmentsMale;
                this.SlotAttachmentsFemale = slotAttachmentsFemale;
            }
        }
    }
}