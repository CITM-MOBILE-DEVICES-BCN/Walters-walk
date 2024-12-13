using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    public int scoreAmount = 10; // Puntaje que se añade al pasar por este trigger
    private void OnTriggerEnter(Collider other)
    {
        // Comprueba si el objeto que atraviesa el trigger es el jugador
        if (other.CompareTag("Person"))
        {
            // Busca el script ScoreManager en la escena y llama a IncreaseScore
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            SoundEffects soundEffects = FindObjectOfType<SoundEffects>();
            if (scoreManager != null)
            {
                scoreManager.IncreaseScore(scoreAmount);
                Debug.Log("Score added");

            }
            if (soundEffects != null)
            {
                Debug.Log("Playing coin sound.");
                soundEffects.PlayCoinSound();
            }
            else
            {
                Debug.LogWarning("SoundEffects not found in the scene.");
            }
        }
    }
}
