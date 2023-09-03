using UnityEngine;
using DG.Tweening;

public class RotateLoadingIcon : MonoBehaviour
{
    public float rotationSpeed = 180f; // Specify the rotation speed in degrees per second.

    private RectTransform rectTransform;
    private Tweener rotationTween;

    private void Start()
    {
        // Get the RectTransform component of the loading icon.
        rectTransform = GetComponent<RectTransform>();

        // Call the StartRotating method to start the infinite rotation animation.
        StartRotating();
    }

    private void StartRotating()
    {
        // Use DOTween to rotate the loading icon infinitely.
        rotationTween = rectTransform.DORotate(new Vector3(0f, 0f, 360f), 1f / rotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart); // Infinite loop with restart.

        // Set the tween to be paused initially.
        rotationTween.Pause();
    }

    // Start the rotation animation when the object is enabled (e.g., when the loading screen appears).
    private void OnEnable()
    {
        rotationTween.Play();
    }

    // Stop the rotation animation when the object is disabled (e.g., when the loading screen disappears).
    private void OnDisable()
    {
        rotationTween.Pause();
    }
}
