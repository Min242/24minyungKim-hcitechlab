using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nextTrialFoot : MonoBehaviour
{
    [HideInInspector] public bool isNext = false;
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.LeftAlt))
        //{
        //    Debug.Log("O");
        //}
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isNext)
        {
            isNext = true;
        }
        else if (!Input.GetKeyDown(KeyCode.LeftAlt) && isNext)
        {
            isNext = false;
        }
    }
}
