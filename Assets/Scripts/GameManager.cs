using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
/// <summary>
/// ----REQUIRED--
///--VARIABLES
///Track skill level
/// - Rain
/// - Cloud
/// - Wind
///Track skill experience progress
/// - Rain
/// - Cloud
/// - Wind
/// track player experience
/// 
///======================
///--FUNCTIONS
///Add experience to skill and subtract from player experience
/// progress level and reset skill experience
/// generate new skill experience goal
/// add experience to player
/// 
/// </summary>



///Enum of skills/magic
///The number value of the skilltype is used for the index in the Skill Lists.
/// Adding a new skill type is simple. Just enter a new one and the rest should be handled automatically
public enum SkillType
{
    Rain, //0
    Cloud, //1
    Wind //2
}
public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    //List of current Skill experience
    public List<float> skillExperience = new List<float>();
    //List of experience needed to level up skill
    public List<float> skillNeededExperience = new List<float>();
    //List of skill levels
    public List<int> skillLevel = new List<int>();
    private int amountOfSkills = 0; // used to track how many skills in the game
    public int coins = 0;

    public static GameManager Instance
    {
        get
        {
            if (_instance is null)
                Debug.LogError("GameManaet is NULL");
            return _instance;
        }
    }

    #region SkillExperience
    /// <summary>
    /// Add experience to specific skill
    /// </summary>
    /// <param name="skill">Which skill to gain xp in</param>
    /// <param name="experience">How much xp to add</param>
    public void AddSkillExperience(SkillType skill, float experience)
    {

        //converts the skill into an int to use as an index for the lists so the correct ones can be updated
        int index = (int)skill;
        skillExperience[index] += experience;

    }
    #region Skill
    /// <summary>
    /// progress level and reset skill experience
    /// </summary>
    /// <param name="skillType">Which skill to level up</param>
    public bool ProgressLevel(SkillType skillType)
    {
        int index = (int)skillType;
        //check that there is enough experience available
        if (skillExperience[index] >= skillNeededExperience[index])
        {
            skillLevel[index] += 1;
            skillExperience[index] -= skillNeededExperience[index];
            skillNeededExperience[index] = GenerateNewSkillTarget(skillType);
            return true;
        } else
        {
            return false;
        }
    }
    /// <summary>
    /// generate new skill experience goal
    /// </summary>
    /// <param name="skillType">Which skill to increase the xp requirement for</param>
    /// <returns></returns>
    int GenerateNewSkillTarget(SkillType skillType)
    {
        int index = (int)skillType;
        int newXpGoal = 1000 + (1000 * (skillLevel[index] - 1));
        return newXpGoal;
    }
    ///<summary>
    /// Initialise the lists and fill them
    /// </summary>
    /// <param name="amountOfSkills">How many skills in the game</param>
    void InitSkills(int amountOfSkills)
    {
        for (int i = 0; i < amountOfSkills; i++)
        {
            skillExperience.Add(0);
            skillLevel.Add(1);
            skillNeededExperience.Add(0);
            skillNeededExperience[i] = GenerateNewSkillTarget((SkillType)i);
        }
    }
    //called when leveling up
    public void LevelUpMagic(int index)
    {
        ProgressLevel((SkillType)index);
    }
    #endregion
    #region Getters and Setters
    public string GetSkillProgressAmount(SkillType skillType)
    {
        return skillExperience[(int)skillType].ToString() + "/" + skillNeededExperience[(int)skillType].ToString();
    }
    public float GetSkillProgressPerc(SkillType skillType)
    {
        return skillExperience[(int)skillType] / skillNeededExperience[(int)skillType];
    }
    public float GetSkillLvl(SkillType skill)
    {
        return skillLevel[(int)skill];
    }
    public int GetPlayerCoins()
    {
        return coins;
    }
    public void AddCoins(int amount)
    {
        coins += amount;
    }
    #endregion
    #endregion
    // Start is called before the first frame update
    void Awake()
    {
        _instance = this;
        //gets the amount of skills in the SkillType enum
        amountOfSkills = Enum.GetNames(typeof(SkillType)).Length;
        //if we ever implement saving
        string dir = "Assets/save.txt";
        //checks if the save exists
        if (File.Exists(dir))
        {
            //load in info
            FileStream f = File.Open(dir, FileMode.Open);
            print("file exists");
        }
        else
        {
            //start game from new
            InitSkills(amountOfSkills);
        }
        //makes sure this persists throughout the game
        DontDestroyOnLoad(this.gameObject);
    }
    
    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
