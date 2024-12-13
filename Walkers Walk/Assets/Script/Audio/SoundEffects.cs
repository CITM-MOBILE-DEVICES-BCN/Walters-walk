using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour
{
    public AudioSource audiosystem; 
    public AudioClip sfx1; 
    public AudioClip sfx2; 
    public AudioClip sfx3; 
    public Transform player;
    public float activationDistance = 10f; 

    private void Update()
    {
        bool soundPlayed = false;

        GameObject[] bikes = GameObject.FindGameObjectsWithTag("Bike");
        foreach (GameObject bike in bikes)
        {
            if (Vector3.Distance(bike.transform.position, player.position) <= activationDistance)
            {
                PlaySound(sfx1);
                soundPlayed = true;
                break; 
            }
        }

        if (!soundPlayed)
        {
            GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
            foreach (GameObject car in cars)
            {
                if (Vector3.Distance(car.transform.position, player.position) <= activationDistance)
                {
                    PlaySound(sfx2);
                    soundPlayed = true;
                    break; 
                }
            }
        }
        if (!soundPlayed)
        {
            GameObject[] trucks = GameObject.FindGameObjectsWithTag("Truck");
            foreach (GameObject truck in trucks)
            {
                if (Vector3.Distance(truck.transform.position, player.position) <= activationDistance)
                {
                    PlaySound(sfx3);
                    soundPlayed = true;
                    break;
                }
            }
        }



        if (!soundPlayed && audiosystem.isPlaying)
        {
            audiosystem.Stop();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audiosystem.clip != clip || !audiosystem.isPlaying)
        {
            audiosystem.clip = clip;
            audiosystem.Play();
        }
    }

}