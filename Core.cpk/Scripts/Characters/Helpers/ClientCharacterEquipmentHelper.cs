namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.NightVision;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Vehicles;
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
            = new(() => GetAttachments("GenericPants"));

        private static readonly Lazy<PlaceholderAttachments[]> GenericShirtAttachments
            = new( // find all T-shirt folders and get attachments for every of them
                () => Api.Shared.GetFolderNamesInFolder(
                             ContentPaths.Textures + "Characters/Equipment/")
                         .Where(fn => fn.StartsWith("GenericTshirt", StringComparison.Ordinal))
                         .Select(name => GetAttachments(name))
                         .ToArray());

        private static int headGenerationId;

        public static void ClientRebuildAppearance(
            ICharacter character,
            BaseCharacterClientState clientState,
            ICharacterPublicState publicState,
            IItem selectedItem)
        {
            if (!character.IsNpc)
            {
                var vehicle = character.SharedGetCurrentVehicle();
                if (vehicle is not null
                    && !vehicle.IsInitialized)
                {
                    // Character has a vehicle which is not yet initialized.
                    // Don't build a skeleton for it now.
                    // When vehicle will be initialized,
                    // it will automatically re-initialize the character and invoke this method.
                    return;
                }
            }

            IProtoCharacterSkeleton newProtoSkeleton = null;
            double skeletonScaleMultiplier = 0;

            if (publicState is PlayerCharacterPublicState playerCharacterPublicState
                && playerCharacterPublicState.CurrentVehicle is not null
                && playerCharacterPublicState.CurrentVehicle.IsInitialized)
            {
                var protoVehicle = (IProtoVehicle)playerCharacterPublicState.CurrentVehicle.ProtoWorldObject;
                protoVehicle.SharedGetSkeletonProto(null,
                                                    out var protoSkeletonVehicle,
                                                    out var scaleResult);
                if (protoSkeletonVehicle is not null)
                {
                    newProtoSkeleton = (ProtoCharacterSkeleton)protoSkeletonVehicle;
                    skeletonScaleMultiplier = scaleResult;
                }
            }

            if (newProtoSkeleton is null)
            {
                character.ProtoCharacter.SharedGetSkeletonProto(
                    character,
                    out newProtoSkeleton,
                    out skeletonScaleMultiplier);
            }

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

            // create shadow renderer
            clientState.RendererShadow?.Destroy();
            clientState.RendererShadow = ((ProtoCharacterSkeleton)newProtoSkeleton)
                .ClientCreateShadowRenderer(character,
                                            skeletonScaleMultiplier);

            if (skeletonRenderer is null
                || isSkeletonChanged)
            {
                skeletonRenderer?.Destroy();
                if (newProtoSkeleton is null
                    || newProtoSkeleton.SkeletonResourceFront is null)
                {
                    return;
                }

                var scale = clientState.CurrentProtoSkeleton.WorldScale
                            * clientState.CurrentProtoSkeletonScaleMultiplier;

                // create new skeleton renderer
                skeletonRenderer = CreateCharacterSkeleton(character.ClientSceneObject, newProtoSkeleton, scale);

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

            if (containerEquipment is not null)
            {
                SetupSkeletonEquipmentForCharacter(
                    character,
                    containerEquipment,
                    skeletonRenderer,
                    clientState.CurrentProtoSkeleton,
                    skeletonComponents);
            }

            if (!character.IsNpc)
            {
                var vehicle = character.SharedGetCurrentVehicle();
                if (vehicle is not null
                    && vehicle.IsInitialized)
                {
                    var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
                    protoVehicle.ClientSetupSkeleton(vehicle,
                                                     newProtoSkeleton,
                                                     skeletonRenderer,
                                                     skeletonComponents);
                }
            }

            if (selectedItem is not null)
            {
                var activeProtoItem = selectedItem.ProtoGameObject as IProtoItemWithCharacterAppearance;
                activeProtoItem?.ClientSetupSkeleton(selectedItem,
                                                     character,
                                                     clientState.CurrentProtoSkeleton,
                                                     skeletonRenderer,
                                                     skeletonComponents);
            }

            if (character.IsCurrentClientCharacter
                && character.ProtoCharacter is PlayerCharacter)
            {
                TryAddArtificialLightArea(character, skeletonComponents);
            }

            // ensure all the added skeleton components are instantly updated
            // to make them ready for rendering (fixes light flickering issue)
            foreach (var c in skeletonComponents)
            {
                if (!(c is ClientComponent component)
                    || !component.IsEnabled)
                {
                    continue;
                }

                component.Update(0);

                if (component.IsLateUpdateEnabled)
                {
                    component.LateUpdate(0);
                }
            }
        }

        public static void ClientRefreshEquipment(
            ICharacter character,
            BaseCharacterClientState clientState,
            ICharacterPublicStateWithEquipment publicState)
        {
            var selectedItem = publicState.SelectedItem;

            var containerEquipment = publicState.ContainerEquipment;
            if (clientState.LastEquipmentContainerHash == containerEquipment.StateHash
                && clientState.SkeletonRenderer is not null
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
            if (protoCharacterSkeleton.SkeletonResourceFront is null)
            {
                return null;
            }

            var skeletonResources = new List<SkeletonResource>();
            skeletonResources.Add(protoCharacterSkeleton.SkeletonResourceFront);
            if (protoCharacterSkeleton.SkeletonResourceBack is not null)
            {
                skeletonResources.Add(protoCharacterSkeleton.SkeletonResourceBack);
            }

            var skeleton = Api.Client.Rendering.CreateSkeletonRenderer(
                sceneObject,
                skeletonResources: skeletonResources,
                defaultSkeleton: protoCharacterSkeleton.SkeletonResourceFront,
                defaultLoopedAnimationName: protoCharacterSkeleton.DefaultAnimationName,
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
            List<IClientComponent> skeletonComponents,
            bool isPreview = false)
        {
            if (!(skeleton is SkeletonHumanMale)
                && !(skeleton is SkeletonHumanFemale))
            {
                // not a human
                // setup only implants
                using var equipmentImplants = Api.Shared.WrapInTempList(
                    containerEquipment.GetItemsOfProto<IProtoItemEquipmentImplant>());
                foreach (var item in equipmentImplants.AsList())
                {
                    var proto = (IProtoItemEquipmentImplant)item.ProtoGameObject;
                    proto.ClientSetupSkeleton(item,
                                              character,
                                              skeletonRenderer,
                                              skeletonComponents,
                                              isPreview);
                }

                return;
            }

            bool isMale, isHeadEquipmentHiddenForSelfAndPartyMembers;
            CharacterHumanFaceStyle faceStyle;
            if (character.ProtoCharacter is PlayerCharacter)
            {
                var publicState = PlayerCharacter.GetPublicState(character);
                faceStyle = publicState.FaceStyle;
                isMale = publicState.IsMale;
                isHeadEquipmentHiddenForSelfAndPartyMembers = publicState.IsHeadEquipmentHiddenForSelfAndPartyMembers;

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
                isHeadEquipmentHiddenForSelfAndPartyMembers = false;
            }

            skeletonRenderer.ResetAttachments();
            skeleton.ClientResetItemInHand(skeletonRenderer);

            var skinToneId = faceStyle.SkinToneId;
            if (string.IsNullOrEmpty(skinToneId))
            {
                skeletonRenderer.DefaultTextureRemapper = null;
            }
            else
            {
                // use colorizer for the original sprites (to apply the skin tone)
                skeletonRenderer.DefaultTextureRemapper
                    = textureResource =>
                      {
                          var filePath = textureResource.LocalPath;
                          if (filePath.IndexOf("/Weapon",  StringComparison.Ordinal) >= 0
                              || filePath.IndexOf("/Head", StringComparison.Ordinal) >= 0)
                          {
                              // no need to remap the original head and weapon sprites
                              // (they're never used as is)
                              return textureResource;
                          }

                          return ClientCharacterSkinTexturesCache.Get(textureResource,
                                                                      skinToneId);
                      };
            }

            // setup equipment items
            using var equipmentItems = Api.Shared.WrapInTempList(
                containerEquipment.GetItemsOfProto<IProtoItemEquipment>());
            if (!IsAllowNakedHumans)
            {
                if (!equipmentItems.AsList().Any(i => i.ProtoGameObject is IProtoItemEquipmentArmor))
                {
                    // no armor equipped - apply generic one
                    var pants = GenericPantsAttachments.Value;
                    ClientSkeletonAttachmentsLoader.SetAttachments(
                        skeletonRenderer,
                        isMale
                            ? pants.SlotAttachmentsMale
                            : pants.SlotAttachmentsFemale);

                    // select a random generic T-shirt based on character ID
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

            IItem headEquipmentForFaceSprite = null;
            foreach (var item in equipmentItems.AsList())
            {
                var proto = (IProtoItemEquipment)item.ProtoGameObject;
                proto.ClientSetupSkeleton(item,
                                          character,
                                          skeletonRenderer,
                                          skeletonComponents,
                                          isPreview);

                if (item.ProtoItem is IProtoItemEquipmentHead
                    && headEquipmentForFaceSprite is null)
                {
                    headEquipmentForFaceSprite = item;
                }
            }

            if (isHeadEquipmentHiddenForSelfAndPartyMembers
                && (character.IsCurrentClientCharacter
                    || PartySystem.ClientIsPartyMember(character.Name)
                    || PveSystem.ClientIsPve(false)))
            {
                headEquipmentForFaceSprite = null;
            }

            // generate head sprites for human players
            const string slotName = "Head",
                         attachmentName = "Head";

            headGenerationId++;

            var spriteQualityOffset = skeletonRenderer.SpriteQualityOffset;
            skeletonRenderer.SetAttachmentSprite(
                skeleton.SkeletonResourceFront,
                slotName,
                attachmentName,
                new ProceduralTexture(
                    $"Head Front CharacterID={character.Id} gen={headGenerationId}",
                    proceduralTextureRequest =>
                        ClientCharacterHeadSpriteComposer.GenerateHeadSprite(
                            new CharacterHeadSpriteData(faceStyle,
                                                        headEquipmentForFaceSprite,
                                                        skeleton.SkeletonResourceFront),
                            proceduralTextureRequest,
                            isMale,
                            headSpriteType: ClientCharacterHeadSpriteComposer.HeadSpriteType.Front,
                            spriteQualityOffset: spriteQualityOffset),
                    isTransparent: true,
                    isUseCache: false));

            skeletonRenderer.SetAttachmentSprite(
                skeleton.SkeletonResourceBack,
                slotName,
                attachmentName,
                new ProceduralTexture(
                    $"Head Back CharacterID={character.Id} gen={headGenerationId}",
                    proceduralTextureRequest =>
                        ClientCharacterHeadSpriteComposer.GenerateHeadSprite(
                            new CharacterHeadSpriteData(faceStyle,
                                                        headEquipmentForFaceSprite,
                                                        skeleton.SkeletonResourceBack),
                            proceduralTextureRequest,
                            isMale,
                            headSpriteType: ClientCharacterHeadSpriteComposer.HeadSpriteType.Back,
                            spriteQualityOffset: spriteQualityOffset),
                    isTransparent: true,
                    isUseCache: false));

            skeletonRenderer.SetAttachmentSprite(
                skeleton.SkeletonResourceBack,
                slotName + "Back",
                attachmentName,
                new ProceduralTexture(
                    $"Head Back2 CharacterID={character.Id} gen={headGenerationId}",
                    proceduralTextureRequest =>
                        ClientCharacterHeadSpriteComposer.GenerateHeadSprite(
                            new CharacterHeadSpriteData(faceStyle,
                                                        headEquipmentForFaceSprite,
                                                        skeleton.SkeletonResourceBack),
                            proceduralTextureRequest,
                            isMale,
                            headSpriteType: ClientCharacterHeadSpriteComposer.HeadSpriteType.BackOverlay,
                            spriteQualityOffset: spriteQualityOffset),
                    isTransparent: true,
                    isUseCache: false));
        }

        private static PlaceholderAttachments GetAttachments(
            string name,
            bool requireEquipmentTextures = true)
        {
            using var tempSourcePaths = Api.Shared.WrapObjectInTempList("Characters/Equipment/" + name);
            using var tempSpritePathLists =
                ClientEquipmentSpriteHelper.CollectSpriteFilePaths(tempSourcePaths.AsList());

            ClientEquipmentSpriteHelper.CollectSlotAttachments(
                tempSpritePathLists.AsList(),
                typeName: name,
                requireEquipmentTextures: requireEquipmentTextures,
                out var slotAttachmentsMale,
                out var slotAttachmentsFemale);

            foreach (var tempList in tempSpritePathLists.AsList())
            {
                tempList.Dispose();
            }

            return new PlaceholderAttachments(slotAttachmentsMale, slotAttachmentsFemale);
        }

        // Adds light area around the player in case it doesn't have any light source or night vision equipped.
        private static void TryAddArtificialLightArea(
            ICharacter character,
            List<IClientComponent> skeletonComponents)
        {
            foreach (var c in skeletonComponents)
            {
                switch (c)
                {
                    case ClientComponentLightInSkeleton lightInSkeleton when lightInSkeleton.IsPrimary:
                    case ClientComponentNightVisionEffect:
                    case ItemImplantArtificialRetina.ClientComponentArtificialRetinaEffect:
                        return;
                }
            }

            // this is current client character and it doesn't have light in hand
            // create a faint light source (see called method comments)
            var lightSource = PlayerCharacter.ClientCreateDefaultLightSource(character);
            if (lightSource is not null)
            {
                skeletonComponents.Add(lightSource);
            }
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