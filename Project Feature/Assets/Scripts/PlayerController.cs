/*
 * Salmoria, Wyatt
 * 05/02/24
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public new Camera camera;
    public GameObject visuals;

    public float gravity = 9.81f;
    public float terminalVelocity = 9.81f * 5.0f;

    //Variable speeds
    public float runSpeed = 10.0f;
    public float walkSpeed = 7.0f;
    public float crouchSpeed = 4.0f;
    public float acceleration = 1.0f;
    public float deceleration = 1.0f;
    public float friction = 2.0f;

    //Variable player movement states
    public float crouchTransitionTime = 0.1f;
    public float maxGroundAngle = 45.0f;
    public float standingHeight = 1.78f;
    public float crouchHeight = 1.0f;
    public float stepHeight = 0.25f;
    public float jumpHeight = 1.25f;

    public Vector3 upDirection = Vector3.up;

    //Mouse related controls in degrees
    public float lookSensitivity = 1.0f;
    private float pitch = 0.0f;
    private float yaw = 0.0f;

    //The specific height of the player at this time
    private float currentHeight = 1.78f;

    //Dictates if the player can jump based on if in air or not
    private bool canJump = false;
    private bool grounded = true;
    private bool atTerminalVelocity = false;

    private Vector3 velocity = -Vector3.one;
    private Vector3 movementDirection = Vector3.zero;

    private new Rigidbody rigidbody;
    private BoxCollider boxCollider;

    //PlayerActionMap Designation
    private PlayerActionMap actions;

    private float JumpForce {
        get
        {
            return Mathf.Sqrt(2.0f * jumpHeight * gravity);
        }
    }

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
        
        MoveAndCollide();
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
        bool wantJump = actions.Actions.Jumping.IsPressed();

        CheckTerminalVelocity();
        CheckGrounded();
        Debug.Log(grounded);
        
        if (grounded)
        {
            if (wantJump)
            {
                Jump();
            }
            else
            {
                velocity.y = 0.0f;

                MoveGrounded();
            }
        }
        else
        {
            if (!atTerminalVelocity)
            {
                velocity.y -= gravity * Time.fixedDeltaTime;
            }

            MoveAir();
        }

        //bool movingForward = Vector2.Dot(dir, Vector2.up) >= 0.455f;

        //if (actions.Actions.Sprint.IsPressed() && movingForward)
        //{
        //    velocity = movementDirection * runSpeed;
        //}
        //else if(actions.Actions.Crouch.IsPressed())
        //{
        //    velocity = movementDirection * crouchSpeed;
        //}
        //else
        //{
        //    velocity = movementDirection * walkSpeed;
        //}
    }

    public void MoveGrounded()
    {
        Vector2 dir = actions.Actions.Movement.ReadValue<Vector2>();
        movementDirection = visuals.transform.rotation * new Vector3(dir.x, 0.0f, dir.y).normalized;

        Vector3 newVelocity = movementDirection;
        newVelocity = ApplyFriction(newVelocity);
        newVelocity = ApplyAcceration(newVelocity);

        newVelocity.y = velocity.y;
        velocity = newVelocity;
    }

    private void MoveAndCollide()
    {
        bool hit = rigidbody.SweepTest(velocity.normalized, out var result, velocity.magnitude * Time.fixedDeltaTime, QueryTriggerInteraction.Collide);
        //Physics.BoxCast(boxCollider.transform.position, boxCollider.size * 0.5f, velocity.normalized, out RaycastHit result, boxCollider.transform.rotation, velocity.magnitude * Time.fixedDeltaTime, 0xF, QueryTriggerInteraction.Collide);

        Vector3 newPosition;
        if (hit)
        {
            newPosition = transform.position + velocity.normalized * result.distance;
            
        } else
        {
            newPosition = transform.position + velocity * Time.fixedDeltaTime;
        }
        rigidbody.MovePosition(newPosition);
    }

    private Vector3 ApplyFriction(Vector3 inVelocity)
    {
        float speed = inVelocity.magnitude;

        if(speed != 0.0f)
        {
            float drop = speed * friction * Time.fixedDeltaTime;
            return inVelocity * Mathf.Max(speed - drop, 0.0f) / speed;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private Vector3 ApplyAcceration(Vector3 inVelocity)
    {
        float currentSpeed = Vector3.Dot(inVelocity, movementDirection);
        float addSpeed = Mathf.Clamp(walkSpeed - currentSpeed, 0.0f, acceleration * Time.fixedDeltaTime);

        return inVelocity + movementDirection * addSpeed;
    }

    public void MoveAir()
    {

    }

    public void Jump()
    {
        velocity.y = JumpForce;
    }

    public void Enter()
    {

    }

    public void CheckTerminalVelocity()
    {
        
    }

    public void CheckGrounded()
    {
        float groundDepth = -1.0f;

        RaycastHit[] hits = rigidbody.SweepTestAll(velocity.normalized, velocity.magnitude * Time.fixedDeltaTime, QueryTriggerInteraction.Collide);
        //RaycastHit[] hits = Physics.BoxCastAll(boxCollider.transform.position, boxCollider.size * 0.5f, velocity.normalized, boxCollider.transform.rotation, velocity.magnitude * Time.fixedDeltaTime, 0xF, QueryTriggerInteraction.Collide);

        if (hits.Length <= 0)
        {
            grounded = false;
        }

        foreach (var hit in hits)
        {
            Physics.ComputePenetration(boxCollider, transform.position, transform.rotation,
                hit.collider, hit.collider.transform.position, hit.collider.transform.rotation,
                out Vector3 direction, out float collisionDepth);

            float groundAngle = Vector3.Angle(upDirection, hit.normal);
            if (groundAngle <= maxGroundAngle + Mathf.Epsilon)
            {
                grounded = true;
                if (groundDepth > collisionDepth)
                {
                    groundDepth = collisionDepth;
                }
            }
        }
    }
}
