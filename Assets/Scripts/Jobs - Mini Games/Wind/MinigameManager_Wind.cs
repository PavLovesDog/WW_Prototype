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

    /// <summary>
    /// TODO:
    ///     - slight tweaks to wind and cloud effects
    ///     - add function to adjust clouds in skybox
    ///     - add in score
    /// </summary>
    [Header("References")]
    public GameManager gm;
    public CMG_WeatherManager weatherManager;
    public GameObject mgWindmill;
    
    ParticleSystem windParticle;

    [Header("Minigame Variables")]
    public float trackSpeed;
    public Transform offset;
    public bool targetHit = false;
    public int sequencesToWin = 30;
    public int correctSequences = 0;
    public int incorrectSequences = 0;
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


    [Header("Difficulty Multipliers")]
    public float difficultyMulti = 1.2f; //general difficulty, will adjust depending on magic level
    public float multMod; //for use with external forces in windParticle
    public float trackSpeedMod = 1;
    public float noiseStrengthMod; //for use with noise strength

    [Header("Scoring")]
    public int currentStreak = 0;
    public int currentScore = 0;
    private int scorePerHit = 10;
    private int shoddyWorkScore = 0;
    public int totalScore = 0;
    public int scoreMultiplier = 1;
    public int xpGained = 0;
    public int coinGained = 0;



    void Start()
    {
        InitVariables();
        if (gm == null)
        {
            gm = FindObjectOfType<GameManager>();
        }
        windParticle = FindObjectOfType<ParticleSystem>();
        ResetMulti();
        ResetLeft();
        GenerateHitZone();

        //Debug.Log("CURRENT SEQUENCE LIST: ");
        //PrintSequenceList(currentSequences.Count);
    }

    private void InitVariables()
    {
        critHitSize = critHitZoneIndicator.rectTransform.rect.width;
        regHitSize = regHitZoneIndicator.rectTransform.rect.width;
        track.maxValue = track.transform.GetChild(0).GetComponent<RectTransform>().rect.width;
        offset.GetComponent<RectTransform>().anchoredPosition = new Vector2(-track.maxValue / 2, 0);

        trackSpeed = 100; // change this to go based of magic level
    }

    void Update()
    {
        if (!targetHit)
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
                trackSpeed *= trackSpeedMod;
            }

        }

    }

    private void HandleHit(bool isCrit)
    {
        if (isCrit)
        {
            trackSpeedMod = 1.15f;
            ModifyWind(true);
            windmillSpeedAdjustment = 1.5f;
        } else
        {
            trackSpeedMod = 1.05f;
            ModifyWind(false);
            windmillSpeedAdjustment = 0.5f;
        }
        AdjustLeft(true);
        track.value = 0;
        AdjustWindmillRotation(windmillSpeedAdjustment);
        GenerateHitZone();
    }

    private void HandleMiss()
    {
        trackSpeedMod = 0.9f;
        windmillSpeedAdjustment = -1f;
        AdjustWindmillRotation(windmillSpeedAdjustment);
        AdjustLeft(false);
        ResetMulti();
    }

    void MoveSlider()
    {
        if (track.value >= track.maxValue)
        {
            sliderDirection *= -1;
        }
        else if (track.value <= track.minValue)
        {
            sliderDirection *= -1;
        }
        track.value += sliderDirection * trackSpeed * Time.deltaTime;
    }
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
            external.multiplier += 0.2f;
            nStrength.constant += noiseStrengthMod;

        }
        else
        {
            external.multiplier -= 0.2f;
            nStrength.constant -= noiseStrengthMod;
        }
    }
    #endregion
    #region Scoring & Game Function
    void ResetMulti()
    {
        scoreMultiplier = 0;
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
    void AdjustLeft(bool isHit )
    {
        if (isHit)
        {
            hitsToWinLeft--;
        } else
        {
            hitsToWinLeft = startingRemaining;
        }
            leftToHit.text = "Left to Hit: "+hitsToWinLeft.ToString();
    }
    void ResetLeft()
    {
        hitsToWinLeft = startingRemaining;
        leftToHit.text = "Left to Hit: " + hitsToWinLeft.ToString();
    }
    //Function for UI button
    public void OnEndButtonPress()
    {
        // load the overworld scene
        SceneManager.LoadScene(0);
    }
    #endregion
}