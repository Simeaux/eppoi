namespace Mapbox.Unity.Utilities
{
    using TMPro;
    using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Button))]
	public class OpenUrlOnButtonClick : MonoBehaviour
	{
		//[SerializeField]
		//TMP_Text _url;

		protected virtual void Awake()
		{
			//GetComponent<Button>().onClick.AddListener(VisitUrl);
		}

        public void VisitUrl(TMP_Text url)
        {
            if (!string.IsNullOrEmpty(url.text))
                Application.OpenURL(url.text);
        }
        public void MakeaCall(TMP_Text url)
        {
            if (!string.IsNullOrEmpty(url.text))
                Application.OpenURL($"tel://{url.text}");
        }
        public void SendMail(TMP_Text url)
        {
            if (!string.IsNullOrEmpty(url.text))
                Application.OpenURL($"mailto:{url.text}");
        }
    }
}
