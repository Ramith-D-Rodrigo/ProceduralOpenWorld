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

    [SerializeField]
    Slider treeScaleSlider;
    [SerializeField]
    TextMeshProUGUI treeScaleText;

    [SerializeField]
    Slider treeDensitySlider;
    [SerializeField]
    TextMeshProUGUI treeDensityText;

    MapGeneratingValues originalValues;

    ProceduralMeshTerrain proceduralMeshTerrain;

    [SerializeField]
    Button applyButton;
    [SerializeField]
    Button resetButton;

    [SerializeField]
    TextMeshProUGUI switchSceneButtonText;

    int sceneIndex;

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
            proceduralMeshTerrain.seed,
            proceduralMeshTerrain.treeDensity,
            proceduralMeshTerrain.treeScale
            );

        ResetSliderValues();

        if(!proceduralMeshTerrain.useThreading)
        {
            //dont lock the cursor
            Cursor.lockState = CursorLockMode.None;

            //deactivate the buttons
            applyButton.gameObject.SetActive(false);
            resetButton.gameObject.SetActive(false);

            //display the tree density and tree scale sliders
            treeDensitySlider.transform.parent.gameObject.SetActive(true);
            treeScaleSlider.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            //lock the cursor
            Cursor.lockState = CursorLockMode.Locked;

            //deactivate the buttons
            applyButton.gameObject.SetActive(true);
            resetButton.gameObject.SetActive(true);

            //display the tree density and tree scale sliders
            treeDensitySlider.transform.parent.gameObject.SetActive(false);
            treeScaleSlider.transform.parent.gameObject.SetActive(false);

            this.gameObject.SetActive(false);
        }

        sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        if(sceneIndex == 0)
        {
            switchSceneButtonText.text = "Switch to Single Terrain";
        }
        else
        {
            switchSceneButtonText.text = "Switch to Endless Terrain";
        }

    }

    public void ResetSliderValues()
    {
        scaleSlider.value = originalValues.scale;
        depthSlider.value = originalValues.depth;
        octaveCountSlider.value = originalValues.octaveCount;
        gainSlider.value = originalValues.gain;
        lacunaritySlider.value = originalValues.lacunarity;
        seedSlider.value = originalValues.seed;
        xOffsetSlider.value = originalValues.xOffSet;
        yOffsetSlider.value = originalValues.yOffSet;
        amplitudeSlider.value = originalValues.startAmplitude;
        frequencySlider.value = originalValues.startFrequency;
        treeDensitySlider.value = originalValues.treeDensity;
        treeScaleSlider.value = originalValues.treeScale;


        scaleText.text = originalValues.scale.ToString();
        depthText.text = originalValues.depth.ToString();
        octaveCountText.text = originalValues.octaveCount.ToString();
        gainText.text = originalValues.gain.ToString();
        lacunarityText.text = originalValues.lacunarity.ToString();
        seedText.text = originalValues.seed.ToString();
        xOffsetText.text = originalValues.xOffSet.ToString();
        yOffsetText.text = originalValues.yOffSet.ToString();
        amplitudeText.text = originalValues.startAmplitude.ToString();
        frequencyText.text = originalValues.startFrequency.ToString();
        treeDensityText.text = originalValues.treeDensity.ToString();
        treeScaleText.text = originalValues.treeScale.ToString();
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
        proceduralMeshTerrain.treeDensity = treeDensitySlider.value;
        proceduralMeshTerrain.treeScale = treeScaleSlider.value;

        proceduralMeshTerrain.ProcessValueChange();
    }

    public void ResetChanges()
    {
        ResetSliderValues();
        ApplyChanges();
    }



    //create setters for the sliders
    public void SetScaleValue(float value)
    {
        scaleText.text = scaleSlider.value.ToString("F2");
        proceduralMeshTerrain.scale = scaleSlider.value;
    }
    public void SetAmplitudeValue(float value) {
        amplitudeText.text = amplitudeSlider.value.ToString("F2");
        proceduralMeshTerrain.startAmplitude = amplitudeSlider.value;
    }

    public void SetFrequencyValue(float value) {
        frequencyText.text = frequencySlider.value.ToString("F2");
        proceduralMeshTerrain.startFrequency = frequencySlider.value;
    }

    public void SetDepthValue(int value)
    {
        depthText.text = depthSlider.value.ToString();
        proceduralMeshTerrain.depth = (int)depthSlider.value;
    }

    public void SetOctaveCountValue(int value)
    {
        octaveCountText.text = octaveCountSlider.value.ToString();
        proceduralMeshTerrain.octaveCount = (int)octaveCountSlider.value;
    }

    public void SetGainValue(float value)
    {
        gainText.text = gainSlider.value.ToString("F2");
        proceduralMeshTerrain.gain = gainSlider.value;
    }

    public void SetLacunarityValue(float value)
    {
        lacunarityText.text = lacunaritySlider.value.ToString("F2");
        proceduralMeshTerrain.lacunarity = lacunaritySlider.value;
    }

    public void SetSeedValue(int value)
    {
        seedText.text = seedSlider.value.ToString();
        proceduralMeshTerrain.seed = (int)seedSlider.value;
    }

    public void SetXOffsetValue(float value)
    {
        xOffsetText.text = xOffsetSlider.value.ToString("F2");
        proceduralMeshTerrain.xOffSet = xOffsetSlider.value;
    }

    public void SetYOffsetValue(float value)
    {
        yOffsetText.text = yOffsetSlider.value.ToString("F2");
        proceduralMeshTerrain.yOffSet = yOffsetSlider.value;
    }

    public void SetTreeDensityValue(float value)
    {
        treeDensityText.text = treeDensitySlider.value.ToString("F2");
        proceduralMeshTerrain.treeDensity = treeDensitySlider.value;
    }

    public void SetTreeScaleValue(float value)
    {
        treeScaleText.text = treeScaleSlider.value.ToString("F2");
        proceduralMeshTerrain.treeScale = treeScaleSlider.value;
    }

    public void SwitchScene()
    {
        //get the scene index
        if(sceneIndex == 0)
        {
            //load scene 1
            StartCoroutine(LoadSceneAsync(1));
        }
        else
        {
            //load scene 0
            StartCoroutine(LoadSceneAsync(0));
        }
    }

    IEnumerator LoadSceneAsync(int sceneIndex)
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
