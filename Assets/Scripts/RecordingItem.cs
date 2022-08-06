using System;
using UnityEngine;
using UnityEngine.UI;

namespace AlphaWorldMap
{
    public class RecordingItem : MonoBehaviour
    {
        [SerializeField] private Image highlightImage;
        [SerializeField] private Button selectButton;
        [SerializeField] private Text labelText;

        public void Initialize(long timestamp, Action<long> onSelection)
        {
            ChangeHighlight(false);
            labelText.text = Utils.UnixTimestampToHumanReadable(timestamp);
            selectButton.onClick.AddListener(() => onSelection?.Invoke(timestamp));
        }

        public void ChangeHighlight(bool enabled)
        {
            var color = highlightImage.color;
            color.a = enabled ? 1f : 0f;
            highlightImage.color = color;
            labelText.color = enabled ? Color.white : Color.black;
        }
    }
}
