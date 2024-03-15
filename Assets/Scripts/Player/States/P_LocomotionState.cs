using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_LocomotionState : PlayerController
{
    //constructor
    public P_LocomotionState(PlayerStateMananger playerStateManager) : base(playerStateManager) { }

    //What happens on Entering LOCOMOTION state?
    public override void OnStateEnter()
    {
        //base.OnStateEnter();

        //update state
        player.state = PlayerStateMananger.State.LOCOMOTION;
        Debug.Log("Entering State: LOCOMOTION");

        //reset sprite rotations from testing anims
        player.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    //What is happening whilst in LOCOMOTION state?
    public override void OnStateUpdate()
    {
        base.OnStateUpdate();
        //Debug.Log("Current State: LOCOMOTION");

        ////Look for interactable objects
        //DetectInteractables();

        //Get movement Vector
        Vector3 moveDirection = new Vector3(xAxisInput, 0, zAxisInput);
        //apply to player object
        player.transform.position += moveDirection * Time.deltaTime * speed;

        
        //Listen for state change
        if (xAxisInput == 0 && zAxisInput == 0) // no input
        {
            player.TransitionTo(new P_IdleState(player));
        }

    }

    //What happens on Exiting LOCOMOTION state?
    public override void OnStateExit()
    {
        //base.OnStateExit();
        Debug.Log("Exiting State: LOCOMOTION");
    }

    //private void OnDrawGizmos()
    //{
    //    //View Interactable Detection Distance
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, sphereCastRadius);
    //}
}
