using System;
using System.Threading.Tasks;
using BepInEx;
using GorillaAlbums.Tools;
using UnityEngine;
using GorillaAlbums.Behaviours;

namespace GorillaAlbums
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        private GameObject _shelfPrefab;

        private void Awake()
        {
            Instance = this;
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        private async void OnGameInitialized(object sender, EventArgs e)
        {
            await SetupShelves();
        }

        private async Task SetupShelves()
        {
            try
            {
                ImageManager.CreateImageFolder();
                ImageManager.LoadAllImages();

                _shelfPrefab = await AssetLoader.LoadAsset<GameObject>("GorillaAlbums");
                if (_shelfPrefab == null)
                {
                    Debug.LogError("[GorillaAlbums] failed to load bundle");
                    return;
                }

                GameObject shelfInstance = Instantiate(_shelfPrefab);
                shelfInstance.SetActive(true);
                shelfInstance.transform.position = new Vector3(-64.7606f, 12.1637f, -84.5819f);
                shelfInstance.transform.rotation = Quaternion.Euler(0f, 271.3724f, 0f);

                ImageManager.ApplyImagesToPhotos(shelfInstance);

                Behaviours.ErrorManager.CheckAndShowError(shelfInstance);

            }
            catch (Exception ex)
            {
                Debug.LogError("[GorillaAlbums] Error setting up shelves: " + ex);
            }
        }
    }
}