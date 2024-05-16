/*
 * Salmoria, Wyatt
 * 05/15/24
 * Custom camera script used for controlling what player controller view is active and the interpolation between them.
 */
using UnityEngine;

public class CustomCamera : MonoBehaviour
{
    // Time it takes to perform a transition between player controllers.
    public float transitionTime = 0.5f;
    // The currently targeted controller that the camera follows.
    public PlayerController targetController;


    private GameObject target;
    private Transform start;
    private float t;

    private void Start()
    {
        t = 0.0f;
        start = transform;
        target = targetController.cameraTarget;
    }

    private void Update()
    {
        t += Time.deltaTime / transitionTime;

        var e = InOutExpo(t);
        if (t < 1.0f)
        {
            transform.position = Vector3.Lerp(start.position, target.transform.position, e);
            transform.rotation = Quaternion.Lerp(start.rotation, target.transform.rotation, e);
        }
        else
        {
            transform.position = target.transform.position;
            transform.rotation = target.transform.rotation;
        }
    }

    /// <summary>
    ///  Sets the new player controller target starting the interpolation animation between the current and new target.
    /// </summary>
    /// <param name="character">Player</param>
    public void ChangeTarget(PlayerController character)
    {
        t = 0.0f;
        targetController = character;
        target = targetController.cameraTarget;
        start = transform;
    }
    
    // Utility functions to make interpolation smoother.
    private static float InExpo(float t) => (float)Mathf.Pow(2, 10 * (t - 1));
    private static float InOutExpo(float t)
    {
        if (t < 0.5) return InExpo(t * 2) / 2;
        return 1 - InExpo((1 - t) * 2) / 2;
    }
}
