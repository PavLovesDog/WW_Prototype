using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class MiniGameManager_Farm : MonoBehaviour
{

    [Header("References")]
    public GameManager gm;
    public MG1_AudioManager mgAudioManager;

    [Header("Lanes & Spawn Points")]
    public GameObject[] lanes;
    public Transform[] arrowSpawnPoints;

    [Header("Arrows")]
    public GameObject[] hitPoints;
    private List<GameObject> arrowsOnScreen = new List<GameObject>();
    private List<GameObject> currentArrowsOnScreen;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject upArrow;
    public GameObject downArrow;
    int lnum = 0;
    int Unum = 0;
    int Dnum = 0;
    int Rnum = 0;
    bool leftArrowAudioFlag = true;
    bool upArrowAudioFlag = true;
    bool downArrowAudioFlag = true;
    bool rightArrowAudioFlag = true;

    [Header("Game variables")]
    public bool gameOver = false;
    public int hitsToWin = 30;
    public int currentHits = 0;
    [Tooltip("Speed at which arrows will scroll down the screen/move")] public float scrollSpeed;
    public int currentArrowsInPlay;
    public int totalArrowsInPlay = 10;
    public float spawnDelayMin = 2f; // 0.5 is a good bottom line
    public float spawnDelayMax = 3f;
    private float spawnTimer = 0f;
    public bool canHitLeftArrow = false;
    public bool canHitUpArrow = false;
    public bool canHitDownArrow = false;
    public bool canHitRightArrow = false;

    [Header("Difficulty Management")]
    public float difficultySegments = 0; // set in inspector (currently at 6)
    public float numberOfDifficultySegments = 0;
    public int hitCounter = 0;
    // Change variables (these will be determined by calculations based off difficulty segments, within their range)
    public float minSpawnDelayChange;
    public float maxSpawnDelayChange;
    public float scrollSpeedChange;
    public float arrowsInPlayChange;

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
    public float cloudCoverageChange = 0.05f;

    private void Start()
    {
        //get reference to the Game Manager & audio source
        gm = FindObjectOfType<GameManager>();
        mgAudioManager = FindObjectOfType<MG1_AudioManager>();

        //Find the cloud settings for runtime manipulation
        if (globalVolume.profile.TryGet<VolumetricClouds>(out cloudsSettings))
        {
            // Successfully retrieved the clouds settings
        }
    }
    


    private void Update()
    {
        if(!gameOver)
        {
            SpawnArrows();
            HandleInteractionAndScoring();
            HandleIncreasingDifficulty();
        }
    }

    private void HandleIncreasingDifficulty()
    {
        // based on total amount of "hits"
        // divide the total of hitsToWin by num of segments/phases
        numberOfDifficultySegments = hitsToWin / difficultySegments; // this is how many "phases" of difficulty there will be per round
        numberOfDifficultySegments = Mathf.Floor(numberOfDifficultySegments); // ensure its an int

        //Calculate difficulty for each Change Variable based on number of segemnts
        //Calculate: minSpawnDelayChange - between the range of 0.5 - 2.5
        //Calculate: maxSpawnDelayChange - between the range of 1 - 3.5
        //Calculate: scrollSpeedChange   - between the range of 1.5 - 5
        //Calculate: arrowsInPlayChange  - between the range of 10 - 30
        minSpawnDelayChange = (0.25f - 2.5f) / difficultySegments; // Negative value, because it decreases
        maxSpawnDelayChange = (1f - 3.5f) / difficultySegments; // Negative value, because it decreases
        scrollSpeedChange = (3f - 1.5f) / difficultySegments; // Positive value, because it increases
        arrowsInPlayChange = (30 - 10) / difficultySegments; // Positive value, because it increases

        // at every interval of numberOfDifficultySegments
        if (hitCounter == numberOfDifficultySegments)
        {
            hitCounter = 0; // reset to count up to determine when next difficulty phase happens

            // Adjust min/max spawn delays within their respective ranges
            spawnDelayMin = Mathf.Max(0.25f, spawnDelayMin + minSpawnDelayChange);
            spawnDelayMax = Mathf.Max(1f, spawnDelayMax + maxSpawnDelayChange);

            // Adjust the scroll speed, clamping within the range
            scrollSpeed = Mathf.Clamp(scrollSpeed + scrollSpeedChange, 1.5f, 3f);

            // Adjust the number of arrows in play, clamping within the range
            totalArrowsInPlay = (int)Mathf.Clamp(totalArrowsInPlay + arrowsInPlayChange, 10, 30);

            //// adjust the min/max spawn delays
            //if(spawnDelayMin >= 0.5f && spawnDelayMin <= 2.5f) // keep changes within range
            //{
            //    //DECREASE
            //    //spawnDelayMin += minSpawnDelayChange
            //}
            //if (spawnDelayMax >= 1f && spawnDelayMax <= 3.5f) // keep changes within range
            //{
            //    //DECREASE
            //    //spawnDelayMax += maxSpawnDelayChange
            //}
            //
            //// adjust the scroll speed slightly faster
            //if (scrollSpeed >= 10 && scrollSpeed <= 30)
            //{
            //    //INCREASE
            //    //have this change based on number of segments and total hitsToWin
            //    //scrollSpeed += scrollSpeedChange
            //}
            //
            //// adjust amount of arrows allowed on screen
            //if (currentArrowsInPlay >= 10 && currentArrowsInPlay <= 30)
            //{
            //    //INCREASE
            //    //currentArrowsInPlay += arrowsInPlayChange
            //}
        }

        //listen for game over event
        if(currentHits >= hitsToWin)
        {
            gameOver = true;
            //show end screen an scores
            MiniGameEnd();
        }
    }

    #region Arrow SPawning Logic
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
    #endregion
    
    //Function to randomize which sound will play for each arrow
    private void RandomizeSoundPlayed(bool flag, AudioSource source1, AudioSource source2)
    {
        if (flag)
        {
            mgAudioManager.PlayAudio(source1);
        }
        else
        {
            mgAudioManager.PlayAudio(source2);
        }
    }

    private void HandleInteractionAndScoring()
    {
        //listen for key press, each individually
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //TESTING
            //leftArrowAudioFlag = !leftArrowAudioFlag; // switch bool state for each tap
            ////Play one of either selected notes attached to LEFT arrow
            //RandomizeSoundPlayed(leftArrowAudioFlag, mgAudioManager.AL_1, mgAudioManager.AL_2);

            // Change colour of hit zone to visually show player button press
            SpriteRenderer hitArrowSprite = hitPoints[0].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (canHitLeftArrow)
            {
                leftArrowAudioFlag = !leftArrowAudioFlag; // switch bool state for each tap

                Debug.Log("HIT the LEFT arrow");

                //increment hit counter
                currentHits++; // for tracking level progression
                hitCounter++; // for tracking difficulty change

                //play a little particle effect to show hit

                //play audio
                //Play one of either selected notes attached to LEFT arrow
                RandomizeSoundPlayed(leftArrowAudioFlag, mgAudioManager.AL_1, mgAudioManager.AL_2);

                //Cloud coverage
                if(cloudsSettings.shapeFactor.value >= 0.3f) // clamp to bottom point (this is for good looking rain clouds)
                    cloudsSettings.shapeFactor.value -= cloudCoverageChange;

                //cloud density
                cloudsSettings.densityMultiplier.value += densityChange;
            }
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //upArrowAudioFlag = !upArrowAudioFlag;
            ////Play one of either selected notes attached to UP arrow
            //RandomizeSoundPlayed(upArrowAudioFlag, mgAudioManager.AU_1, mgAudioManager.AU_2);

            SpriteRenderer hitArrowSprite = hitPoints[1].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));


            if (canHitUpArrow)
            {
                upArrowAudioFlag = !upArrowAudioFlag;
                Debug.Log("HIT the UP arrow");
                currentHits++;
                hitCounter++;
                //Play one of either selected notes attached to UP arrow
                RandomizeSoundPlayed(upArrowAudioFlag, mgAudioManager.AU_1, mgAudioManager.AU_2);
                cloudsSettings.shapeFactor.value -= cloudCoverageChange;
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //downArrowAudioFlag = !downArrowAudioFlag;
            ////Play one of either selected notes attached to DOWN arrow
            //RandomizeSoundPlayed(downArrowAudioFlag, mgAudioManager.AD_1, mgAudioManager.AD_2);

            SpriteRenderer hitArrowSprite = hitPoints[2].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));


            if (canHitDownArrow)
            {
                downArrowAudioFlag = !downArrowAudioFlag;
                Debug.Log("HIT the DOWN arrow");
                currentHits++;
                hitCounter++;
                //Play one of either selected notes attached to DOWN arrow
                RandomizeSoundPlayed(downArrowAudioFlag, mgAudioManager.AD_1, mgAudioManager.AD_2);
                cloudsSettings.shapeFactor.value -= cloudCoverageChange;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //rightArrowAudioFlag = !rightArrowAudioFlag;
            ////Play one of either selected notes attached to RIGHT arrow
            //RandomizeSoundPlayed(rightArrowAudioFlag, mgAudioManager.AR_1, mgAudioManager.AR_2);

            SpriteRenderer hitArrowSprite = hitPoints[3].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));


            if (canHitRightArrow)
            {
                rightArrowAudioFlag = !rightArrowAudioFlag;
                Debug.Log("HIT the RIGHT arrow");
                currentHits++;
                hitCounter++;
                //Play one of either selected notes attached to RIGHT arrow
                RandomizeSoundPlayed(rightArrowAudioFlag, mgAudioManager.AR_1, mgAudioManager.AR_2);
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
    

    //when the mini game ends (only a temp title)
    void MiniGameEnd()
    {
        //probably get the xp gained from score
        int xpGained = 100;
        gm.AddPlayerExperience(xpGained); 
    }
}
