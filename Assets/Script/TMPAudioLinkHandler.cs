using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TMPAudioLinkHandler : MonoBehaviour
{
    [Header("TextMeshPro")]
    public TMP_Text testo;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Interfaccia Audio")]
    public GameObject audioPanel;
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;
    public Slider progressSlider;
    public Slider volumeSlider;
    public TMP_Text tempoText;
    public TMP_Text titoloAudioText;

    private Camera cam;

    private string audioCorrenteUrl;

    private bool audioCaricato = false;
    private bool stoAggiornandoSlider = false;

    private Coroutine caricamentoAudio;


    private void Awake()
    {
        if (testo == null)
            testo = GetComponent<TMP_Text>();

        cam = Camera.main;

        // Nasconde inizialmente il pannello
        if (audioPanel != null)
            audioPanel.SetActive(false);

        // Collegamento pulsanti
        if (playButton != null)
            playButton.onClick.AddListener(PlayAudio);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(PausaAudio);

        if (stopButton != null)
            stopButton.onClick.AddListener(StopAudio);

        // Slider avanzamento
        if (progressSlider != null)
            progressSlider.onValueChanged.AddListener(CambiaPosizioneAudio);

        // Slider volume
        if (volumeSlider != null)
        {
            volumeSlider.value = 1f;
            volumeSlider.onValueChanged.AddListener(CambiaVolume);
        }
    }


    private void Update()
    {
        // Mouse
        if (Input.GetMouseButtonDown(0))
        {
            ControllaClick(Input.mousePosition);
        }

        // Touch Android
        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ControllaClick(Input.GetTouch(0).position);
        }

        AggiornaInterfacciaAudio();
    }


    private void ControllaClick(Vector3 posizione)
    {
        if (testo == null)
            return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            testo,
            posizione,
            cam
        );

        if (linkIndex == -1)
            return;

        TMP_LinkInfo linkInfo =
            testo.textInfo.linkInfo[linkIndex];

        string url = linkInfo.GetLinkID();

        if (string.IsNullOrEmpty(url))
            return;

        Debug.Log("Audio cliccato: " + url);

        // Mostra pannello
        if (audioPanel != null)
            audioPanel.SetActive(true);

        // Salva URL
        audioCorrenteUrl = url;

        // Ferma eventuale caricamento precedente
        if (caricamentoAudio != null)
            StopCoroutine(caricamentoAudio);

        // Carica nuovo audio
        caricamentoAudio = StartCoroutine(
            CaricaEAavviaAudio(url)
        );
    }


    private IEnumerator CaricaEAavviaAudio(string url)
    {
        Debug.Log("Download audio: " + url);

        audioCaricato = false;

        if (audioSource != null)
            audioSource.Stop();

        using (UnityWebRequest request =
               UnityWebRequestMultimedia.GetAudioClip(
                   url,
                   AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "Errore caricamento audio: " +
                    request.error
                );

                yield break;
            }

            AudioClip clip =
                DownloadHandlerAudioClip.GetContent(request);

            if (clip == null)
            {
                Debug.LogError("AudioClip nullo.");
                yield break;
            }

            if (audioSource == null)
            {
                Debug.LogError(
                    "AudioSource non assegnato."
                );

                yield break;
            }

            audioSource.Stop();

            audioSource.clip = clip;

            audioSource.volume =
                volumeSlider != null
                    ? volumeSlider.value
                    : 1f;

            audioSource.Play();

            audioCaricato = true;

            if (progressSlider != null)
            {
                progressSlider.minValue = 0;
                progressSlider.maxValue = clip.length;
                progressSlider.value = 0;
            }

            Debug.Log("Audio avviato.");
        }
    }


    private void PlayAudio()
    {
        if (audioSource == null)
            return;

        if (!audioCaricato || audioSource.clip == null)
            return;

        audioSource.Play();

        Debug.Log("Audio Play");
    }


    private void PausaAudio()
    {
        if (audioSource == null)
            return;

        if (audioSource.isPlaying)
        {
            audioSource.Pause();

            Debug.Log("Audio Pausa");
        }
    }


    public void StopAudio()
    {

        if (audioSource == null)
            return;

        audioSource.Stop();

        if (audioSource.clip != null)
            audioSource.time = 0;

        if (progressSlider != null)
            progressSlider.value = 0;

        AggiornaTempo(0);

        Debug.Log("Audio Stop");
    }


    private void CambiaPosizioneAudio(float valore)
    {
        if (stoAggiornandoSlider)
            return;

        if (audioSource == null)
            return;

        if (audioSource.clip == null)
            return;

        audioSource.time = valore;
    }


    private void CambiaVolume(float valore)
    {
        if (audioSource == null)
            return;

        audioSource.volume = valore;
    }


    private void AggiornaInterfacciaAudio()
    {
        if (audioSource == null)
            return;

        if (audioSource.clip == null)
            return;

        if (progressSlider != null)
        {
            stoAggiornandoSlider = true;

            progressSlider.value =
                audioSource.time;

            stoAggiornandoSlider = false;
        }

        AggiornaTempo(audioSource.time);
    }


    private void AggiornaTempo(float tempo)
    {
        if (tempoText == null)
            return;

        float durata =
            audioSource != null &&
            audioSource.clip != null
                ? audioSource.clip.length
                : 0;

        tempoText.text =
            FormattaTempo(tempo) +
            " / " +
            FormattaTempo(durata);
    }


    private string FormattaTempo(float secondi)
    {
        int minuti = Mathf.FloorToInt(secondi / 60f);
        int secondiRimanenti =
            Mathf.FloorToInt(secondi % 60f);

        return minuti.ToString("00") +
               ":" +
               secondiRimanenti.ToString("00");
    }


    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(PlayAudio);

        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(PausaAudio);

        if (stopButton != null)
            stopButton.onClick.RemoveListener(StopAudio);
    }
}