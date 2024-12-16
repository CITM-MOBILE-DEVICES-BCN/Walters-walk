using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SoundManager.PlaySound(SoundType.CHECKPOINT);
            GameManager.Instance.UpdateCheckpointPosition(transform.position);
            Debug.Log("checkpoint saved");
        }

    }
}
