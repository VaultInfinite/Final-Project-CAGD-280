/*
 * Salmoria, Wyatt
 * 05/07/24
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
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
    public float radius = 0.5f;
    
    //Mouse related controls in degrees
    public float lookSensitivity = 1.0f;
    private float pitch = 0.0f;
    private float yaw = 0.0f;

    //The specific height of the player at this time
    private float currentHeight = 1.78f;
    
    private bool grounded = true;
    private bool atTerminalVelocity = false;

    private Vector3 velocity = Vector3.zero;
    private Vector3 movementDirection = Vector3.zero;

    private CharacterController controller;

    //PlayerActionMap Designation
    private PlayerActionMap actions;

    private float JumpForce => Mathf.Sqrt(2.0f * jumpHeight * gravity);
    private Vector3 FeetPosition => Vector3.up * radius;

    void Awake()
    {
        actions = new PlayerActionMap();
        actions.Enable();

        controller = GetComponent<CharacterController>();
    }

    protected virtual void Update()
    {
        controller.stepOffset = stepHeight;
        controller.height = currentHeight;
        controller.slopeLimit = maxGroundAngle;
        controller.radius = radius;
        controller.center = Vector3.up * (currentHeight / 2.0f);

        Look();
    }

    void FixedUpdate()
    {
        Move();
       
        //Debug.Log("Velocity " + velocity);
        CollisionFlags flag = controller.Move(velocity * Time.fixedDeltaTime);
        velocity = controller.velocity;
        
        grounded = false;
        if (Physics.SphereCast(transform.position + FeetPosition, radius, Vector3.down, out RaycastHit hit, 0.1f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle < maxGroundAngle + Mathf.Epsilon)
            {
                grounded = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + FeetPosition, radius);
        
        Vector3 start = transform.position;
        Vector3 end = transform.position + velocity;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, end);

        end = transform.position + movementDirection;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);
    }

    public void Look()
    {
        Vector2 mouseDelta = actions.Actions.Look.ReadValue<Vector2>();

        //print("Yaw " + yaw);
        //print("Pitch " + pitch);

        yaw = (yaw + mouseDelta.x * lookSensitivity) % 360.0f;
        pitch = Mathf.Clamp(pitch - mouseDelta.y * lookSensitivity, -89.9f, 89.9f);

        camera.transform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);
        visuals.transform.rotation = Quaternion.Euler(0.0f, yaw, 0.0f);
    }

    public void Move()
    {
        bool wantJump = actions.Actions.Jumping.IsPressed();
        bool wantCrouch = actions.Actions.Crouch.IsPressed();
        
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

        if (wantCrouch)
        {
            currentHeight = crouchHeight;
        }
        else
        {
            currentHeight = standingHeight;
        }

        Vector3 scale = visuals.transform.localScale;
        scale.y = currentHeight / standingHeight;
        visuals.transform.localScale = scale;
        
        CheckTerminalVelocity();
        // CheckGrounded();
    }

    public void MoveGrounded()
    {
        Vector2 dir = actions.Actions.Movement.ReadValue<Vector2>();
        movementDirection = visuals.transform.rotation * new Vector3(dir.x, 0.0f, dir.y).normalized;

        Vector3 newVelocity = velocity;
        newVelocity = ApplyFriction(newVelocity);
        newVelocity = ApplyAcceleration(newVelocity);

        newVelocity.y = velocity.y;
        velocity = newVelocity;
    }

    private static Vector3 Slide(Vector3 motion, Vector3 normal)
    {
        return motion - normal * Vector3.Dot(motion, normal);
    }

    private Vector3 ApplyFriction(Vector3 inVelocity)
    {
        float speed = inVelocity.magnitude;

        if (speed == 0.0f)
        {
            return Vector3.zero;
        }

        float drop = speed * friction * Time.fixedDeltaTime;
        return inVelocity * Mathf.Max(speed - drop, 0.0f) / speed;
    }

    private Vector3 ApplyAcceleration(Vector3 inVelocity)
    {
        float currentSpeed = Vector3.Dot(inVelocity, movementDirection);
        float addSpeed = Mathf.Clamp(walkSpeed - currentSpeed, 0.0f, acceleration * Time.fixedDeltaTime);

        return inVelocity + movementDirection * addSpeed;
    }

    public void MoveAir()
    {
        Vector2 dir = actions.Actions.Movement.ReadValue<Vector2>();
        movementDirection = visuals.transform.rotation * new Vector3(dir.x, 0.0f, dir.y).normalized;

        Vector3 newVelocity = velocity;
        newVelocity = ApplyAcceleration(newVelocity);

        newVelocity.y = velocity.y;
        velocity = newVelocity;
    }

    public void Jump()
    {
        velocity.y = JumpForce;
    }

    public void CheckTerminalVelocity()
    {
        atTerminalVelocity = false || velocity.y >= terminalVelocity;
    }

    // public void CheckGrounded()
    // {
    //     grounded = false;
    //     
    //     bool hit = TestMotion(Vector3.down * 0.1f, out RaycastHit result, false);
    //
    //     if (hit)
    //     {
    //         float groundAngle = Vector3.Angle(upDirection, result.normal);
    //         if (groundAngle <= maxGroundAngle + Mathf.Epsilon)
    //         {
    //             grounded = true;
    //         }
    //     }
    // }

    // private bool TestMotion(Vector3 motion, out RaycastHit result, bool boxCast = false)
    // {
    //     return boxCast switch
    //     {
    //         true => Physics.BoxCast(boxCollider.transform.position, boxCollider.size * 0.5f, motion.normalized, out result,
    //             boxCollider.transform.rotation, motion.magnitude, collisionLayers, QueryTriggerInteraction.Ignore),
    //         false => rigidBody.SweepTest(motion.normalized, out result, motion.magnitude,
    //             QueryTriggerInteraction.Ignore)
    //     };
    // }
    //
    // private RaycastHit[] TestMotionAll(Vector3 motion, bool boxCast = false)
    // {
    //     if (boxCast)
    //     {
    //         return Physics.BoxCastAll(boxCollider.transform.position, boxCollider.size * 0.5f, motion.normalized,
    //             boxCollider.transform.rotation, motion.magnitude, collisionLayers, QueryTriggerInteraction.Ignore);
    //     }
    //     return rigidBody.SweepTestAll(motion.normalized, motion.magnitude,
    //         QueryTriggerInteraction.Ignore);
    // }
}