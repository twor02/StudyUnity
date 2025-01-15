using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseScreen;

    private PlayerControls playerControls;
    private bool isPaused;

    private const string MAINMENU_SCENE = "MainMenu";

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        playerControls.Player.Pause.performed += _ => PauseUnpause();
    }
    // Update is called once per frame
    void Update()
    {
       
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(MAINMENU_SCENE);
        Time.timeScale = 1f;
    }

    public void PauseUnpause()
    {
        if (isPaused)
        {
            isPaused = false;
            pauseScreen.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            isPaused = true;
            pauseScreen.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
