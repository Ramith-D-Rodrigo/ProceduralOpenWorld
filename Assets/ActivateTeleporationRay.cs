using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActivateTeleporationRay : MonoBehaviour
{

    public GameObject teleporation;

    public InputActionProperty teleportationAction;

    // Update is called once per frame
    void Update()
    {
        teleporation.SetActive(teleportationAction.action.ReadValue<float>() > 0.1f);
    }
}
