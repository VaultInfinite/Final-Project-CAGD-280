/*
 * Salmoria, Wyatt
 * 05/02/24
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mech : PlayerController, IEnterable
{
    public string Message => "Enter Mech";

    public void Enter(PlayerController controller)
    {
        throw new System.NotImplementedException();
    }

    
}
