using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlyEnemy : Enemy
{
    private bool isChasingPlayer = false;
    
    public override void Patrol()
    {
        // Lógica de patrulla 
    }

    public override void OnPlayerDetected()
    {
        isChasingPlayer = true;
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    public override void Die()
    {
        Debug.Log("Fly defeated!");
        base.Die();
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 collisionDirection = (collision.transform.position - transform.position).normalized;

            float angle = Vector2.Angle(Vector2.up, collisionDirection);

            if (angle < 45)
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
