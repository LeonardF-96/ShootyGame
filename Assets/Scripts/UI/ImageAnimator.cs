using System.Collections;
using UnityEngine;

public class ImageAnimator : MonoBehaviour
{
    public float frameRate = 0.12f; // Time between frames
    private Transform[] frames;
    private int currentFrame;
    private bool isAnimating;
    private bool isInitialized = false;

    private void InitializeFrames()
    {
        // Get all child images
        frames = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            frames[i] = transform.GetChild(i);
            frames[i].gameObject.SetActive(false); // Hide all frames initially
        }
        isInitialized = true;
        Debug.Log("frames.Length: " + frames.Length);
    }

    public void StartAnimation()
    {
        Debug.Log("StartAnimation called");
        if (!isAnimating)
        {
            if (!isInitialized)
            {
                InitializeFrames();
            }

            if (frames == null || frames.Length == 0)
            {
                Debug.LogError("No frames found for animation.");
                return;
            }

            StartCoroutine(Animate());
        }
    }

    private IEnumerator Animate()
    {
        isAnimating = true;
        currentFrame = frames.Length - 1; // Start from the last frame

        while (currentFrame >= 0)
        {
            if (frames[currentFrame] == null)
            {
                Debug.LogError("Frame at index " + currentFrame + " is null.");
                isAnimating = false;
                yield break;
            }

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
        if (frames[0] != null)
        {
            frames[0].gameObject.SetActive(true);
        }
        isAnimating = false;
    }
}