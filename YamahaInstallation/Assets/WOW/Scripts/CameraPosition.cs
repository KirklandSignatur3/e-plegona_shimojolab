using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [SerializeField] float frustumWidth = 19.2f;
    void Start()
    {
        var camera = gameObject.GetComponent<Camera>();
        var frustumHeight = frustumWidth / camera.aspect;
        var cameraDistance = frustumHeight * 0.5f / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        camera.transform.localPosition = new Vector3(camera.transform.localPosition.x, camera.transform.localPosition.y, -cameraDistance);
    }
}
