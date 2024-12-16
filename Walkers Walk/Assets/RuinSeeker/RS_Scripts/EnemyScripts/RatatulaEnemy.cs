using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatatulaEnemy : Enemy
{
    private bool isAttached = false;

    public override void Patrol() { }

    public override void OnPlayerDetected()
    {
        if (!isAttached)
        {
            AttachToPlayer();
        }
    }

    private void AttachToPlayer()
    {
        isAttached = true;
        transform.SetParent(player);
        transform.localPosition = Vector3.zero;
        StartCoroutine(InvertControls());    
    }

    private IEnumerator InvertControls()
    {
        Debug.Log("Attached Ratatula to player");
        player.GetComponent<PlayerMovement>().InvertControls();
        yield return new WaitForSeconds(5f);
        player.GetComponent<PlayerMovement>().InvertControls();
        Die();
    }

    public override void Die()
    {
        transform.SetParent(null);
        base.Die();
    }
}
