using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerState currentState;

    //State Enums to show visually in inspector what our enemy is doing
    public enum State
    {
        IDLE,
        LOCOMOTION,
        INTERACTING
    };

    [Header("Current State")]
    public State state = State.IDLE; //initialize state to idle

    void Start()
    {
        TransitionTo(new P_IdleState(this));
    }

    void Update()
    {
        currentState?.OnStateUpdate();
    }

    public void TransitionTo(PlayerState newState)
    {
        currentState?.OnStateExit();
        currentState = newState;
        currentState.OnStateEnter();
    }
}
