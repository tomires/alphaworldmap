using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AlphaWorldMap
{
    public class MapTiler : MonoSingleton<MapTiler>
    {
        [SerializeField] private SpriteRenderer tilePrefab;
        private Stack<SpriteRenderer> _tilePool = new Stack<SpriteRenderer>();
        private Dictionary<Vector2, SpriteRenderer> _renderedTiles = new Dictionary<Vector2, SpriteRenderer>();

        public void RenderTiles(Vector2 centerCoords)
        {
            RemoveFarTiles(centerCoords);

            for (var xi = centerCoords.x - Constants.RENDER_RADIUS; xi <= centerCoords.x + Constants.RENDER_RADIUS; xi++)
            {
                for (var yi = centerCoords.y - Constants.RENDER_RADIUS; yi <= centerCoords.y + Constants.RENDER_RADIUS; yi++)
                {
                    var tileCoords = new Vector2(xi, yi);
                    if (_renderedTiles.ContainsKey(tileCoords)) continue;
                    var big = Utils.GetBigTileCoords(tileCoords);
                    var path = Application.persistentDataPath + string.Format(Constants.TILE_PATH, big.Item1, big.Item2, xi, yi);
                    var tile = GetFreshTile(tileCoords);
                    SetTexture(ref tile, path);
                    _renderedTiles.Add(tileCoords, tile);
                }
            }
        }

        private void RemoveFarTiles(Vector2 centerCoords)
        {
            var tilesToRemove = new Stack<Vector2>();

            foreach (var tile in _renderedTiles)
            {
                if (Mathf.Abs(tile.Key.x - centerCoords.x) > Constants.RENDER_RADIUS 
                    || Mathf.Abs(tile.Key.y - centerCoords.y) > Constants.RENDER_RADIUS)
                {
                    ReturnTile(tile.Value);
                    tilesToRemove.Push(tile.Key);
                }
            }

            while (tilesToRemove.Count > 0)
                _renderedTiles.Remove(tilesToRemove.Pop());
        }

        private void SetTexture(ref SpriteRenderer renderer, string path)
        {
            if (!File.Exists(path))
            {
                renderer.sprite = null;
                return;
            }

            var bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), Constants.TILE_PIXEL_WIDTH);
            renderer.sprite = sprite;
        }

        private SpriteRenderer GetFreshTile(Vector2 coords)
        {
            coords.y *= -1;
            if (_tilePool.Count == 0)
                return Instantiate(tilePrefab, coords, Quaternion.identity);

            var tile = _tilePool.Pop();
            tile.transform.position = coords;
            tile.gameObject.SetActive(true);
            return tile;
        }

        private void ReturnTile(SpriteRenderer tile)
        {
            tile.gameObject.SetActive(false);
            _tilePool.Push(tile);
        }
    }
}
