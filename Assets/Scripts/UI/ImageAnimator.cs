using System.Collections;
using UnityEngine;

public class ImageAnimator : MonoBehaviour
{
    public float frameRate = 0.1f; // Time between frames
    private Transform[] frames;
    private int currentFrame;
    private bool isAnimating;

    void Start()
    {
        // Get all child images
        frames = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            frames[i] = transform.GetChild(i);
            frames[i].gameObject.SetActive(false); // Hide all frames initially
        }
        Debug.Log("frames.Length: " + frames.Length);
    }

    public void StartAnimation()
    {
        Debug.Log("StartAnimation called");
        if (!isAnimating)
        {
            StartCoroutine(Animate());
        }
    }

    private IEnumerator Animate()
    {
        isAnimating = true;
        currentFrame = frames.Length - 1; // Start from the last frame

        while (currentFrame >= 0)
        {
            // Show the current frame
            frames[currentFrame].gameObject.SetActive(true);

            // Wait for the frame duration
            yield return new WaitForSeconds(frameRate);

            // Hide the current frame
            frames[currentFrame].gameObject.SetActive(false);

            // Move to the previous frame
            currentFrame--;
        }

        // Show the last frame
        frames[0].gameObject.SetActive(true);
        isAnimating = false;
    }
}