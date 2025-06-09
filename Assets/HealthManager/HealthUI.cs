using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthUI : MonoBehaviour
{


    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string scorePrefix = "Score: ";
    [SerializeField] public PlayerHealth playerHealth;

    private int lastScore = -1;

    void Start()
    {

        UpdateScoreDisplay();

    }
    void Update()
    {

        if (playerHealth != null)
        {
            int currentScore = playerHealth.GetcurrentHealth();
            if (currentScore != lastScore)
            {
                UpdateScoreDisplay();
                lastScore = currentScore;
            }
        }

    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null && playerHealth != null)
        {
            scoreText.text = scorePrefix + playerHealth.GetcurrentHealth().ToString();
        }
    }
}
