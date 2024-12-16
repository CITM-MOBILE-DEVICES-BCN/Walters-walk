using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoomerangEnemy : Enemy
{
    public GameObject boomerangPrefab;
    public float boomerangSpeed = 5f;
    public float boomerangRange = 10f;

    private bool isThrowingBoomerang;

    public override void Patrol()
    {
        // Lógica de patrulla (No hay)s
    }

    public override void OnPlayerDetected()
    {
        if (!isThrowingBoomerang)
        {
            StartCoroutine(ThrowBoomerang());
        }
    }

    private IEnumerator ThrowBoomerang()
    {
        SoundManager.PlaySound(SoundType.BOOMERANGSHOT);

        isThrowingBoomerang = true;

        var boomerang = Instantiate(boomerangPrefab, transform.position, Quaternion.identity);

        var boomerangScript = boomerang.GetComponent<Boomerang>();

        boomerangScript.Initialize(transform.position, player.position, boomerangSpeed);

        yield return new WaitUntil(() => boomerangScript.HasReturned);

        isThrowingBoomerang = false;
    }

    public override void Die()
    {
        Debug.Log("Boomerang Guy defeated!");
        base.Die();
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 collisionDirection = (collision.transform.position - transform.position).normalized;

            float angle = Vector2.Angle(Vector2.up, collisionDirection);

            if (angle < 70)
            {
                Die();
                GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerMovement>().JumpAfterKillingEnemy();
            }
            else
            {
                collision.gameObject.GetComponent<PlayerMovement>().DeadFunction();
            }
        }
     
    }
}
