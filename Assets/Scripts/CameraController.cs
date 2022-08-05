using AlphaWorldMap.Models;
using System.Threading.Tasks;
using UnityEngine;

namespace AlphaWorldMap
{
    public class CameraController : MonoSingleton<MapTiler>
    {
        [SerializeField] private GameObject runtimePositionIndicator;
        [SerializeField] private Transform runtimeDirectionIndicator;
        private bool _dragging = false;
        private Vector3 _previousMousePosition;
        private float _updateTilesTimer;

        private void Start()
        {
            UpdateTiles();
            RuntimePositioner();
            Application.runInBackground = true;
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

                }
                await Task.Delay(Constants.RUNTIME_COORDS_POLLING_PERIOD);
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _previousMousePosition = Input.mousePosition;
                _updateTilesTimer = Constants.TILE_UPDATE_PERIOD;
                _dragging = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _dragging = false;
                UpdateTiles(true);
            }

            if (_dragging)
            {
                var mousePosition = Input.mousePosition;
                var delta = _previousMousePosition - mousePosition;
                Camera.main.transform.position += Constants.MOUSE_MULTIPLIER * delta;
                _previousMousePosition = mousePosition;
                UpdateTiles();
            }
        }

        private void UpdateTiles(bool force = false)
        {
            _updateTilesTimer -= Time.deltaTime;
            if (_updateTilesTimer < 0 || force)
            {
                MapTiler.Instance.RenderTiles(new Vector2(
                    Mathf.FloorToInt(Camera.main.transform.position.x),
                    -Mathf.FloorToInt(Camera.main.transform.position.y)));
                _updateTilesTimer = Constants.TILE_UPDATE_PERIOD;
            }
        }
    }
}
