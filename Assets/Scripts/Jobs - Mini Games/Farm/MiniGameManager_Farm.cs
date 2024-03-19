using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class MiniGameManager_Farm : MonoBehaviour
{

    [Header("Lanes")]
    public GameObject[] lanes;

    [Header("Arrows")]
    public GameObject[] hitPoints;
    private List<GameObject> arrowsOnScreen = new List<GameObject>();
    private List<GameObject> currentArrowsOnScreen;

    [Header("Arrow Spawn Points")]
    public Transform[] arrowSpawnPoints;

    [Header("Game variables")]
    public int hitsForCompletion = 30;
    public int currentHits = 0;
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
    int lnum = 0;
    int Unum = 0;
    int Dnum = 0;
    int Rnum = 0;
    
    [Header("Cloud Variables")]
    public Volume globalVolume;
    private VolumetricClouds cloudsSettings;
    public float maxDensity = 1;
    [Tooltip("Control how much light is let through clouds (0 - 1)")] 
    public float currentDensity = 0;
    public float densityChange = 0.005f;
    public float minCloudCoverage = 0;
    [Tooltip("Control amount of clouds in sky, smaller values = more clouds (0-1))")] 
    public float currentCloudCoverage = 1;
    public float cloudCoverageChange = 0.005f;

    private void Start()
    {
        if (globalVolume.profile.TryGet<VolumetricClouds>(out cloudsSettings))
        {
            // Successfully retrieved the clouds settings
        }

        //find and assign clouds based on "globalVolume" set in inspector
        //cloudsSettings = globalVolume.GetComponent<VolumetricClouds>();
    }
    


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
                        arrowToAdd.name = "Arrow_Left" + lnum.ToString();
                        lnum++;
                        break;
                    case "Arrow_Up":
                        arrowToAdd = Instantiate(arrow, arrowSpawnPoints[1].transform.position, new Quaternion(0, 0, 0, 0), lanes[1].transform);
                        arrowToAdd.name = "Arrow_Up" + Unum.ToString();
                        Unum++;
                        break;
                    case "Arrow_Down":
                        arrowToAdd = Instantiate(arrow, arrowSpawnPoints[2].transform.position, new Quaternion(0, 0, 0, 0), lanes[2].transform);
                        arrowToAdd.name = "Arrow_Down" + Dnum.ToString();
                        Dnum++;
                        break;
                    case "Arrow_Right":
                        arrowToAdd = Instantiate(arrow, arrowSpawnPoints[3].transform.position, new Quaternion(0, 0, 0, 0), lanes[3].transform);
                        arrowToAdd.name = "Arrow_Right" + Rnum.ToString();
                        Rnum++;
                        break;

                }

                // add to directional arrows list
                arrowsOnScreen.Add(arrowToAdd);

                //increment currentArrowsInPlay
                currentArrowsInPlay++;
            }
                //arrow now spawned, it will handle its own movement

        
        }

        CheckArrowsForRemoval();
    }

    private void CheckArrowsForRemoval()
    {
        //fill temp list to work on
        currentArrowsOnScreen = new List<GameObject>(arrowsOnScreen);
        bool foundArrowToDelete = false;

        //iterate through in reverse, if removing elements from a list while iterating over it, its safer
        for (int i = currentArrowsOnScreen.Count - 1; i >= 0; i--)
        {
            if(foundArrowToDelete)
                break;

            GameObject arrow = currentArrowsOnScreen[i];
            if (arrow == null)
            {
                currentArrowsOnScreen.RemoveAt(i); // Just in case there's a null reference
                continue;
            }
            else if (arrow != null)
            {

                ArrowCollision script = arrow.GetComponent<ArrowCollision>();
                if (script != null && script.canReset)
                {
                    //Debug.Log(arrow.name + " CAN reset");
                    // maybe adding a flag or state to indicate it's being destroyed
                    //Debug.Log("DELETING: " + arrow.name);
                    
                    //Deletion & object cleanup
                    StartCoroutine(DelayedDestroyObject(arrow, i, currentArrowsOnScreen));

                    //script.canReset = false;

                    //leave loop as object has been found and deleted
                    foundArrowToDelete = true;
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

            if (canHitLeftArrow)
            {
                Debug.Log("HIT the LEFT arrow");
                //increment hit counter
                currentHits++;
                //play a little particle effect to show hit
                //play audio to show hit
                cloudsSettings.shapeFactor.value -= cloudCoverageChange;
            }
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[1].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (canHitUpArrow)
            {
                Debug.Log("HIT the UP arrow");
                currentHits++;
                cloudsSettings.shapeFactor.value -= cloudCoverageChange;
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[2].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (canHitDownArrow)
            {
                Debug.Log("HIT the DOWN arrow");
                currentHits++;
                cloudsSettings.shapeFactor.value -= cloudCoverageChange;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[3].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (canHitRightArrow)
            {
                Debug.Log("HIT the RIGHT arrow");
                currentHits++;
                cloudsSettings.shapeFactor.value -= cloudCoverageChange;
            }
        }
        ////if the corresponding arrow bool is enabled, register a hit
    }

    private void TransitionCloudCoverage()
    {
        
        //clouds.densityMultiplier
    }

    private void TransitionClouddensity()
    {

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
    IEnumerator DelayedDestroyObject(GameObject objectToDelete, int index, List<GameObject> list) 
    { 
        yield return new WaitForEndOfFrame();

        if (objectToDelete != null)
        {
            currentArrowsInPlay--;
            list.RemoveAt(index); // remove object form list
            Destroy(objectToDelete); // remove object from game
            arrowsOnScreen = currentArrowsOnScreen; // reassign original list
            //StartCoroutine(UpdateList());
        }
    }
}
