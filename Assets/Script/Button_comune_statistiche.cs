using UnityEngine;
using UnityEngine.UI;

public class Button_comune_statistiche : MonoBehaviour
{
    [Header("UI")]
    public Image foto;
    public Text nomeComune;
    public Text pesoComune;
    public Text Id;
    private DBClass _DBClass;

    void Start()
    {
        GameObject sqliteObject =
    GameObject.FindWithTag("SQLite");

        if (sqliteObject == null)
        {
            Debug.LogError("[START] Oggetto SQLite non trovato.");
            return;
        }
        _DBClass =
            sqliteObject.GetComponent<DBClass>();

        if (_DBClass == null)
        {
            Debug.LogError("[START] DBClass non trovato.");
            return;
        }
    }
    public void ImpostaComune(
        string nome,
        long peso,
        byte[] immagine,
        int id
    )
    {
        GameObject sqliteObject = GameObject.FindWithTag("SQLite");

        if (sqliteObject == null)
        {
            Debug.LogError("[START] Oggetto SQLite non trovato.");
            return;
        }
        _DBClass = sqliteObject.GetComponent<DBClass>();
        if (_DBClass == null)
        {
            Debug.LogError("[START] DBClass non trovato.");
            return;
        }
        // Nome
        if (nomeComune != null)
        {
            nomeComune.text = nome;
        }

        // Id
        if (Id != null)
        {
            Id.text = id.ToString();
        }

        // Peso
        if (pesoComune != null)
        {
            pesoComune.text =
                _DBClass.FormattaDimensione(peso);
        }

        // Foto
        if (foto != null && immagine != null && immagine.Length > 0)
        {
            try
            {
                Texture2D texture =
                    new Texture2D(
                        2,
                        2,
                        TextureFormat.RGBA32,
                        false
                    );

                bool caricata =
                    texture.LoadImage(immagine);

                if (caricata)
                {
                    Sprite sprite =
                        Sprite.Create(
                            texture,
                            new Rect(
                                0,
                                0,
                                texture.width,
                                texture.height
                            ),
                            new Vector2(
                                0.5f,
                                0.5f
                            )
                        );

                    foto.sprite = sprite;
                    foto.preserveAspect = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(
                    "[STATISTICHE] Errore caricamento foto: " +
                    e.Message
                );
            }
        }
    }
}