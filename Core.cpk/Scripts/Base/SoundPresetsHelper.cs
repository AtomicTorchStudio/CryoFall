namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class SoundPresetsHelper
    {
        public static GroundSoundMaterial SharedGetGroundSoundMaterial(
            this IProtoObjectWithGroundSoundMaterial protoFloor)
        {
            return protoFloor.GroundSoundMaterial;
        }

        public static ReadOnlySoundPreset<ItemSound> SharedGetItemSoundPreset(this IProtoItem protoItem)
        {
            return ((IProtoItemWithSoundPreset)protoItem).SoundPresetItem;
        }

        public static ObjectSoundMaterial SharedGetObjectSoundMaterial(this IProtoWorldObject protoWorldObject)
        {
            return ((IProtoWorldObjectWithSoundPresets)protoWorldObject).ObjectSoundMaterial;
        }

        public static ReadOnlySoundPreset<ObjectSound> SharedGetObjectSoundPreset(
            this IProtoWorldObject protoWorldObject)
        {
            if (protoWorldObject == null)
            {
                throw new ArgumentNullException(nameof(protoWorldObject));
            }

            if (protoWorldObject is IProtoWorldObjectWithSoundPresets protoWorldObjectWithSoundPresets)
            {
                return protoWorldObjectWithSoundPresets.SoundPresetObject;
            }

            Api.Logger.Error(
                protoWorldObject
                + " type is not inherited from "
                + nameof(IProtoWorldObjectWithSoundPresets)
                + " - cannot get object sound preset. Using default (generic) sound preset instead.");
            return ObjectsSoundsPresets.ObjectGeneric;
        }
    }
}