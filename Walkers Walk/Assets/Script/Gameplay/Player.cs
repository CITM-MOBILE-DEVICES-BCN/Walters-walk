using PopUpSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    private bool isMoving = true;
    private Vector3 initialPosition;
    [SerializeField] private PlayerData playerData;

    private void OnEnable()
    {
        playerData.OnDeathAction += Die;
    }
    private void OnDisable()
    {
        playerData.OnDeathAction -= Die;
    }

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

    public void ToggleMovement(PhoneController.PhoneState state)
    {
        if (state != PhoneController.PhoneState.Closed)
        {
            return;
        }

        isMoving = !isMoving;
    }

    void OnCollisionStay(Collision collision)
    {
        Debug.Log($"Collision detected with: {collision.gameObject.name}");
        playerData.OnDeath();
    }

    public void Die()
    {
        transform.position = initialPosition;
    }
}
