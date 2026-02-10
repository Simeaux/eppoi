using UnityEngine;
using Vuforia;

public class AttivaVuforia : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        VuforiaApplication.Instance.Initialize();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
