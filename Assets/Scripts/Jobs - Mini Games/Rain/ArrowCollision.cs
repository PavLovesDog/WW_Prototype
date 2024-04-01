using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowCollision : MonoBehaviour
{
    public ArrowCollision() { }

    public bool inHitPoint = false;
    public bool canReset = false;
    MiniGameManager_Farm MGManager;

    private void Awake()
    {
        //should only be one manager
        MGManager = FindObjectOfType<MiniGameManager_Farm>();

        //Debug.Log(gameObject.name);
    }

    private void Update()
    {
        float zChange = Time.deltaTime * MGManager.scrollSpeed; // apply movement speed to z xais
        Vector3 translation = new Vector3(0, 0, zChange); // store in vector

        transform.localPosition += translation; // apply transformation

        //if the get too far below parented object (Out Of View)
        if (transform.localPosition.z >= 6f)
        {
            canReset = true;
        }
    }

    private void SwitchArrowSpecificHitBools(bool state)
    {
        // Check if the gameObject's name contains any of the substrings
        if (gameObject.name.Contains("Arrow_Left"))
        {
            MGManager.canHitLeftArrow = state;
        }
        else if (gameObject.name.Contains("Arrow_Up"))
        {
            MGManager.canHitUpArrow = state;
        }
        else if (gameObject.name.Contains("Arrow_Down"))
        {
            MGManager.canHitDownArrow = state;
        }
        else if (gameObject.name.Contains("Arrow_Right"))
        {
            MGManager.canHitRightArrow = state;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitPoint"))
        {
            // Handle the collision event
            // (e.g., score increment, play sound, etc.)
            //Debug.Log(gameObject.name + ": hit " + other.name);
            SwitchArrowSpecificHitBools(true);

            inHitPoint = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("HitPoint"))
        {
            //keep bool true
            SwitchArrowSpecificHitBools(true);
            inHitPoint = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("HitPoint"))
        {
            // "deactivate"
            SwitchArrowSpecificHitBools(false);
            inHitPoint = false;
        }
    }
}
