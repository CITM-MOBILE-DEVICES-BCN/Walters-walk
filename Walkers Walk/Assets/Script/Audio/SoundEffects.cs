using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour
{
    public AudioSource audiosystem;
    public AudioClip sfx1;

    public void Bike()
    {
        audiosystem.clip = sfx1;
        audiosystem.Play();

    }

}
