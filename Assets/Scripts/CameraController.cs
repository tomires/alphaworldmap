using AlphaWorldMap.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AlphaWorldMap
{
    public class CameraController : MonoSingleton<CameraController>
    {
        [SerializeField] private GameObject runtimePositionIndicator;
        [SerializeField] private Transform runtimeDirectionIndicator;
        private bool _dragging = false;
        private Vector3 _previousMousePosition;
        private Vector2 _lastUpdatedPosition = Vector2.zero;
        private float _zoomLevel = Constants.ZOOM_LEVEL_DEFAULT;
        private bool _inputLocked = false;

        public Action<Vector2> OnRuntimeCoordinatesUpdated;

        private void Start()
        {
            UpdateTiles();
            RuntimePositioner();
            Application.runInBackground = true;
            SetZoomLevel();
            InterfaceManager.Instance.OnWindowMinimized += LockInput;
            InterfaceManager.Instance.OnWindowMaximized += UnlockInput;
        }

        public void JumpToCoordinates(Vector2 worldCoords)
        {
            var tileCoords = Utils.WorldToTileCoords(worldCoords);
            Camera.main.transform.position = new Vector3(
                tileCoords.x, tileCoords.y, Camera.main.transform.position.z);
        }

        private async void RuntimePositioner()
        {
            while (Application.isPlaying)
            {
                var playerWorldCoords = Utils.GetRuntimeCoordinates();
                var awInFocus = playerWorldCoords.Item1 != Constants.RUNTIME_COORDS_DEFAULT;
                runtimePositionIndicator.SetActive(awInFocus);
                if (awInFocus)
                {
                    OnRuntimeCoordinatesUpdated?.Invoke(playerWorldCoords.Item1);
                    var playerTileCoords = Utils.WorldToTileCoords(playerWorldCoords.Item1);
                    Camera.main.transform.position = new Vector3(
                        playerTileCoords.x, playerTileCoords.y, Camera.main.transform.position.z);

                    runtimeDirectionIndicator.localRotation = Quaternion.Euler(0, 0, playerWorldCoords.Item2 switch
                    {
                        Direction.N => 45,
                        Direction.NE => 0,
                        Direction.E => 315,
                        Direction.SE => 270,
                        Direction.S => 225,
                        Direction.SW => 180,
                        Direction.W => 135,
                        _ => 90
                    });

                    var distance = Vector2.Distance(Camera.main.transform.position, _lastUpdatedPosition);
                    if (distance > Constants.TILE_UPDATE_DISTANCE)
                        UpdateTiles();
                }
                await Task.Delay(Constants.RUNTIME_COORDS_POLLING_PERIOD);
            }
        }

        private void Update()
        {
            if (_inputLocked) return;

            if (Input.GetMouseButtonDown(0) && !Utils.IsMouseOverUIObject())
            {
                _previousMousePosition = Input.mousePosition;
                _dragging = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _dragging = false;
                UpdateTiles();
            }

            if (Input.mouseScrollDelta.y != 0)
                ChangeZoomLevel(Input.mouseScrollDelta.y < 0);

            if (_dragging)
            {
                var mousePosition = Input.mousePosition;
                var delta = _previousMousePosition - mousePosition;
                Camera.main.transform.position += Constants.MOUSE_MULTIPLIER * _zoomLevel * delta;
                _previousMousePosition = mousePosition;
            }
        }

        private void UpdateTiles()
        {
            _lastUpdatedPosition = Camera.main.transform.position;
            MapTiler.Instance.RenderTiles(new Vector2(
                Mathf.FloorToInt(Camera.main.transform.position.x),
                -Mathf.FloorToInt(Camera.main.transform.position.y)));
        }

        private void ChangeZoomLevel(bool increase)
        {
            _zoomLevel = increase
                ? Mathf.Min(_zoomLevel + 1f, Constants.ZOOM_LEVEL_MAX)
                : Mathf.Max(_zoomLevel - 1f, Constants.ZOOM_LEVEL_MIN);
            SetZoomLevel();
        }

        private void SetZoomLevel()
            => Camera.main.orthographicSize = _zoomLevel;

        private void LockInput()
        {
            _inputLocked = true;
            Camera.main.orthographicSize = Constants.ZOOM_LEVEL_LOCKED;
        }

        private void UnlockInput()
        {
            _inputLocked = false;
            Camera.main.orthographicSize = _zoomLevel;
        }
    }
}
