/*
 * Salmoria, Wyatt
 * 05/15/24
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pilot : PlayerController
{
    private bool inMechArea = false;
    private Mech mech;
    protected override void Update()
    {
        base.Update();

        if (inMechArea)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                mech.enabled = true;
                mech.camera.gameObject.SetActive(true);
                mech.GetComponent<CharacterController>().enabled = true;
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.tag == "Mech")
        {
            mech = other.gameObject.GetComponent<Mech>();
            print(mech);
            inMechArea = true;
            //Insert UI Element here informing player of controls
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Mech")
        {
            inMechArea = false;
            //Remove UI Element here
        }
    }
}

