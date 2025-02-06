using System.Collections;
using Mapbox.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Function : MonoBehaviour
{

    private void Start()
    {
        StartCoroutine(nameof(CallService));
    }

    //Test chiamata webservice
    public static bool WebRequestResultIsError(UnityWebRequest request)
    {
#if UNITY_2020_3_OR_NEWER
        return (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError);
#else
            return (request.isNetworkError || request.isHttpError);
#endif
    }

    IEnumerator CallService()
    {
        string indirizzo = "https://www.comune.macerata.it/wp-json/rest_api_ws/v1/wsGETLuogo";
        var www = UnityWebRequest.Get(indirizzo);

        yield return www.SendWebRequest();

        if (WebRequestResultIsError(www))
        {
            Debug.Log(www.error);
            Debug.Log(indirizzo);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
            CreateTextObjects(www.downloadHandler.text);
        }
    }

    void CreateTextObjects(string text)
    {
        JObject json = JObject.Parse(text);
        Debug.Log(json);
    }

}