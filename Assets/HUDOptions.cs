using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDOptions : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    Slider scaleSlider;
    [SerializeField]
    TextMeshProUGUI scaleText;

    [SerializeField]
    Slider depthSlider;
    [SerializeField]
    TextMeshProUGUI depthText;

    [SerializeField]
    Slider octaveCountSlider;
    [SerializeField]
    TextMeshProUGUI octaveCountText;


    [SerializeField]
    Slider gainSlider;
    [SerializeField]
    TextMeshProUGUI gainText;

    [SerializeField]
    Slider lacunaritySlider;
    [SerializeField]
    TextMeshProUGUI lacunarityText;

    [SerializeField]
    Slider seedSlider;
    [SerializeField]
    TextMeshProUGUI seedText;

    [SerializeField]
    Slider xOffsetSlider;
    [SerializeField]
    TextMeshProUGUI xOffsetText;

    [SerializeField]
    Slider yOffsetSlider;
    [SerializeField]
    TextMeshProUGUI yOffsetText;

    [SerializeField]
    Slider amplitudeSlider;
    [SerializeField]
    TextMeshProUGUI amplitudeText;

    [SerializeField]
    Slider frequencySlider;
    [SerializeField]
    TextMeshProUGUI frequencyText;

    MapGeneratingValues originalValues;

    ProceduralMeshTerrain proceduralMeshTerrain;

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }

    void Start()
    {
        proceduralMeshTerrain = FindObjectOfType<ProceduralMeshTerrain>();      

        originalValues = new MapGeneratingValues(proceduralMeshTerrain.depth, 
            proceduralMeshTerrain.scale,
            proceduralMeshTerrain.startFrequency,
            proceduralMeshTerrain.startAmplitude,
            proceduralMeshTerrain.gain, 
            proceduralMeshTerrain.lacunarity, 
            proceduralMeshTerrain.octaveCount, 
            proceduralMeshTerrain.xOffSet, 
            proceduralMeshTerrain.yOffSet, 
            proceduralMeshTerrain.seed
            );

        ResetSliderValues();
    }

    public void ResetSliderValues()
    {
        scaleSlider.value = proceduralMeshTerrain.scale;
        depthSlider.value = proceduralMeshTerrain.depth;
        octaveCountSlider.value = proceduralMeshTerrain.octaveCount;
        gainSlider.value = proceduralMeshTerrain.gain;
        lacunaritySlider.value = proceduralMeshTerrain.lacunarity;
        seedSlider.value = proceduralMeshTerrain.seed;
        xOffsetSlider.value = proceduralMeshTerrain.xOffSet;
        yOffsetSlider.value = proceduralMeshTerrain.yOffSet;
        amplitudeSlider.value = proceduralMeshTerrain.startAmplitude;
        frequencySlider.value = proceduralMeshTerrain.startFrequency;

        scaleText.text = proceduralMeshTerrain.scale.ToString();
        depthText.text = proceduralMeshTerrain.depth.ToString();
        octaveCountText.text = proceduralMeshTerrain.octaveCount.ToString();
        gainText.text = proceduralMeshTerrain.gain.ToString();
        lacunarityText.text = proceduralMeshTerrain.lacunarity.ToString();
        seedText.text = proceduralMeshTerrain.seed.ToString();
        xOffsetText.text = proceduralMeshTerrain.xOffSet.ToString();
        yOffsetText.text = proceduralMeshTerrain.yOffSet.ToString();
        amplitudeText.text = proceduralMeshTerrain.startAmplitude.ToString();
        frequencyText.text = proceduralMeshTerrain.startFrequency.ToString();
    }

    public void ApplyChanges()
    {
        proceduralMeshTerrain.scale = scaleSlider.value;
        proceduralMeshTerrain.depth = (int)depthSlider.value;
        proceduralMeshTerrain.startFrequency = frequencySlider.value;
        proceduralMeshTerrain.startAmplitude = amplitudeSlider.value;
        proceduralMeshTerrain.seed = (int)seedSlider.value;
        proceduralMeshTerrain.octaveCount = (int)octaveCountSlider.value;
        proceduralMeshTerrain.lacunarity = lacunaritySlider.value;
        proceduralMeshTerrain.gain = gainSlider.value;
        proceduralMeshTerrain.xOffSet = xOffsetSlider.value;
        proceduralMeshTerrain.yOffSet = yOffsetSlider.value;

        proceduralMeshTerrain.ProcessValueChange();
    }



    //create setters for the sliders
    public void SetScaleValue(float value)
    {
        scaleText.text = scaleSlider.value.ToString();
    }
    public void SetAmplitudeValue(float value) {
        amplitudeText.text = amplitudeSlider.value.ToString();
    }

    public void SetFrequencyValue(float value) {
        frequencyText.text = frequencySlider.value.ToString();
    }

    public void SetDepthValue(int value)
    {
        depthText.text = depthSlider.value.ToString();
    }

    public void SetOctaveCountValue(int value)
    {
        octaveCountText.text = octaveCountSlider.value.ToString();
    }

    public void SetGainValue(float value)
    {
        gainText.text = gainSlider.value.ToString();
    }

    public void SetLacunarityValue(float value)
    {
        lacunarityText.text = lacunaritySlider.value.ToString();
    }

    public void SetSeedValue(int value)
    {
        seedText.text = seedSlider.value.ToString();
    }

    public void SetXOffsetValue(float value)
    {
        xOffsetText.text = xOffsetSlider.value.ToString();
    }

    public void SetYOffsetValue(float value)
    {
        yOffsetText.text = yOffsetSlider.value.ToString();
    }
}
