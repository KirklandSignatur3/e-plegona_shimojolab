using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AppController : MonoBehaviour
{
    void Start()
    {
        Input.multiTouchEnabled = true;
        Cursor.visible = false;
        DOTween.SetTweensCapacity(500, 315);

#if UNITY_EDITOR
        Application.targetFrameRate = 60;
        Cursor.visible = true;
#endif

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}