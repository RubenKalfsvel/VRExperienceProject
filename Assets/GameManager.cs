using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject deathScreen;
    public GameObject victoryScreen;
    public TextMeshProUGUI victoryScreenScoreText;
    public Transform playerHead;
    public ScoreManager scoreManager;

    [SerializeField] private float restartDelay = 5f;

    public void ShowDeathScreen()
    {
        deathScreen.SetActive(true);
        PositionScreen(deathScreen);
        Time.timeScale = 0f;
        StartCoroutine(RestartAfterDelay());
    }

    public void ShowVictoryScreen()
    {
        Time.timeScale = 0f;
        victoryScreen.SetActive(true);
        int score = scoreManager.GetScore();
        PositionScreen(victoryScreen);
        Debug.Log("Victory screen showing with score: " + score);
        victoryScreenScoreText.text = score.ToString();
        StartCoroutine(RestartAfterDelay());
    }

    void PositionScreen(GameObject screen)
    {
        screen.transform.position = playerHead.position + playerHead.forward * 2f;
        screen.transform.rotation = Quaternion.LookRotation(playerHead.forward);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScreen");
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay);
        RestartLevel();
    }
}