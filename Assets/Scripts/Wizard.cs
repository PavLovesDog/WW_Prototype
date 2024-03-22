using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Wizard : Interactable
{
    GameManager gm;
    public GameObject dialogBox;
    public UnityEngine.UI.Slider rainSlider;
    public UnityEngine.UI.Slider cloudSlider;
    public UnityEngine.UI.Slider windSlider;
    public TMP_Text rainPercentage;
    public TMP_Text cloudPercentage;
    public TMP_Text windPercentage;
    public UnityEngine.UI.Button btnRain;
    public UnityEngine.UI.Button btnCloud;
    public UnityEngine.UI.Button btnWind;
    public TMP_Text dialog;
    /// <summary>
    /// leveling up
    ///     gain experience
    ///         -   gain experience in that specific magic
    ///         
    ///     spend experience
    ///         - see wizard spend that specific when you have enough to level up
    ///
    /// 
    ///  dialog
    ///        
    ///     - wizard give you options which ones you can upgrade
    ///        - upgrade 
    ///             - buttons (wind Cloud, Rain)
    ///             - displays progress bar (currentExp/exp needed)
    ///        - leave
    ///             - leaves
    /// </summary>
    public void LevelUpSkill(SkillType chosenType)
    {
        gm.ProgressLevel(chosenType);
    }
    // Start is called before the first frame update
    void Awake()
    {
        if (objectCanvas == null)
        {
            //Find canvas object
            objectCanvas = GetComponentInChildren<Canvas>();
        }

        //set initial state
        EnableInteractable(false);
        gm = FindAnyObjectByType<GameManager>();
    }
    
    public void RunInteraction()
    {
        ///display dialog
        dialogBox.SetActive(true);
        string dialogMessage = "Welcome apprentice, which magic would you like to upgrade?";
        UpdateDialogBox();
        dialog.text = dialogMessage;
        ///get player to choose how much 
        return;
    }
    void UpdateDialogBox()
    {
        UpdatePercentage();
        UpdateProgress();
        UpdateButtonText();
    }


    void UpdatePercentage()
    {
        rainPercentage.text = gm.GetSkillProgressAmount(SkillType.Rain);
        cloudPercentage.text = gm.GetSkillProgressAmount(SkillType.Cloud).ToString();
        windPercentage.text = gm.GetSkillProgressAmount(SkillType.Wind).ToString(); 
    }
    
    void UpdateProgress()
    {
        rainSlider.value = gm.GetSkillProgressPerc(SkillType.Rain);
        cloudSlider.value = gm.GetSkillProgressPerc(SkillType.Cloud);
        windSlider.value = gm.GetSkillProgressPerc(SkillType.Wind);
    }
    void UpdateButtonText()
    {
        btnRain.GetComponentInChildren<TMP_Text>().text = "Rain - " + gm.skillLevel[(int)SkillType.Rain].ToString();
        btnCloud.GetComponentInChildren<TMP_Text>().text = "Cloud - " + gm.skillLevel[(int)SkillType.Cloud].ToString();
        btnWind.GetComponentInChildren<TMP_Text>().text = "Wind - " + gm.skillLevel[(int)SkillType.Wind].ToString();
    }
    //called when leveling up
    public void LevelUpMagic(int index)
    {
        gm.ProgressLevel((SkillType)index);
        UpdateDialogBox();
    }
    public void LeaveWizardDialog()
    {
        dialogBox.SetActive(false);
    }
}
