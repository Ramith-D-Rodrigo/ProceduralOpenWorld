using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    KeyCode viewSwitchKey = KeyCode.V;

    public bool isThirdPerson = true;
    private bool isMoving = true;

    public Transform CurrentTransform
    {
        get
        {
            if(isThirdPerson)
            {
                return thirdPersonTransform;
            }
            else
            {
                return firstPersonTransform;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

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
        if(isThirdPerson)
        {
            thirdPersonController.enabled = true;
        }
        else
        {
            firstPersonController.enabled = true;
        }

        isMoving = true;
    }

    public void SwitchView()
    {
        StopAllMovements();

        if (isThirdPerson)
        {
            thirdPersonController.enabled = false;
            thirdPersonAnimator.enabled = false;
            thirdPersonGameObject.SetActive(false);

            firstPersonController.enabled = true;
            firstPersonAnimator.enabled = true;
            firstPersonGameObject.SetActive(true);

            firstPersonTransform.SetPositionAndRotation(thirdPersonTransform.position, thirdPersonTransform.rotation);
        }
        else
        {
            firstPersonController.enabled = false;
            firstPersonAnimator.enabled = false;
            firstPersonGameObject.SetActive(false);

            thirdPersonController.enabled = true;
            thirdPersonAnimator.enabled = true;
            thirdPersonGameObject.SetActive(true);

            thirdPersonTransform.SetPositionAndRotation(firstPersonTransform.position, firstPersonTransform.rotation);
        }

        isThirdPerson = !isThirdPerson;

        isMoving = true;
    }
}
