using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : Interactable
{
    GameManager gm;

    /// <summary>
    /// Need:
    /// dialog with wizard
    /// handle adding experience
    /// handle unlocking new skills
    /// 
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

        ///give choice of what to upgrade
        
        ///
        return;
    }
    /// <summary>
    /// adds the experience amount to the specified skill
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="experience"></param>
    /// <returns></returns>
    public bool AddExperienceSkill(SkillType skill, int experience)
    {
        gm.AddSkillExperience(skill,experience);
        return true;
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
