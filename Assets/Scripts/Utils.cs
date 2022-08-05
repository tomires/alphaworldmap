using AlphaWorldMap.Models;
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace AlphaWorldMap
{
    public static class Utils
    {
        public static (int, int) GetBigTileCoords(Vector2 tileCoords) 
            => (Mathf.FloorToInt(tileCoords.x / Constants.BIGTILE_WIDTH), Mathf.FloorToInt(tileCoords.y / Constants.BIGTILE_WIDTH));

        public static Vector2 WorldToTileCoords(Vector2 worldCoords)
            => Constants.GZ_COORDS + Constants.WORLD_COORD_MULTIPLIER * new Vector2(worldCoords.x, worldCoords.y);

        public static (Vector2, Direction) GetRuntimeCoordinates()
        {
            var title = GetActiveWindowTitle();
            if (!title.Contains(Constants.AW_HEADER)) return (Constants.RUNTIME_COORDS_DEFAULT, Direction.X);
            var coords = title.Replace(Constants.AW_HEADER, string.Empty).Split(' ');
            var direction = (Direction)Enum.Parse(typeof(Direction), coords[3]);
            if (coords[0] == "ground") return (Vector2.zero, direction); /* ground zero */
            var lat = coords[0].Contains("N")
                ? int.Parse(coords[0].Replace("N", string.Empty))
                : -int.Parse(coords[0].Replace("S", string.Empty));
            var lon = coords[1].Contains("E")
                ? int.Parse(coords[1].Replace("E", string.Empty))
                : -int.Parse(coords[1].Replace("W", string.Empty));
            return (new Vector2(lon, lat), direction);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            short X,
            short Y,
            short cx,
            short cy,
            uint uFlags
        );

        [DllImport("user32.dll")]
        public static extern System.IntPtr SetWindowLong(
             IntPtr hWnd,
             int nIndex,
             uint dwNewLong
        );

        [DllImport("user32.dll")]
        public static extern System.IntPtr GetWindowLong(
            IntPtr hWnd,
            int nIndex
        );

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
                return Buff.ToString();
            return "";
        }
    }
}
