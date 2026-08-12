using UnityEngine;
using UnityEngine.UI;

public class Button_comune_statistiche : MonoBehaviour
{
    [Header("UI")]
    public Image foto;
    public Text nomeComune;
    public Text pesoComune;
    public Text Id;

    public void ImpostaComune(
        string nome,
        long peso,
        byte[] immagine,
        int id
    )
    {
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
                FormattaDimensione(peso);
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

    private string FormattaDimensione(long bytes)
    {
        if (bytes < 1024)
            return bytes + " B";

        double kb =
            bytes / 1024.0;

        if (kb < 1024)
            return kb.ToString("F2") + " KB";

        double mb =
            kb / 1024.0;

        if (mb < 1024)
            return mb.ToString("F2") + " MB";

        double gb =
            mb / 1024.0;

        return gb.ToString("F2") + " GB";
    }
}