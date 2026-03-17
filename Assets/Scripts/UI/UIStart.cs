using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class UIStart : MonoBehaviour
{
    [Header("Animation UI")]
    public RectTransform titleImage;
    public RectTransform titleText;
    public float moveSpeed = 800f;

    private bool hasStarted = false;
    private bool hasPressed = false;
    private bool isSettingOpen = false;

    [Header("UI Elements")]
    public GameObject settingPanel;
    public Button playButton;

    private Sequence introSequence;
    private Tween pulseTweenImage;
    private Tween pulseTweenText;

    private void Start()
    {
        AudioManager.instance?.PlayMainMenuBGM();

        settingPanel.SetActive(false);
        playButton.onClick.AddListener(OnPlayButtonClicked);

        AnimateIntro();
    }

    private void AnimateIntro()
    {
        CanvasGroup imageGroup = titleImage.GetComponent<CanvasGroup>() 
            ?? titleImage.gameObject.AddComponent<CanvasGroup>();
        CanvasGroup textGroup = titleText.GetComponent<CanvasGroup>() 
            ?? titleText.gameObject.AddComponent<CanvasGroup>();

        imageGroup.alpha = 0;
        textGroup.alpha = 0;

        Vector2 imgStartPos = titleImage.anchoredPosition + new Vector2(0, 260f);
        Vector2 textStartPos = titleText.anchoredPosition - new Vector2(0, 180f);

        titleImage.anchoredPosition = imgStartPos;
        titleText.anchoredPosition = textStartPos;

        titleImage.localScale = Vector3.one * 0.6f;
        titleText.localScale = Vector3.one * 0.6f;

        introSequence = DOTween.Sequence();

        introSequence
            // Logo image
            .Append(imageGroup.DOFade(1f, 0.6f))
            .Join(titleImage.DOAnchorPosY(imgStartPos.y - 260f, 1f)
                .SetEase(Ease.OutExpo))
            .Join(titleImage.DOScale(1.1f, 0.8f)
                .SetEase(Ease.OutBack))
            .Append(titleImage.DOScale(1f, 0.25f).SetEase(Ease.InOutSine))

            // Text
            .AppendInterval(0.15f)
            .Append(textGroup.DOFade(1f, 0.5f))
            .Join(titleText.DOAnchorPosY(textStartPos.y + 180f, 0.9f)
                .SetEase(Ease.OutExpo))
            .Join(titleText.DOScale(1.05f, 0.7f)
                .SetEase(Ease.OutBack))
            .Append(titleText.DOScale(1f, 0.25f))

            .AppendCallback(() =>
            {
                hasStarted = true;
                StartIdleAnimation();
            });
    }

    private void StartIdleAnimation()
    {
        pulseTweenImage = titleImage
            .DOScale(1.04f, 2.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        pulseTweenText = titleText
            .DOScale(1.02f, 1.8f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Float nhẹ
        titleImage.DOAnchorPosY(titleImage.anchoredPosition.y + 12f, 2.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        titleText.DOAnchorPosY(titleText.anchoredPosition.y - 8f, 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Glow quét sáng
        Image img = titleImage.GetComponent<Image>();
        if (img != null)
        {
            img.DOColor(new Color(1f, 0.92f, 0.85f), 1.8f)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnPlayButtonClicked()
    {
        if (hasStarted && !hasPressed && !isSettingOpen)
        {
            hasPressed = true;
            StopAllTweens();
            AnimateExit();
        }
    }

    private void AnimateExit()
    {
        Sequence exitSeq = DOTween.Sequence();

        // Hiệu ứng thu nhỏ và fade mượt
        exitSeq.Append(titleImage.DOScale(0.8f, 0.5f).SetEase(Ease.InOutSine))
               .Join(titleText.DOScale(0.8f, 0.5f).SetEase(Ease.InOutSine))
               .Join(titleImage.GetComponent<CanvasGroup>().DOFade(0, 0.5f))
               .Join(titleText.GetComponent<CanvasGroup>().DOFade(0, 0.5f))
               .AppendInterval(0.2f)
               .OnComplete(() =>
               {
                   // Tạo hiệu ứng fade đen trước khi vào game
                   var fadePanel = new GameObject("FadePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                   fadePanel.transform.SetParent(transform.parent, false);
                   var img = fadePanel.GetComponent<Image>();
                   img.color = new Color(0, 0, 0, 0);
                   img.rectTransform.anchorMin = Vector2.zero;
                   img.rectTransform.anchorMax = Vector2.one;
                   img.rectTransform.offsetMin = Vector2.zero;
                   img.rectTransform.offsetMax = Vector2.zero;

                   img.DOFade(1f, 0.6f).OnComplete(() =>
                   {
                       SceneManager.LoadScene("GameScene");
                   });
               });
    }

    private void StopAllTweens()
    {
        pulseTweenImage?.Kill();
        pulseTweenText?.Kill();
        introSequence?.Kill();
    }
}
