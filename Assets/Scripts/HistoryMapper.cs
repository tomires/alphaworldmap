using SFB;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AlphaWorldMap
{
    [RequireComponent(typeof(LineRenderer))]
    public class HistoryMapper : MonoSingleton<MapTiler>
    {

        private LineRenderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<LineRenderer>();
            StandaloneFileBrowser.OpenFilePanelAsync("Select log to process", "", "csv", false, PlayLog);
        }

        private void PlayLog(string[] path)
        {
            var coords = new List<Vector3>();

            using (var reader = new StreamReader(path[0]))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split(Constants.CSV_DELIMETER);
                    var tileCoord = Utils.WorldToTileCoords(new Vector2(int.Parse(line[1]), int.Parse(line[0])));
                    coords.Add(new Vector3(tileCoord.x, tileCoord.y, Constants.PATH_Z_POS));
                }
            }

            _renderer.positionCount = coords.Count;
            _renderer.SetPositions(coords.ToArray());
        }
    }
}
