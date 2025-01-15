using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToAnotherWorld : MonoBehaviour
{
    [SerializeField] private GameObject interactPrompt;
  
    public void ShowInteractPrompt(bool showPrompt)
    {
        if (showPrompt == true)
        {
            interactPrompt.SetActive(true);
        }
        else
        {
            interactPrompt.SetActive(false);
        }
    }
}
