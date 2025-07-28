using UnityEngine;
using UnityEngine.UI;

// I usually attach this to my main camera, but in theory you can attach it to any object in scene, since it uses cam instead of "this".
public class CameraMovement : MonoBehaviour
{
    private Vector3 MouseDownPosition = Vector3.zero;
    public Camera cam;
    public Text Dx;
    public Text Dy;
    void Update()
    {
        // If mouse wheel scrolled vertically, apply zoom...
        // TODO: Add pinch to zoom support (touch input)
        if (Input.mouseScrollDelta.y != 0)
        {
            // Save location of mouse prior to zoom
            var preZoomPosition = getWorldPoint(Input.mousePosition);

            // Apply zoom (might want to multiply Input.mouseScrollDelta.y by some speed factor if you want faster/slower zooming
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + Input.mouseScrollDelta.y, 5, 80);

            // How much did mouse move when we zoomed?
            var delta = getWorldPoint(Input.mousePosition) - preZoomPosition;

            // Rotate camera to top-down (right angle = 90) before applying adjustment (otherwise we get "slide" in direction of camera angle).
            // TODO: If we allow camera to rotate on other axis we probably need to adjust that also.  At any rate, you want camera pointing "straight down" for this part to work.
            var rot = cam.transform.localEulerAngles;
            cam.transform.localEulerAngles = new Vector3(90, rot.y, rot.z);

            // Move the camera by the amount mouse moved, so that mouse is back in same position now.
            cam.transform.Translate(delta.x, delta.z, 0);

            // Restore camera rotation
            cam.transform.localEulerAngles = rot;
        }

        // When mouse is first pressed, just save location of mouse/finger.
        if (Input.GetMouseButtonDown(0))
        {
            if (MouseDownPosition == Vector3.zero)
                MouseDownPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            if (Dx.text == "-1" && Dy.text == "-1")
            {
                MouseDownPosition = Input.mousePosition;
            }
            // Total distance finger/mouse has moved while button is down
            var delta = Input.mousePosition - MouseDownPosition;
            Dx.text = delta.x.ToString();
            Dy.text = delta.y.ToString();
        }
        else
        {
            Dx.text = "0";
            Dy.text = "0";
        }
        // While mouse button/finger is down...
        if (Input.GetMouseButtonUp(0))
        {
            
            // Adjust camera by distance moved, so mouse/finger stays at exact location (in world, since we are using getWorldPoint for everything).
            //cam.transform.Translate(delta.x, delta.z, 0);
            MouseDownPosition = Vector3.zero;
            Dx.text = "0";
            Dy.text = "0";
        }
    }

    // This works by casting a ray.  For this to work well, this ray should always hit your "ground".  Setup ignore layers if you need to ignore other colliders.
    // Only tested this with a simple box collider as ground (just one flat ground).
    private Vector3 getWorldPoint(Vector2 screenPoint)
    {
        RaycastHit hit;
        Physics.Raycast(cam.ScreenPointToRay(screenPoint), out hit);
        return hit.point;
    }
}