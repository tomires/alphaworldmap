using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AlphaWorldMap
{
    public class Downloader : MonoSingleton<MapTiler>
    {
        private void Start()
        {
            StartCoroutine(DownloadAll());
        }

        private IEnumerator DownloadAll()
        {
            var x = Constants.X_MIN;
            while (x <= Constants.X_MAX)
            {
                var y = Constants.Y_MIN;
                while (y <= Constants.Y_MAX)
                {
                    var big = Utils.GetBigTileCoords(new Vector2(x, y));
                    var path = Application.persistentDataPath + string.Format(Constants.TILE_PATH, big.Item1, big.Item2, x, y);
                    var directory = Application.persistentDataPath + string.Format(Constants.TILE_DIRECTORY, big.Item1, big.Item2);
                    if (!File.Exists(path))
                    {
                        var url = string.Format(Constants.TILE_URL, big.Item1, big.Item2, x, y);
                        Debug.Log(url);
                        var www = UnityWebRequestTexture.GetTexture(url);
                        yield return www.SendWebRequest();
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            Directory.CreateDirectory(directory);
                            File.WriteAllBytes(path, www.downloadHandler.data);
                        }
                    }
                    y++;
                }
                x++;
            }
        }
    }
}
