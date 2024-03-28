using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Interactable : MonoBehaviour
{
    public bool canInteract = false;
    public Canvas objectCanvas;

    public NPC_Dialogue NPC_Dialogue;
    public bool initiateDialogue = false;

    private void Awake()
    {
        if (objectCanvas == null)
        {
            //Find canvas object
            objectCanvas = GetComponentInChildren<Canvas>();
        }

        //set initial state
        EnableInteractable(false);
    }

    //Method to change the interact bool and canvas visibility
    //To be used by player scripts to enable item
    public void EnableInteractable(bool state)
    {
        canInteract = state;
        if (objectCanvas != null)
            objectCanvas.enabled = state; // Show or hide canvas based on state
    }

    private void Update()
    {
        // player is near and can interact with the object
        if (canInteract)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HandleInteractions();
            }
        }
    }

    private void HandleInteractions()
    {
        // whats the tag of this gameobject?
        switch (gameObject.tag)
        {
            case "InteractableObject":
                Debug.Log("This is an Interactable OBJECT!");
                // do object specific thing here
                // i.e move object, animate object etc.
                {
                    //Test Movement
                    Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                    Vector3 jumpImpulse = new Vector3(0, 5, 0);
                    rb.AddForce(jumpImpulse * 10f, ForceMode.Impulse);
                }
                break;

            case "InteractablePickup":
                Debug.Log("This is an Interactable PICKUP!");
                // add object to inventory
                // destroy object

                //Temp Demonstration
                {
                    MeshRenderer mesh = gameObject.GetComponentInChildren<MeshRenderer>();
                    SpriteRenderer sprite = gameObject.GetComponentInChildren<SpriteRenderer>();
                    if (mesh != null)
                        mesh.enabled = false;
                    else if (sprite != null)
                        sprite.enabled = false;

                    TMP_Text canvasText = objectCanvas.GetComponentInChildren<TMP_Text>();
                    canvasText.text = "Added to Inventory!";

                    StartCoroutine(DelayedDeletion());
                }

                break;

            case "InteractableNPC":
                Debug.Log("This is an Interactable NPC!");
                // enable text box of this specific NPC to hear dialogue
                initiateDialogue = true;

                // Temp demonstration
                {
                    TMP_Text canvasText = objectCanvas.GetComponentInChildren<TMP_Text>();
                    canvasText.text = "Hello!";

                    //start coroutine to reset canvas
                    StartCoroutine(ResetCanvas(canvasText));
                }

                break;
            case "InteractableWizard":
                //run level up here
                GetComponent<Wizard>().RunInteraction();
                break;
            default:
                // enable the text box of this item for some "flavour Text"
                break;

        }
    }



    IEnumerator DelayedDeletion()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

    IEnumerator ResetCanvas(TMP_Text canvasText)
    {
        yield return new WaitForSeconds(1.25f);
        canvasText.text = "(E) to Interact";
    }

}
