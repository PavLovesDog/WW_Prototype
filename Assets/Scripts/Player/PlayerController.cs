using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    protected PlayerStateMananger player;

    [Header("Actor variables")]
    public GameObject playerActor;

    [Header("Locomotion variables")]
    public float xAxisInput = Input.GetAxis("Horizontal");
    public float zAxisInput = Input.GetAxis("Vertical");
    public float speed = 7.5f;

    [Header("Interaction variables")]
    public float sphereCastRadius = 2.5f;
    public LayerMask interactableLayer;
    private List<Interactable> lastInteractables = new List<Interactable>();

    //constructor
    public PlayerController(PlayerStateMananger player)
    {
        this.player = player;

        //Initialize player reference
        playerActor = GameObject.FindGameObjectWithTag("Player");

        //Initialize layer mask for interactables
        interactableLayer = LayerMask.GetMask("Interactable");
        Debug.Log("Interactable Layer: " + interactableLayer);
    }


    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }
    public virtual void OnStateUpdate() 
    {
        //always listen for input
        xAxisInput = Input.GetAxis("Horizontal");
        zAxisInput = Input.GetAxis("Vertical");

        DetectInteractables();
    }

    public void DetectInteractables()
    {
        ///<summary>
        ///if any objects marked as interactable are within vicinity of player
        ///make their canvas text visible and the canInteract bool of that item true
        ///if player leaves the area of the interactable object
        ///make the interactable object canvas text invisible and the canInteract of said object false
        ///NOTE interactables will have a seperate script on them to handle when the player presses the 'interact' button
        /// </summary>

        Vector3 origin = player.transform.position;

        //get all colliders in radius on the object layer
        Collider[] hitColliders = Physics.OverlapSphere(origin, sphereCastRadius, interactableLayer);

        List<Interactable> currentInteractables = new List<Interactable>(); // create list for objects detected

        //Find all interactable colliders in range
        foreach(var hitCollider in hitColliders)
        {
            //get the hit objects script component
            Interactable interactable = hitCollider.GetComponent<Interactable>();

            if(interactable != null)
            {
                interactable.EnableInteractable(true); // set the canvas and interactable bools true
                currentInteractables.Add(interactable); // add them to our list
            }
        }

        //Reset all interactable colliders OUT of range
        foreach(var interactable in lastInteractables)
        {
            // if the interactable object is no longer in our current list
            if(!currentInteractables.Contains(interactable))
            {
                if(interactable != null)
                    interactable.EnableInteractable(false); // set the canvas and interactable bools false
            }
        }

        //Update lists for next frame
        lastInteractables = currentInteractables;
    }

    private void OnDrawGizmos()
    {
        //View Interactable Detection Distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereCastRadius);
    }
}
