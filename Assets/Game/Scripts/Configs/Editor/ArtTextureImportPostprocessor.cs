using System;
using UnityEditor;
using UnityEngine;

namespace AIGame.ShootEmUp.Configs.Editor
{
    internal sealed class ArtTextureImportPostprocessor : AssetPostprocessor
    {
        private static readonly string[] ManagedRoots =
        {
            "Assets/Game/Art/Sprites/",
            "Assets/Game/Art/UI/",
            "Assets/Game/Art/VFX/",
            "Assets/Game/Art/Placeholder/"
        };

        [MenuItem("Tools/ShootEmUp/Art/Reimport Managed Art Textures")]
        private static void ReimportManagedArtTextures()
        {
            var updated = 0;
            var guids = AssetDatabase.FindAssets("t:Texture2D", ManagedRoots);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsManagedArtPath(path))
                {
                    continue;
                }

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                updated++;
            }

            Debug.Log($"Reimported {updated} managed art textures.");
        }

        private void OnPreprocessTexture()
        {
            if (!IsManagedArtPath(assetPath))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = false;
            importer.sRGBTexture = true;

            // Atlases keep Multiple mode for Sprite Editor slicing; others use Single.
            if (assetPath.Contains("/Sprites/Atlases/", StringComparison.OrdinalIgnoreCase))
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
            }
            else
            {
                importer.spriteImportMode = SpriteImportMode.Single;
            }

            // Keep default platform quality stable across machines.
            var defaultSettings = importer.GetDefaultPlatformTextureSettings();
            defaultSettings.overridden = false;
            defaultSettings.format = TextureImporterFormat.Automatic;
            defaultSettings.textureCompression = TextureImporterCompression.Compressed;
            defaultSettings.crunchedCompression = false;
            defaultSettings.compressionQuality = 50;
            importer.SetPlatformTextureSettings(defaultSettings);
        }

        private static bool IsManagedArtPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            foreach (var root in ManagedRoots)
            {
                if (path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
