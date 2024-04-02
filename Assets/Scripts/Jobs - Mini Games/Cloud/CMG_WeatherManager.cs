using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class CMG_WeatherManager : MonoBehaviour
{
    [Header("Cloud Variables")]
    public Volume globalVolume;
    private VolumetricClouds cloudsSettings;
    public float maxDensity = 1;
    [Tooltip("Control how much light is let through clouds (0 - 1)")]
    public float currentDensity = 0;
    public float densityChange = 0.005f;
    public float minCloudCoverage = 0;
    [Tooltip("Control amount of clouds in sky, smaller values = more clouds (0-1))")]
    public float currentCloudCoverage = 1;
    public float cloudCoverageChange = 0.05f;

    [Header("Light Varibales")]
    public Light sunLight;
    public float temperatureChange = 130f;

    public Coroutine cloudDensityCoroutine;
    public Coroutine cloudShapeCoroutine;
    public Coroutine lightTemperatureCoroutine;

    void Start()
    {
        //Find the cloud settings for runtime manipulation
        if (globalVolume.profile.TryGet<VolumetricClouds>(out cloudsSettings))
        {
            // Successfully retrieved the clouds settings
        }

        //begin recursivly calling Coroutines
    }

    // Update is called once per frame
    void Update()
    {
        //??? below could all be done in the coroutines....
        //constantly change the clouds to moor dense and thicker coverage
        //constanbtly change light temp darker (+kelvin)
        //
        //Listen for changes in values
    }

    //Couroutine to change the clooud density

    //Couroutine to change the clooud shape factor

    //Couroutine to change the Light temp

    //Have all coroutines assigned to public coroutiner variables,
    //and ONLY have one for each variable running at a time.
    // i.e. Stop relevent coroutine to start the new change
}
