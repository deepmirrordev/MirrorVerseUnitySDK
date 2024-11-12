using UnityEngine;

public class RotationEffect : MonoBehaviour
{
    private void Update()
    {
        gameObject.transform.Rotate(100 * Time.deltaTime * Vector3.back);
    }
}
