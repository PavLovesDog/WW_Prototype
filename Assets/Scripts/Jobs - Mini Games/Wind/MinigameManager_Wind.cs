using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;


public class MinigameManager_Wind : MonoBehaviour
{
    struct HitZone
    {
        public float minHit;
        public float maxHit;

        public HitZone(float _minHit, float _maxHit)
        {
            minHit = _minHit;
            maxHit = _maxHit;
        }
    }

    [Header("References")]
    public GameManager gm;
    public WMG_WeatherManager weatherManager;
    public GameObject mgWindmill;
    ParticleSystem windParticle;

    [Header("Minigame Variables")]
    public float trackSpeed;
    public Transform offset;
    public int hitsToWinLeft;
    public int startingRemaining = 30;
    public float critHitSize = 1;//size of critical hit zone
    public float regHitSize = 2; // size of regular hit zone either side of critical zone

    HitZone regHit;
    HitZone critHit;
    float windmillSpeedAdjustment;
    float sliderDirection = 1;

    [Header("UI")]
    public Slider track;
    public Image regHitZoneIndicator;
    public Image critHitZoneIndicator;
    public TMP_Text scoreMulti;
    public TMP_Text leftToHit;
    public TMP_Text scoreText;

    [Header("Difficulty Multipliers")]
    public float difficultyMulti = 1.2f; //general difficulty, will adjust depending on magic level
    public float noiseStrengthMod; //for use with noise strength
    public float trackSpeedMod = 1;

    [Header("Scoring")]
    public int currentStreak = 0;
    public int currentScore = 0;
    public float critMulti = 1.5f;
    private int scorePerHit = 10;
    private int shoddyWorkScore = 0;
    public int totalScore = 0;
    public int scoreMultiplier = 1;
    public int xpGained = 0;
    public int coinGained = 0;

    [Header("End Game Variables")]
    public GameObject gameOverScreen;
    public TMP_Text endScoreText;
    public TMP_Text endShoddyWorkText;
    public TMP_Text endTotalScoreText;
    public TMP_Text endCoinEarnedText;
    public TMP_Text endXPEarnedText;
    public Button returnBtn;
    /// <summary>
    /// Just initialising some variables
    /// </summary>
    private void InitVariables()
    {

        critHitSize = critHitZoneIndicator.rectTransform.rect.width;
        regHitSize = regHitZoneIndicator.rectTransform.rect.width;
        track.maxValue = track.transform.GetChild(0).GetComponent<RectTransform>().rect.width;
        offset.GetComponent<RectTransform>().anchoredPosition = new Vector2(-track.maxValue / 2, 0);
        hitsToWinLeft = startingRemaining;
        track.value = 0;
    }



    void Start()
    {
        if (gm == null)
        {
            gm = FindObjectOfType<GameManager>();
        }
        InitDifficulty();
        InitVariables();
        windParticle = FindObjectOfType<ParticleSystem>();
        ResetMulti();
        LeftToHitUpdate();
        GenerateHitZone();

        //Debug.Log("CURRENT SEQUENCE LIST: ");
        //PrintSequenceList(currentSequences.Count);

        //set initial sky values
        weatherManager.InitializeSkyValues(0.85f, 0.8f);
    }

    private void InitDifficulty()
    {
        difficultyMulti = 1.2f + (gm.GetSkillLvl(SkillType.Wind) * 0.15f);
        startingRemaining = (int)(difficultyMulti * 30);
        trackSpeed = 100 * difficultyMulti;
    }

    void Update()
    {
        MoveSlider();
        //check for space key hit
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //check whether the position is within the bounds
            if (track.value >= critHit.minHit && track.value <= critHit.maxHit)
            {
                print("HIT CRIT");
                HandleHit(true);
                AdjustMulti(true);
            }
            else if (track.value >= regHit.minHit && track.value <= regHit.maxHit)
            {
                print("hit reg");

                HandleHit(false);
                AdjustMulti(false);
            }
            else
            {
                print("Miss");
                HandleMiss();
            }
            //check if hits left is 0
            if (hitsToWinLeft <= 0)
            {
                EndGame(true);
            }
            else
            {
                trackSpeed *= trackSpeedMod * difficultyMulti;
            }
        }
    }
    #region MiniGameMechanics
    /// <summary>
    /// Gets called whenever the player hits the target
    /// </summary>
    /// <param name="isCrit">true if the critical zone was hit, false if the regular zone was hit</param>
    private void HandleHit(bool isCrit)
    {
        if (isCrit)
        {
            trackSpeedMod = 1.15f;
            ModifyWind(true);
            windmillSpeedAdjustment = 1.5f;
            AddScore((int)(scorePerHit * critMulti));
            DisplayScore();
        }
        else
        {
            trackSpeedMod = 1.05f;
            ModifyWind(false);
            windmillSpeedAdjustment = 0.5f;
            AddScore(scorePerHit);
            DisplayScore();
        }
        AdjustLeft();
        LeftToHitUpdate();
        track.value = 0;
        AdjustWindmillRotation(windmillSpeedAdjustment);
        GenerateHitZone();
    }
    /// <summary>
    /// Gets called when the player misses the hit zone
    /// </summary>
    private void HandleMiss()
    {
        trackSpeedMod = 0.9f;
        windmillSpeedAdjustment = -1f;
        shoddyWorkScore += 10;
        AdjustWindmillRotation(windmillSpeedAdjustment);
        ResetMulti();
    }
    /// <summary>
    /// Function that moves the slider. Checks whether the position is at the end or beginning of the track, if either is true then change direction
    /// </summary>
    void MoveSlider()
    {
        if (track.value >= track.maxValue || track.value <= track.minValue)
        {
            sliderDirection *= -1;
        }
        track.value += sliderDirection * trackSpeed * Time.deltaTime;
    }
    #endregion
    #region Generation
    private void GenerateHitZone()
    {

        GeneratePerfectHitZone();
        GenerateRegularHitZone();
    }
    /// <summary>
    ///Generate the zones for the perfect hit zone
    /// </summary>
    private void GeneratePerfectHitZone()
    {
        float upperLimit = track.maxValue;
        float lowerLimit = track.minValue;
        float newY = critHitZoneIndicator.rectTransform.anchoredPosition.y;
        float upper = Random.Range(lowerLimit, upperLimit);
        float lower = upper - critHitSize;
        critHit = new HitZone(lower, upper);
        critHitZoneIndicator.rectTransform.anchoredPosition = new Vector2(upper - critHitSize / 2, newY);
    }
    /// <summary>
    /// Generate the hit zones for the regular  hit zone
    /// </summary>
    private void GenerateRegularHitZone()
    {
        float upper = (critHit.maxHit - critHitSize / 2) + (regHitSize / 2);
        float lower = (critHit.minHit + critHitSize / 2) - (regHitSize / 2);
        float newY = regHitZoneIndicator.rectTransform.anchoredPosition.y;
        regHit = new HitZone(lower, upper);
        regHitZoneIndicator.rectTransform.anchoredPosition = new Vector2(upper - regHitSize / 2, newY);
    }
    #endregion
    #region Progress Adjustments
    /// <summary>
    /// Adjust windmill's blade speed by adding speed
    /// </summary>
    /// <param name="speed">amount to add to blades speed</param>
    private void AdjustWindmillRotation(float speed)
    {
        Transform blades = mgWindmill.transform.GetChild(2);
        if (speed < 0 && blades.GetComponent<Rotate>().speed < (-speed))
        {
            blades.GetComponent<Rotate>().speed = 0;
        }
        else
        {
            blades.GetComponent<Rotate>().speed += speed;
        }
    }

    private void ModifyWind(bool faster)
    {

        ParticleSystem.NoiseModule noise = windParticle.noise;
        var nStrength = noise.strength;
        ParticleSystem.ExternalForcesModule external = windParticle.externalForces;
        if (faster)
        {
            nStrength.constant += noiseStrengthMod;
        }
        else
        {
            nStrength.constant -= noiseStrengthMod;
        }
    }
    #endregion
    #region Scoring & Game Function
    void ResetMulti()
    {
        scoreMultiplier = 1;
        scoreMulti.text = "Score Multiplier: " + scoreMultiplier.ToString();
    }
    void AdjustMulti(bool isCrit)
    {
        if (isCrit)
            scoreMultiplier += 2;
        else
            scoreMultiplier++;
        scoreMulti.text = "Score Multiplier: " + scoreMultiplier.ToString();
    }
    void AdjustLeft()
    {
        hitsToWinLeft--;
    }
    private void LeftToHitUpdate()
    {
        leftToHit.text = "Left to Hit: " + hitsToWinLeft.ToString();
    }
    void AddScore(int amount)
    {
        currentScore += amount * scoreMultiplier;
    }
    void DisplayScore()
    {
        scoreText.text = "Score: " + currentScore.ToString();
    }

    void CalculateRewards()
    {
        int shoddyMod = (int)(shoddyWorkScore * difficultyMulti);
        //calculate score
        totalScore = (currentScore) - (int)(shoddyMod * difficultyMulti);
        //calculate earned exp
        xpGained = (totalScore / 3) * (int)difficultyMulti;
        //calculate earned coin
        coinGained = Mathf.FloorToInt(totalScore / (10 - (gm.GetSkillLvl(SkillType.Wind)*0.1f)));
    }
    void EndGame(bool reachedGoal)
    {
        if (reachedGoal)
        {
            trackSpeed = 0;
            //display end game dialog
            gameOverScreen.SetActive(true);
            //calculate end rewards
            CalculateRewards();
            gm.AddSkillExperience(SkillType.Wind, xpGained);
            gm.AddCoins(coinGained);
            //set dialog texts
            SetEndGamesTexts();
        }
    }

    private void SetEndGamesTexts()
    {
        int shoddyMod = (int)(shoddyWorkScore * difficultyMulti);
        endCoinEarnedText.text = "Coin earned: " + coinGained.ToString();
        endXPEarnedText.text = "XP earned: " + xpGained.ToString();
        endTotalScoreText.text = "Total Score: " + totalScore.ToString();
        endScoreText.text = "Score: " + currentScore.ToString();
        endShoddyWorkText.text = shoddyMod.ToString() + " -  Shoddy Work";
    }

    //Function for UI button
    public void OnEndButtonPress()
    {
        // load the overworld scene
        SceneManager.LoadScene(1);
    }
    #endregion
}