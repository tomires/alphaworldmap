using System;
using UnityEngine;
using UnityEngine.UI;

namespace AlphaWorldMap
{
    public class InterfaceManager : MonoSingleton<InterfaceManager>
    {
        [SerializeField] Button minimizeButton;
        [SerializeField] Button maximizeButton;
        [SerializeField] Button recordButton;
        [SerializeField] GameObject recordingsContainer;
        [SerializeField] GameObject border;

        public Action WindowMinimized;
        public Action WindowMaximized;

        private System.IntPtr _hWnd;
        private Utils.RECT _windowRect;

        private void Start()
        {
            minimizeButton.onClick.AddListener(MinimizeWindow);
            maximizeButton.onClick.AddListener(MaximizeWindow);
            _hWnd = Utils.GetActiveWindow();
        }

        public void MinimizeWindow()
        {
            if (Application.isEditor) return;
            SetUIVisibility(true);
            var style = Utils.GetWindowLong(_hWnd, Constants.GWL_STYLE).ToInt32();
            Utils.GetWindowRect(_hWnd, ref _windowRect);
            Utils.SetWindowLong(_hWnd, Constants.GWL_STYLE, (uint)(style & ~(Constants.WS_CAPTION | Constants.WS_SIZEBOX)));
            var res = Screen.currentResolution;
            var dpiMultiplier = Screen.dpi / Constants.REFERENCE_DPI;
            var minimapSize = dpiMultiplier * Constants.MINIMAP_SIZE;
            Utils.SetWindowPos(
                _hWnd, Constants.HWND_TOPMOST, 
                (short)(res.width - minimapSize - dpiMultiplier * Constants.MINIMAP_MARGIN_RIGHT), 
                (short)(dpiMultiplier * Constants.MINIMAP_MARGIN_TOP), 
                (short)minimapSize, (short)minimapSize, 
                Constants.SWP_SHOWWINDOW);
            WindowMinimized?.Invoke();
        }

        public void MaximizeWindow()
        {
            if (Application.isEditor) return;
            SetUIVisibility(false);
            var style = Utils.GetWindowLong(_hWnd, Constants.GWL_STYLE).ToInt32();
            Utils.SetWindowLong(_hWnd, Constants.GWL_STYLE, (uint)(style | Constants.WS_CAPTION | Constants.WS_SIZEBOX));
            Utils.SetWindowPos(_hWnd, Constants.HWND_NOTOPMOST, 
                (short)_windowRect.Left, (short)_windowRect.Top, 
                (short)(_windowRect.Right - _windowRect.Left), (short)(_windowRect.Bottom - _windowRect.Top), 
                Constants.SWP_SHOWWINDOW);
            WindowMaximized?.Invoke();
        }

        private void SetUIVisibility(bool minimized)
        {
            border.SetActive(minimized);
            minimizeButton.gameObject.SetActive(!minimized);
            maximizeButton.gameObject.SetActive(minimized);
            recordButton.gameObject.SetActive(!minimized);
            recordingsContainer.gameObject.SetActive(!minimized);
        }
    }
}
