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
    public List<GameObject> arrowsOnScreen;

    [Header("Arrow Spawn Points")]
    public Transform[] arrowSpawnPoints;

    [Header("Game variables")]
    [Tooltip("Speed at which arrows will scroll down the screen/move")] public float scrollSpeed;
    public int currentArrowsInPlay;
    public int totalArrowsInPlay = 10;
    public float spawnDelayMin = 0.5f;
    public float spawnDelayMax = 2f;
    private float spawnTimer = 0f;

    public bool canHitLeftArrow = false;
    public bool canHitUpArrow = false;
    public bool canHitDownArrow = false;
    public bool canHitRightArrow = false;

    [Header("Prefabs")]
    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject upArrow;
    public GameObject downArrow;


    private void Update()
    {
        //MoveArrows();
        SpawnArrows();
        HandlePlayerInput();
    }


    private void SpawnArrows()
    {
        //set a number of arrows to spawn
        if(currentArrowsInPlay < totalArrowsInPlay) // can spawn arrow 
        {
            // stagger their spawning in even amounts (maybe by a float, or time dependent)
            //increment spanw timer for delays
            spawnTimer += Time.deltaTime;

            float spawnDelay = Random.Range(spawnDelayMin, spawnDelayMax);

            if(spawnTimer > spawnDelay) // get spawnin'
            {
                //reset timer
                spawnTimer = 0f;

                // randomize from which position the next arrow will spawn from, to keep from repeating patterns frequently
                //get which arrow to instantiate randomly
                GameObject arrow = SelectRandomArrow();
                GameObject arrowToAdd = new GameObject();
                //Debug.Log(arrow.name);

                // spawn arrows in their correct lanes, i.e parented. (left arrow in lane 1(lanes[0]), up arrow in lane 2(lanes[1]), etc.)
                switch (arrow.name) // names will be the same as saved prefab
                {
                    case "Arrow_Left":
                        arrowToAdd = Instantiate(arrow, arrowSpawnPoints[0].transform.position, new Quaternion(0,0,0,0), lanes[0].transform);
                        break;
                    case "Arrow_Up":
                        arrowToAdd = Instantiate(arrow, arrowSpawnPoints[1].transform.position, new Quaternion(0, 0, 0, 0), lanes[1].transform);
                        break;
                    case "Arrow_Down":
                        arrowToAdd = Instantiate(arrow, arrowSpawnPoints[2].transform.position, new Quaternion(0, 0, 0, 0), lanes[2].transform);
                        break;
                    case "Arrow_Right":
                        arrowToAdd = Instantiate(arrow, arrowSpawnPoints[3].transform.position, new Quaternion(0, 0, 0, 0), lanes[3].transform);
                        break;

                }

                // add to directional arrows list
                arrowsOnScreen.Add(arrowToAdd);

                //increment currentArrowsInPlay
                currentArrowsInPlay++;
            }
                //arrow now spawned, it will handle its own movement

        
        }

        // handle each arrow on screen, iterate backwards through for safe removal
        for (int i = arrowsOnScreen.Count - 1; i >= 0; i--)
        {
            GameObject arrow = arrowsOnScreen[i];
            if (arrow != null)
            {
                //get the arrow unique script
                ArrowCollision script = arrow.GetComponent<ArrowCollision>();
                Debug.Log(script.canReset);

                //listen for when an arrow is now out of screen view
                if (script.canReset)
                {
                    Debug.Log(arrow.name + " CAN reset");
                    //reset bool
                    script.canReset = false;

                    //arrows are now off screen,
                    //remove them from list
                    arrowsOnScreen.RemoveAt(i);

                    currentArrowsInPlay--;
                    //destroy them after
                    StartCoroutine(DelayedDestroyObject(arrow));
                }
            }
        }
    }
    
    //Buddy Method for SpawnArrows()
    private GameObject SelectRandomArrow()
    {
        int randNum = Random.Range(0, 4);
        //Debug.Log(randNum); // display number in console
        GameObject arrow = new GameObject();
        switch(randNum)
        {
            case 0: // Left arrow

                arrow = leftArrow;
                break;
            case 1: // Up arrow
                arrow = upArrow;
                break;
            case 2: // Down arrow
                arrow = downArrow;
                break;
            case 3: // Right arrow
                arrow = rightArrow;
                break;

        }
        return arrow;
    }

    //TODO think of way to handle hit recognition
    private void HandlePlayerInput()
    {
        //listen for key press, each individually
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Change colour of hit zone to visually show player button press
            SpriteRenderer hitArrowSprite = hitPoints[0].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            //if (true /*any arrow in the left lanbe is registering true*/)
            //{
            //    Debug.Log("HIT the LEFT arrow");
            //    //play a little particle effect to show hit
            //    //play audio to show hit
            //}
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[1].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            //if (arrowsOnScreen != null && arrowsOnScreen[1].HitPoint == true)
            //{
            //    Debug.Log("HIT the UP arrow");
            //}
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[2].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            //if (arrowsOnScreen != null && arrowsOnScreen[2].HitPoint == true)
            //{
            //    Debug.Log("HIT the DOWN arrow");
            //}
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[3].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            //if (arrowsOnScreen != null && arrowsOnScreen[3].HitPoint == true)
            //{
            //    Debug.Log("HIT the RIGHT arrow");
            //}
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
    //Coroutine to delete an arrow after delay
    IEnumerator DelayedDestroyObject(GameObject objectToDelete) 
    { 
        yield return new WaitForEndOfFrame();
        
        Destroy(objectToDelete);
    }
    
    
    // Move the arrows down the play area
    // TODO, Randomize arrow placements & ensure and equal distance is always between arrows
    // distance between arrows could be controlled to add difficulty as tiime went on
    //private void MoveArrows()
    //{
    //    //Move Arrows down the playing field
    //    foreach (ArrowCollision arrow in directionalArrows)
    //    {
    //        // move them down z axis at a speed
    //        float zChange = Time.deltaTime * scrollSpeed; // apply movement speed to z xais
    //        Vector3 translation = new Vector3(0, 0, zChange); // store in vector
    //
    //        arrow.gameObject.transform.localPosition += translation; // apply transformation
    //
    //        //if the get too far below parented object (Out Of View)
    //        if (arrow.gameObject.transform.localPosition.z >= 6f)
    //        {
    //            Vector3 resetPos = arrow.transform.position;
    //            resetPos.x = 0f;
    //            resetPos.y = 0f;
    //            resetPos.z = -10f;
    //
    //            arrow.gameObject.transform.localPosition = resetPos;
    //        }
    //    }
    //}

}
