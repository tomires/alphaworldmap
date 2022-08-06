using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AlphaWorldMap
{
    [RequireComponent(typeof(LineRenderer))]
    public class RecordingController : MonoSingleton<RecordingController>
    {
        [SerializeField] private Material pathDefaultMaterial;
        [SerializeField] private Material pathRecordingMaterial;

        private LineRenderer _renderer;
        private bool _recording = false;
        private string _recordingPath;
        private float _rendererBaseWidth;

        private void Start()
        {
            Directory.CreateDirectory(Constants.RECORDINGS_DIRECTORY);
            CameraController.Instance.OnRuntimeCoordinatesUpdated += RecordCoordinates;
            CameraController.Instance.OnZoomLevelChanged += ResizeLineRenderer;
            InterfaceManager.Instance.OnRecordButtonClicked += OnRecordButtonClicked;
            InterfaceManager.Instance.OnRecordingClicked += PlayRecording;
            _renderer = GetComponent<LineRenderer>();
            _rendererBaseWidth = _renderer.startWidth / Constants.ZOOM_LEVEL_DEFAULT;
            UpdateRecordingsList();
        }

        public void OnRecordButtonClicked()
        {
            _recording = !_recording;
            _renderer.material = _recording ? pathRecordingMaterial : pathDefaultMaterial;
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
            _renderer.positionCount++;

            var tileCoords = Utils.WorldToTileCoords(coords);
            _renderer.SetPosition(_renderer.positionCount - 1, new Vector3(tileCoords.x, tileCoords.y, Constants.PATH_Z_POS));
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

            var minCoord = new Vector2(float.MaxValue, float.MaxValue);
            var maxCoord = new Vector2(float.MinValue, float.MinValue);

            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split(Constants.CSV_DELIMETER);
                    var worldCoord = new Vector2(int.Parse(line[0]), int.Parse(line[1]));
                    minCoord.x = Mathf.Min(minCoord.x, worldCoord.x);
                    maxCoord.x = Mathf.Max(maxCoord.x, worldCoord.x);
                    minCoord.y = Mathf.Min(minCoord.y, worldCoord.y);
                    maxCoord.y = Mathf.Max(maxCoord.y, worldCoord.y);
                    var tileCoord = Utils.WorldToTileCoords(worldCoord);
                    coords.Add(new Vector3(tileCoord.x, tileCoord.y, Constants.PATH_Z_POS));
                }
            }

            _renderer.positionCount = coords.Count;
            _renderer.SetPositions(coords.ToArray());

            var centerCoords = new Vector2(minCoord.x + (maxCoord.x - minCoord.x) / 2f, minCoord.y + (maxCoord.y - minCoord.y) / 2f);
            CameraController.Instance.JumpToCoordinates(centerCoords);
        }

        private void ResizeLineRenderer(float zoomLevel)
        {
            _renderer.startWidth = _renderer.endWidth = _rendererBaseWidth * zoomLevel;
        }
    }
}
