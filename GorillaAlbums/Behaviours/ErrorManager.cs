using UnityEngine;

namespace GorillaAlbums.Behaviours
{
    public static class ErrorManager
    {
        public static bool ShouldPlayMusic { get; private set; } = false;

        public static void CheckAndShowError(GameObject shelf)
        {
            if (shelf == null) return;

            bool showError = ImageManager.HasError || ImageManager.Albums.Count < 4;
            ShouldPlayMusic = !showError;

            Transform errorObj = shelf.transform.Find("Error");
            if (errorObj != null)
                errorObj.gameObject.SetActive(showError);
        }
    }
}
