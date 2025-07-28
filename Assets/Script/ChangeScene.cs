using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ARLocation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ChangeScene : MonoBehaviour
{
    //public static ChangeScene Instance;

    public GameObject _loaderCanvas;
    public Image _progressBar;

    private float _target;
    private DBClass _DBClass;

    private void Start()
    {
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
    }

    private void Awake()
    {
       /* if(Instance ==  null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
       */
    }
    
    public void ResetPlayer()
    {
        PlayerPrefs.SetInt("show_grid_poi", 0);
        PlayerPrefs.SetString("istat", "");
        PlayerPrefs.SetInt("percorso_selezionato", 0);
        PlayerPrefs.SetInt("evento_selezionato", 0);
        PlayerPrefs.SetInt("poi_selezionato", 0);
    }
    public void Load_ARRoute(int score)
    {
        ResetPlayer();
        Debug.Log("Load_ARRoute:" + score);
        PlayerPrefs.SetInt("percorso", score);
        PlayerPrefs.SetString("ID", "");
        ActiveSceneAndDeactivateTheActiveOne("ARRoute");

    }
    public void Load_ARRoute(string ID)
    {
        ResetPlayer();
        Debug.Log("Load_ARRoute with ID:" + ID);
        PlayerPrefs.SetInt("percorso", 0);
        PlayerPrefs.SetString("ID", ID);
        ActiveSceneAndDeactivateTheActiveOne("ARRoute");
    }
    public void Load_Map()
    {
        StartCoroutine(_DBClass.GetLatLonUsingGPS());
        ResetPlayer();
        Debug.Log("Load_Map");
        ActiveSceneAndDeactivateTheActiveOne("Map");
    }
    public void Load_NFCTools()
    {
        ResetPlayer();
        Debug.Log("Load_NFCTools");
        ActiveSceneAndDeactivateTheActiveOne("NFCTools");
    }
    public void Load_Map_And_Open_Grid()
    {
        Load_Map();
        PlayerPrefs.SetInt("show_grid_poi", 1);
    }
    public void Load_Menu()
    {
        ResetPlayer();
        Load_Menu_Without_reset_Player();
    }
    public void Load_Menu_Without_reset_Player()
    { 
        Debug.Log("Load_Menu");
        ActiveSceneAndDeactivateTheActiveOne("Menu");
    }
    
    public void Quit()
    {
        ResetPlayer();
        Debug.Log("Quit");
        Application.Quit();
    }
    private async void ActiveSceneAndDeactivateTheActiveOne(string scene_name)
    {
        ARSession session = FindObjectOfType<ARSession>();
        if(session != null)
            session.Reset();
        ARLocationManager aRLocationManager = FindObjectOfType<ARLocationManager>();
        if(aRLocationManager != null)
            aRLocationManager.ResetARSession();
        if (_loaderCanvas != null)
            _loaderCanvas.SetActive(true);

        //var scene_to_unload = SceneManager.GetActiveScene().name;
        var scene = SceneManager.LoadSceneAsync(scene_name);
        scene.allowSceneActivation = false;

        do
        {
            await Task.Delay(100);
            _target = scene.progress;
        } while (scene.progress < 0.9f);

        await Task.Delay(100);


        scene.allowSceneActivation = true;
        //_loaderCanvas.SetActive(false);

        //if (!string.IsNullOrEmpty(scene_to_unload))
        //    SceneManager.UnloadSceneAsync(scene_to_unload);

        

        //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        //SceneManager.LoadScene(scene_name);
    }
    private void Update()
    {
//        Debug.Log("_target " + _target);
        if(_progressBar != null)
            _progressBar.fillAmount = Mathf.MoveTowards(_progressBar.fillAmount, _target, 3 * Time.deltaTime);
    }
}
