using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;

public class CardView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _nameText;
    [SerializeField]
    private TextMeshProUGUI _hpText;
    [SerializeField]
    private SpriteRenderer _cardRenderer;
    [SerializeField]
    private GameObject _frontFace;
    [SerializeField]
    private GameObject _backFace;
    [SerializeField]
    private Collider _collider;
    [SerializeField]
    private DOTweenAnimation _damagedAnim;

    [Space(5)]

    [SerializeField]
    private Button _infoButton;
    [SerializeField]
    private GameObject _infoPopup;
    [SerializeField]
    private TextMeshProUGUI _descText;

    private CancellationTokenSource _outlineCts;
    private CancellationTokenSource _liftCts;

    public void Initialize(CardInstance card)
    {
        var cardData = card.Data;
        _nameText.text = cardData.CardName;
        _descText.text = cardData.Description;
        _cardRenderer.sprite = cardData.CardSprite;

        Card = card;
        SetFace(false);
        SetPickable(false);

        _infoButton.onClick.AddListener(() => _infoPopup.SetActive(!_infoPopup.activeSelf));
        _infoPopup.SetActive(false);

        OnHpChanged(Card.CurrentHp);
        Card.HpChanged += OnHpChanged;
    }

    private void OnDestroy()
    {
        if (Card != null)
            Card.HpChanged -= OnHpChanged;

        _outlineCts?.Cancel();
        _outlineCts?.Dispose();
        _liftCts?.Cancel();
        _liftCts?.Dispose();
    }

    public void SetPickable(bool isPickable)
    {
        _collider.enabled = isPickable;
    }

    public void SetPickedUp(bool isPickedUp)
    {
        _liftCts?.Cancel();
        _liftCts?.Dispose();
        _liftCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        float targetY = isPickedUp ? 0.55f : 0f;
        PickUpLiftAsync(targetY, _liftCts.Token).Forget();

        if (isPickedUp)
            SFXManager.Instance.Play(SFXType.PickUpCard);
    }

    public async UniTask RevealAsync(float duration, CancellationToken token)
    {
        Vector3 baseScale = transform.localScale;
        float half = Mathf.Max(duration * 0.5f, 0.0001f);

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            ApplyScaleX(baseScale, Mathf.Lerp(1f, 0f, elapsed / half));
            await UniTask.Yield(token);
        }
        ApplyScaleX(baseScale, 0f);
        SetFace(true);
        
        transform.localScale = (Card.OwnerSide == PlayerSide.Player) ? Vector3.one : new Vector3(-1f, -1f, 1f);
        baseScale = transform.localScale;

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            ApplyScaleX(baseScale, Mathf.Lerp(0f, 1f, elapsed / half));
            await UniTask.Yield(token);
        }
        ApplyScaleX(baseScale, 1f);
    }

    public async UniTask TakeDamageAsync(VFXType attackVfxType, SFXType sfxType, CancellationToken token)
    {
        _damagedAnim.DORestart();

        _hpText.DOKill(complete: true);
        Color originalColor = _hpText.color;
        _hpText.color = Color.red;
        _ = _hpText.DOColor(originalColor, 0f).SetDelay(0.2f);

        SFXManager.Instance.Play(sfxType);
        var attackVfx = VFXManager.Instance.Spawn(attackVfxType);
        attackVfx.transform.position = transform.position;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.25f), cancellationToken: token);
        }
        finally
        {
            VFXManager.Instance.Despawn(attackVfx);
            _hpText.DOKill(complete: true);
        }
    }

    public async UniTask HealAsync(CancellationToken token)
    {
        _hpText.DOKill(complete: true);
        Color originalColor = _hpText.color;
        _ = DOTween.Sequence()
            .Append(_hpText.DOColor(Color.green, 0.5f))
            .Append(_hpText.DOColor(originalColor, 0.5f))
            .SetTarget(_hpText);

        SFXManager.Instance.Play(SFXType.Healing);
        var healingVfx = VFXManager.Instance.Spawn(VFXType.Healing);
        healingVfx.transform.position = transform.position;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f), cancellationToken: token);
        }
        finally
        {
            VFXManager.Instance.Despawn(healingVfx);
            _hpText.DOKill(complete: true);
        }
    }

    public void SetOutlineHighlight(bool isOn)
    {
        _outlineCts?.Cancel();
        _outlineCts?.Dispose();
        _outlineCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

        Material material = _cardRenderer.material;
        material.SetFloat("_OutlineAlpha", 0f);
        if (isOn)
        {
            FadeOutlineAlphaAsync(material, _outlineCts.Token).Forget();
        }
    }

    private async UniTask FadeOutlineAlphaAsync(Material material, CancellationToken token)
    {
        int outlineAlphaId = Shader.PropertyToID("_OutlineAlpha");
        float fadeDuration = 0.35f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            material.SetFloat(outlineAlphaId, Mathf.Clamp01(elapsed / fadeDuration));
            await UniTask.Yield(token);
        }
        material.SetFloat(outlineAlphaId, 1f);
    }

    private async UniTask PickUpLiftAsync(float targetY, CancellationToken token)
    {
        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition = new Vector3(startPosition.x, targetY, startPosition.z);

        float liftDuration = 0.15f;
        float elapsed = 0f;
        while (elapsed < liftDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / liftDuration));
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            await UniTask.Yield(token);
        }
        transform.localPosition = targetPosition;
    }

    private void SetFace(bool showFront)
    {
        _frontFace.SetActive(showFront);
        _backFace.SetActive(!showFront);
    }

    private void ApplyScaleX(Vector3 baseScale, float factor)
    {
        transform.localScale = new Vector3(baseScale.x * factor, baseScale.y, baseScale.z);
    }

    private void OnHpChanged(int hp)
    {
        _hpText.text = hp.ToString();
    }

    public CardInstance Card { get; private set; }
}
