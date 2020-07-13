namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public static class ClientDamageTypeIconHelper
    {
        private static readonly IReadOnlyDictionary<DamageType, ImageSource> Dictionary;

        static ClientDamageTypeIconHelper()
        {
            var dictionary = new Dictionary<DamageType, ImageSource>();

            foreach (var damageType in EnumExtensions.GetValues<DamageType>())
            {
                var resource = Api.Client.UI.GetApplicationResource<ImageSource>(
                    "ImageSourceDamageType" + damageType.ToString());
                dictionary[damageType] = resource;
            }

            Dictionary = dictionary;
        }

        public static ImageSource GetImageSource(DamageType damageType)
        {
            return Dictionary[damageType];
        }
    }
}