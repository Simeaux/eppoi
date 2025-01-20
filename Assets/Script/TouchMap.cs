using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchMap : MonoBehaviour
{
    public GameObject DetailPOI;
    public GameObject ARMenuCanvas;
    public Canvas canvasDetailPOI;
    public Camera MapCamera;
    float clicktime;
    Ray ray;
    RaycastHit hit;
    // Start is called before the first frame update
    void Start()
    {
        DetailPOI.GetComponent<DetailPOIManager>().CloseDetailPOI();
    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("nfc")))
        {
            Debug.Log("Eccomi " + PlayerPrefs.GetInt("show_grid_poi"));
            DetailPOI.GetComponent<DetailPOIManager>().OpenDetailAtID(PlayerPrefs.GetString("nfc"), false, true, 3);
            ARMenuCanvas.GetComponent<ARLocation.MapboxRoutes.SampleProject.ArMenuController>().ZeroSizeMinimap();
            PlayerPrefs.SetString("nfc", string.Empty);
        }
        if (ARMenuCanvas.GetComponent<ARLocation.MapboxRoutes.SampleProject.ArMenuController>().Settings.MenuController.MapSize < 513)
        {
            bool click = false;

            if (Input.GetMouseButtonDown(0))
            {
                clicktime = Time.time;
            }
            if (Input.touchCount == 2)
            {
                var touchZero = Input.GetTouch(0);
                var touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagniture = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagniture = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagniture - prevMagniture;

                pinch_zoom(difference * 0.001f);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                var clickup = Time.time;
                if (clickup - clicktime < 0.15f)
                {
                    click = true;
                }
            }

            if (click)
            {
                Debug.Log("TouchMap CLiccked!!");
                // Bit shift the index of the layer (8) to get a bit mask
                int layerMask = 1 << 8;

                // This would cast rays only against colliders in layer 8.
                // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
                layerMask = ~layerMask;
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    // da quì ho il tag del gameobject cliccato e posso aprire il dettaglio o chiedere se vuole arrivarci
                    var p = hit.transform.gameObject as GameObject;
                    Debug.Log("Ci siamo quasi tag:" + p.tag + " name:" + p.name);
                    if (p.CompareTag("Portami_al_POI") || p.CompareTag("Close_Detail"))
                    {
                        Debug.Log("Chiudo il detail tag:" + p.tag + " name:" + p.name);
                        DetailPOI.GetComponent<DetailPOIManager>().CloseDetailPOI();
                        //reset della grandezza della minimap
                        ARMenuCanvas.GetComponent<ARLocation.MapboxRoutes.SampleProject.ArMenuController>().ResetSizeMinimap();
                    }
                    else if (ARMenuCanvas.GetComponent<ARLocation.MapboxRoutes.SampleProject.ArMenuController>().Settings.MenuController.MapSize < 513)
                    {
                        Debug.Log("Eccomi " + PlayerPrefs.GetInt("show_grid_poi"));
                        DetailPOI.GetComponent<DetailPOIManager>().OpenDetailAtID(p.name, false, true, 3);
                        ARMenuCanvas.GetComponent<ARLocation.MapboxRoutes.SampleProject.ArMenuController>().ZeroSizeMinimap();
                    }

                }

            }
        }
    }
    void pinch_zoom(float increment)
    {
        MapCamera.orthographicSize = Mathf.Clamp(MapCamera.orthographicSize - increment, 5, 20);
        //double.TryParse(zoom.text, out selectedzoom);
        //zoom.text = (selectedzoom + increment).ToString();
    }
}
