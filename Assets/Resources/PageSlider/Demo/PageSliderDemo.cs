#region Includes
using System;

using UnityEngine;
#endregion

namespace TS.PageSlider.Demo
{
    public class PageSliderDemo : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [SerializeField] private PageSlider _slider;
        [SerializeField] private SliderPage _pagePrefab;

        [Header("Configuration")]
        [SerializeField] private SliderItem[] _items;

        #endregion

        private void Start()
        {
            for (int i = 0; i < _items.Length; i++)
            {
                var page = Instantiate(_pagePrefab);
                page._Image = _items[i]._Image;

                _slider.AddPage((RectTransform)page.transform);
            }
        }
    }

    [Serializable]
    public class SliderItem
    {
        [SerializeField] private string _text;
        [SerializeField] private Sprite _image;

        public string _Text { get { return _text; } }
        public Sprite _Image { get { return _image; } }
    }
}
