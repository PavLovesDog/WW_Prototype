using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG1_AudioManager : MonoBehaviour
{
    [Header("Arrow Sound Source")]
    public AudioSource AL_1;
    public AudioSource AL_2;
    public AudioSource AU_1;
    public AudioSource AU_2;
    public AudioSource AD_1;
    public AudioSource AD_2;
    public AudioSource AR_1;
    public AudioSource AR_2;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void PlayAudio(AudioSource source)
    {
        source.PlayOneShot(source.clip);
    }
}
