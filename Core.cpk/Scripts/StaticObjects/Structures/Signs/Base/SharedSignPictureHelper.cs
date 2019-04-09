namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class SharedSignPictureHelper
    {
        private const string TexturePath = "StaticObjects/Structures/Signs/Pictures/";

        public static readonly IReadOnlyList<string> AllImagesFileNames;

        static SharedSignPictureHelper()
        {
            var api = Api.Shared;
            using (var tempFilesList = api.FindFilesWithTrailingNumbers(ContentPaths.Textures + TexturePath))
            {
                var list = tempFilesList.AsList();
                var array = new string[list.Count];

                for (var index = 0; index < list.Count; index++)
                {
                    var filePath = list[index];
                    array[index] = api.GetFileNameWithoutExtension(filePath);
                }

                AllImagesFileNames = array;
            }
        }

        public static TextureResource GetTextureResource(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var textureResource = new TextureResource(TexturePath + filePath,
                                                      qualityOffset: -100);
            return textureResource;
        }
    }
}