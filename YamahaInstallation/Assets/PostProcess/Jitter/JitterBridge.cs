using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
//[ExecuteInEditMode]
public class JitterBridge : MonoBehaviour
{
    public PostProcessVolume volume;
    public Jitter layer;

    public bool isOn = false;
    public float intensity;

    void Update()
    {
        if(layer == null)
        {
            volume = GameObject.Find("Post").GetComponent<PostProcessVolume>();
            if(volume) volume.profile.TryGetSettings(out layer);
        }

        if (layer != null && volume != null) 
        {
            layer.intensity.value = intensity;
            layer.enabled.value = isOn;
        }
    }
}