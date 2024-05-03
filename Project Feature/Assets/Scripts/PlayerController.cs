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

    //Mouse related controls
    public float lookSensitivity = 1.0f;
    private float pitch = 0.0f;
    private float yaw = 0.0f;

    //The specific height of the player at this time
    private float currentHeight = 1.78f;

    //Dictates if the player can jump based on if in air or not
    private bool canJump = false;

    private Vector3 velocity = Vector3.zero;
    private Vector2 movementDirection = Vector2.zero;

    private new Rigidbody rigidbody;
    private BoxCollider boxCollider;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        Vector3 start = transform.position;
        Vector3 end = transform.position + velocity.normalized;
        Debug.DrawLine(start, end, Color.red);

        end = transform.position + new Vector3(movementDirection.x, 0.0f, movementDirection.y).normalized;
        Debug.DrawLine(start, end, Color.green);
    }

    public void Look()
    {

    }

    public void Move()
    {

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
