using UnityEngine;

namespace ARLocation.Utils
{
    public class RotateObjectAR : MonoBehaviour
    {
        public float Speed = 22.0f;

        void Update()
        {
            transform.Rotate(0, 0, Speed * Time.deltaTime);
        }
    }
}
