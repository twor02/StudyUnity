using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    private const string MAINMENU_SCENE = "MainMenu";

    public void MainMenu()
    {
        SceneManager.LoadScene(MAINMENU_SCENE);
        Time.timeScale = 1f;
    }

    public void LeaveGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
