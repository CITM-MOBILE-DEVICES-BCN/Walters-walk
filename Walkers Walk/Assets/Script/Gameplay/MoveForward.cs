using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public float speed;   // Velocidad de movimiento
    public float limit;   // Distancia máxima que puede recorrer

    private Vector3 startPosition;

    public void Initialize(float moveSpeed, float moveLimit)
    {
        speed = moveSpeed;
        limit = moveLimit;
        startPosition = transform.position;
    }

    private void Update()
    {
        // Mueve el objeto hacia adelante en el eje Z
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Comprueba si el objeto ha superado el límite
        if (Vector3.Distance(startPosition, transform.position) >= limit)
        {
            Destroy(gameObject); // Destruye el objeto cuando supera el límite
        }
    }

}
