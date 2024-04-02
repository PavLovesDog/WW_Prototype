using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class MinigameManager_Cloud : MonoBehaviour
{
    public CMG_WeatherManager weatherManager;

    //Varibales for all UI aspects

    // Arrow Images
    // Direct references to change the sprite of each arrow
    public Image[] currentArrows;
    public Image[] nextArrows;
    public Image[] previousArrows;
    public Sprite[] arrowSprites;

    // timer slider
    public Slider timerSlider;
    public TMP_Text hitMessage;
    //on screen variables
    public TMP_Text scoreText;
    public TMP_Text streakText;
    public TMP_Text multiplierText;

    [Header("Endscreen Variables")]
    public GameObject gameOverScreen;
    public TMP_Text go_ScoreText;
    public TMP_Text go_ShoddyWorkText;
    public TMP_Text go_TotalScoreText;
    public TMP_Text go_CoinEarnedText;
    public TMP_Text go_XPEarnedText;

    //Lists to hold the arrows
    public List<int[]> currentSequences = new List<int[]>();
    public List<int[]> tempSequences = new List<int[]>();
    public List<int[]> previousSequences = new List<int[]>();

    // Variables for sequence generation, display, and previous
    public float timerSpeed = 0.2f; // 0.2 is roughly 5 seconds
    public float timerIncrease = 0.05f; // value to make timer faster

    void Start()
    {
        //initalize first 2 arrows llists to access
        currentSequences.Add(GenerateRandomSequence());
        currentSequences.Add(GenerateRandomSequence());
        UpdateArrowVisuals();

        //hide unused arrows
        for (int i = 2; i < 7; i++)
        {
            HideImageSequenceArrows(previousArrows, i);
        }

        Debug.Log("CURRENT SEQUENCE LIST: ");
        PrintSequenceList(currentSequences.Count);
    }


    void Update()
    {
        timerSlider.value -= Time.deltaTime * timerSpeed;

        if (timerSlider.value <= 0)
        {
            timerSlider.value = 1;

            UpdateArrowsList();
            UpdateArrowVisuals();
        }
    }

    private void UpdateArrowVisuals()
    {
        // there are 7 visual representations of the sequences of the list
        // index currentSequences.Count - 1 = the "Up Next" sequence
        // index currentSequences.Count - 2 = always the sequence for players to try input
        // other indexes = the previous sequences


        //Update NEXT sprites using int[] at last index
        UpdateSequenceImageArrows(nextArrows, currentSequences, 0, false);

        //Update CURRENT arrow sprites
        UpdateSequenceImageArrows(currentArrows, currentSequences, 1, false);

        // Now theres more than the base Do the shit
        if (currentSequences.Count > 6) // 7 Sequences are now in play
        {
            UpdateSequenceImageArrows(previousArrows, currentSequences, 6, true);
        }

        if (currentSequences.Count > 5) // 6 sequences in play
        {
            UpdateSequenceImageArrows(previousArrows, currentSequences, 5, true);
        }
        
        if (currentSequences.Count > 4) // 5 sequences in play
        {
            UpdateSequenceImageArrows(previousArrows, currentSequences, 4, true);
        }
        
        if (currentSequences.Count > 3) // 4 sequences in play
        {
            UpdateSequenceImageArrows(previousArrows, currentSequences, 3, true);
        }
        
        if (currentSequences.Count > 2) // 3 sequences in play
        {
            UpdateSequenceImageArrows(previousArrows, currentSequences, 2, true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arrows"></param>
    /// <param name="index"></param>
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
    /// 
    /// </summary>
    /// <param name="arrows"> the image[] you want to manipulate</param>
    /// <param name="sequence"> sequence list of int arrays in which to take the ints from for arrow assignment</param>
    /// <param name="index">Which element of the sequence to work on</param>
    private void UpdateSequenceImageArrows(Image[] arrows, List<int[]> sequence, int index, bool previousArrows)
    {
        if (index < 0 || index >= sequence.Count) return; // Bounds check for 'index'

        int[] currentContextSequence = sequence[index]; // Get the integer array at the specified 'index'
        //Debug.Log("CCS: " + currentContextSequence[0] + " " + currentContextSequence[1] + " " 
        //    + currentContextSequence[2] + " " + currentContextSequence[3] + " " + currentContextSequence[4]);
        //Debug.Log("CCS: " + currentContextSequence.ToString());
        
        switch(previousArrows)
        {
            case false: // just work on the current context and next up arrows

                //Iterate through chosen array and set the arrow sprites
                for (int i = 0; i < arrows.Length; i++)
                {
                    // check if 'i' is in range
                    if (i < currentContextSequence.Length)
                    {
                        // get the value from the sequence at i position
                        int spriteIndex = currentContextSequence[i];

                        if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                        {
                            //Debug.Log("INDEX: " + i.ToString());
                            //Debug.Log("SPRITE INDEX: " + spriteIndex.ToString());

                            // Assign the sprite based on the value in 'currentContextSequence'
                            arrows[i].sprite = arrowSprites[spriteIndex];
                        }
                        else
                        {
                            // Handle invalid spriteIndex, such as by assigning a default sprite or logging an error
                        }
                    }
                    else
                    {
                        // Handle the case where there are more 'arrows' than values in 'currentSequence'
                        // For example, by hiding the extra 'arrows' or assigning a default sprite
                    }
                }

                break;

            case true: // now we're handling the previous arrows
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
                        if (i < currentContextSequence.Length)
                        {
                            // get the value from the sequence at i position
                            int spriteIndex = currentContextSequence[i];

                            if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                            {
                                //enable, in case of disabled
                                arrows[i].enabled = true;

                                // Assign the sprite based on the value in 'currentContextSequence'
                                arrows[i].sprite = arrowSprites[spriteIndex];
                            }
                            else
                            {
                                // Handle invalid spriteIndex, such as by assigning a default sprite or logging an error
                            }
                        }
                        else
                        {
                            // Handle the case where there are more 'arrows' than values in 'currentSequence'
                            // For example, by hiding the extra 'arrows' or assigning a default sprite
                        }
                    }
                }
                else if (index == 3) //5 - 9
                {
                    // run 5 tyimes
                    for (int i = 0; i < 5; i++)
                    {
                        // check if 'i' is in range
                        if (i < currentContextSequence.Length)
                        {
                            // get the value from the sequence at i position
                            int spriteIndex = currentContextSequence[i];

                            if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                            {
                                //enable, in case of disabled
                                arrows[i + 5].enabled = true;

                                // Assign the sprite based on the value in 'currentContextSequence'
                                arrows[i + 5].sprite = arrowSprites[spriteIndex];
                            }
                            else
                            {
                                // Handle invalid spriteIndex, such as by assigning a default sprite or logging an error
                            }
                        }
                        else
                        {
                            // Handle the case where there are more 'arrows' than values in 'currentSequence'
                            // For example, by hiding the extra 'arrows' or assigning a default sprite
                        }
                    }
                }
                else if (index == 4) //10 - 14 
                {
                    // run 5 tyimes
                    for (int i = 0; i < 5; i++)
                    {
                        // check if 'i' is in range
                        if (i < currentContextSequence.Length)
                        {
                            // get the value from the sequence at i position
                            int spriteIndex = currentContextSequence[i];

                            if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                            {
                                //enable, in case of disabled
                                arrows[i + 10].enabled = true;
                                // Assign the sprite based on the value in 'currentContextSequence'
                                arrows[i + 10].sprite = arrowSprites[spriteIndex];
                            }
                            else
                            {
                                // Handle invalid spriteIndex, such as by assigning a default sprite or logging an error
                            }
                        }
                        else
                        {
                            // Handle the case where there are more 'arrows' than values in 'currentSequence'
                            // For example, by hiding the extra 'arrows' or assigning a default sprite
                        }
                    }
                }
                else if (index == 5) //15 - 19
                {
                    // run 5 tyimes
                    for (int i = 0; i < 5; i++)
                    {
                        // check if 'i' is in range
                        if (i < currentContextSequence.Length)
                        {
                            // get the value from the sequence at i position
                            int spriteIndex = currentContextSequence[i];

                            if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                            {
                                //enable, in case of disabled
                                arrows[i + 15].enabled = true;
                                // Assign the sprite based on the value in 'currentContextSequence'
                                arrows[i + 15].sprite = arrowSprites[spriteIndex];
                            }
                            else
                            {
                                // Handle invalid spriteIndex, such as by assigning a default sprite or logging an error
                            }
                        }
                        else
                        {
                            // Handle the case where there are more 'arrows' than values in 'currentSequence'
                            // For example, by hiding the extra 'arrows' or assigning a default sprite
                        }
                    }
                }
                else if (index == 6) //20 - 24
                {
                    // run 5 tyimes
                    for (int i = 0; i < 5; i++)
                    {
                        // check if 'i' is in range
                        if (i < currentContextSequence.Length)
                        {
                            // get the value from the sequence at i position
                            int spriteIndex = currentContextSequence[i];

                            if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
                            {
                                //enable, in case of disabled
                                arrows[i + 20].enabled = true;
                                // Assign the sprite based on the value in 'currentContextSequence'
                                arrows[i + 20].sprite = arrowSprites[spriteIndex];
                            }
                            else
                            {
                                // Handle invalid spriteIndex, such as by assigning a default sprite or logging an error
                            }
                        }
                        else
                        {
                            // Handle the case where there are more 'arrows' than values in 'currentSequence'
                            // For example, by hiding the extra 'arrows' or assigning a default sprite
                        }
                    }
                }

                break;
        }
        


        //switch (index)
        //{
        //    case 0: // NEXT SEQUENCE
        //
        //        for (int i = 0; i < arrows.Length; i++)
        //        {
        //            // check if 'i' is in range
        //            if (i < currentContextSequence.Length)
        //            {
        //                // get the value from the sequence at i position
        //                int spriteIndex = currentContextSequence[i];
        //
        //                if (spriteIndex >= 0 && spriteIndex < arrowSprites.Length) // if within range
        //                {
        //                    //Debug.Log("INDEX: " + i.ToString());
        //                    //Debug.Log("SPRITE INDEX: " + spriteIndex.ToString());
        //                    // Assign the sprite based on the value in 'currentContextSequence'
        //                    arrows[i].sprite = arrowSprites[spriteIndex];
        //                }
        //                else
        //                {
        //                    // Handle invalid spriteIndex, such as by assigning a default sprite or logging an error
        //                }
        //            }
        //            else
        //            {
        //                // Handle the case where there are more 'arrows' than values in 'currentSequence'
        //                // For example, by hiding the extra 'arrows' or assigning a default sprite
        //            }
        //        }
        //        break;
        //
        //    case 1: // PLAY SPACE SEQUENCE
        //        break;
        //    case 2: // Previous Sequence 1
        //        break;
        //    case 3: // Previous Sequence 2
        //        break;
        //    case 4: // Previous Sequence 3
        //        break;
        //    case 5: // Previous Sequence 4
        //        break;
        //    case 6: // Previous Sequence 5
        //        break;
        //
        //}
    }

    /// <summary>
    /// Updates the currentSequences list on the FIFO (First In, First Out) principle.
    /// This is use to control which sequences are displayed where, correctly
    /// </summary>
    private void UpdateArrowsList()
    {
        // Add a new item to list, then print it

        // if currentSequence list count is greater than 7, update lists
        // FOLLOW THE FIFO RULE (First In, First Out)
        // save the current list in the temp list and previous list
        // now need to remove the first added sequence from temp list
        // (which was the FIRST added in current, should be index[0])
        // add the new int[] to the end of list (as first in sequence, is first out)
        // (note: adding always ADDs the item to the end of the list)
        // ??? do indexs of other items auto-update with the removals ???
        // reassign current list to equal temp list (to reflect changes)
        // leave the previous list alone.

        // print that list


        if (currentSequences.Count < 7)
        {
            // Add a sequence to the START of a list
            currentSequences.Insert(0, GenerateRandomSequence());
            Debug.Log("----------------------------CURRENT SEQUENCE LIST: ");
            PrintSequenceList(currentSequences.Count);
        }
        else // list is now full, cycle numbers through
        {
            // fill the temp list with the current list in play for safe modification
            tempSequences = new List<int[]>(currentSequences);
        
            tempSequences.RemoveAt(currentSequences.Count - 1); // remove sequence first in, which is now at end of list

            //now add a new sequence to fill the gap
            tempSequences.Insert(0, GenerateRandomSequence()); // this will be pushed into 0 slot

            //reassign list
            currentSequences = new List<int[]>(tempSequences);
        
            // print outcomes
            Debug.Log("----------------------------CURRENT SEQUENCE LIST: ");
            PrintSequenceList(currentSequences.Count);
        }

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
    private int[] GenerateRandomSequence()
    {
        //generate 5 random ints between 0-3 (4 possibilities)
        int num1 = Random.Range(0, 4);
        int num2 = Random.Range(0, 4);
        int num3 = Random.Range(0, 4);
        int num4 = Random.Range(0, 4);
        int num5 = Random.Range(0, 4);

        //save numbers generated in array
        int[] sequence = new int[] {num1, num2, num3, num4, num5};

        return sequence;
    }

    /// <summary>
    /// Print out the current list of sequences in play tp the console
    /// </summary>
    /// <param name="size">the current size of currentSequences list</param>
    private void PrintSequenceList(int size)
    {
        for (int i = 0; i < size; i++)
        {
            Debug.Log(currentSequences[i][0] + " " + currentSequences[i][1] + " " + currentSequences[i][2]
                + " " + currentSequences[i][3] + " " + currentSequences[i][4]);
        }
    }



    //ARRAY LIST CREATION / ACCESS STUFF
    //Test single int array generation & print
    //int[] sequence = GenerateRandomSequence();
    //Debug.Log(sequence[0] + " " + sequence[1] + " " + sequence[2] + " " + sequence[3] + " " + sequence[4]);

    //// Test sequence list generation and access
    //for (int i = 0; i < 7; i++)
    //{
    //    currentSequences.Add(GenerateRandomSequence());
    //}
    ////print
    //for (int i = 0; i < 7; i++)
    //{
    //        Debug.Log(currentSequences[i][0] + " " + currentSequences[i][1] + " " + currentSequences[i][2] 
    //            + " " + currentSequences[i][3] + " " + currentSequences[i][4]);
    //}
    ////remove for new generation
    //for (int i = 6; i > -1; i--)
    //{
    //    currentSequences.Remove(currentSequences[i]);
    //}
}
