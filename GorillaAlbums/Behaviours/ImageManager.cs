using System;
using System.IO;
using System.Linq;
using UnityEngine;
using BepInEx;

namespace GorillaAlbums.Behaviours
{
    //this way of doing it is horrible, but it works for now, if you can improve it please do..
    public static class ImageManager
    {
        private static Texture2D[] loadedImages;
        private static string imageFolderPath;
        private static string lastDisplayFilePath;

        public static void CreateImageFolder()
        {
            imageFolderPath = Path.Combine(Paths.PluginPath, "AlbumCovers");
            if (!Directory.Exists(imageFolderPath))
                Directory.CreateDirectory(imageFolderPath);

            string logFolder = Path.Combine(Paths.BepInExRootPath, "GorillaAlbums");
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            lastDisplayFilePath = Path.Combine(logFolder, "LastDisplay.txt");
        }

        public static void LoadAllImages()
        {
            string[] files = Directory.GetFiles(imageFolderPath, "*.*")
                .Where(s => s.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                            || s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            loadedImages = new Texture2D[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                byte[] fileData = File.ReadAllBytes(files[i]);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                tex.name = Path.GetFileNameWithoutExtension(files[i]);
                loadedImages[i] = tex;
            }
        }

        public static void ApplyImagesToPhotos(GameObject parentObject)
        {
            if (parentObject == null)
                return;

            if (loadedImages == null || loadedImages.Length < 4)
            {
                ErrorManager.CheckAndShowError(parentObject);
                return;
            }

            string[] photoNames = { "Record1", "Record2", "Record3", "Record4" };
            Texture2D[] assignedTextures = new Texture2D[4];

            for (int i = 0; i < photoNames.Length; i++)
            {
                Transform photoTransform = FindDeepChild(parentObject.transform, photoNames[i]);
                if (photoTransform != null)
                {
                    Renderer rend = photoTransform.GetComponentInChildren<Renderer>();
                    if (rend != null)
                    {
                        Texture2D tex = loadedImages[i % loadedImages.Length];
                        rend.material.mainTexture = tex;
                        assignedTextures[i] = tex;
                    }
                }
            }

 
            Transform displayTransform = FindDeepChild(parentObject.transform, "RecordDisplayArt");
            if (displayTransform != null)
            {
                Renderer displayRend = displayTransform.GetComponentInChildren<Renderer>();
                if (displayRend != null)
                {
                    Texture2D lastTexture = null;
                    if (File.Exists(lastDisplayFilePath))
                    {
                        string lastName = File.ReadAllText(lastDisplayFilePath);
                        lastTexture = assignedTextures.FirstOrDefault(tex => tex.name == lastName);
                    }

                    Texture2D chosenTex;
                    if (lastTexture != null && assignedTextures.Length > 1)
                    {
                        var options = assignedTextures.Where(tex => tex != lastTexture).ToArray();
                        chosenTex = options[UnityEngine.Random.Range(0, options.Length)];
                    }
                    else
                    {
                        chosenTex = assignedTextures[UnityEngine.Random.Range(0, assignedTextures.Length)];
                    }

                    displayRend.material.mainTexture = chosenTex;

                    if (!string.IsNullOrEmpty(chosenTex.name))
                        File.WriteAllText(lastDisplayFilePath, chosenTex.name);
                }
            }
        }

        public static int GetLoadedImageCount()
        {
            return loadedImages != null ? loadedImages.Length : 0;
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                Transform result = FindDeepChild(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
