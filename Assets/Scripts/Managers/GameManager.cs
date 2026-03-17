using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private UIGame uiGame;

    [Header("Timer Settings")]
    [SerializeField] private float gameTime = 300f;
    [SerializeField] private TextMeshProUGUI timeText;

    private float remainingTime;
    private bool timerRunning = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        uiGame = FindObjectOfType<UIGame>();

        remainingTime = gameTime;
        timerRunning = true;

        UpdateTimerUI();
    }

    private void Update()
    {
        if (!timerRunning) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timerRunning = false;
            GameOver();
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timeText != null)
        {
            timeText.text = FormatTime(remainingTime);
        }
    }

    string FormatTime(float time)
    {
        int min = Mathf.FloorToInt(time / 60f);
        int sec = Mathf.FloorToInt(time % 60f);
        return $"{min:00}:{sec:00}";
    }

    public void GameOver()
    {
        if (uiGame != null)
            uiGame.ShowGameOver();
    }

    public void WinGame()
    {
        timerRunning = false;

        float usedTime = gameTime - remainingTime;
        int rank = LeaderboardManager.AddTime(usedTime);

        if (uiGame != null)
            uiGame.ShowWinGame(usedTime, rank);
    }

    public bool IsTimeRemaining()
    {
        return remainingTime > 0f;
    }
}
