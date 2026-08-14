using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CanvasStatistiche : MonoBehaviour
{
    [Header("Database")]
    private DBClass _DBClass;

    [Header("Text dove mostrare il peso")]
    public Text testoPeso;
    public Text testoPesoSistema;
    public Text testoPesoImmagini;
    public Text testoPesoTesti;

    [Header("Barra peso database")]
    public RectTransform barraPeso;

    [Header("Segmenti barra")]
    public Image segmentoSistema;
    public Image segmentoImmagini;
    public Image segmentoTesti;

    [Header("Riassunto comuni")]
    public Text riassuntoComuni;
    [Header("Lista comuni")]
    public Transform contentComuni;
    public GameObject prefabComune;


    private void Start()
    {
        AggiornaPesoApp();
        AggiornaListaComuni();
    }

    /// <summary>
    /// Calcola la dimensione totale dell'app e
    /// aggiorna statistiche e barra grafica.
    /// </summary>
    public void AggiornaPesoApp()
    {
        // ---------------------------------------------------------
        // DATABASE
        // ---------------------------------------------------------

        GameObject sqliteObject =
            GameObject.FindWithTag("SQLite");

        if (sqliteObject == null)
        {
            Debug.LogError(
                "[STATISTICHE] Oggetto con tag SQLite non trovato."
            );

            return;
        }

        _DBClass =
            sqliteObject.GetComponent<DBClass>();

        if (_DBClass == null)
        {
            Debug.LogError(
                "[STATISTICHE] DBClass non trovato."
            );

            return;
        }

        // ---------------------------------------------------------
        // PESO TOTALE DATI APP
        // ---------------------------------------------------------

        long dimensioneBytes = 0;

        dimensioneBytes =
            CalcolaDimensioneDirectory(
                Application.persistentDataPath
            );

        /*
        string percorsoDB = Path.Combine(Application.persistentDataPath, "mydatabase.db");


        if (File.Exists(percorsoDB))
        {
            dimensioneBytes =
                new FileInfo(percorsoDB).Length;
        }
        else
        {
            Debug.LogWarning(
                "[STATISTICHE] Database non trovato: " +
                percorsoDB
            );
        }
      */
        string peso =
            FormattaDimensione(dimensioneBytes);

        Debug.Log(
            "[STATISTICHE] Dimensione dati app: " +
            peso
        );

        if (testoPeso != null)
        {
            testoPeso.text = peso;
        }

        // ---------------------------------------------------------
        // PESO IMMAGINI
        // ---------------------------------------------------------

        long peso_immagini =
            _DBClass.GetIMMAGINI_PesoTabella();

        // ---------------------------------------------------------
        // PESO TESTI
        // ---------------------------------------------------------

        long peso_testi =
            _DBClass.GetTEXT_PesoTabella();

        // ---------------------------------------------------------
        // PESO SISTEMA
        // ---------------------------------------------------------

        long peso_sistema =
            dimensioneBytes -
            peso_immagini -
            peso_testi;

        // Evitiamo valori negativi nel caso in cui
        // i dati delle tabelle siano superiori alla
        // dimensione considerata.

        if (peso_sistema < 0)
            peso_sistema = 0;

        // ---------------------------------------------------------
        // TEXT SISTEMA
        // ---------------------------------------------------------

        if (testoPesoSistema != null)
        {
            testoPesoSistema.text =
                "Sistema: " +
                FormattaDimensione(
                    peso_sistema
                );
        }

        // ---------------------------------------------------------
        // TEXT TESTI
        // ---------------------------------------------------------

        if (testoPesoTesti != null)
        {
            testoPesoTesti.text =
                "Testi: " +
                FormattaDimensione(
                    peso_testi
                );
        }

        // ---------------------------------------------------------
        // TEXT IMMAGINI
        // ---------------------------------------------------------

        if (testoPesoImmagini != null)
        {
            testoPesoImmagini.text =
                "Comuni: " +
                FormattaDimensione(
                    peso_immagini + peso_testi
                );
        }

        // ---------------------------------------------------------
        // AGGIORNA BARRA
        // ---------------------------------------------------------

        AggiornaBarraPeso(
            peso_sistema,
            peso_immagini,
            peso_testi
        );
    }

    /// <summary>
    /// Aggiorna la barra proporzionale:
    ///
    /// ARANCIONE = Sistema
    /// VERDE     = Immagini
    /// BLU       = Testi
    /// </summary>
    private void AggiornaBarraPeso(
        long pesoSistema,
        long pesoImmagini,
        long pesoTesti)
    {
        long totale =
            pesoSistema +
            pesoImmagini +
            pesoTesti;

        if (totale <= 0)
        {
            Debug.LogWarning(
                "[STATISTICHE] Impossibile creare la barra: " +
                "peso totale uguale a zero."
            );

            return;
        }

        // ---------------------------------------------------------
        // CALCOLO PERCENTUALI
        // ---------------------------------------------------------

        float percentualeSistema =
            (float)pesoSistema / totale;

        float percentualeImmagini =
            (float)pesoImmagini / totale;

        float percentualeTesti =
            (float)pesoTesti / totale;

        Debug.Log(
            "[STATISTICHE] Barra: " +
            "Sistema " +
            (percentualeSistema * 100f).ToString("F1") +
            "% | Immagini " +
            (percentualeImmagini * 100f).ToString("F1") +
            "% | Testi " +
            (percentualeTesti * 100f).ToString("F1") +
            "%"
        );


        // ---------------------------------------------------------
        // POSIZIONAMENTO SEGMENTO SISTEMA
        // ---------------------------------------------------------

        ImpostaSegmento(
            segmentoSistema,
            0f,
            percentualeSistema
        );

        // ---------------------------------------------------------
        // POSIZIONAMENTO SEGMENTO IMMAGINI
        // ---------------------------------------------------------

        ImpostaSegmento(
            segmentoImmagini,
            percentualeSistema,
            percentualeImmagini
        );

        // ---------------------------------------------------------
        // POSIZIONAMENTO SEGMENTO TESTI
        // ---------------------------------------------------------

        ImpostaSegmento(
            segmentoTesti,
            percentualeSistema +
            percentualeImmagini,
            percentualeTesti
        );
    }

    /// <summary>
    /// Posiziona un segmento della barra in percentuale.
    /// </summary>
    private void ImpostaSegmento(
        Image segmento,
        float posizione,
        float larghezza)
    {
        if (segmento == null)
            return;

        RectTransform rect =
            segmento.GetComponent<RectTransform>();

        if (rect == null)
            return;

        rect.anchorMin =
            new Vector2(
                posizione,
                0f
            );

        rect.anchorMax =
            new Vector2(
                posizione + larghezza,
                1f
            );

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;
    }

    /// <summary>
    /// Calcola ricorsivamente la dimensione
    /// di una directory.
    /// </summary>
    private long CalcolaDimensioneDirectory(
        string percorso)
    {
        if (!Directory.Exists(percorso))
        {
            Debug.LogWarning(
                "[STATISTICHE] Directory non trovata: " +
                percorso
            );

            return 0;
        }

        long dimensioneTotale = 0;

        try
        {
            // -----------------------------------------------------
            // FILE
            // -----------------------------------------------------

            string[] files =
                Directory.GetFiles(percorso);

            foreach (string file in files)
            {
                try
                {
                    FileInfo fileInfo =
                        new FileInfo(file);

                    dimensioneTotale +=
                        fileInfo.Length;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(
                        "[STATISTICHE] Impossibile leggere il file: " +
                        file +
                        " - " +
                        e.Message
                    );
                }
            }

            // -----------------------------------------------------
            // CARTELLE
            // -----------------------------------------------------

            string[] directories =
                Directory.GetDirectories(percorso);

            foreach (string directory in directories)
            {
                dimensioneTotale +=
                    CalcolaDimensioneDirectory(
                        directory
                    );
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "[STATISTICHE] Errore durante il calcolo: " +
                e.Message
            );
        }

        return dimensioneTotale;
    }

    /// <summary>
    /// Converte i byte in B / KB / MB / GB.
    /// </summary>
    private string FormattaDimensione(
        long bytes)
    {
        if (bytes < 1024)
        {
            return bytes + " B";
        }

        double kb =
            bytes / 1024.0;

        if (kb < 1024)
        {
            return kb.ToString("F2") +
                   " KB";
        }

        double mb =
            kb / 1024.0;

        if (mb < 1024)
        {
            return mb.ToString("F2") +
                   " MB";
        }

        double gb =
            mb / 1024.0;

        return gb.ToString("F2") +
               " GB";
    }
    public void AggiornaListaComuni()
    {
        if (_DBClass == null)
        {
            Debug.LogError(
                "[STATISTICHE] DBClass non disponibile."
            );

            return;
        }

        if (contentComuni == null)
        {
            Debug.LogError(
                "[STATISTICHE] ContentComuni non assegnato."
            );

            return;
        }

        if (prefabComune == null)
        {
            Debug.LogError(
                "[STATISTICHE] PrefabComune non assegnato."
            );

            return;
        }

        // ---------------------------------------------------------
        // PULIZIA LISTA PRECEDENTE
        // ---------------------------------------------------------

        for (int i = contentComuni.childCount - 1; i >= 0; i--)
        {
            Destroy(
                contentComuni.GetChild(i).gameObject
            );
        }

        // ---------------------------------------------------------
        // RECUPERO COMUNI
        // ---------------------------------------------------------

        List<DBClass.COMUNE> comuni =
            _DBClass.GetCOMUNI(
                null,
                null,
                null,
                null,
                null,
                true
            );

        if (comuni == null || comuni.Count == 0)
        {
            Debug.LogWarning(
                "[STATISTICHE] Nessun comune trovato."
            );

            return;
        }

        Debug.Log(
            "[STATISTICHE] Comuni trovati: " +
            comuni.Count
        );

        // ---------------------------------------------------------
        // PARAMETRI LISTA
        // ---------------------------------------------------------

        float altezzaElemento = 350f;
        float spazioTraElementi = 10f;

        // ---------------------------------------------------------
        // CREAZIONE ELEMENTI
        // ---------------------------------------------------------


        int indice = 0;

        foreach (DBClass.COMUNE comune in comuni)
        {
            if (comune == null)
                continue;

            GameObject elemento =
                Instantiate(
                    prefabComune,
                    contentComuni
                );
            // -----------------------------------------------------
            // ALTEZZA ELEMENTO
            // -----------------------------------------------------

            RectTransform elementoRect =
                elemento.GetComponent<RectTransform>();

            if (elementoRect != null)
            {
                float posizioneY = -(indice * (altezzaElemento + spazioTraElementi));

                elementoRect.anchoredPosition =
                    new Vector2(
                        0f,
                        posizioneY
                    );

                elementoRect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    altezzaElemento
                );
            }

            // -----------------------------------------------------
            // LAYOUT ELEMENT
            // -----------------------------------------------------

            LayoutElement layout =
                elemento.GetComponent<LayoutElement>();

            if (layout == null)
            {
                layout =
                    elemento.AddComponent<LayoutElement>();
            }

            layout.minHeight =
                altezzaElemento;

            layout.preferredHeight =
                altezzaElemento;

            layout.flexibleHeight = 0f;

            // -----------------------------------------------------
            // SCRIPT
            // -----------------------------------------------------
            Button_comune_statistiche script =
                elemento.GetComponent<Button_comune_statistiche>();

            if (script == null)
            {
                Debug.LogError(
                    "[STATISTICHE] Il prefab Comune non contiene " +
                    "Button_comune_statistiche."
                );

                Destroy(elemento);

                continue;
            }

            // -----------------------------------------------------
            // FOTO PRINCIPALE DEL COMUNE
            // -----------------------------------------------------

            byte[] immagine = null;

            List<DBClass.COMUNE_IMMAGINI> immagini =
                _DBClass.getCOMUNI_IMMAGINI(
                    null,
                    comune.id
                );

            if (immagini != null && immagini.Count > 0)
            {
                DBClass.COMUNE_IMMAGINI immaginePrincipale = null;

                // Prima cerchiamo quella principale
                foreach (
                    DBClass.COMUNE_IMMAGINI img
                    in immagini)
                {
                    if (img.principale)
                    {
                        immaginePrincipale = img;
                        break;
                    }
                }

                // Se non esiste una principale,
                // prendiamo la prima disponibile.
                if (immaginePrincipale == null)
                {
                    immaginePrincipale =
                        immagini[0];
                }

                if (immaginePrincipale != null)
                {
                    immagine =
                        immaginePrincipale.image;
                }
            }

            // -----------------------------------------------------
            // PESO DEL COMUNE
            // -----------------------------------------------------

            long pesoComune =
                CalcolaPesoComune(
                    comune.id
                );

            // -----------------------------------------------------
            // POPOLIAMO L'ELEMENTO
            // -----------------------------------------------------

            script.ImpostaComune(
                comune.nome_comune,
                pesoComune,
                immagine,
                comune.id
            );
            // ---------------------------------------------------------
            // DIMENSIONE CONTENT
            // ---------------------------------------------------------

            RectTransform contentRectFinal =
                contentComuni.GetComponent<RectTransform>();

            if (contentRectFinal != null)
            {
                int numeroElementi =
                    contentComuni.childCount;

                float altezzaTotale =
                    (numeroElementi * altezzaElemento) +
                    ((numeroElementi - 1) * spazioTraElementi);

                contentRectFinal.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    altezzaTotale
                );

                Debug.Log(
                    "[STATISTICHE] Content altezza: " +
                    altezzaTotale +
                    " px - Elementi: " +
                    numeroElementi
                );
            }

            // ---------------------------------------------------------
            // FORZIAMO AGGIORNAMENTO LAYOUT
            // ---------------------------------------------------------

            Canvas.ForceUpdateCanvases();

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                contentRectFinal
            );
            indice++;
        }
        if (indice == 0)
        {
            riassuntoComuni.text = "Nessun comune nel telefono.";
        }
        else if (indice == 1)
        {
            riassuntoComuni.text = "Un comune nel telefono.";
        }
        else
        {
            riassuntoComuni.text = indice + " comuni nel telefono.";
        }
    }
    private long CalcolaPesoComune(int comuneId)
    {
        long peso = 0;
        peso = _DBClass.getPesoComuneInDB(comuneId);
        return peso;
    }
}
