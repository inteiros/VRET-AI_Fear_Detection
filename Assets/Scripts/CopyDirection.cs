using UnityEngine;

public class CopyDirection : MonoBehaviour
{
    public GameObject copy;

    void Start() { }

    void Update()
    {
        transform.localPosition = copy.transform.localPosition;
        transform.localRotation = copy.transform.localRotation;
    }
}
