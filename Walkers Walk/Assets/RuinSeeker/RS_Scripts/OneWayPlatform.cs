using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    private Collider2D platformCollider;
    public Transform playerPosition;

    void Start()
    {
        platformCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if ((playerPosition.position.y - (playerPosition.localScale.y / 2)) > transform.position.y)
        {
            platformCollider.enabled = true;
        }
        else
        {
            platformCollider.enabled = false;
        }
    }
}
