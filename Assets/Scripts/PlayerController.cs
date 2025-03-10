// PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 20f;

    private bool canMove = true; // Flag to control player movement
    void Update()
    {
        if (!canMove)
            return; // If player movement is disabled, do not process input

        // Initialize input values
        float horizontalInput = 0f;
        float verticalInput = 0f;

        // Check for horizontal input
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }

        // Check for vertical input
        if (Input.GetKey(KeyCode.W))
        {
            verticalInput = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            verticalInput = -1f;
        }

        // Print the input values to the console
        //Debug.Log("Horizontal Input: " + horizontalInput);
        //Debug.Log("Vertical Input: " + verticalInput);

        // Calculate movement direction based on input
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Rotate the movement vector based on the player's Y rotation
        movement = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * movement;

        // Move the player if there is any input
        if (movement.magnitude > 0.1f)
        {
            transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
        }
    }
    public void DisablePlayerMovement()
    {
        canMove = false;
    }

    public void EnablePlayerMovement()
    {
        canMove = true;
    }
}
