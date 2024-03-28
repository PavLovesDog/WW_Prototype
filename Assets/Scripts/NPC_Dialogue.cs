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
    /// </summary>

    [Header("Text Variables")]
    public string[] NPCLines;
    public string[] buttonDialogue;
    public float textSpeed = 0.03f;
    private int index;

    private GameObject dialogueCanvasObject;
    public Canvas dialogueCanvas;
    //public Interactable interactable; // the script of the interactable object SET IN INSPECTOR

    private TMP_Text npcText;
    private Image npcSprite;
    private Button acceptBtn;
    private TMP_Text acceptBtnText;
    private Button refuseBtn;
    private TMP_Text refuseBtnText;
    private Button continueBtn;
    private TMP_Text continueBtnText;

    private void Awake()
    {
        if (dialogueCanvas == null)
        {
            //Find canvas object
            //dialogueCanvasObject = GameObject.FindGameObjectWithTag("DialogueCanvas");
            dialogueCanvas = GameObject.FindGameObjectWithTag("DialogueCanvas").GetComponent<Canvas>();
            dialogueCanvas.enabled = false;
        }


        if (objectCanvas == null)
        {
            //Find canvas object
            objectCanvas = GetComponentInChildren<Canvas>();
        }

        //set interactable state
        EnableInteractable(false);

        //set initial states
        InitializeUIElements();

        //InitiateDialogue();
    }

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

    }

    #region Dialogue Button Functions

    public void OnAcceptButtonPress()
    {
        //generate Positive line response
        NextLine();

        //TEMP load scene for Rain Minigame
        SceneManager.LoadScene(1);
    }

    public void OnRefuseButtonPress()
    {
        //generate negative line response
        NextLine();
    }

    public void OnContinueButtonPress()
    {
        //generate the next lot of text, if there is one
        NextLine();
    }

    public void OnLeaveButtonPress()
    {
        //diable the dialogue canvas
        dialogueCanvas.enabled = false;
        initiateDialogue = false;
    }

    #endregion

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

        //link buttons of NPC to the canvas
        //Set up each buttons OnClick() event
        //  - tie it to the specific script of the current NPC
        acceptBtn.onClick.AddListener(OnAcceptButtonPress);
        refuseBtn.onClick.AddListener(OnRefuseButtonPress);
        continueBtn.onClick.AddListener(OnContinueButtonPress);

        


        StartDialogue();
    }

    #region Text Generation
    public void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    public void SkipLine()
    {
        //audioSource.Stop();

        if (npcText.text == NPCLines[index])
        {
            NextLine();
        }
        else
        {
            StopAllCoroutines();
            npcText.text = NPCLines[index];
        }
    }

    IEnumerator TypeLine()
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

    void NextLine()
    {
        if (index < NPCLines.Length - 1)
        {
            index++;
            npcText.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            //if (!inGameScene)
            //{
            //    LoadNextScene();
            //}
            //else // canvas is within game
            //{
            //    //// Notify DialogueManager that dialogue has ended
            //    //DialogueManager.Instance.EndDialogue();
            //
            //    //deactivate canvas & empty string
            //    //canvas.enabled = false;
            //}
                npcText.text = string.Empty;
        }
    }
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
