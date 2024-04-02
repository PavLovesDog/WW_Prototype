using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.InputManagerEntry;
using System;
using UnityEngine.SceneManagement;

public class NPC_Dialogue : Interactable
{
    /// <summary>
    /// Script to control; 
    ///     - when the dialogue box will be visible
    ///     - what the dialogue will say (accessible from the inspector)
    ///     - which buttons are active
    ///     - buttons will be the players response
    /// 
    /// NPC Lines array to be filled with context dependent lines
    /// Indexes as follows:
    /// 
    /// 0 = greetings
    /// 1 = job offer
    /// 2 = acceptence response
    /// 3 = refusal response
    /// 4 = repeatable job offer text
    /// 5 - n = flavour text stuff
    /// 
    /// </summary>
 
    //Canvas Variables
    private GameObject dialogueCanvasObject;
    private Canvas dialogueCanvas;

    //Enum for differentiating NPC text type
    public enum DialogueType
    {
        JobGiver = 0,
        FlavourText = 1
    }
    public DialogueType NPCType; // Enum to set in inspector
    public bool startJob = false;
    public string jobSceneName; // string which will hold the name of job scene to open
    private Coroutine dialogueTypeCoroutine;

    private bool inGreetings = true;

    [Header("Text Variables")]
    public string[] NPCLines;
    public string[] buttonDialogue;
    public float textSpeed = 0.03f;
    private int FTIndex = 5; // Flavour Text Index - this will control which flavour text responses are shown/cycled through
    private TMP_Text npcText;
    private Image npcSprite;
    private Button acceptBtn;
    private TMP_Text acceptBtnText;
    private Button refuseBtn;
    private TMP_Text refuseBtnText;
    private Button continueBtn;
    private TMP_Text continueBtnText;
    private Button leaveBtn;


    private void InitializeUIElements()
    {
        // Find the npcText by iterating through children of dialogueCanvas
        npcText = FindComponentInChildrenWithTag<TMP_Text>(dialogueCanvas.gameObject, "NPC_DialogueText");
        npcSprite = FindComponentInChildrenWithTag<Image>(dialogueCanvas.gameObject, "NPC_DialogueSprite");

        //itialize buttons
        acceptBtn = FindComponentInChildrenWithTag<Button>(dialogueCanvas.gameObject, "AcceptBtn");
        acceptBtnText = acceptBtn.GetComponentInChildren<TMP_Text>();
        acceptBtnText.text = buttonDialogue[0];

        refuseBtn = FindComponentInChildrenWithTag<Button>(dialogueCanvas.gameObject, "RefuseBtn");
        refuseBtnText = refuseBtn.GetComponentInChildren<TMP_Text>();
        refuseBtnText.text = buttonDialogue[1];
        
        continueBtn = FindComponentInChildrenWithTag<Button>(dialogueCanvas.gameObject, "ContinueBtn");
        continueBtnText = continueBtn.GetComponentInChildren<TMP_Text>();
        continueBtnText.text = buttonDialogue[2];

        leaveBtn = FindComponentInChildrenWithTag<Button>(dialogueCanvas.gameObject, "LeaveBtn");

    }

    private void Awake()
    {
        //Find Dialogue Canvas object and enable
        if (dialogueCanvas == null)
        {
            dialogueCanvas = GameObject.FindGameObjectWithTag("DialogueCanvas").GetComponent<Canvas>();
            dialogueCanvas.enabled = false;
        }

        // initiate objects own canvas
        if (objectCanvas == null)
        {
            //Find canvas object
            objectCanvas = GetComponentInChildren<Canvas>();
        }

        //set interactable state
        EnableInteractable(false);

        //set initial states
        InitializeUIElements();
    }

    private void Update()
    {
       if(initiateDialogue)
       {
            initiateDialogue = false;
            InitiateDialogue();
       }

        // player is near and can interact with the object
        if (canInteract)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HandleInteractions();
            }
        }
    }

    private void InitiateDialogue()
    {
        //make the dialogue box visible
        dialogueCanvas.enabled = true;

        //link buttons of unique NPC to the canvas
        //Set up each buttons OnClick() event
        acceptBtn.onClick.AddListener(OnAcceptButtonPress);
        refuseBtn.onClick.AddListener(OnRefuseButtonPress);
        continueBtn.onClick.AddListener(OnContinueButtonPress);
        leaveBtn.onClick.AddListener(OnLeaveButtonPress);

        ////do necessary setup (button activation), based on NPC type
        //if (NPCType == DialogueType.JobGiver)
        //{
        //    acceptBtn.interactable = true;
        //    refuseBtn.interactable = true;
        //    continueBtn.interactable = true;
        //}
        //else if (NPCType != DialogueType.FlavourText)
        //{
        //    acceptBtn.interactable = false;
        //    refuseBtn.interactable = false;
        //    continueBtn.interactable = true;
        //}

        acceptBtn.interactable = false;
        refuseBtn.interactable = false;
        continueBtn.interactable = true;

        //begin displaying text, Greetings text
        StartDialogue(0);
    }

    public void HandlePostGreetingButtonActivation()
    {
        //do necessary setup (button activation), based on NPC type
        if (NPCType == DialogueType.JobGiver)
        {
            acceptBtn.interactable = true;
            refuseBtn.interactable = true;
            continueBtn.interactable = true;
        }
        else if (NPCType != DialogueType.FlavourText)
        {
            acceptBtn.interactable = false;
            refuseBtn.interactable = false;
            continueBtn.interactable = true;
        }
    }

    #region Dialogue Button Functions

    public void OnAcceptButtonPress()
    {
        //generate Positive line response
        StartDialogue(2);

        //set the buttons
        acceptBtn.interactable = false;
        refuseBtn.interactable = false;

        startJob = true; // set bool to begin job when continue button is pressed
    }

    public void OnRefuseButtonPress()
    {
        //generate negative line response
        StartDialogue(3);

        //end convo
        // by greying out the dialogue buttons, leaving only the "Leave" button
        acceptBtn.interactable = false;
        refuseBtn.interactable = false;
        continueBtn.interactable = false;

        //Reset greetings bool
        inGreetings = true;
    }

    public void OnContinueButtonPress()
    {

        //JOB GIVER STUFF
        if(NPCType == DialogueType.JobGiver)
        {
            if(inGreetings)
            {
                inGreetings = false;
                StartDialogue(1);
                HandlePostGreetingButtonActivation();
                return; // stop further execution

            }
            
            //check if job needs to start
            if (startJob)
            {
                startJob = false;
                SceneManager.LoadScene(jobSceneName);
            }
            else
            {
                StartDialogue(4); //play repeatable text
            }
        }

        //Flavour Texter
        if(NPCType == DialogueType.FlavourText)
        {
            //system to run through index 5 onwards until end

            //if indedx is within range, write the line
            if (FTIndex < NPCLines.Length - 1)
            {
                StartDialogue(FTIndex);
                FTIndex++; //increment for next round
            }
            else // last text line has been written, now just repeat it
            {
                FTIndex = NPCLines.Length - 1; //set index to final text message
                StartDialogue(FTIndex);
            }

        }
    }

    public void OnLeaveButtonPress()
    {
        //remove buttons OnClick() event
        acceptBtn.onClick.RemoveListener(OnAcceptButtonPress);
        refuseBtn.onClick.RemoveListener(OnRefuseButtonPress);
        continueBtn.onClick.RemoveListener(OnContinueButtonPress);
        leaveBtn.onClick.RemoveListener(OnLeaveButtonPress);

        //reset FTIndex
        FTIndex = 5;

        //re-enable all buttons for next run
        acceptBtn.interactable = true;
        refuseBtn.interactable = true;
        continueBtn.interactable = true;

        //diable the dialogue canvas
        dialogueCanvas.enabled = false;
        initiateDialogue = false;
    }

    #endregion



    #region Text Generation
    public void StartDialogue(int index)
    {
        if (dialogueTypeCoroutine != null)
        {
            StopCoroutine(dialogueTypeCoroutine);
            StartCoroutine(TypeLine(index));
        }
        else
        {
            if(dialogueTypeCoroutine != null)
                StopCoroutine(dialogueTypeCoroutine);
            dialogueTypeCoroutine = StartCoroutine(TypeLine(index));
        }
    }

    public IEnumerator EndDialogue()
    {
        yield return new WaitForSeconds(1);
    }

    //public void SkipLine()
    //{
    //    //audioSource.Stop();
    //
    //    if (npcText.text == NPCLines[index])
    //    {
    //        NextLine();
    //    }
    //    else
    //    {
    //        StopAllCoroutines();
    //        npcText.text = NPCLines[index];
    //    }
    //}

    /// <summary>
    /// Function which types out the text to dialogue box during runtime
    /// </summary>
    /// <param name="index">the index of text line within NPCLines array</param>
    /// <returns></returns>
    IEnumerator TypeLine(int index)
    {
        //audioSource.Play(); // Play audio with characters
        npcText.text = string.Empty; // clear previous text
        //canvas.enabled = enabled; // ensure the canvas is enabled while characters to be written
        foreach (char c in NPCLines[index].ToCharArray())
        {
            npcText.text += c; // add character
            //audioSource.pitch = Random.Range(minPitch, maxPitch); // change pitch each character
            yield return new WaitForSeconds(textSpeed);
        }
        //audioSource.Stop(); // stop audio once done
    }

    //void NextLine()
    //{
    //    if (index < NPCLines.Length - 1)
    //    {
    //        index++;
    //        npcText.text = string.Empty;
    //        StartCoroutine(TypeLine());
    //    }
    //    else
    //    {
    //        //if (!inGameScene)
    //        //{
    //        //    LoadNextScene();
    //        //}
    //        //else // canvas is within game
    //        //{
    //        //    //// Notify DialogueManager that dialogue has ended
    //        //    //DialogueManager.Instance.EndDialogue();
    //        //
    //        //    //deactivate canvas & empty string
    //        //    //canvas.enabled = false;
    //        //}
    //            npcText.text = string.Empty;
    //    }
    //}
    #endregion

    // Generic method to find a component of type T in the children of a given parent GameObject by tag, recursively
    private T FindComponentInChildrenWithTag<T>(GameObject parent, string tag) where T : Component
    {
        // Check if the parent itself matches the tag and has the component
        if (parent.CompareTag(tag) && parent.GetComponent<T>() != null)
        {
            return parent.GetComponent<T>();
        }

        // Recursively iterate through all children and search for the tag and component
        foreach (Transform child in parent.transform)
        {
            T component = FindComponentInChildrenWithTag<T>(child.gameObject, tag);
            if (component != null)
            {
                return component;
            }
        }

        // If nothing is found, return null
        return null;
    }
}
