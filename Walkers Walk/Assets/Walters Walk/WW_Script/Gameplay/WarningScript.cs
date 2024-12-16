using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningScript : MonoBehaviour
{
    [System.Serializable]
    public class TagType
    {
        public string tag; // Etiqueta para los objetos a rastrear.
        public Image leftImage; // Imagen para objetos a la izquierda.
        public Image rightImage; // Imagen para objetos a la derecha.
    }

    public TagType[] tagTypes; // Lista de tipos de etiquetas.
    public float distanceThreshold = 5f; // Distancia para mostrar imágenes (izquierda/derecha).
    public float closeThreshold = 2f; // Distancia para desactivar imágenes cuando está cerca.

    private void Update()
    {
        foreach (var tagType in tagTypes)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tagType.tag); // Encuentra todos los objetos con esta etiqueta.

            bool leftActive = false;
            bool rightActive = false;

            foreach (var obj in objectsWithTag)
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                float xDifference = obj.transform.position.x - transform.position.x;

                // Si está dentro de la distancia cercana, ignora este objeto.
                if (distance <= closeThreshold)
                    continue;

                // Si está dentro del rango de distancia, activa las imágenes correspondientes.
                if (Mathf.Abs(xDifference) <= distanceThreshold)
                {
                    if (xDifference < 0) // A la izquierda.
                    {
                        leftActive = true;
                    }
                    else // A la derecha.
                    {
                        rightActive = true;
                    }
                }
            }

            // Activar o desactivar las imágenes según las banderas.
            tagType.leftImage.gameObject.SetActive(leftActive);
            tagType.rightImage.gameObject.SetActive(rightActive);
        }
    }
}
