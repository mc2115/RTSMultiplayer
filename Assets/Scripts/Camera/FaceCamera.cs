using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    // Start is called before the first frame update
    private Transform mainCameraTransform;
    void Start()
    {
        mainCameraTransform=Camera.main.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position+mainCameraTransform.rotation*Vector3.forward, mainCameraTransform.rotation*Vector3.up);
    }
}
