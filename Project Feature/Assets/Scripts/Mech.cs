/*
 * Salmoria, Wyatt
 * 05/02/24
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mech : MonoBehaviour, IEnterable
{
    public string Message => "Enter Mech";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Enter(PlayerController controller)
    {
        throw new System.NotImplementedException();
    }
}
