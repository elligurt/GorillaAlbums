using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaAlbums.Behaviours
{
    public static class MusicManager
    {
        public static AudioClip LoadAudioClip(string path)
        {
            if (!File.Exists(path)) return null;

            string url = "file:///" + path.Replace("\\", "/");
            AudioType type;
            string ext = Path.GetExtension(path).ToLower();

            switch (ext)
            {
                case ".mp3": type = AudioType.MPEG; break;
                case ".wav": type = AudioType.WAV; break;
                case ".ogg": type = AudioType.OGGVORBIS; break;
                default:
                    Debug.LogWarning($"[GorillaAlbums] Unsupported audio format {ext}");
                    return null;
            }

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type))
            {
                var req = www.SendWebRequest();
                while (!req.isDone) { }

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[GorillaAlbums] Failed to load audio: {www.error}");
                    return null;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                clip.name = Path.GetFileNameWithoutExtension(path);
                return clip;
            }
        }

        public static void PlayTrack(GameObject shelf, AudioClip clip)
        {
            if (shelf == null || clip == null) return;

            AudioSource source = shelf.GetComponent<AudioSource>();
            if (source == null)
            source = shelf.AddComponent<AudioSource>();

            source.clip = clip;
            source.loop = false;
            source.playOnAwake = false;
            source.Play();
        }
    }
}
