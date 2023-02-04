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

    private void Update()
    {
        if (Input.GetKeyDown(menuKey)) TogglePauseMenu();
    }

    private void TogglePauseMenu()
    {
        if (gameEnded) return;

        menuOn = !menuOn;

        if (menuOn)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void ShowEndMenu()
    {
        pauseMenu.SetActive(false);
        endMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Next()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        SceneManager.LoadScene(menuSceneIndex);
    }

}
