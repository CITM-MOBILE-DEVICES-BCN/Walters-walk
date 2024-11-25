using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidad del movimiento
    private bool isMoving = true; // Controla si el personaje est� en movimiento
    private Vector3 initialPosition; // Posici�n inicial del personaje

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

    public void ToggleMovement()
    {
        // Alternar el estado de movimiento
        isMoving = !isMoving;
    }
    //void OnCollisionEnter(Collision collision)
    //{
    //    // Comprobar si el objeto con el que chocamos tiene el tag especificado
    //    if (collision.gameObject.CompareTag("Truck"))
    //    {
    //        // Volver a la posici�n inicial
    //        transform.position = initialPosition;
    //    }
    //    else if (collision.gameObject.CompareTag("Car"))
    //    {
    //        // Volver a la posici�n inicial
    //        transform.position = initialPosition;
    //    }
    //    else if (collision.gameObject.CompareTag("Bike"))
    //    {
    //        // Volver a la posici�n inicial
    //        transform.position = initialPosition;
    //    }
    //}

    void OnCollisionEnter(Collision collision)
    {
        // Volver a la posici�n inicial sin verificar tags
        transform.position = initialPosition;
    }
}
