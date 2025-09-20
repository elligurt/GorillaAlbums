using System;
using System.Threading.Tasks;
using BepInEx;
using GorillaAlbums.Behaviours;
using GorillaAlbums.Tools;
using UnityEngine;

namespace GorillaAlbums
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        private GameObject _shelfPrefab;
        private bool _initialized;

        private void Start()
        {
            Instance = this;
            GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
        }

        private void OnPlayerSpawned()
        {
            if (_initialized) return;
            _initialized = true;
            _ = SetupObjects();
        }

        private async Task SetupObjects()
        {
            try
            {
                ImageManager.CreateImageFolder();
                ImageManager.LoadAllAlbums();

                _shelfPrefab = await AssetLoader.LoadAsset<GameObject>("GorillaAlbums");
                if (_shelfPrefab == null)
                {
                    Debug.LogError("[GorillaAlbums] Failed to load shelf bundle");
                    return;
                }

                var shelfInstance = Instantiate(_shelfPrefab);
                shelfInstance.SetActive(true);
                shelfInstance.transform.position = new Vector3(-64.7606f, 12.1637f, -84.5819f);
                shelfInstance.transform.rotation = Quaternion.Euler(0f, 271.3724f, 0f);


                ImageManager.ApplyImagesToRecords(shelfInstance);
                ErrorManager.CheckAndShowError(shelfInstance);

                if (ErrorManager.ShouldPlayMusic)
                {
                    shelfInstance.AddComponent<MusicUpdater>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[GorillaAlbums] Error setting up shelves: " + ex);
            }
        }
    }
}
