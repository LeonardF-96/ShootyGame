using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float rotationAngle = 90f; // Angle to rotate upon being hit
    public float rotationDuration = 0.4f; // Duration of the rotation animation
    public Transform rotationPivot; // Reference to the parent object that acts as the rotation pivot

    //public int bodyScore = 1; // Score awarded for hitting the body
    //public int headScore = 2; // Score awarded for hitting the head
    private bool isHit = false; // To track if the target has been hit

    void Start()
    {
        // Ensure the rotation pivot is assigned
        if (rotationPivot == null)
        {
            Debug.LogError("Rotation pivot is not assigned!");
        }
    }

    public void OnHit(bool isHeadShot)
    {
        Debug.Log("OnHit called"); // Log to check if OnHit is called
        if (!isHit)
        {
            isHit = true;
            Debug.Log("Target hit for the first time, starting rotation"); // Log to confirm first hit
            int scoreToAdd = isHeadShot ? 20 : 10;
            if (GameManager.instance != null)
            {
                GameManager.instance.AddScore(scoreToAdd);
            }
            else
            {
                Debug.LogError("GameManager instance is null.");
            }
            StartCoroutine(RotateTarget());
        }
        else
        {
            Debug.Log("Target already hit, no action taken"); // Log if target was already hit
        }
    }

    private IEnumerator RotateTarget()
    {
        Debug.Log("RotateTarget coroutine started"); // Log to check if coroutine starts
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, 0f, rotationAngle));
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            rotationPivot.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rotationPivot.rotation = endRotation;
        Debug.Log("Rotation complete"); // Log to confirm rotation completion
    }
}
