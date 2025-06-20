using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class SEPlayer : SingletonMonoBehaviour<SEPlayer>
{
    [SerializeField] public List<AudioClip> clips;
    private AudioSource _audioSource;
    private void Start() 
    {    
        _audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlaySE(int index)
    {
        _audioSource.PlayOneShot(clips[index]);
    }
}