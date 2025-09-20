using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using UnityEngine;

namespace GorillaAlbums.Behaviours
{
    public static class ImageManager
    {
        public class AlbumContent
        {
            public Texture2D Cover;
            public AudioClip Track;
            public string Name;
        }

        private static string rootFolder;
        private static string lastDisplayFilePath;

        public static List<AlbumContent> Albums { get; private set; } = new List<AlbumContent>();
        public static bool HasError { get; private set; }

        private static List<AlbumContent> shuffled = new List<AlbumContent>();
        private static int shuffleIndex = 0;

        public static void CreateImageFolder()
        {
            rootFolder = Path.Combine(Paths.PluginPath, "AlbumCovers");
            Directory.CreateDirectory(rootFolder);

            string logFolder = Path.Combine(Paths.BepInExRootPath, "GorillaAlbums");
            Directory.CreateDirectory(logFolder);
            lastDisplayFilePath = Path.Combine(logFolder, "LastDisplay.txt");

            for (int i = 1; i <= 4; i++)
            {
                string subFolder = Path.Combine(rootFolder, $"Album{i}");
                Directory.CreateDirectory(subFolder);
            }
        }

        public static void LoadAllAlbums()
        {
            Albums.Clear();
            HasError = false;

            if (!Directory.Exists(rootFolder)) return;

            var subDirs = Directory.GetDirectories(rootFolder).OrderBy(d => d).ToArray();

            foreach (var subDir in subDirs)
            {
                var images = Directory.GetFiles(subDir, "*.*")
                    .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                var audioFiles = Directory.GetFiles(subDir, "*.*")
                    .Where(f => f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (images.Length != 1 || audioFiles.Length != 1)
                {
                    Debug.LogWarning($"[GorillaAlbums] Folder {Path.GetFileName(subDir)} must contain exactly 1 image and 1 audio.");
                    HasError = true;
                    continue;
                }

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(images[0]));
                tex.name = Path.GetFileNameWithoutExtension(images[0]);

                AudioClip clip = MusicManager.LoadAudioClip(audioFiles[0]);
                if (clip == null)
                {
                    Debug.LogWarning($"[GorillaAlbums] Failed to load audio");
                    HasError = true;
                    continue;
                }

                Albums.Add(new AlbumContent
                {
                    Cover = tex,
                    Track = clip,
                    Name = Path.GetFileNameWithoutExtension(audioFiles[0])
                });
            }

            if (Albums.Count == 4 && !HasError)
                ShuffleAlbums();
            else
                HasError = true;
        }

        private static void ShuffleAlbums()
        {
            string lastPlayedName = null;
            if (File.Exists(lastDisplayFilePath))
                lastPlayedName = File.ReadAllText(lastDisplayFilePath);

            shuffled = Albums.OrderBy(a => UnityEngine.Random.value).ToList();

            if (!string.IsNullOrEmpty(lastPlayedName))
            {
                var lastAlbum = shuffled.FirstOrDefault(a => a.Name == lastPlayedName);
                if (lastAlbum != null)
                {
                    shuffled.Remove(lastAlbum);
                    shuffled.Add(lastAlbum);
                }
            }

            shuffleIndex = 0;
        }

        public static AlbumContent PickNextAlbum()
        {
            if (Albums.Count == 0) return null;

            if (shuffleIndex >= shuffled.Count)
                ShuffleAlbums();

            var chosen = shuffled[shuffleIndex++];
            if (!string.IsNullOrEmpty(chosen.Name))
                File.WriteAllText(lastDisplayFilePath, chosen.Name);

            return chosen;
        }

        public static void ApplyImagesToRecords(GameObject parent)
        {
            if (parent == null || HasError || Albums.Count < 4) return;

            string[] recordNames = { "Record1", "Record2", "Record3", "Record4" };
            for (int i = 0; i < recordNames.Length; i++)
            {
                var t = parent.transform.FindDeepChild(recordNames[i]);
                if (t == null) continue;

                var rend = t.GetComponentInChildren<Renderer>();
                if (rend == null) continue;

                rend.material.mainTexture = Albums[i].Cover;
            }
        }

        public static Transform FindDeepChild(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                var result = child.FindDeepChild(name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
