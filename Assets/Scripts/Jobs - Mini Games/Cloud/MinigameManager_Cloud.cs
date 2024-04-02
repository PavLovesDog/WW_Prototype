using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public struct Sequence
{
    public int[] intSequence; // int array containing numbers ranging from 0-3, this correlates with the index of arrowSprites array
    private bool[] arrowHits;

    public Sequence(int[] intSequence)
    {
        this.intSequence = intSequence;
        this.arrowHits = new bool[5]; // Assuming there are always 5 arrows
    }

    public bool this[int index]
    {
        get { return arrowHits[index]; }
        set { arrowHits[index] = value; }
    }

    // Method to set an arrow hit status
    public void SetArrowHit(int index, bool hit)
    {
        if (index >= 0 && index < arrowHits.Length)
        {
            arrowHits[index] = hit;
        }
    }

    // Method to get an arrow hit status
    public bool GetArrowHit(int index)
    {
        if (index >= 0 && index < arrowHits.Length)
        {
            return arrowHits[index];
        }
        return false;
    }
}

public class MinigameManager_Cloud : MonoBehaviour
{
    [Header("References")]
    public GameManager gm;
    public CMG_WeatherManager weatherManager;

    public ParticleSystem crystalParticles;
    //Varibales for all UI aspects
    #region UI Variables
    // Arrow Images
    // Direct references to change the sprite of each arrow
    public Image[] currentArrows;
    public Image[] nextArrows;
    public Image[] previousArrows;
    public Sprite[] arrowSprites;

    // timer slider
    public Slider timerSlider;
    public TMP_Text hitMessage;
    private string message;
    //on screen variables
    public TMP_Text scoreText;
    public TMP_Text streakText;
    public TMP_Text multiplierText;
    public TMP_Text hitsToWinText;

    [Header("Endscreen Variables")]
    public GameObject gameOverScreen;
    public TMP_Text go_ScoreText;
    public TMP_Text go_ShoddyWorkText;
    public TMP_Text go_TotalScoreText;
    public TMP_Text go_CoinEarnedText;
    public TMP_Text go_XPEarnedText;

    // Variables for sequence generation, display, and previous
    public float timerSpeed = 0.2f; // 0.2 is roughly 5 seconds
    public float timerIncrease = 0.05f; // value to make timer faster
    #endregion


    [Header("Minigame Variables")]
    public bool gameOver = false;
    public int sequencesToWin = 30;
    public int correctSequences = 0;
    public int incorrectSequences = 0;
    public int hitsToWinLeft;

    // Bools to track which arrows were hit per sequence
    public bool hitArrow1 = false;
    public bool hitArrow2 = false;
    public bool hitArrow3 = false;
    public bool hitArrow4 = false;
    public bool hitArrow5 = false;

    [Header("Input Variables")]
    public bool playerInputLeft = false;
    public bool playerInputUp = false;
    public bool playerInputDown = false;
    public bool playerInputRight = false;

    [Header("Scoring")]
    public int currentStreak = 0;
    public int currentScore = 0;
    private int scorePerHit = 10;
    private int shoddyWorkScore = 0;
    public int totalScore = 0;
    public int scoreMultiplier = 1;
    public int xpGained = 0;
    public int coinGained = 0;

    //Lists to hold the arrows
    public List<Sequence> currentSequences = new List<Sequence>();
    public List<Sequence> tempSequences = new List<Sequence>();
    //public List<int[]> previousSequences = new List<int[]>();


    void Start()
    {
        if(gm == null)
        {
            //find it!
            gm = FindObjectOfType<GameManager>();
        }

        

        hitsToWinLeft = sequencesToWin;

        //initalize first 2 arrows llists to access
        currentSequences.Add(GenerateSequenceObject());
        currentSequences.Add(GenerateSequenceObject());
        UpdateArrowVisuals();

        //hide unused arrows
        for (int i = 2; i < 7; i++)
        {
            HideImageSequenceArrows(previousArrows, i);
        }

        //Debug.Log("CURRENT SEQUENCE LIST: ");
        //PrintSequenceList(currentSequences.Count);
    }


    void Update()
    {
        if (!gameOver)
        {
            timerSlider.value -= Time.deltaTime * timerSpeed;

            //TEMP - change to coroutines which handle breaks/pauses between successful hits/whatnot
            if (timerSlider.value <= 0)
            {
                //if timer reaches zero. fail!
                SequenceFailCondition(currentSequences[1]);
            }

            //Listen for any input
            ListenForPlayerInput();
            ListenForSequenceCompleteCondition();
            HandleTimerSpeeds();

            ListenForWinCondition();
        }
 
        HandleUI();
        HandleVisualParticleSystem();
    }

    private void HandleUI()
    {
        scoreText.text = "Score: " + currentScore.ToString();
        multiplierText.text = "x" + scoreMultiplier.ToString();
        streakText.text = currentStreak.ToString();
        hitsToWinText.text = hitsToWinLeft.ToString() + " to win";
        hitMessage.text = message;

        //End Screen UI
        go_ScoreText.text = "Score: " + currentScore.ToString();
        go_ShoddyWorkText.text = "Shoddy Work: -" + shoddyWorkScore.ToString();
        go_TotalScoreText.text = "Total: " + totalScore.ToString();
        go_CoinEarnedText.text = "Coin Earned: " + coinGained.ToString();
        go_XPEarnedText.text = "EX Earned: " + xpGained.ToString();
    }

    private void HandleVisualParticleSystem()
    {
        var emission = crystalParticles.emission; // Get the EmissionModule

        if (hitsToWinLeft == sequencesToWin)
        {
            emission.rateOverTime = 2;
        }
        else if(hitsToWinLeft == 25)
        {
            emission.rateOverTime = 7;
        }
        else if (hitsToWinLeft == 20)
        {
            emission.rateOverTime = 10;
        }
        else if (hitsToWinLeft == 15)
        {
            emission.rateOverTime = 15;
        }
        else if (hitsToWinLeft == 10)
        {
            emission.rateOverTime = 25;
        }
        else if (hitsToWinLeft == 5)
        {
            emission.rateOverTime = 30;
        }
        else if (hitsToWinLeft == 0)
        {
            // full particles
            emission.rateOverTime = 50;
        }
    }

    #region Scoring & Game Function

    private void HandleTimerSpeeds()
    {
        if (currentStreak >= 25)
        {
            timerSpeed = 0.6f;
        }
        else if (currentStreak >= 15)
        {
            timerSpeed = 0.5f;
        }
        else if (currentStreak >= 10)
        {
            timerSpeed = 0.4f;
        }
        else if (currentStreak >= 5)
        {
            timerSpeed = 0.3f;
        }
        else
        {
            //default timer speed
            timerSpeed = 0.2f;
        }
    }

    private void ListenForWinCondition()
    {
        if(correctSequences >= sequencesToWin)
        { 
            gameOver = true;
            MiniGameEnd();
        }
    }

    private void ListenForSequenceCompleteCondition()
    {
        if(
        hitArrow1 == true &&
        hitArrow2 == true &&
        hitArrow3 == true &&
        hitArrow4 == true &&
        hitArrow5 == true)
        {
            //Sequence complete! Full points!
            scoreMultiplier = 5; // max multiplier for max hits!
            AddPoints();

            currentStreak++; // increment streak
            correctSequences++; // increment counter
            hitsToWinLeft--; // decrement counter to show visually how many player has left

            message = "SUCCESS!";

            // move on- VIA COROUTINE?
            SetNextSequence();
        }
    }

    private void SequenceFailCondition(Sequence sequence)
    {
        currentStreak = 0; // reset streak
        //check how many arrows succseffully hit
        int numOfHits = 0;

        //iterate over sequence
        for (int i = 0; i < sequence.intSequence.Length; i++)
        {
            //check for hit on each arrow
            bool didHit = sequence.GetArrowHit(i);

            // if it did, increment hit counter
            if (didHit == true)
                numOfHits++;
        }

        if (numOfHits == 4)
            message = "SO CLOSE!";
        if (numOfHits >= 1 && numOfHits <= 4)
            message = "NEARLY...";
        if (numOfHits == 0)
            message = "MISS";

        //Set up a scoring thing with these number of hits!
        scoreMultiplier = numOfHits;
        AddPoints();

        //account for missed arrows in score
        int messyWorkNum = 5 - numOfHits;
        shoddyWorkScore += messyWorkNum * 3;

        //Move on to next sequence
        SetNextSequence();
    }

    private void AddPoints() 
    {
        currentScore += scorePerHit * scoreMultiplier;
    }

    private void SetNextSequence()
    {
        //Reset timer
        timerSlider.value = 1;

        UpdateArrowsList();
        UpdateArrowVisuals();

        //reset bools
        hitArrow1 = false;
        hitArrow2 = false;
        hitArrow3 = false;
        hitArrow4 = false;
        hitArrow5 = false;

        //Set color
        foreach (Image sprite in currentArrows)
        {
            //revert colour
            sprite.color = Color.red;
        }
    }
    #endregion

    #region Player Input
    private void HandlePlayerInput(int inputValue)
    {
        // get current list
        Sequence currentSequence = currentSequences[1]; // CHANGE THIS?

        if(!hitArrow1) // if they haven't gotten the first arrow, we focus on it to hit it
        {
            if (inputValue == currentSequence.intSequence[0])
            {
                //SUCCESSFUL HIT
                Debug.Log("Arrow 1 Success!");
                
                hitArrow1 = true;//update input listening bool
                currentSequence.SetArrowHit(0, true); //update sequence object bools

                //Temp?
                currentArrows[0].color = Color.green;
            }
            else // INCORRECT INPUT
            {
                SequenceFailCondition(currentSequence);
            }
        }
        else if(!hitArrow2) 
        {
            if (inputValue == currentSequence.intSequence[1])
            {
                Debug.Log("Arrow 2 Success!");

                hitArrow2 = true;
                currentSequence.SetArrowHit(1, true);

                currentArrows[1].color = Color.green;

            }
            else
            {
                SequenceFailCondition(currentSequence);
            }
        }
        else if(!hitArrow3)
        {
            if (inputValue == currentSequence.intSequence[2])
            {
                Debug.Log("Arrow 3 Success!");

                hitArrow3 = true;
                currentSequence.SetArrowHit(2, true);

                currentArrows[2].color = Color.green;
            }
            else
            {
                SequenceFailCondition(currentSequence);
            }
        }
        else if(!hitArrow4)
        {
            if (inputValue == currentSequence.intSequence[3])
            {
                Debug.Log("Arrow 4 Success!");

                hitArrow4 = true;
                currentSequence.SetArrowHit(3, true);

                currentArrows[3].color = Color.green;
            }
            else
            {
                SequenceFailCondition(currentSequence);
            }
        }
        else if(!hitArrow5)
        {
            if (inputValue == currentSequence.intSequence[4])
            {
                Debug.Log("Arrow 5 Success!");

                hitArrow5 = true;
                currentSequence.SetArrowHit(4, true);

                currentArrows[4].color = Color.green;
            }
            else
            {
                SequenceFailCondition(currentSequence);
            }
        }
    }

    private void ListenForPlayerInput()
    {
        //initialize index
        int imageIndex = 0;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            playerInputLeft = true;
            imageIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            playerInputUp = true;
            imageIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            playerInputDown = true;
            imageIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            playerInputRight = true;
            imageIndex = 3;
        }

        if(playerInputLeft || playerInputUp || playerInputDown || playerInputRight) 
        {
            
            HandlePlayerInput(imageIndex);
            
            //Reset bools ?
            playerInputLeft = false;
            playerInputUp = false;
            playerInputDown = false;
            playerInputRight = false;
        }
    }
    #endregion

    #region Sequence Visualization

    /// <summary>
    /// Function is in its name.
    /// There are 7 visual representations of the sequences of the list
    /// index 0 = the "Up Next" sequence
    /// index 1 = always the sequence for players to try input
    /// indexes 2-6 = the previous sequences
    /// </summary>
    private void UpdateArrowVisuals()
    {
        //Update NEXT sprites using int[] at last index
        UpdateSequenceImageArrows(nextArrows, currentSequences, 0);

        //Update CURRENT arrow sprites
        UpdateSequenceImageArrows(currentArrows, currentSequences, 1);

        //Handle the PREVIOUS arrow sprites
        {
            // Now theres more than the base Do the shit
            if (currentSequences.Count > 6) // 7 Sequences are now in play
            {
                UpdatePreviousArrowSprites(previousArrows, currentSequences, 6);
            }

            if (currentSequences.Count > 5) // 6 sequences in play
            {
                UpdatePreviousArrowSprites(previousArrows, currentSequences, 5);
            }

            if (currentSequences.Count > 4) // 5 sequences in play
            {
                UpdatePreviousArrowSprites(previousArrows, currentSequences, 4);
            }

            if (currentSequences.Count > 3) // 4 sequences in play
            {
                UpdatePreviousArrowSprites(previousArrows, currentSequences, 3);
            }

            if (currentSequences.Count > 2) // 3 sequences in play
            {
                UpdatePreviousArrowSprites(previousArrows, currentSequences, 2);
            }
        }
    }

    /// <summary>
    /// Disables each Image object within the given Image[]
    /// </summary>
    /// <param name="arrows">Which image[] to manipulate</param>
    /// <param name="index">which section of arrows to change. NOTE, working with "previousArrows", index should be between 2-6 inclusive</param>
    private void HideImageSequenceArrows(Image[] arrows, int index)
    {
        if(index == 2) //0 - 4
        {
            //Iterate through chosen array and set the arrow sprites
            for (int i = 0; i < 5; i++)
            {
                arrows[i].enabled = false; // Hide the arrow
            }
        }
        else if (index == 3) //5 - 9
        {
            for (int i = 0; i < 5; i++)
            {
                arrows[i + 5].enabled = false;
            }
        }
        else if (index == 4) //10 - 14
        {
            for (int i = 0; i < 5; i++)
            {
                arrows[i + 10].enabled = false;
            }
        }
        else if (index == 5) //15 - 19
        {
            for (int i = 0; i < 5; i++)
            {
                arrows[i + 15].enabled = false;
            }
        }
        else if (index == 6) //20 - 24
        {
            for (int i = 0; i < 5; i++)
            {
                arrows[i + 20].enabled = false;
            }
        }
        
    }

    /// <summary>
    /// Updates the arrow images in the UI to reflect the ints stored in the sequence list
    /// </summary>
    /// <param name="arrows"> the image[] you want to manipulate</param>
    /// <param name="sequence"> sequence list of int arrays in which to take the ints from for arrow assignment</param>
    /// <param name="index">Which element of chosen sequence to work on (between 0-6)</param>
    /// <param name="previousArrows">Whether we're working in the previous arrows context or not</param>
    private void UpdateSequenceImageArrows(Image[] arrows, List<Sequence> sequence, int index/*, bool previousArrows*/)
    {
        if (index < 0 || index >= sequence.Count) return; // Bounds check for 'index'

        Sequence currentContextSequence = sequence[index]; // Get the integer array at the specified 'index'
                                                           //Debug.Log("CCS: " + currentContextSequence[0] + " " + currentContextSequence[1] + " " 
                                                           //    + currentContextSequence[2] + " " + currentContextSequence[3] + " " + currentContextSequence[4]);
                                                           //Debug.Log("CCS: " + currentContextSequence.ToString());

        //Iterate through chosen array and set the arrow sprites
        for (int i = 0; i < arrows.Length; i++)
        {
            // check if 'i' is in range
            if (i < currentContextSequence.intSequence.Length)
            {
                // get the value from the sequence at i position
                int spriteIndex = currentContextSequence.intSequence[i];

                if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                {
                    //Debug.Log("INDEX: " + i.ToString());
                    //Debug.Log("SPRITE INDEX: " + spriteIndex.ToString());

                    // Assign the sprite based on the value in 'currentContextSequence'
                    arrows[i].sprite = arrowSprites[spriteIndex];
                }
                else
                {
                    // Handle invalid spriteIndex?
                    // assign default sprite
                    // log an error
                }
            }
            else
            {
                // Handle the case where there are more 'arrows' than values in 'currentSequence'??
            }
        }

        //switch (previousArrows)
        //{
        //    case false: // just work on the current context and next up arrows
        //
        //        //Iterate through chosen array and set the arrow sprites
        //        for (int i = 0; i < arrows.Length; i++)
        //        {
        //            // check if 'i' is in range
        //            if (i < currentContextSequence.intSequence.Length)
        //            {
        //                // get the value from the sequence at i position
        //                int spriteIndex = currentContextSequence.intSequence[i];
        //
        //                if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
        //                {
        //                    //Debug.Log("INDEX: " + i.ToString());
        //                    //Debug.Log("SPRITE INDEX: " + spriteIndex.ToString());
        //
        //                    // Assign the sprite based on the value in 'currentContextSequence'
        //                    arrows[i].sprite = arrowSprites[spriteIndex];
        //                }
        //                else
        //                {
        //                    // Handle invalid spriteIndex?
        //                    // assign default sprite
        //                    // log an error
        //                }
        //            }
        //            else
        //            {
        //                // Handle the case where there are more 'arrows' than values in 'currentSequence'??
        //            }
        //        }
        //
        //    break;
        //
        //    case true: // now we're handling the previous arrows
        //        // this array is 25 items long. needs to be broken up into segments
        //        //0 - 4 = previous arrow image sequence 1
        //        //5 - 9 = previous arrow image sequence 2
        //        //10 - 14 = previous arrow image sequence 3
        //        //15 - 19 = previous arrow image sequence 4
        //        //20 - 24 = previous arrow image sequence 5
        //
        //        if (index == 2) // 3 sequences in play indexs 0 - 4
        //        {
        //            // run 5 tyimes
        //            for (int i = 0; i < 5; i++)
        //            {
        //                // check if 'i' is in range
        //                if (i < currentContextSequence.intSequence.Length)
        //                {
        //                    // get the value from the sequence at i position
        //                    int spriteIndex = currentContextSequence.intSequence[i];
        //
        //                    if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
        //                    {
        //                        //enable, in case of disabled
        //                        arrows[i].enabled = true;
        //
        //                        // Assign the sprite based on the value in 'currentContextSequence'
        //                        arrows[i].sprite = arrowSprites[spriteIndex];
        //
        //                        //check if corresponding "arrowHit" bool is true and change sprite color accordingly
        //                        // Set the color based on whether the arrow was hit
        //                        bool arrowHit = currentContextSequence.GetArrowHit(i % 5); // modulo 5 to keep within 0 -4 (just incase index exceeds)
        //                        float alpha = arrowHit ? 0.5f : 0.05f; // half alpha if hit, fractional if not
        //                        arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, alpha);
        //                    }
        //                    else
        //                    {
        //                        // Handle invalid spriteIndex?
        //                        // default sprite?
        //                        // log an error?
        //                    }
        //                }
        //                else
        //                {
        //                    // Handle out of range?
        //                    // default sprite?
        //                    // log an error?
        //                }
        //            }
        //        }
        //        else if (index == 3) //5 - 9
        //        {
        //            // run 5 tyimes
        //            for (int i = 0; i < 5; i++)
        //            {
        //                // check if 'i' is in range
        //                if (i < currentContextSequence.intSequence.Length)
        //                {
        //                    // get the value from the sequence at i position
        //                    int spriteIndex = currentContextSequence.intSequence[i];
        //
        //                    if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
        //                    {
        //                        //enable, in case of disabled
        //                        arrows[i + 5].enabled = true;
        //
        //                        // Assign the sprite based on the value in 'currentContextSequence'
        //                        arrows[i + 5].sprite = arrowSprites[spriteIndex];
        //
        //                        // Set the color based on whether the arrow was hit
        //                        bool arrowHit = currentContextSequence.GetArrowHit(i + 5 % 5); // modulo 5 to keep within 0 -4 (just incase index exceeds)
        //                        float alpha = arrowHit ? 0.5f : 0.05f; // half alpha if hit, fractional if not
        //                        arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, alpha);
        //                    }
        //                }
        //            }
        //        }
        //        else if (index == 4) //10 - 14 
        //        {
        //            // run 5 tyimes
        //            for (int i = 0; i < 5; i++)
        //            {
        //                // check if 'i' is in range
        //                if (i < currentContextSequence.intSequence.Length)
        //                {
        //                    // get the value from the sequence at i position
        //                    int spriteIndex = currentContextSequence.intSequence[i];
        //
        //                    if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
        //                    {
        //                        //enable, in case of disabled
        //                        arrows[i + 10].enabled = true;
        //                        // Assign the sprite based on the value in 'currentContextSequence'
        //                        arrows[i + 10].sprite = arrowSprites[spriteIndex];
        //
        //                        // Set the color based on whether the arrow was hit
        //                        bool arrowHit = currentContextSequence.GetArrowHit(i + 10 % 5); // modulo 5 to keep within 0 -4 (just incase index exceeds)
        //                        float alpha = arrowHit ? 0.5f : 0.05f; // half alpha if hit, fractional if not
        //                        arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, alpha);
        //                    }
        //                }
        //            }
        //        }
        //        else if (index == 5) //15 - 19
        //        {
        //            // run 5 tyimes
        //            for (int i = 0; i < 5; i++)
        //            {
        //                // check if 'i' is in range
        //                if (i < currentContextSequence.intSequence.Length)
        //                {
        //                    // get the value from the sequence at i position
        //                    int spriteIndex = currentContextSequence.intSequence[i];
        //
        //                    if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
        //                    {
        //                        //enable, in case of disabled
        //                        arrows[i + 15].enabled = true;
        //                        // Assign the sprite based on the value in 'currentContextSequence'
        //                        arrows[i + 15].sprite = arrowSprites[spriteIndex];
        //
        //                        // Set the color based on whether the arrow was hit
        //                        bool arrowHit = currentContextSequence.GetArrowHit(i + 15 % 5); // modulo 5 to keep within 0 -4 (just incase index exceeds)
        //                        float alpha = arrowHit ? 0.5f : 0.05f; // half alpha if hit, fractional if not
        //                        arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, alpha);
        //                    }
        //                }
        //            }
        //        }
        //        else if (index == 6) //20 - 24
        //        {
        //            // run 5 tyimes
        //            for (int i = 0; i < 5; i++)
        //            {
        //                // check if 'i' is in range
        //                if (i < currentContextSequence.intSequence.Length)
        //                {
        //                    // get the value from the sequence at i position
        //                    int spriteIndex = currentContextSequence.intSequence[i];
        //
        //                    if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
        //                    {
        //                        //enable, in case of disabled
        //                        arrows[i + 20].enabled = true;
        //                        // Assign the sprite based on the value in 'currentContextSequence'
        //                        arrows[i + 20].sprite = arrowSprites[spriteIndex];
        //
        //                        // Set the color based on whether the arrow was hit
        //                        bool arrowHit = currentContextSequence.GetArrowHit(i + 20 % 5); // modulo 5 to keep within 0 -4 (just incase index exceeds)
        //                        float alpha = arrowHit ? 0.5f : 0.05f; // half alpha if hit, fractional if not
        //                        arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, alpha);
        //                    }
        //                }
        //            }
        //        }
        //
        //    break;
        //}
    }

    /// <summary>
    /// Updates the  previous arrow images in the UI to reflect the ints stored in the sequence list
    /// keeps the color change within context.
    /// </summary>
    /// <param name="arrows">the image[] you want to manipulate</param>
    /// <param name="sequence">Which list of sequences to read from</param>
    /// <param name="index">The index of specific Sequence to access</param>
    private void UpdatePreviousArrowSprites(Image[] arrows, List<Sequence> sequence, int index)
    {
        Sequence currentContextSequence = sequence[index];

        // this array is 25 items long. needs to be broken up into segments
        //0 - 4 = previous arrow image sequence 1
        //5 - 9 = previous arrow image sequence 2
        //10 - 14 = previous arrow image sequence 3
        //15 - 19 = previous arrow image sequence 4
        //20 - 24 = previous arrow image sequence 5

        if (index == 2) // 3 sequences in play indexs 0 - 4
        {
            // run 5 tyimes
            for (int i = 0; i < 5; i++)
            {
                // check if 'i' is in range
                if (i < currentContextSequence.intSequence.Length)
                {
                    // get the value from the sequence at i position
                    int spriteIndex = currentContextSequence.intSequence[i];

                    if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                    {
                        //enable, in case of disabled
                        arrows[i].enabled = true;

                        // Assign the sprite based on the value in 'currentContextSequence'
                        arrows[i].sprite = arrowSprites[spriteIndex];

                        //check if corresponding "arrowHit" bool is true and change sprite color accordingly
                        // Set the color based on whether the arrow was hit
                        bool arrowHit = currentContextSequence.GetArrowHit(i % 5); // modulo 5 to keep within 0 -4 (just incase index exceeds)
                        float alpha = arrowHit ? 0.5f : 0.05f; // half alpha if hit, fractional if not
                        arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, alpha);
                    }
                    else
                    {
                        // Handle invalid spriteIndex?
                        // default sprite?
                        // log an error?
                    }
                }
                else
                {
                    // Handle out of range?
                    // default sprite?
                    // log an error?
                }
            }
        }
        else if (index == 3) //5 - 9
        {
            // run 5 times
            for (int i = 0; i < 5; i++)
            {
                // get the value from the sequence at i position
                int spriteIndex = currentContextSequence.intSequence[i];

                if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                {
                    //enable, in case of disabled
                    arrows[i + 5].enabled = true;

                    // Assign the sprite based on the value in 'currentContextSequence'
                    arrows[i + 5].sprite = arrowSprites[spriteIndex];

                    // Set the color based on whether the arrow was hit
                    bool arrowHit = currentContextSequence.GetArrowHit(i + 5 % 5); // modulo 5 to keep within 0 -4 (just incase index exceeds)
                    float alpha = arrowHit ? 0.5f : 0.05f; // half alpha if hit, fractional if not
                    arrows[i + 5].color = new Color(arrows[i + 5].color.r, arrows[i + 5].color.g, arrows[i + 5].color.b, alpha);
                }
            }
        }
        else if (index == 4) //10 - 14 
        {
            // run 5 tyimes
            for (int i = 0; i < 5; i++)
            {
                // check if 'i' is in range
                if (i < currentContextSequence.intSequence.Length)
                {
                    // get the value from the sequence at i position
                    int spriteIndex = currentContextSequence.intSequence[i];

                    if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                    {
                        //enable, in case of disabled
                        arrows[i + 10].enabled = true;
                        // Assign the sprite based on the value in 'currentContextSequence'
                        arrows[i + 10].sprite = arrowSprites[spriteIndex];

                        // Set the color based on whether the arrow was hit
                        bool arrowHit = currentContextSequence.GetArrowHit(i + 10 % 5); // modulo 5 to keep within 0 -4 (just incase index exceeds)
                        float alpha = arrowHit ? 0.5f : 0.05f; // half alpha if hit, fractional if not
                        arrows[i + 10].color = new Color(arrows[i + 10].color.r, arrows[i + 10].color.g, arrows[i + 10].color.b, alpha);
                    }
                }
            }
        }
        else if (index == 5) //15 - 19
        {
            // run 5 tyimes
            for (int i = 0; i < 5; i++)
            {
                // check if 'i' is in range
                if (i < currentContextSequence.intSequence.Length)
                {
                    // get the value from the sequence at i position
                    int spriteIndex = currentContextSequence.intSequence[i];

                    if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                    {
                        //enable, in case of disabled
                        arrows[i + 15].enabled = true;
                        // Assign the sprite based on the value in 'currentContextSequence'
                        arrows[i + 15].sprite = arrowSprites[spriteIndex];

                        // Set the color based on whether the arrow was hit
                        bool arrowHit = currentContextSequence.GetArrowHit(i + 15 % 5); // modulo 5 to keep within 0 -4 (just incase index exceeds)
                        float alpha = arrowHit ? 0.5f : 0.05f; // half alpha if hit, fractional if not
                        arrows[i + 15].color = new Color(arrows[i + 15].color.r, arrows[i + 15].color.g, arrows[i + 15].color.b, alpha);
                    }
                }
            }
        }
        else if (index == 6) //20 - 24
        {
            // run 5 tyimes
            for (int i = 0; i < 5; i++)
            {
                // check if 'i' is in range
                if (i < currentContextSequence.intSequence.Length)
                {
                    // get the value from the sequence at i position
                    int spriteIndex = currentContextSequence.intSequence[i];

                    if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                    {
                        //enable, in case of disabled
                        arrows[i + 20].enabled = true;
                        // Assign the sprite based on the value in 'currentContextSequence'
                        arrows[i + 20].sprite = arrowSprites[spriteIndex];

                        // Set the color based on whether the arrow was hit
                        bool arrowHit = currentContextSequence.GetArrowHit(i + 20 % 5); // modulo 5 to keep within 0 -4 (just incase index exceeds)
                        float alpha = arrowHit ? 0.5f : 0.05f; // half alpha if hit, fractional if not
                        arrows[i + 20].color = new Color(arrows[i + 20].color.r, arrows[i + 20].color.g, arrows[i + 20].color.b, alpha);
                    }
                }
            }
        }
    }
   
    #endregion

    #region Sequence Generation

    /// <summary>
    /// Updates the currentSequences list on the FIFO (First In, First Out) principle.
    /// This is use to control which sequences are displayed where, correctly
    /// </summary>
    private void UpdateArrowsList()
    {
        if (currentSequences.Count < 7)
        {
            // Add a sequence to the START of a list
            currentSequences.Insert(0, GenerateSequenceObject());
            //Debug.Log("----------------------------CURRENT SEQUENCE LIST: ");
            //PrintSequenceList(currentSequences.Count);
        }
        else // list is now full, cycle numbers through
        {
            // fill the temp list with the current list in play for safe modification
            tempSequences = new List<Sequence>(currentSequences);
        
            tempSequences.RemoveAt(currentSequences.Count - 1); // remove sequence first in, which is now at end of list

            //now add a new sequence to fill the gap
            tempSequences.Insert(0, GenerateSequenceObject()); // this will be pushed into 0 slot

            //reassign list
            currentSequences = new List<Sequence>(tempSequences);
        
            // print outcomes
            //Debug.Log("----------------------------CURRENT SEQUENCE LIST: ");
            //PrintSequenceList(currentSequences.Count);
        }

        #region Old shiz
        //if (currentSequences.Count < 7)
        //{
        //    currentSequences.Add(GenerateRandomSequence());
        //    Debug.Log("----------------------------CURRENT SEQUENCE LIST: ");
        //    PrintSequenceList(currentSequences.Count);
        //}
        //else // list is now full, cycle numbers through
        //{
        //    // fill the temp list with the current list in play for safe modification
        //    tempSequences = new List<int[]>(currentSequences);
        //
        //    // FOLLOW THE FIFO RULE (First In, First Out)
        //    tempSequences.RemoveAt(0); // remove sequence first in
        //
        //    //now add a new sequence to fill the gap
        //    tempSequences.Add(GenerateRandomSequence()); // this will be tacked on the end
        //
        //    //reassign list
        //    currentSequences = new List<int[]>(tempSequences);
        //
        //    // print outcomes
        //    Debug.Log("----------------------------CURRENT SEQUENCE LIST: ");
        //    PrintSequenceList(currentSequences.Count);
        //}
        #endregion
    }

    /// <summary>
    /// Function to generate an int array of 5 numbers between 0 - 3
    /// this generation is used to dictate which in-game arrows will be used;
    /// 0 = left arrow
    /// 1 = up arrow
    /// 2 = down arrow
    /// 3 = right arrow
    /// </summary>
    /// <returns>the int array of 5 numbers to store in the list</returns>
    private Sequence GenerateSequenceObject()
    {

        //generate 5 random ints between 0-3 (4 possibilities)
        int num1 = Random.Range(0, 4);
        int num2 = Random.Range(0, 4);
        int num3 = Random.Range(0, 4);
        int num4 = Random.Range(0, 4);
        int num5 = Random.Range(0, 4);

        //save numbers generated in array
        int[] sequence = new int[] {num1, num2, num3, num4, num5};

        Sequence sequenceObject = new Sequence(sequence);
        //Set Sequence Object variables
        //sequenceObject.intSequence = sequence;
        sequenceObject.SetArrowHit(0, false);
        sequenceObject.SetArrowHit(1, false);
        sequenceObject.SetArrowHit(2, false);
        sequenceObject.SetArrowHit(3, false);
        sequenceObject.SetArrowHit(4, false);

        return sequenceObject;
    }

    /// <summary>
    /// Print out the current list of sequences in play tp the console
    /// </summary>
    /// <param name="size">the current size of currentSequences list</param>
    private void PrintSequenceList(int size)
    {
        for (int i = 0; i < size; i++)
        {
            Debug.Log(currentSequences[i].intSequence[0] + " " + currentSequences[i].intSequence[1] + " " + currentSequences[i].intSequence[2]
                + " " + currentSequences[i].intSequence[3] + " " + currentSequences[i].intSequence[4]);
        }
    }

    #endregion


    void MiniGameEnd()
    {
        //Initiate final visual changes
        totalScore = currentScore - shoddyWorkScore;
        //probably get the xp gained from score
        //Calculate xp gained based on total score
        xpGained = totalScore / 3 /* * satisfactionMultiplier*/;

        //coin
        coinGained = (int)Mathf.Floor(totalScore / 10);

        //set the end screen active
        StartCoroutine(DelayEndScreen());

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
