﻿namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public static class ClientLighting
    {
        /// <summary>
        /// With this constant you can enable current lighting mask rendering.
        /// </summary>
        public const bool IsDisplayMask = false;

        public const string RenderingTag = nameof(ClientLighting) + "LayerLighting";

        public static ClientComponentSpriteLightSource CreateLightSourceSpot(
            IClientSceneObject sceneObject,
            Color color,
            Size2F size,
            Size2F? logicalSize = null,
            Vector2D? spritePivotPoint = null,
            Vector2D? positionOffset = null)
        {
            var result = sceneObject.AddComponent<ClientComponentSpriteLightSource>();
            result.Color = color;
            result.RenderingSize = size;
            result.LogicalSize = logicalSize ?? size;

            if (spritePivotPoint.HasValue)
            {
                result.SpritePivotPoint = spritePivotPoint.Value;
            }

            if (positionOffset.HasValue)
            {
                result.PositionOffset = positionOffset.Value;
            }

            return result;
        }

        [CanBeNull]
        public static BaseClientComponentLightSource CreateLightSourceSpot(
            IClientSceneObject sceneObject,
            IReadOnlyItemLightConfig itemLightConfig,
            Vector2D? overrideWorldOffset = null)
        {
            if (!itemLightConfig.IsLightEnabled)
            {
                return null;
            }

            return CreateLightSourceSpot(
                sceneObject,
                itemLightConfig.Color,
                size: itemLightConfig.Size,
                logicalSize: itemLightConfig.LogicalSize,
                positionOffset: overrideWorldOffset ?? itemLightConfig.WorldOffset);
        }
    }
}