using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowCollision : MonoBehaviour
{
    public bool HitPoint = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitPoint"))
        {
            // Handle the collision event
            // (e.g., score increment, play sound, etc.)
            //Debug.Log(gameObject.name + ": hit " + other.name);

            HitPoint = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("HitPoint"))
        {
            //keep bool true
            HitPoint = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("HitPoint"))
        {
            // "deactivate"
            HitPoint = false;
        }
    }
}
