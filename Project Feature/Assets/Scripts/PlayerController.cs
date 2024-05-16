/*
 * Salmoria, Wyatt
 * 05/15/24
 * The controller for the player, both Mech & Pilot; Determines many different things related to both XYZ Movement and camera pitch & yaw
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    //Designation for Camera
    public new CustomCamera camera;
    //Designation for the target of the camera to follow
    public GameObject cameraTarget;
    //Designation for player visuals to turn with the camera.
    public GameObject visuals;

    

    //Variable speeds
    public float walkSpeed = 7.0f;
    public float dashSpeed = 15.0f;
    public float acceleration = 1.0f;
    public float deceleration = 1.0f;
    public float friction = 2.0f;
    public float dashTime = 0.05f;
    public float dashCooldown = 1.2f;


    //Variable player movement states
    public float maxGroundAngle = 45.0f;
    public float stepHeight = 0.25f;
    public float jumpHeight = 1.25f;
    public float radius = 0.5f;
    
    //Mouse related controls in degrees
    public float lookSensitivity = 1.0f;
    private float pitch = 0.0f;
    private float yaw = 0.0f;

    //Vertical velocity variables
    public float gravity = 9.81f;
    public float terminalVelocity = 9.81f * 5.0f;

    //Booleans to check for specific aspects of the script.
    private bool dashing = false;
    protected bool shouldJump = false;
    private bool canDash = true;
    private bool grounded = true;
    private bool atTerminalVelocity = false;

    //Vector3 designation for Velocity
    private Vector3 velocity = Vector3.zero;
    //Vector3 designation for movementDirection to allow directional movement.
    private Vector3 movementDirection = Vector3.zero;

    //Designation for the characterController of the player.
    protected CharacterController controller;

    //PlayerActionMap Designation
    protected PlayerActionMap actions;

    //The jumpforce of the player based off of jumpheight & gravity alongside other mathematics.
    private float JumpForce => Mathf.Sqrt(2.0f * jumpHeight * gravity);
    //The position for the ground check; makes sure the player doesn't fall through the map.
    private Vector3 FeetPosition => Vector3.up * radius;

    void Awake()
    {
        actions = new PlayerActionMap();
        actions.Enable();

        controller = GetComponent<CharacterController>();
    }

    protected virtual void Update()
    {
        //Sets specific aspects of the character controller based off stepheight, maxgroundangle, and radius.
        controller.stepOffset = stepHeight;
        controller.slopeLimit = maxGroundAngle;
        controller.radius = radius;
        
        //Call for the camera look function.
        Look();
    }

    void FixedUpdate()
    {
        //Call for the player movement function.
        Move();
       
        //Debug.Log("Velocity " + velocity);
        CollisionFlags flag = controller.Move(velocity * Time.fixedDeltaTime);
        velocity = controller.velocity;

        //Checks if the player is grounded or not by utilizing the feet position and several other factors.
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
    /// <summary>
    /// Built in unity function to allow drawn aspects; allows debug visuals for Velocity.
    /// </summary>
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
    /// <summary>
    /// Allows the player to look in a 360 degree area while also keeping it from completely breaking. Also allows the visuals of the player- both mech & pilot- to follow the camera.
    /// </summary>
    public void Look()
    {
        Vector2 mouseDelta = actions.Actions.Look.ReadValue<Vector2>();

        yaw = (yaw + mouseDelta.x * lookSensitivity) % 360.0f;
        pitch = Mathf.Clamp(pitch - mouseDelta.y * lookSensitivity, -89.9f, 89.9f);

        cameraTarget.transform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);
        visuals.transform.rotation = Quaternion.Euler(0.0f, yaw, 0.0f);
    }
    /// <summary>
    /// Sets the pitch & Yaw of the camera
    /// </summary>
    /// <param name="pitch">Vertical Look</param>
    /// <param name="yaw">Horizontal Look</param>
    public void LookAt(float pitch, float yaw)
    {
        this.pitch = pitch;
        this.yaw = yaw;

        Look();
    }
    /// <summary>
    /// Allows the Mech to dash upon hitting spacebar; only accessible through mech.
    /// </summary>
    public void Dash()
    {
        if (canDash)
        {
            dashing = true;
            canDash = false;
            Invoke("StopDash", dashTime);
            Invoke("SetCanDash", dashTime + dashCooldown);
        }        
    }
    /// <summary>
    /// Stops the mech dash.
    /// </summary>
    private void StopDash()
    {
        dashing = false;
    }
    /// <summary>
    /// Sets the mech to be able to dash following a cooldown.
    /// </summary>
    private void SetCanDash()
    {
        canDash = true;
    }
    /// <summary>
    /// Controls the movement for the mech & pilot while also checking for other movement states, such as dashing.
    /// </summary>
    public void Move()
    {
        if (grounded && !dashing)
        {
            if (shouldJump)
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

        if (dashing)
        {
            MoveDash();
        }
        
        CheckTerminalVelocity();
    }
    /// <summary>
    /// For movement while on the ground.
    /// </summary>
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
    /// <summary>
    /// Controls player movement during the dash.
    /// </summary>
    public void MoveDash()
    {
        Vector3 dir = (movementDirection + velocity.normalized).normalized;

        Vector3 newVelocity = dir * dashSpeed;
        velocity = newVelocity;
    }

    /// <summary>
    /// Applies friction to the movement of the player.
    /// </summary>
    /// <param name="inVelocity">Input Velocity</param>
    /// <returns>Returns the calculated input velocity for Friction</returns>
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

    /// <summary>
    /// Applies acceleration to the player.
    /// </summary>
    /// <param name="inVelocity">Input Velocity</param>
    /// <returns>Returns the calculated input velocity for Acceleration</returns>
    private Vector3 ApplyAcceleration(Vector3 inVelocity)
    {
        float currentSpeed = Vector3.Dot(inVelocity, movementDirection);
        float addSpeed = Mathf.Clamp(walkSpeed - currentSpeed, 0.0f, acceleration * Time.fixedDeltaTime);

        if (movementDirection.magnitude <= Mathf.Epsilon)
        {
            float speed = inVelocity.magnitude;
            if (speed == 0.0f)
            {
                return Vector3.zero;
            }
            
            float drop = deceleration * Time.fixedDeltaTime;
            return inVelocity * Mathf.Max(speed - drop, 0.0f) / speed;
        }
        return inVelocity + movementDirection * addSpeed;
    }
    /// <summary>
    /// Allows movement in the air following a pilot jump.
    /// </summary>
    public void MoveAir()
    {
        Vector2 dir = actions.Actions.Movement.ReadValue<Vector2>();
        movementDirection = visuals.transform.rotation * new Vector3(dir.x, 0.0f, dir.y).normalized;

        Vector3 newVelocity = velocity;
        newVelocity = ApplyAcceleration(newVelocity);

        newVelocity.y = velocity.y;
        velocity = newVelocity;
    }
    /// <summary>
    /// Allows the pilot to jump.
    /// </summary>
    public void Jump()
    {
        velocity.y = JumpForce;
    }
    /// <summary>
    /// Checks to make sure the pilot has not reach terminal velocity by limiting vertical velocity.
    /// </summary>
    public void CheckTerminalVelocity()
    {
        atTerminalVelocity = velocity.y >= terminalVelocity;
    }
}