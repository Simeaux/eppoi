using System.Collections;
using System.Collections.Generic;
using GoodEnough.TextToSpeech;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DBClass;

public class ReadForMe : MonoBehaviour
{
    public TMP_Text testo;
    public Button Btn_play;
    public Button Btn_stop;
    public Button Container;
    public GameObject ReaderPanel;
    public Text txtLabel;

    private Color lightgray = new Color(0.8f, 0.8f, 0.8f, 1.0f);

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
    public void Speak()
    {
        var speechParameters = new SpeechUtteranceParameters();
        speechParameters.PitchMultiplier = 1f;
        speechParameters.SpeechRate = 0.5f;
        speechParameters.Volume = 1f;
        speechParameters.PreUtteranceDelay = 0.1f;
        speechParameters.PreUtteranceDelay = 0.3f;
        speechParameters.Voice = TTS.GetVoiceForLanguage(PlayerPrefs.GetInt("lingua_selezionata") == 1 ? "it-IT" : "en-UK");

        TTS.Speak(testo.text, speechParameters);
        setPlayButton(false);
    }
    public void StopSpeak()
    {
        TTS.Stop();
        setPlayButton(true);
    }
    public void PauseSpeak()
    {
        TTS.Pause();
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
