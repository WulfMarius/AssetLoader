﻿namespace AssetLoader
{
    public class AssetUtils
    {
        internal static UIAtlas GetRequiredAtlas(UISprite sprite, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return sprite.atlas;
            }

            UIAtlas atlas = ModAssetBundleManager.GetSpriteAtlas(value);
            if (atlas != null)
            {
                return atlas;
            }

            SaveAtlas restoreAtlas = sprite.gameObject.GetComponent<SaveAtlas>();
            if (restoreAtlas != null)
            {
                return restoreAtlas.original;
            }

            return sprite.atlas;
        }

        internal static void SaveOriginalAtlas(UISprite sprite)
        {
            SaveAtlas restoreAtlas = sprite.gameObject.GetComponent<SaveAtlas>();
            if (restoreAtlas == null)
            {
                restoreAtlas = sprite.gameObject.AddComponent<SaveAtlas>();
                restoreAtlas.original = sprite.atlas;
            }
        }
    }
}