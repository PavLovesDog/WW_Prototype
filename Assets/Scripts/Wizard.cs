using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Wizard : Interactable
{
    GameManager gm;

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
    

    // Start is called before the first frame update
    void Awake()
    {
        gm = FindAnyObjectByType<GameManager>();
    }
    
    public void RunInteraction()
    {
        ///display dialog
        
        string dialogMessage = "";

        TMP_Text dialogBox = objectCanvas.GetComponentInChildren<TMP_Text>();

        ///get player to choose how much 
        return;
    }

    private void LevelUp(SkillType chosenType)
    {
        switch (chosenType)
        {
            case SkillType.Rain:
                break;
            case SkillType.Wind:
                break;
            case SkillType.Cloud:
                break;
            default:
                break;
        }
    }
}
