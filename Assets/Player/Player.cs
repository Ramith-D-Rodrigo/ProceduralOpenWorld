using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Player : MonoBehaviour
{
    public ThirdPersonController thirdPersonController;
    public Animator thirdPersonAnimator;
    public Transform thirdPersonTransform;
    public GameObject thirdPersonGameObject;

    public FirstPersonController firstPersonController;
    public Animator firstPersonAnimator;
    public Transform firstPersonTransform;
    public GameObject firstPersonGameObject;
    public GameObject firstPersonCamera;
    public Transform firstPersonCameraRoot;

    public GameObject XROrigin;
    public GameObject XRCameraOffset;
    public GameObject interactionManager;
    public GameObject playerVR;
    public Transform playerVRTransform;
    public Transform playerVRCameraPlacement;
    private Vector3 XROriginPlacementOffset;
    public VRHandler VRHandler;

    KeyCode viewSwitchKey = KeyCode.V;

    public enum PlayerView
    {
        ThirdPerson,
        FirstPerson,
        VR
    }

    public PlayerView currentView;
    private PlayerView viewBeforeVR;
    private bool isMoving = true;

    public Transform CurrentTransform
    {
        get
        {
            if(currentView == PlayerView.ThirdPerson)
            {
                return thirdPersonTransform;
            }
            else if(currentView == PlayerView.FirstPerson)
            {
                return firstPersonTransform;
            }
            else
            {
                return playerVRTransform;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        thirdPersonController.enabled = true;
        firstPersonController.enabled = false;

        firstPersonGameObject.SetActive(false);
        playerVR.SetActive(false);

        XRCameraOffset.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(viewSwitchKey) && isMoving)
        {
            SwitchView();
        }
    }

    public void StopAllMovements()
    {

        thirdPersonController.StopAllCoroutines();
        thirdPersonController.enabled = false;
        firstPersonController.StopAllCoroutines();
        firstPersonController.enabled = false;

        thirdPersonAnimator.SetFloat("Speed", 0.0f);
        firstPersonAnimator.SetFloat("Speed", 0.0f);

        isMoving = false;
    }

    public void EnableAllMovements()
    {
        if(currentView == PlayerView.ThirdPerson)
        {
            thirdPersonController.enabled = true;
        }
        else if(currentView == PlayerView.FirstPerson)
        {
            firstPersonController.enabled = true;
        }

        isMoving = true;
    }

    public void SwitchView()
    {
        StopAllMovements();

        if (currentView == PlayerView.ThirdPerson)
        {
            thirdPersonController.enabled = false;
            thirdPersonAnimator.enabled = false;
            thirdPersonGameObject.SetActive(false);

            firstPersonController.enabled = true;
            firstPersonAnimator.enabled = true;
            firstPersonGameObject.SetActive(true);

            firstPersonTransform.SetPositionAndRotation(thirdPersonTransform.position, thirdPersonTransform.rotation);

            currentView = PlayerView.FirstPerson;
        }
        else if(currentView == PlayerView.FirstPerson)
        {
            firstPersonController.enabled = false;
            firstPersonAnimator.enabled = false;
            firstPersonGameObject.SetActive(false);

            thirdPersonController.enabled = true;
            thirdPersonAnimator.enabled = true;
            thirdPersonGameObject.SetActive(true);

            thirdPersonTransform.SetPositionAndRotation(firstPersonTransform.position, firstPersonTransform.rotation);

            currentView = PlayerView.ThirdPerson;
        }

        isMoving = true;
    }

    public void EnableVR()
    {
        //disable all movements
        StopAllMovements();

        //disable all third person components
        thirdPersonController.enabled = false;
        thirdPersonAnimator.enabled = false;
        thirdPersonGameObject.SetActive(false);

        //disable the following first person components
        firstPersonController.enabled = false;
        firstPersonAnimator.enabled = false;
        firstPersonGameObject.SetActive(false);

        //enable VR components
        XROrigin.transform.position = CurrentTransform.position;
        XRCameraOffset.SetActive(true);
        XROrigin.SetActive(true);
        playerVR.SetActive(true);

        viewBeforeVR = currentView;
        currentView = PlayerView.VR;
    }

    public void DisableVR()
    {
        //disable VR components
        XRCameraOffset.SetActive(false);
        XROrigin.SetActive(false);
        playerVR.SetActive(false);

        //enable the following components based on the view before VR
        if(viewBeforeVR == PlayerView.ThirdPerson)
        {
            thirdPersonController.enabled = true;
            thirdPersonAnimator.enabled = true;
            thirdPersonGameObject.SetActive(true);

            thirdPersonTransform.SetPositionAndRotation(playerVRTransform.position, playerVRTransform.rotation);
        }
        else if(viewBeforeVR == PlayerView.FirstPerson)
        {
            firstPersonController.enabled = true;
            firstPersonAnimator.enabled = true;
            firstPersonGameObject.SetActive(true);

            firstPersonTransform.SetPositionAndRotation(playerVRTransform.position, playerVRTransform.rotation);
        }

        currentView = viewBeforeVR;
    }
}
