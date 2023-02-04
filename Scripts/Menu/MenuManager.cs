using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private int menuSceneIndex = 0;

    [Header("Input")]
    [SerializeField] private KeyCode menuKey = KeyCode.Escape;

    [Header("References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject endMenu;

    private bool menuOn = false;
    private bool gameEnded = false;
    private bool dead = false;

    private void Update()
    {
        if (Input.GetKeyDown(menuKey)) TogglePauseMenu();
    }

    private void TogglePauseMenu()
    {
        if (gameEnded) return;
        if (dead) return;

        menuOn = !menuOn;

        if (menuOn)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ShowDeathMenu()
    {
        if (gameEnded) return;
        dead = true;

        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowEndMenu()
    {
        gameEnded = true;

        pauseMenu.SetActive(false);
        endMenu.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Next()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Restart()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        SceneManager.LoadScene(menuSceneIndex);
    }

}
