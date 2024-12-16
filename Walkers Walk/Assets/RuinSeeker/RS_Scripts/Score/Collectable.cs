using UnityEngine;

public abstract class Collectable : MonoBehaviour
{
    [Header("Collectable Settings")]
    [SerializeField] protected int value = 1;
    [SerializeField] protected bool destroyOnCollect = true;

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollect();
            SoundManager.PlaySound(SoundType.GEMPICKUP);

        }
    }

    protected virtual void OnCollect()
    {
        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
    }
}