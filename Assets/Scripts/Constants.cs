using UnityEngine;

namespace AlphaWorldMap
{
    public static class Constants
    {
        public static string TILE_DIRECTORY = $"/{{0}}_{{1}}";
        public static string TILE_PATH = $"{TILE_DIRECTORY}/9_{{2}}_{{3}}.png";
        public static string TILE_URL = $"http://mapper.activeworlds.com/alphaworld/{{0}}_{{1}}/9_{{2}}_{{3}}.png";
        public const string AW_HEADER = "Activeworlds - AlphaWorld at ";
        public const char CSV_DELIMETER = ':';

        public static int TILE_PIXEL_WIDTH = 320;
        public static int BIGTILE_WIDTH = 8;
        public static int X_MIN = 224;
        public static int X_MAX = 303;
        public static int Y_MIN = 232;
        public static int Y_MAX = 279;
        public static int RENDER_RADIUS = 8;
        public static int RUNTIME_COORDS_POLLING_PERIOD = 100;
        public static int MINIMAP_SIZE = 400;
        public static int MINIMAP_MARGIN_TOP = 100;
        public static int MINIMAP_MARGIN_RIGHT = 50;

        public static float ZOOM_LEVEL_DEFAULT = 3;
        public static float ZOOM_LEVEL_MIN = 1;
        public static float ZOOM_LEVEL_MAX = 5;
        public static float ZOOM_LEVEL_LOCKED = .5f;
        public static float TILE_UPDATE_DISTANCE = 1.5f;
        public static float MOUSE_MULTIPLIER = .00105f;
        public static float WORLD_COORD_MULTIPLIER = .0078125f; /* 1/128 */
        public static float PATH_Z_POS = -.1f;
        public static float REFERENCE_DPI = 163f;

        public static Vector2 GZ_COORDS = new Vector2(255.5f, -255.5f);
        public static Vector2 RUNTIME_COORDS_DEFAULT = new Vector2(float.MaxValue, float.MaxValue);

        public static System.IntPtr HWND_TOP = new System.IntPtr(0);
        public static System.IntPtr HWND_TOPMOST = new System.IntPtr(-1);
        public static System.IntPtr HWND_NOTOPMOST = new System.IntPtr(-2);

        public const int SWP_SHOWWINDOW = 0x40;
        public const uint WS_SIZEBOX = 0x00040000;
        public const int GWL_STYLE = -16;
        public const int WS_BORDER = 0x00800000;
        public const int WS_DLGFRAME = 0x00400000;
        public const int WS_CAPTION = WS_BORDER | WS_DLGFRAME;
    }
}
