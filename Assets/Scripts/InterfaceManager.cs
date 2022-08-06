using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AlphaWorldMap
{
    public class InterfaceManager : MonoSingleton<InterfaceManager>
    {
        [SerializeField] Button minimizeButton;
        [SerializeField] Button maximizeButton;
        [SerializeField] Button recordButton;
        [SerializeField] Image recordIcon;
        [SerializeField] Image stopRecordingIcon;
        [SerializeField] GameObject recordingsContainer;
        [SerializeField] GameObject border;
        [SerializeField] RecordingItem recordingItemPrefab;
        [SerializeField] Transform recordingItemParent;

        public Action OnWindowMinimized;
        public Action OnWindowMaximized;
        public Action OnRecordButtonClicked;
        public Action<long> OnRecordingClicked;

        private IntPtr _hWnd;
        private Utils.RECT _windowRect;
        private Dictionary<long, RecordingItem> _spawnedRecordingItems = new Dictionary<long, RecordingItem>();
        private RecordingItem _selectedRecordingItem;

        private void Start()
        {
            minimizeButton.onClick.AddListener(OnMinimizeWindow);
            maximizeButton.onClick.AddListener(OnMaximizeWindow);
            recordButton.onClick.AddListener(() => OnRecordButtonClicked?.Invoke());
            _hWnd = Utils.GetActiveWindow();
        }

        public void PropagateRecordingStatus(bool recording)
        {
            recordIcon.enabled = !recording;
            stopRecordingIcon.enabled = recording;

            _selectedRecordingItem?.ChangeHighlight(false);
            OnRecordingClicked?.Invoke(long.MinValue);
            _selectedRecordingItem = null;
        }

        public void ListRecordings(List<long> timestamps)
        {
            foreach (var item in _spawnedRecordingItems)
                Destroy(item.Value.gameObject);
            _spawnedRecordingItems.Clear();

            foreach (var timestamp in timestamps)
            {
                var item = Instantiate(recordingItemPrefab, recordingItemParent);
                item.Initialize(timestamp, OnRecordingSelected);
                _spawnedRecordingItems.Add(timestamp, item);
            }
        }

        private void OnRecordingSelected(long timestamp)
        {
            _selectedRecordingItem?.ChangeHighlight(false);
            if (_selectedRecordingItem == _spawnedRecordingItems[timestamp])
            {
                _selectedRecordingItem = null;
                OnRecordingClicked?.Invoke(long.MinValue);
                return;
            }
            _selectedRecordingItem = _spawnedRecordingItems[timestamp];
            _selectedRecordingItem.ChangeHighlight(true);
            OnRecordingClicked?.Invoke(timestamp);
        }

        private void OnMinimizeWindow()
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
            OnWindowMinimized?.Invoke();
        }

        private void OnMaximizeWindow()
        {
            if (Application.isEditor) return;
            SetUIVisibility(false);
            var style = Utils.GetWindowLong(_hWnd, Constants.GWL_STYLE).ToInt32();
            Utils.SetWindowLong(_hWnd, Constants.GWL_STYLE, (uint)(style | Constants.WS_CAPTION | Constants.WS_SIZEBOX));
            Utils.SetWindowPos(_hWnd, Constants.HWND_NOTOPMOST, 
                (short)_windowRect.Left, (short)_windowRect.Top, 
                (short)(_windowRect.Right - _windowRect.Left), (short)(_windowRect.Bottom - _windowRect.Top), 
                Constants.SWP_SHOWWINDOW);
            OnWindowMaximized?.Invoke();
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
