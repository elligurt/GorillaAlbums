using GorillaAlbums.Behaviours;
using UnityEngine;

namespace GorillaAlbums.Behaviours
{
    public static class ErrorManager
    {
        public static void CheckAndShowError(GameObject shelf)
        {
            if (shelf == null)
                return;

            if (ImageManager.GetLoadedImageCount() < 4)
            {
                Transform errorObj = shelf.transform.Find("Error");
                if (errorObj != null)
                {
                    errorObj.gameObject.SetActive(true);
                }
            }
            else
            {
                Transform errorObj = shelf.transform.Find("Error");
                if (errorObj != null)
                    errorObj.gameObject.SetActive(false);
            }
        }
    }
}
