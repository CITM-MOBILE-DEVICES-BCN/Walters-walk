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
            if (scoreManager != null)
            {
                scoreManager.IncreaseScore(scoreAmount);
            }

            // Opcional: Desactiva este trigger si no quieres que sume puntaje varias veces
            gameObject.SetActive(false);
        }
    }
}
