using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStateMananger : MonoBehaviour
{
    private PlayerController currentState;

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
        // Get the active scene
        Scene scene = SceneManager.GetActiveScene();
        int sceneIndex = scene.buildIndex; // get its build index to compare for starting states

        //NOTE Adjust this if other scenes will require player movement
        if (sceneIndex == 0) // the overworld scene
            TransitionTo(new P_IdleState(this));
        else // scene is a job scene
            TransitionTo(new P_InteractingState(this));
    }

    void Update()
    {
        currentState?.OnStateUpdate();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="newState">State which the player is transitioning to</param>
    public void TransitionTo(PlayerController newState)
    {
        currentState?.OnStateExit();
        currentState = newState;
        currentState.OnStateEnter();
    }
}
