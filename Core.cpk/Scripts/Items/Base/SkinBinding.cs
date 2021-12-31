namespace AtomicTorch.CBND.CoreMod.Items
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public static class SkinBinding
    {
        private static readonly Dictionary<SkinId, IProtoItem> BindingEnumToProtoItem = new();

        private static readonly Dictionary<string, SkinId> BindingStringIdToEnum;

        static SkinBinding()
        {
            var skinIds = EnumExtensions.GetValues<SkinId>();

            var dictionary = new Dictionary<string, SkinId>();
            foreach (var skinId in skinIds)
            {
                if (!dictionary.TryAdd(skinId.ToString(), skinId))
                {
                    Api.Logger.Error(
                        $"SkinId enum error: there is a value duplicate for ID number: {(ushort)skinId} which is currently reserved for {skinId}");
                }
            }

            BindingStringIdToEnum = dictionary;
        }

        public static SkinId BindSkin(IProtoItem protoItem)
        {
            if (((IProtoItemWithSkinData)protoItem).IsModSkin)
            {
                // it's a modded skin so it's definitely free
                return SkinId.None;
            }
            
            if (!BindingStringIdToEnum.TryGetValue(protoItem.ShortId, out var skinId))
            {
                Api.Logger.Error("Unknown skin: " + protoItem.ShortId);
            }

            if (!BindingEnumToProtoItem.TryAdd(skinId, protoItem))
            {
                Api.Logger.Error(
                    $"Skin duplicate: {protoItem} already has a skin under the same ID: {BindingEnumToProtoItem[skinId]}");
            }

            return skinId;
        }

#if !NET5_0_OR_GREATER
        private static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }
#endif

        [PrepareOrder(afterType: typeof(IProtoItem))]
        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                VerifyBinding();
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                VerifyBinding();
            }

            private static void VerifyBinding()
            {
                if (BindingStringIdToEnum.Count - 1 == BindingEnumToProtoItem.Count)
                {
                    // every enum entry is properly bound
                    return;
                }

                var missingSkins = BindingStringIdToEnum.Values
                                                        .ExceptOne(SkinId.None)
                                                        .Except(BindingEnumToProtoItem.Keys)
                                                        .ToList();
                Logger.Error("The following SkinId entries have no corresponding Skin classes:"
                             + Environment.NewLine
                             + missingSkins.Select(s => "* " + s)
                                           .GetJoinedString(Environment.NewLine));
            }
        }
    }
}