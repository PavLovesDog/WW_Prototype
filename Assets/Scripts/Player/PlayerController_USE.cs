using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController_USE : MonoBehaviour
{
    [Header("Actor variables")]
    public GameObject playerActor;

    [Header("Locomotion variables")]
    public float xAxisInput;
    public float zAxisInput; 
    public float speed = 7.5f;

    [Header("Interaction variables")]
    public float sphereCastRadius = 2.5f;
    public LayerMask interactableLayer;
    private List<Interactable> lastInteractables = new List<Interactable>();

    float maxAngle = 10.0f;

    private void Update()
    {
        //always listen for input
        xAxisInput = Input.GetAxis("Horizontal");
        zAxisInput = Input.GetAxis("Vertical");

        //Get movement Vector
        Vector3 moveDirection = new Vector3(xAxisInput, 0, zAxisInput);

        //apply to player object
        transform.position += moveDirection * Time.deltaTime * speed;


        //Listen for state change
        if (xAxisInput == 0 && zAxisInput == 0) // no input
        {
            //DO IDLING STUFF
            //  i.e play idle animation
            float zRotateAngle = Mathf.Sin(Time.time * speed) * (maxAngle * 0.35f);
            transform.eulerAngles = new Vector3(0, 0, zRotateAngle);
        }
        else
        {
            //reset rotations
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

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

        Vector3 origin = transform.position;

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
