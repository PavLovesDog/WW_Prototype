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
    public float maxDensity = 0.95f;
    public float minDensity = 0;    
    [Tooltip("Control how much light is let through clouds (0 - 1)")]
    public float currentDensity = 0;
    public float densityChange = 0.05f;
    [Tooltip("Control amount of clouds in sky, smaller values = more clouds (0-1))")]
    public float currentCloudCoverage = 0.3f;
    public float cloudCoverageChange = 0.0005f;

    [Header("Transition Speeds")]
    public float densityChangeSpeed = 2;
    public float shapeChangeSpeed = 2;
    public float lightTempChangeSpeed = 2;


    [Header("Light Varibales")]
    public Light sunLight;
    public float temperatureChange = 130f;

    public Coroutine cloudDensityCoroutine;
    public Coroutine cloudShapeCoroutine;
    public Coroutine lightTemperatureCoroutine;

    private bool isCloudDensityCoroutineRunning = false;
    private bool isCloudShapeCoroutineRunning =  false;
    private bool isLightTemperatureCoroutineRunning = false;

    void Start()
    {
        //Find the cloud settings for runtime manipulation
        if (globalVolume.profile.TryGet<VolumetricClouds>(out cloudsSettings))
        {
            // Successfully retrieved the clouds settings
        }

        //begin recursivly calling Coroutines
    }

    public void InitializeSKyValues()
    {
        cloudsSettings.shapeFactor.value = 0.7f;
        cloudsSettings.densityMultiplier.value = 0.8f;
    }

    // Update is called once per frame
    void Update()
    {
        //if the cloud density and shape reversal coroutines are not running, slower make the clouds form
        if( !isCloudDensityCoroutineRunning &&
            !isCloudShapeCoroutineRunning) 
        {
            //increase density
            //cloudsSettings.densityMultiplier.value += densityChange * Time.deltaTime * densityChangeSpeed;

            //increase cloud coverage
            cloudsSettings.shapeFactor.value -= cloudCoverageChange * Time.deltaTime * shapeChangeSpeed;
        }

        UpdateLightTemperatureBasedOnCloudShape();

        //??? below could all be done in the coroutines....
        //constantly change the clouds to moor dense and thicker coverage
        //constanbtly change light temp darker (+kelvin)
        //
        //Listen for changes in values
    }

    public void TransitionCloudDensityWrapper()
    {
        StartCoroutine(TransitionCloudDensityCoroutine());
    }

    //Couroutine to change the clooud density
    IEnumerator TransitionCloudDensityCoroutine()
    {
        isCloudDensityCoroutineRunning = true;

        float shapeValueChange = 0.2f;
        float duration = 0.75f;
        float startValue = cloudsSettings.densityMultiplier.value;
        float endValue = startValue - shapeValueChange;

        float time = 0;
        while (time < duration)
        {
            // Lerp from startValue to endValue over 'duration' seconds
            cloudsSettings.densityMultiplier.value = Mathf.Lerp(startValue, endValue, time / duration);

            // Increment the time by the time passed since the last frame
            time += Time.deltaTime;

            // Yield until the next frame
            yield return null;
        }

        // Ensure the value is set to endValue at the end of the coroutine
        cloudsSettings.shapeFactor.value = endValue;

        yield return new WaitForSeconds(0.5f);
        isCloudDensityCoroutineRunning = false;
    }

    public void TransitionCloudShapeWrapper()
    {
        StartCoroutine(TransitionCloudShapeCoroutine());
    }

    //Couroutine to change the clooud shape factor
    IEnumerator TransitionCloudShapeCoroutine()
    {
        isCloudDensityCoroutineRunning = true;

        float shapeValueChange = 0.1f;
        float duration = 0.75f;
        float startValue = cloudsSettings.shapeFactor.value;
        float endValue = startValue + shapeValueChange;

        float time = 0;
        while (time < duration)
        {
            // Lerp from startValue to endValue over 'duration' seconds
            cloudsSettings.shapeFactor.value = Mathf.Lerp(startValue, endValue, time / duration);

            // Increment the time by the time passed since the last frame
            time += Time.deltaTime;

            // Yield until the next frame
            yield return null;
        }

        // Ensure the value is set to endValue at the end of the coroutine
        cloudsSettings.shapeFactor.value = endValue;

        yield return new WaitForSeconds(0.5f); 
        isCloudDensityCoroutineRunning = false;
    }


    void UpdateLightTemperatureBasedOnCloudShape()
    {
        // The cloudsSettings.shapeFactor.value ranges from 0 (more clouds) to 1.0 (less clouds)
        // light temperature range of 7500 to 10000 (<-- Chosen range)
        float shapeFactor = cloudsSettings.shapeFactor.value;

        // Map the shapeFactor to the light temperature range
        // 0.6f is where the cloud cover thickens. so here we want it darker
        sunLight.colorTemperature = MapRange(shapeFactor, 0.6f, 1.0f, 10000, 7500);
    }

    // Helper function to map a value from one range to another
    float MapRange(float value, float fromSource, float toSource, float fromTarget, float toTarget)
    {
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }

    ////Couroutine to change the Light temp
    //IEnumerator TransitionLigthTemperature()
    //{
    //    isCloudDensityCoroutineRunning = true;
    //    // Coroutine logic here
    //    if (sunLight != null)
    //    {
    //        sunLight.colorTemperature += temperatureChange;
    //        // Clamp the temperature to HDRP's allowed range if necessary
    //        sunLight.colorTemperature = Mathf.Clamp(sunLight.colorTemperature, 6000, 20000);
    //    }
    //    yield return null; // Placeholder for actual coroutine work
    //    isCloudDensityCoroutineRunning = false;
    //}
    //Have all coroutines assigned to public coroutiner variables,
    //and ONLY have one for each variable running at a time.
    // i.e. Stop relevent coroutine to start the new change
}
