/*
 * Salmoria, Wyatt
 * 05/15/24
 * Child Class for the Pilot, determines jumping, and checks if in Mech radius, allowing transition if in radius.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pilot : PlayerController
{
    //Bool to see if player is in collision box of Mech
    private bool inMechArea = false;
    //Designation for the mech
    private Mech mech;
    //Designation for the UIEnter element
    public GameObject UIEnter;

    protected override void Update()
    {
        base.Update();

        shouldJump = actions.Actions.Jumping.IsPressed();
        //If inmechArea is true and E key is pressed, enter the mech and disable UIEnter element.
        if (inMechArea)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                mech.EnterMech(this);
                UIEnter.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //If Pilot collides with Mech, grab Mech GameObject, set boolean inMechArea to true, and UIEnter to active.
        if (other.CompareTag("Mech"))
        {
            mech = other.gameObject.GetComponent<Mech>();
            print(mech);
            inMechArea = true;
            UIEnter.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //If Pilot no longer collides with mech, set boolean inMechArea to false, and UIEnter to inactive.
        if (other.CompareTag("Mech"))
        {
            inMechArea = false;
            UIEnter.SetActive(false);
        }
    }
}

