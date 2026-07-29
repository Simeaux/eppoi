#region Includes
using UnityEngine;
using UnityEngine.UI;
#endregion

namespace TS.PageSlider.Demo
{
    public class SliderPage : MonoBehaviour
    {
        #region Variables

        [Header("Children")]
        [SerializeField] private Image _image;

        public Sprite _Image
        {
            get { return _image.sprite; }
            set
            {
                _image.sprite = value;
                var aspectFitter = _image.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                if (aspectFitter == null)
                {
                    aspectFitter = _image.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                }

                // 3. Configuri il componente su Envelope Parent e imposti il corretto rapporto d'aspetto
                if (_image.sprite != null)
                {
                    aspectFitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;

                    // Calcola il rapporto Larghezza / Altezza
                    var tex = _image.sprite.texture;
                    aspectFitter.aspectRatio = (float)tex.width / tex.height;
                }
            }
        }

        #endregion
    }
}