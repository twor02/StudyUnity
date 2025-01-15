using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string OVERWORLD_SCENE = "OverworldScene";

    private PartyManager partyManager;

    void Start()
    {
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
    }

    public void StartGame()
    {
        partyManager.Awake();
        SceneManager.LoadScene(OVERWORLD_SCENE);
       
    }

    public void LeaveGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
