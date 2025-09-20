using UnityEngine;
using UnityEngine.InputSystem;
using GorillaAlbums.Behaviours;

namespace GorillaAlbums.Behaviours
{
    public class MusicUpdater : MonoBehaviour
    {
        private AudioSource _audioSource;
        private ImageManager.AlbumContent _currentAlbum;
        private bool _isPaused = false;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            PlayNextAlbum();
        }

        private void Update()
        {
            if (_audioSource.clip == null) return;

            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            {
                if (_audioSource.isPlaying)
                {
                    _audioSource.Pause();
                    _isPaused = true;
                }
                else if (_isPaused)
                {
                    _audioSource.UnPause();
                    _isPaused = false;
                }

                UpdateNowPlayingText();
            }

            if (!_isPaused && !_audioSource.isPlaying && _audioSource.time >= _audioSource.clip.length)
            {
                PlayNextAlbum();
            }
        }

        private void PlayNextAlbum()
        {
            _currentAlbum = ImageManager.PickNextAlbum();
            if (_currentAlbum == null) return;

            var display = transform.FindDeepChild("RecordDisplay")?.GetComponentInChildren<Renderer>();
            if (display != null)
                display.material.mainTexture = _currentAlbum.Cover;

            _audioSource.clip = _currentAlbum.Track;
            _audioSource.loop = false;
            _audioSource.Play();

            _isPaused = false;
            UpdateNowPlayingText();
        }

        private void UpdateNowPlayingText()
        {
            var nowPlayingText = transform.FindDeepChild("NowPlaying")?.GetComponent<TMPro.TextMeshPro>();
            if (nowPlayingText == null || _currentAlbum == null) return;

            nowPlayingText.text = _isPaused
                ? $"Paused, Please press the 'M' key to continue"
                : $"Now Playing: {_currentAlbum.Name}";
        }
    }
}
