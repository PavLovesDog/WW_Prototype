using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OverworldUIManager : MonoBehaviour
{
    GameManager gm;

    public TMP_Text RainMagicLvl; 
    public TMP_Text CloudMagicLvl;
    public TMP_Text WindMagicLvl;

    public TMP_Text Gold;

    // Start is called before the first frame update
    void Start()
    {
        if (gm == null)
        {
            //find it!
            gm = FindObjectOfType<GameManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        RainMagicLvl.text = gm.skillLevel[0].ToString();
        CloudMagicLvl.text = gm.skillLevel[1].ToString();
        WindMagicLvl.text = gm.skillLevel[2].ToString();

        Gold.text = ": " + gm.coins.ToString() + " gold";
    }
}
