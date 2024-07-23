using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeFlyController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 5.0f;
    // Start is called before the first frame update
    bool isManipulating = false;

    void Start()
    {
        isManipulating = false;
        //hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            ToggleManipulation();
        }

        if (isManipulating)
        {
            return;
        }

        if (Input.GetKey(KeyCode.Mouse1)) //holding right mouse button
        {
            transform.Rotate(Vector3.right, -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime);
        }
        //using else if to avoid rotating on both axes at the same time
        else if(Input.GetKey(KeyCode.Mouse0)) //holding left mouse button
        {
            transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.forward * Time.deltaTime * moveSpeed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.forward * Time.deltaTime * moveSpeed;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.right * Time.deltaTime * moveSpeed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * Time.deltaTime * moveSpeed;
            }
        }

    }

    private void ToggleManipulation()
    {
        if (isManipulating)
        {
            Cursor.lockState = CursorLockMode.Locked;
            isManipulating = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            isManipulating = true;
        }
    }
}
