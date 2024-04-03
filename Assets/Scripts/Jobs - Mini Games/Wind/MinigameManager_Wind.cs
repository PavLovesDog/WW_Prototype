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
    /// Need:
    ///     -slider
    ///     - region for perfect hit
    ///         - figure out how to display this on slider nicely
    ///     - region for regular hit
    ///         - figure out how to display this on slider nicely
    ///     - score
    ///     - score multiplier for consecutive hits
    ///     - reference to windmill
    ///
    ///functions:
    ///     - when moving slider handle move faster each time hit is accurate
    /// </summary>
    [Header("References")]
    public GameManager gm;
    public CMG_WeatherManager weatherManager;
    public GameObject mgWindmill;

    [Header("Minigame Variables")]
    public float trackSpeed;
    public Transform offset;
    float windmillSpeedAdjustment;
    float sliderDirection = 1;
    float difficultyMulti = 1.2f;
    public bool targetHit = false;
    public int sequencesToWin = 30;
    public int correctSequences = 0;
    public int incorrectSequences = 0;
    public int hitsToWinLeft;
    HitZone regHit;
    HitZone critHit;
    public float critHitSize = 1;//size of critical hit zone
    public float regHitSize = 2; // size of regular hit zone either side of critical zone

    [Header("UI")]
    public Slider track;
    public Image regHitZoneIndicator;
    public Image critHitZoneIndicator;


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
                    windmillSpeedAdjustment = 1.5f;
                    HandleHit();
                }
                else if (track.value >= regHit.minHit && track.value <= regHit.maxHit)
                {
                    print("hit reg");
                    windmillSpeedAdjustment = 0.5f;
                    HandleHit();
                }
                else
                {
                    print("Miss");
                    windmillSpeedAdjustment = -1;
                    HandleMiss();
                }
            }

        }

    }

    private void HandleHit()
    {
        AdjustWindmillRotation(windmillSpeedAdjustment);
        track.value = 0;
        trackSpeed *= difficultyMulti;
        GenerateHitZone();
    }

    private void HandleMiss()
    {
        AdjustWindmillRotation(windmillSpeedAdjustment);
    }
    /// <summary>
    /// max = 10 / 500
    /// 
    /// </summary>
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
    private void GenerateHitZone()
    {

        GeneratePerfectHitZone();
        GenerateRegularHitZone();
    }
    /// <summary>
    ///
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

    private void GenerateRegularHitZone()
    {
        float upper = (critHit.maxHit - critHitSize / 2) + (regHitSize / 2);
        float lower = (critHit.minHit + critHitSize / 2) - (regHitSize / 2);
        float newY = regHitZoneIndicator.rectTransform.anchoredPosition.y;
        regHit = new HitZone(lower, upper);
        regHitZoneIndicator.rectTransform.anchoredPosition = new Vector2(upper - regHitSize / 2, newY);
    }
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



    #region Scoring & Game Function


    //Function for UI button
    public void OnEndButtonPress()
    {
        // load the overworld scene
        SceneManager.LoadScene(0);
    }
    #endregion
}