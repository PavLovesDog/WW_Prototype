using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_IdleState : PlayerController
{
    //constructor
    public P_IdleState(PlayerStateMananger playerStateManager) : base(playerStateManager) { }

    //TEMP Variables
    float maxAngle = 10.0f; // amgle at which to rotate for temp animation

    //What happens on Entering IDLE state?
    public override void OnStateEnter()
    {
        //base.OnStateEnter();

        //update state
        player.state = PlayerStateMananger.State.IDLE;
        Debug.Log("Entering State: IDLE");

        //do other idle intro stuff
    }

    //What is happening whilst in IDLE state?
    public override void OnStateUpdate()
    {
        //base.OnStateUpdate();
        //Debug.Log("Current State: IDLE");

        ////Look for interactable objects
        //DetectInteractables();

        //DO IDLING STUFF
        //  i.e play idle animation
        float zRotateAngle = Mathf.Sin(Time.time * speed) * (maxAngle*0.5f);
        player.transform.eulerAngles = new Vector3(0, 0, zRotateAngle);

        //Listen for state change
        float xAxisInput = Input.GetAxis("Horizontal");
        float zAxisInput = Input.GetAxis("Vertical");
        if (xAxisInput != 0 || zAxisInput != 0)
        {
            player.TransitionTo(new P_LocomotionState(player));
        }
    }

    //What happens on Exiting IDLE state?
    public override void OnStateExit()
    {
        //base.OnStateExit();
        Debug.Log("Exiting State: IDLE");

    }
}
