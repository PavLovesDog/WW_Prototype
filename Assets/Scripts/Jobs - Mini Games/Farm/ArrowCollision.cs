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
        MGManager = GameObject.FindAnyObjectByType<MiniGameManager_Farm>();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitPoint"))
        {
            // Handle the collision event
            // (e.g., score increment, play sound, etc.)
            //Debug.Log(gameObject.name + ": hit " + other.name);

            inHitPoint = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("HitPoint"))
        {
            //keep bool true
            inHitPoint = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("HitPoint"))
        {
            // "deactivate"
            inHitPoint = false;
        }
    }
}
