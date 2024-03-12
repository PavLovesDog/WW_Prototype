using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class MiniGameManager_Farm : MonoBehaviour
{
    [Header("Lanes")]
    public GameObject[] lanes;

    [Header("Arrows")]
    public GameObject[] hitPoints;
    public ArrowCollision[] directionalArrows;


    [Header("Arrow Movement variables")]
    public float scrollSpeed;

    private void Update()
    {
        MoveArrows();
        HandlePlayerInput();
    }


    private void HandlePlayerInput()
    {
        //listen for key press, each individually
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Change colour of hit zone to visually show player button press
            SpriteRenderer hitArrowSprite = hitPoints[0].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (directionalArrows[0].HitPoint == true)
            {
                Debug.Log("HIT the LEFT arrow");
            }
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[1].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (directionalArrows[1].HitPoint == true)
            {
                Debug.Log("HIT the UP arrow");
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[2].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (directionalArrows[2].HitPoint == true)
            {
                Debug.Log("HIT the DOWN arrow");
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[3].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (directionalArrows[3].HitPoint == true)
            {
                Debug.Log("HIT the RIGHT arrow");
            }
        }
        //if the corresponding arrow bool is enabled, register a hit
    }

    //buddy coroutine for Input press
    IEnumerator ChangeSpriteAplha(SpriteRenderer sprite)
    {
        float visibleAlpha = 1f;
        float dullAlpha = 0.18f; // get original alpha (currently set to 18)

        //make colour bright
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, visibleAlpha);

        yield return new WaitForSeconds(0.15f);

        //make colour dull
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, dullAlpha);
    }

    // Move the arrows down the play area
    // TODO, Randomize arrow placements & ensure and equal distance is always between arrows
    // distance between arrows could be controlled to add difficulty as tiime went on
    private void MoveArrows()
    {
        //Move Arrows down the playing field
        foreach (ArrowCollision arrow in directionalArrows)
        {
            // move them down z axis at a speed
            float zChange = Time.deltaTime * scrollSpeed; // apply movement speed to z xais
            Vector3 translation = new Vector3(0, 0, zChange); // store in vector

            arrow.gameObject.transform.localPosition += translation; // apply transformation

            //if the get too far below parented object (Out Of View)
            if (arrow.gameObject.transform.localPosition.z >= 6f)
            {
                Vector3 resetPos = arrow.transform.position;
                resetPos.x = 0f;
                resetPos.y = 0f;
                resetPos.z = -10f;

                arrow.gameObject.transform.localPosition = resetPos;
            }
        }
    }

}
