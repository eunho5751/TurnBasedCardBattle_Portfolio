using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class HudView : MonoBehaviour
{
    [SerializeField]
    private RectTransform _playerTurnBanner;
    [SerializeField]
    private RectTransform _enemyTurnBanner;
    [SerializeField]
    private GameObject _gameOverPanel;
    [SerializeField]
    private GameObject _gameOverWin;
    [SerializeField]
    private GameObject _gameOverLose;

    private void Awake()
    {
        _playerTurnBanner.gameObject.SetActive(false);
        _enemyTurnBanner.gameObject.SetActive(false);

        _gameOverPanel.SetActive(false);
        _gameOverWin.SetActive(false);
        _gameOverLose.SetActive(false);
    }

    public void ShowGameOver(PlayerSide winner)
    {
        bool isPlayerWin = winner == PlayerSide.Player;
        _gameOverPanel.SetActive(true);
        _gameOverWin.SetActive(isPlayerWin);
        _gameOverLose.SetActive(!isPlayerWin);
    }

    public async UniTask PlayTurnBannerAsync(PlayerSide side, CancellationToken token)
    {
        bool isPlayer = side == PlayerSide.Player;
        RectTransform rectTranform = isPlayer ? _playerTurnBanner : _enemyTurnBanner;
        CanvasGroup canvasGroup = rectTranform.GetComponent<CanvasGroup>();
        Vector2 centerPosition = isPlayer ? _playerTurnBanner.anchoredPosition : _enemyTurnBanner.anchoredPosition;
        Vector2 startPosition = centerPosition + Vector2.down * 9f;

        _playerTurnBanner.gameObject.SetActive(isPlayer);
        _enemyTurnBanner.gameObject.SetActive(!isPlayer);

        rectTranform.anchoredPosition = startPosition;
        canvasGroup.alpha = 0f;

        await TweenBannerAsync(rectTranform, canvasGroup, startPosition, centerPosition, 0f, 1f, destroyCancellationToken);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        await TweenBannerAsync(rectTranform, canvasGroup, centerPosition, startPosition, 1f, 0f, destroyCancellationToken);

        rectTranform.gameObject.SetActive(false);
    }

    private async UniTask TweenBannerAsync(RectTransform rectTransform, CanvasGroup canvasGroup, Vector2 fromPosition, Vector2 toPosition, float fromAlpha, float toAlpha, CancellationToken token)
    {
        float moveDuration = 0.35f;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(fromPosition, toPosition, t);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            await UniTask.Yield(token);
        }
        rectTransform.anchoredPosition = toPosition;
        canvasGroup.alpha = toAlpha;
    }
}
