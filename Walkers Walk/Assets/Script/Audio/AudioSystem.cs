using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
    [SerializeField] private AudioClip ambientSound;
    private AudioSource audioSource;
    void Awake()
    {      
        audioSource.clip = ambientSound;
        audioSource.loop = true; 
        audioSource.playOnAwake = false; 
    }

    private void Start()
    {       
        audioSource.Play();        
    }


}
