using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_InteractingState : PlayerController
{
    //constructor
    public P_InteractingState(PlayerStateMananger playerStateManager) : base(playerStateManager) { }

    //TEMP Variables
    float maxAngle = 10.0f; // amgle at which to rotate for temp animation

    public override void OnStateEnter()
    {
        //base.OnStateEnter();

        //update state
        player.state = PlayerStateMananger.State.INTERACTING;
        Debug.Log("Entering State: IDLE");
    }

    public override void OnStateExit()
    {
        //base.OnStateExit();
    }

    public override void OnStateUpdate()
    {
        //base.OnStateUpdate();

        //  i.e play idle animation for interacting
        float zRotateAngle = Mathf.Sin(Time.time * speed) * (maxAngle * 0.5f);
        player.transform.eulerAngles = new Vector3(0, 0, zRotateAngle);

        //Listen for state change
        //  this will be from the ending of certain actions or dialoges
    }

}
