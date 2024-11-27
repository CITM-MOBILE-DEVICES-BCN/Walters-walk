using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationAngle = 45f;
    public float rotationSpeed = 2f;
    public float pauseDuration = 0.5f;
    private bool isMoving = true;
    private bool isLooking = false;

    public void LookBothWays()
    {
        if (isMoving)
        {
            isMoving = false;
            if(!isLooking)
            {
                StartCoroutine(LookRoutine());
            }
        }
        else
        {
            isMoving = true;
        }
    }

    private IEnumerator LookRoutine()
    {
        isLooking = true;
        yield return RotateToAngle(rotationAngle);
        yield return new WaitForSeconds(pauseDuration);

        yield return RotateToAngle(-rotationAngle * 2);
        yield return new WaitForSeconds(pauseDuration);

        yield return RotateToAngle(rotationAngle);
        isLooking = false;
    }

    private IEnumerator RotateToAngle(float angle)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + angle, transform.eulerAngles.z);

        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime);
            elapsedTime += Time.deltaTime * rotationSpeed;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
