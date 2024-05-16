/*
 * Salmoria, Wyatt
 * 05/15/24
 * Child class for the mech, Allows the mech to dash, controls to exit the mech and swap back to the pilot.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Mech : PlayerController
{
    //Exit point for the pilot
    public GameObject pilotExitPoint;
    //Designation for the pilot
    private Pilot pilot;
    //Designation for the UIExit element
    public GameObject UIExit;


    protected override void Update()
    {
        base.Update();
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            ExitMech();
        }

        if (actions.Actions.Jumping.WasPressedThisFrame())
        {
            Dash();
        }
    }
    /// <summary>
    /// Enables UIExit, changes the Camera from Pilot to Mech, and then disables the Pilot.
    /// </summary>
    /// <param name="who">The Player</param>
    public void EnterMech(Pilot who)
    {
        UIExit.SetActive(true);
        Invoke("HideUIExit", 3.0f);

        pilot = who;

        enabled = true;
        // camera.SetActive(true);
        pilot.GetComponent<CharacterController>().enabled = false;
        controller.enabled = true;
        
        pilot.enabled = false;
        pilot.cameraTarget.SetActive(false);
        camera.ChangeTarget(this);
    }
    /// <summary>
    /// Disables the Mech, changes Camera back to Pilot, and reenables Pilot at new position.
    /// </summary>
    public void ExitMech()
    {
        enabled = false;
        cameraTarget.SetActive(false);

        pilot.transform.position = pilotExitPoint.transform.position;
        pilot.GetComponent<CharacterController>().enabled = true;
        controller.enabled = false;
        pilot.enabled = true;
        // pilot.camera.SetActive(true);

        float pitch = pilotExitPoint.transform.rotation.eulerAngles.x;
        float yaw = pilotExitPoint.transform.rotation.eulerAngles.y;
        pilot.LookAt(pitch, yaw);
        camera.ChangeTarget(pilot);
    }
    /// <summary>
    /// Hides the UIExit Element after 3 seconds.
    /// </summary>
    private void HideUIExit()
    {
        UIExit.SetActive(false);
    }
}
