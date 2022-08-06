using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AlphaWorldMap
{
    [RequireComponent(typeof(LineRenderer))]
    public class RecordingController : MonoSingleton<RecordingController>
    {
        private LineRenderer _renderer;
        private bool _recording = false;
        private string _recordingPath;

        private void Start()
        {
            Directory.CreateDirectory(Constants.RECORDINGS_DIRECTORY);
            CameraController.Instance.OnRuntimeCoordinatesUpdated += RecordCoordinates;
            InterfaceManager.Instance.OnRecordButtonClicked += OnRecordButtonClicked;
            InterfaceManager.Instance.OnRecordingClicked += PlayRecording;
            _renderer = GetComponent<LineRenderer>();
            UpdateRecordingsList();
        }

        public void OnRecordButtonClicked()
        {
            _recording = !_recording;
            InterfaceManager.Instance.PropagateRecordingStatus(_recording);
            if (_recording)
                _recordingPath = string.Format(Constants.RECORDING_PATH, Utils.GetUnixTimestamp());
            else
                UpdateRecordingsList();
        }

        private void UpdateRecordingsList()
        {
            var timestamps = new List<long>();
            foreach (var filename in Directory.EnumerateFiles(Constants.RECORDINGS_DIRECTORY).Reverse())
                timestamps.Add(long.Parse(Path.GetFileName(filename)));
            InterfaceManager.Instance.ListRecordings(timestamps);
        }

        private void RecordCoordinates(Vector2 coords)
        {
            if (!_recording) return;
            using (var writer = new StreamWriter(_recordingPath, true))
                writer.WriteLine($"{coords.x}{Constants.CSV_DELIMETER}{coords.y}");
        }

        private void PlayRecording(long timestamp)
        {
            if (timestamp == long.MinValue)
            {
                _renderer.positionCount = 0;
                return;
            }

            var path = string.Format(Constants.RECORDING_PATH, timestamp);
            var coords = new List<Vector3>();

            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split(Constants.CSV_DELIMETER);
                    var tileCoord = Utils.WorldToTileCoords(new Vector2(int.Parse(line[0]), int.Parse(line[1])));
                    coords.Add(new Vector3(tileCoord.x, tileCoord.y, Constants.PATH_Z_POS));
                }
            }

            _renderer.positionCount = coords.Count;
            _renderer.SetPositions(coords.ToArray());
        }
    }
}
