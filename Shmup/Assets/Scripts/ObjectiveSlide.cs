using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectiveSlide : MonoBehaviour
{
    [SerializeField] private Transform onscreenPos;
    [SerializeField] private Transform offscreenPos;

    public bool moveOnScreen = false;

    public float lerpSpeed;


    void Update()
    {
        if(moveOnScreen)
        {
            MoveOnScreen();
        }
        else
        {
            MoveOffScreen();
        }
    }


    public void Toggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!moveOnScreen)
            {
                moveOnScreen = true;
            }
            else
            {
                moveOnScreen = false;
            }
        }
    }


    private void MoveOnScreen()
    {
        transform.position = Vector3.Lerp(transform.position, onscreenPos.transform.position, lerpSpeed * Time.deltaTime);
    }

    private void MoveOffScreen()
    {
        transform.position = Vector3.Lerp(transform.position, offscreenPos.transform.position, lerpSpeed * Time.deltaTime);
    }
}
