/*
 * Salmoria, Wyatt
 * 05/02/24
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public new Camera camera;
    public GameObject visuals;

    //Variable speeds
    public float runSpeed = 10.0f;
    public float walkSpeed = 7.0f;
    public float crouchSpeed = 4.0f;
    public float acceleration = 1.0f;
    public float deceleration = 1.0f;
    public float friction = 2.0f;

    //Variable stances
    public float crouchTransitionTime = 0.1f;
    public float maxGroundAngle = 45.0f;
    public float standingHeight = 1.78f;
    public float crouchHeight = 1.0f;
    public float stepHeight = 0.25f;
    public float jumpHeight = 1.25f;

    //Mouse related controls in degrees
    public float lookSensitivity = 1.0f;
    private float pitch = 0.0f;
    private float yaw = 0.0f;

    //The specific height of the player at this time
    private float currentHeight = 1.78f;

    //Dictates if the player can jump based on if in air or not
    private bool canJump = false;

    private Vector3 velocity = Vector3.zero;
    private Vector3 movementDirection = Vector3.zero;

    private new Rigidbody rigidbody;
    private BoxCollider boxCollider;

    //PlayerActionMap Designation
    private PlayerActionMap actions;
    

    void Awake()
    {
        actions = new PlayerActionMap();
        actions.Enable();

        rigidbody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        Look();
    }

    void FixedUpdate()
    {
        Vector3 start = transform.position;
        Vector3 end = transform.position + velocity.normalized;
        Debug.DrawLine(start, end, Color.red);

        end = transform.position + new Vector3(movementDirection.x, 0.0f, movementDirection.y).normalized;
        Debug.DrawLine(start, end, Color.green);

        Move();

        transform.Translate(velocity * Time.fixedDeltaTime);
    }

    public void Look()
    {
        Vector2 mouseDelta = actions.Actions.Look.ReadValue<Vector2>();

        yaw = (yaw + mouseDelta.x * lookSensitivity) % 360.0f;
        pitch = Mathf.Clamp(pitch - mouseDelta.y * lookSensitivity, -89.9f, 89.9f);

        camera.transform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);
        visuals.transform.rotation = Quaternion.Euler(0.0f, yaw, 0.0f);
    }

    public void Move()
    {
        Vector2 dir = actions.Actions.Movement.ReadValue<Vector2>();
        movementDirection = visuals.transform.rotation * new Vector3(dir.x, 0.0f, dir.y);
        bool movingForward = Vector2.Dot(dir, Vector2.up) >= 0.455f;

        if (actions.Actions.Sprint.IsPressed() && movingForward)
        {
            velocity = movementDirection * runSpeed;
        }
        else if(actions.Actions.Crouch.IsPressed())
        {
            velocity = movementDirection * crouchSpeed;
        }
        else
        {
            velocity = movementDirection * walkSpeed;
        }
    }

    public void Jump()
    {

    }

    public void Enter()
    {

    }

    public void IsGrounded()
    {

    }
}
