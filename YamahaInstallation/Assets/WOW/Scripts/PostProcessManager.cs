using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessManager : SingletonMonoBehaviour<PostProcessManager>
{
	[SerializeField] public PostProcessVolume guiPostProcess;
    public Glitch guiGlith = null;
    
    void Awake()
    {
        foreach (PostProcessEffectSettings item in guiPostProcess.profile.settings)
        {
            if (item as Glitch)
            { 
                guiGlith = item as Glitch;
            }
        }
    }
}