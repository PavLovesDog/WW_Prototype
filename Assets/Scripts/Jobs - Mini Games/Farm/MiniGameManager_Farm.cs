using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement; // for transitions back to overworld
using TMPro;

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
    public ParticleSystem[] hitParticles;
    // name suffix numbers (to uniquely number each arrow spawned)
    int lnum = 0;
    int Unum = 0;
    int Dnum = 0;
    int Rnum = 0;
    // flags to control which note will be played on each arrow
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

    [Header("Scoring")]
    public int currentStreak = 0;
    public int currentScore = 0;
    private int scorePerHit = 10;
    private int shoddyWorkScore = 0;
    public int totalScore =0;
    public int scoreMultiplier = 1;
    public int xpGained = 0;

    [Header("UI")]
    public GameObject gameOverScreen;
    public TMP_Text go_ScoreText;
    public TMP_Text go_ShoddyWorkText;
    public TMP_Text go_TotalScoreText;
    public TMP_Text go_CoinEarnedText;
    public TMP_Text go_XPEarnedText;
    //on screen variables
    public TMP_Text scoreText;
    public TMP_Text streakText;
    public TMP_Text multiplierText;

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

    [Header("Light Varibales")]
    public Light sunLight;
    public float temperatureChange = 130f;

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
            HandleIncreasingDifficulty();
            HandleScoringMultiplier();
            HandleScoreUI();
        }

        HandlePlayerInteraction();
    }

    #region Scoring & Difficulty Functions
    private void HandleScoringMultiplier()
    {
        //reward greater streaks of "hits"
        if (currentStreak >= 5)
        {
            scoreMultiplier = 2;
        }
        else if (currentStreak >= 10)
        {
            scoreMultiplier = 4;
        }
        else if (currentStreak >= 15)
        {
            scoreMultiplier = 8;
        }
        else if (currentStreak >= 25)
        {
            scoreMultiplier = 16;
        }
        else
        {
            //default multiplier
            scoreMultiplier = 1;
        }

        //*score is added when player hits correct notes 
    }
    private void DeductScore(int amount)
    {
        if (currentScore > 0)
            currentScore -= amount;
    }
    private void IncrementScore()
    {
        // increment score based on score per hit and scor multiplier
        currentScore += scorePerHit * scoreMultiplier;
    }
    private void HandleScoreUI()
    {
        // On screen UI (Visible While Playing)
        scoreText.text = "Score: " + currentScore.ToString(); // Show current score
        multiplierText.text = "x" + scoreMultiplier.ToString();
        streakText.text = currentStreak.ToString();

        //End Screen UI
        go_ScoreText.text = "Score: " + currentScore.ToString();
        go_ShoddyWorkText.text = "Shoddy Work: -" + shoddyWorkScore.ToString();
        go_TotalScoreText.text = "Total: " +  totalScore.ToString();
        go_CoinEarnedText.text = "Coin Earned: " + (Mathf.Floor(totalScore / 10)).ToString();
        go_XPEarnedText.text = "EX Earned: " + xpGained.ToString();
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

    #endregion

    #region Arrow Spawning Logic
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

    #region Player Interaction Logic
    //Function to randomize which sound will play for each arrow
    private void RandomizeHitSoundPlayed(bool flag, AudioSource source1, AudioSource source2)
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

    private void RandomizeMissSoundPlayed()
    {
        //generate a rand number
        int rand = Random.Range(0, 5);
        //use random nuimbner to determine which note will be played
        switch(rand) 
        { 
            case 0:
                mgAudioManager.PlayAudio(mgAudioManager.M1);
                break;
            case 1:
                mgAudioManager.PlayAudio(mgAudioManager.M2);
                break;
            case 2:
                mgAudioManager.PlayAudio(mgAudioManager.M3);
                break;
            case 3:
                mgAudioManager.PlayAudio(mgAudioManager.M4);
                break;
            case 4:
                mgAudioManager.PlayAudio(mgAudioManager.M5);
                break;
        }
    }

    private void HandlePlayerInteraction()
    {
        //listen for key press, each individually
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Change colour of hit zone to visually show player button press
            SpriteRenderer hitArrowSprite = hitPoints[0].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if(!gameOver)
                HandleArrowHit(1, canHitLeftArrow);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[1].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (!gameOver)
                HandleArrowHit(2, canHitUpArrow);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SpriteRenderer hitArrowSprite = hitPoints[2].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (!gameOver)
                HandleArrowHit(3, canHitDownArrow);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //Visualise key press
            SpriteRenderer hitArrowSprite = hitPoints[3].GetComponentInChildren<SpriteRenderer>();
            StartCoroutine(ChangeSpriteAplha(hitArrowSprite));

            if (!gameOver)
                HandleArrowHit(4, canHitRightArrow);
        }
    }

    private void HandleArrowHit(int identifier, bool arrowHitFlag)
    {
        if (arrowHitFlag)
        {
            //increment hit counter
            currentHits++; // for tracking level progression
            hitCounter++; // for tracking difficulty change
            currentStreak++;
            IncrementScore();

            //play a little particle effect to show hit
            hitParticles[identifier - 1].Play();

            //Play Audio
            if (identifier == 1) // Left Arrow
            {
                leftArrowAudioFlag = !leftArrowAudioFlag;
                RandomizeHitSoundPlayed(leftArrowAudioFlag, mgAudioManager.AL_1, mgAudioManager.AL_2);
            }
            if (identifier == 2) // Up Arrow
            {
                upArrowAudioFlag = !upArrowAudioFlag;
                RandomizeHitSoundPlayed(upArrowAudioFlag, mgAudioManager.AU_1, mgAudioManager.AU_2);
            }
            if (identifier == 3) // Down Arrow
            {
                downArrowAudioFlag = !downArrowAudioFlag;
                RandomizeHitSoundPlayed(downArrowAudioFlag, mgAudioManager.AD_1, mgAudioManager.AD_2);
            }
            if (identifier == 4) // Right arrow
            {
                rightArrowAudioFlag = !rightArrowAudioFlag;
                RandomizeHitSoundPlayed(rightArrowAudioFlag, mgAudioManager.AR_1, mgAudioManager.AR_2);
            }

            //make cloud visual changes
            TransitionCloudCoverage(); // Increase Clouds
            TransitionCloudDensity(); // Increase density of clouds
            ChangeTemperature(temperatureChange); // change the light to reflect a rainy day
        }
        else // missed "hit"
        {
            //zero streak and add to "shoddy work" score
            currentStreak = 0;
            shoddyWorkScore += 4; // increase the total to deduct form score at end

            // Play random wrong hit note
            RandomizeMissSoundPlayed();
        }
    }

    private void TransitionCloudCoverage()
    {
        if (cloudsSettings.shapeFactor.value >= 0.3f) // clamp to bottom point (this is for good looking rain clouds)
            cloudsSettings.shapeFactor.value -= cloudCoverageChange;
    }

    private void TransitionCloudDensity()
    {
        cloudsSettings.densityMultiplier.value += densityChange;
    }

    private void ChangeTemperature(float change)
    {
        if (sunLight != null)
        {
            sunLight.colorTemperature += change;
            // Clamp the temperature to HDRP's allowed range if necessary
            sunLight.colorTemperature = Mathf.Clamp(sunLight.colorTemperature, 6000, 20000);
        }
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
    #endregion

    //when the mini game ends (only a temp title)
    void MiniGameEnd()
    {
        //Initiate final visual changes

        //set the end screen active
        StartCoroutine(DelayEndScreen());

        totalScore = currentScore - shoddyWorkScore;
        //probably get the xp gained from score
        //Calculate xp gained based on total score
        xpGained = totalScore / 3 /* * satisfactionMultiplier*/;

        //Add xp to proper magic skill abd amount
        gm.AddSkillExperience(SkillType.Rain, xpGained);
            
    }

    IEnumerator DelayEndScreen()
    {
        yield return new WaitForSeconds(5);
        gameOverScreen.SetActive(true);
    }

    //Function for UI button
    public void OnEndButtonPress()
    {
        // load the overworld scene
        SceneManager.LoadScene(0);
    }
}
