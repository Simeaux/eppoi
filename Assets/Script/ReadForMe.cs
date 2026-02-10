using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GoodEnough.TextToSpeech;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DBClass;

using System.Runtime.InteropServices;

public class ReadForMe : MonoBehaviour
{
    public TMP_Text testo;
    public Button Btn_play;
    public Button Btn_stop;
    public Button Container;
    public GameObject ReaderPanel;
    public Text txtLabel;

    private Color lightgray = new Color(0.8f, 0.8f, 0.8f, 1.0f);

#if UNITY_ANDROID
    AndroidJavaObject _tts;
    TTSListener listener;
#endif
    public void Start()
    {
#if UNITY_ANDROID
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            listener = new TTSListener();
            _tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", context, listener);
        }
#endif
    }
    private void OnEnable()
    {
        Container.enabled = true;
        if ((testo != null))
        {
            if (string.IsNullOrEmpty(testo.text) || string.IsNullOrWhiteSpace(testo.text))
            {
                Container.enabled = false;
                Container.gameObject.GetComponentInParent<Image>().color = lightgray;
                txtLabel.color = lightgray;
            }
            else
            {
                Container.gameObject.GetComponentInParent<Image>().color = Color.black;
                txtLabel.color = Color.black;
            }
        }
    }
    // Start is called before the first frame update
    void Update()
    {
        if (!(testo != null) && Container.enabled)
        {
            Container.enabled = false;
            Container.gameObject.GetComponentInParent<Image>().color = lightgray;
            txtLabel.color = lightgray;
        }
        else if (testo != null && testo.isActiveAndEnabled && !Container.enabled)
        {
            Container.enabled = true;
            Container.gameObject.GetComponentInParent<Image>().color = Color.black;
            txtLabel.color = Color.black;
        }
        if (TTS.IsSpeaking && Btn_play.gameObject.activeSelf)
            setPlayButton(false);
        if (!TTS.IsSpeaking && !Btn_play.gameObject.activeSelf)
            setPlayButton(true);
    }
    private Process ttsProcess;

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _iosSpeak(string text, string language);
    [DllImport("__Internal")]
    private static extern void _iosStopSpeak();
#endif

    public void Speak()
    {

#if UNITY_IOS        
        var lingua_selezionata = @"it-IT";
        if (PlayerPrefs.GetInt("lingua_selezionata") == 2)
            lingua_selezionata = "en-US";
        _iosSpeak(testo.text, lingua_selezionata);
#elif UNITY_ANDROID
        SpeakAndroid(testo.text, 0);
#else
        // Esempio rapido per testare l'audio su Mac Editor
        StopSpeak();

        ttsProcess = new Process();
        ttsProcess.StartInfo.FileName = "say";
        ttsProcess.StartInfo.Arguments = testo.text;
        ttsProcess.Start();
        //System.Diagnostics.Process.Start("say", testo.text);

#endif
        setPlayButton(false);
    }

    private void SpeakAndroid(string message, int tipo_chiamata)
    {
#if UNITY_ANDROID
        if (listener != null)
        {
            _tts.Call<int>("speak", message, tipo_chiamata, null, "uniqueId");
        }
        else
        {
            UnityEngine.Debug.LogWarning("TTS non ancora inizializzato!");
        }
#endif
    }

    // Listener necessario per l'inizializzazione Android
    class TTSListener : AndroidJavaProxy
    {
        public TTSListener() : base("android.speech.tts.TextToSpeech$OnInitListener") { }
        void onInit(int status) { }
    }


    public void StopSpeak()
    {
#if UNITY_IOS
        _iosStopSpeak();
#elif UNITY_ANDROID
        StopAndroid();
#else
        if (ttsProcess != null && !ttsProcess.HasExited)
        {
            ttsProcess.Kill(); // Chiude istantaneamente il processo 'say'
            ttsProcess.Dispose();
            ttsProcess = null;
        }
#endif
        //TTS.Stop();
        setPlayButton(true);
    }
    // Metodo Stop per Android
    private void StopAndroid()
    {
#if UNITY_ANDROID
        _tts.Call<int>("stop");
#endif
    }
    public void PauseSpeak()
    {
        if (ttsProcess != null && !ttsProcess.HasExited)
        {
            Process.Start("kill", "-STOP " + ttsProcess.Id);
        }
        //   TTS.Pause();
        setPlayButton(true);
    }
    private void setPlayButton(bool set)
    {
        //Btn_play.gameObject.SetActive(set);
        //Btn_stop.gameObject.SetActive(!Btn_play.gameObject.activeSelf);
    }
    public void OpenCloseReader(bool open)
    {
        Container.gameObject.SetActive(!open);
        ReaderPanel.SetActive(open);
        if (open)
        {
            Color _c = new Color();
            if (ColorUtility.TryParseHtmlString("#E8531E", out _c))
                txtLabel.color = _c;
        }
    }
}
