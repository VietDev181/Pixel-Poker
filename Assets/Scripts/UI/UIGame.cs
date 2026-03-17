using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UIGame : MonoBehaviour
{
    public Button homeButton;
    public Button pauseButton;
    public Button settingButton;
    public Button resumeButton;
    public Button resumeSettingButton;
    public Button replayButton;
    public Button replayGameOverButton;
    public Button replayWinGameButton;

    [Header("Win Result UI")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI rankText;

    [Header("Leaderboard UI")]
    public TextMeshProUGUI top1;
    public TextMeshProUGUI top2;
    public TextMeshProUGUI top3;   

    [Header("Win Text Colors - Teal Background")]
    public Color top1Color = new Color32(255, 220, 80, 255);   // vàng kim
    public Color top2Color = new Color32(245, 250, 255, 255); // trắng sáng
    public Color top3Color = new Color32(255, 165, 90, 255);  // cam đồng
    public Color normalColor = new Color32(230, 235, 240, 255);

    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject settingPanel;
    public GameObject winGamePanel;
    public GameObject leaderboardPanel;

    private void Start()
    {
        AudioManager.instance.PlayGameBGM();

        homeButton.onClick.AddListener(OnHomeGame);
        pauseButton.onClick.AddListener(OnPauseGame);
        settingButton.onClick.AddListener(OnSettingGame);
        resumeButton.onClick.AddListener(OnResumeGame);
        resumeSettingButton.onClick.AddListener(OnResumeGame);
        replayButton.onClick.AddListener(OnReplayGame);
        replayGameOverButton.onClick.AddListener(OnReplayGame);
        replayWinGameButton.onClick.AddListener(OnReplayGame);

        pausePanel.SetActive(false);
        settingPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        winGamePanel.SetActive(false);
        leaderboardPanel.SetActive(false);
    }

    private void OnHomeGame()
    {
        AudioManager.instance.PlayClickSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }

    private void OnPauseGame()
    {
        AudioManager.instance.PlayClickSFX();
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    private void OnSettingGame()
    {
        AudioManager.instance.PlayClickSFX();
        Time.timeScale = 0f;
        settingPanel.SetActive(true);
    }

    private void OnResumeGame()
    {
        AudioManager.instance.PlayClickSFX();
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        settingPanel.SetActive(false);
    }

    private void OnReplayGame()
    {
        AudioManager.instance.PlayClickSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowGameOver()
    {
        AudioManager.instance.PlayGameOverBGM();
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
    }

    public void ShowWinGame(float time, int rank)
    {
        AudioManager.instance.PlayWinGameBGM();
        Time.timeScale = 0f;

        timeText.text = $"TIME\n{FormatTime(time)}";

        Color useColor = normalColor;

        if (rank == 1)
        {
            rankText.text = "TOP 1";
            useColor = top1Color;
        }
        else if (rank == 2)
        {
            rankText.text = "TOP 2";
            useColor = top2Color;
        }
        else if (rank == 3)
        {
            rankText.text = "TOP 3";
            useColor = top3Color;
        }
        else
        {
            rankText.text = "COMPLETED";
            useColor = normalColor;
        }

        // áp màu
        rankText.color = useColor;
        timeText.color = useColor;

        // reset scale
        rankText.transform.localScale = Vector3.one;
        timeText.transform.localScale = Vector3.one;

        // hiệu ứng pop nhẹ
        DOTween.Kill(rankText.transform);
        DOTween.Kill(timeText.transform);

        rankText.transform
            .DOScale(1.2f, 0.3f)
            .SetEase(Ease.OutBack);

        timeText.transform
            .DOScale(1.1f, 0.3f)
            .SetEase(Ease.OutBack)
            .SetDelay(0.1f);

        winGamePanel.SetActive(true);
    }

    public void ShowLeaderboard()
    {
        Time.timeScale = 0f;

        var times = LeaderboardManager.GetTimes();

        // set text
        top1.text = times.Count > 0 ? $"TOP 1  {FormatTime(times[0])}" : "TOP 1  —";
        top2.text = times.Count > 1 ? $"TOP 2  {FormatTime(times[1])}" : "TOP 2  —";
        top3.text = times.Count > 2 ? $"TOP 3  {FormatTime(times[2])}" : "TOP 3  —";

        // màu rõ ràng từng hạng
        top1.color = new Color32(255, 215, 0, 255);    // 🥇 vàng
        top2.color = new Color32(255, 200, 150, 255);
        top3.color = new Color32(200, 140, 255, 255); // 🥉 tím

        leaderboardPanel.SetActive(true);

        // reset scale chữ
        top1.transform.localScale = Vector3.one;
        top2.transform.localScale = Vector3.one;
        top3.transform.localScale = Vector3.one;

        // kill tween cũ (tránh bug)
        DOTween.Kill(top1.transform);
        DOTween.Kill(top2.transform);
        DOTween.Kill(top3.transform);

        // hiệu ứng pop chữ
        top1.transform
            .DOScale(1.15f, 0.25f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
                top1.transform
                    .DOScale(1.05f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
            );

        top2.transform
            .DOScale(1.1f, 0.25f)
            .SetEase(Ease.OutBack)
            .SetDelay(0.1f);

        top3.transform
            .DOScale(1.1f, 0.25f)
            .SetEase(Ease.OutBack)
            .SetDelay(0.2f);
    }

    public void CloseLeaderboard()
    {
        Time.timeScale = 1f;
        leaderboardPanel.SetActive(false);
    }

    string FormatTime(float time)
    {
        int min = Mathf.FloorToInt(time / 60f);
        int sec = Mathf.FloorToInt(time % 60f);
        return $"{min:00}:{sec:00}";
    }
}
