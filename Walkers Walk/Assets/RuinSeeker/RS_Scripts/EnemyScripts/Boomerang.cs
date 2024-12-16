using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boomerang : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float speed;
    private bool returning = false;
    private bool hasReturned = false;

    public bool HasReturned => hasReturned;

    public void Initialize(Vector3 startPos, Vector3 playerPos, float boomerangSpeed)
    {
        startPosition = startPos;
        speed = boomerangSpeed;
        Vector3 directionToPlayer = (playerPos - startPos).normalized;
        targetPosition = startPos + directionToPlayer * Vector3.Distance(startPos, playerPos);
    }

    private void Update()
    {
        if (!returning)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition)<0.1f)
            {
                returning = true;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, startPosition) < 0.1f)
            {
                hasReturned = true;
                Destroy(gameObject);
            }
        }
    }
}
