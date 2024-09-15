using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class VRHandler : MonoBehaviour
{
    private XRManagerSettings xrManagerSettings;
    private bool isVREnabled = false;

    public Player player;

    void Start()
    {
        xrManagerSettings = XRGeneralSettings.Instance.Manager;
        TurnOffVRSettings();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            if(isVREnabled)
            {
                DisableVR();
            }
            else
            {
                EnableVR();
            }
        }
    }

    void DisableVR()
    {
        TurnOffVRSettings();
        player.DisableVR();
    }

    void TurnOffVRSettings()
    {
        xrManagerSettings.StopSubsystems();
        xrManagerSettings.DeinitializeLoader();
        isVREnabled = false;
    }

    void EnableVR()
    {
        xrManagerSettings.InitializeLoaderSync();
        xrManagerSettings.StartSubsystems();
        isVREnabled = true;
        player.EnableVR();
    }
}
