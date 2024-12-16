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
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, startPosition.y, transform.position.z);
        if (Vector3.Distance(startPosition, transform.position) >= limit)
        {
            Destroy(gameObject);
        }
    }

}
