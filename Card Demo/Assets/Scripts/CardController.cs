using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// CardController manages individual card behavior, animation, and identification.
/// Features: Smooth flip animation, state management, event callbacks.
/// </summary>
public class CardController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image cardFrontImage;
    [SerializeField] private Image cardBackImage;
    [SerializeField] private Button cardButton; // Injected via Inspector
    [SerializeField] private CanvasGroup canvasGroup; // Injected via Inspector

    // Card identification
    private int cardID;
    private int matchValue;
    private string cardName;

    // Card state
    private bool isFaceUp = false;
    private bool isLocked = false;
    private bool isMatched = false;

    // References
    private GameManager gameManager;
    private AudioManager audioManager;
    private GameConfiguration gameConfig;

    // Animation
    private Coroutine flipCoroutine;
    private float flipDuration = 0.25f;

    // Delegate for callbacks
    public delegate void CardFlipHandler(CardController card);
    public event CardFlipHandler OnCardFlipped;

    /// <summary>
    /// Note: Button and CanvasGroup must be assigned via Inspector for optimal performance.
    /// This avoids GetComponent() calls at runtime which can be expensive with many objects.
    /// </summary>
    private void OnEnable()
    {
        // Validate that required components are assigned
        if (cardButton == null)
        {
            Debug.LogWarning("CardController: Button not assigned in Inspector!", gameObject);
        }
        if (canvasGroup == null)
        {
            Debug.LogWarning("CardController: CanvasGroup not assigned in Inspector!", gameObject);
        }
    }

    /// <summary>
    /// Initializes the card with all necessary data.
    /// </summary>
    public void Initialize(int id, int matchVal, string name, Sprite frontSprite, 
                          Sprite backSprite, GameManager manager, AudioManager audioMan,
                          GameConfiguration config)
    {
        cardID = id;
        matchValue = matchVal;
        cardName = name;
        cardFrontImage.sprite = frontSprite;
        cardBackImage.sprite = backSprite;
        gameManager = manager;
        audioManager = audioMan;
        gameConfig = config;

        flipDuration = config.CardFlipDuration;

        // Initialize state
        isFaceUp = false;
        isLocked = false;
        isMatched = false;

        // UI setup
        cardFrontImage.gameObject.SetActive(false);
        cardBackImage.gameObject.SetActive(true);

        if (cardButton != null)
            cardButton.onClick.AddListener(OnCardClicked);
    }

    /// <summary>
    /// Called when the card is clicked.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked();
    }

    private void OnCardClicked()
    {
        if (CanBeFlipped())
        {
            gameManager.CardClicked(this);
        }
    }

    /// <summary>
    /// Determines if the card can be flipped.
    /// </summary>
    private bool CanBeFlipped()
    {
        return !isFaceUp && !isLocked && !isMatched && gameManager.CanInteract();
    }

    /// <summary>
    /// Flips the card with animation.
    /// </summary>
    public void Flip()
    {
        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);
        flipCoroutine = StartCoroutine(FlipAnimation(true));
    }

    /// <summary>
    /// Flips the card back to face-down.
    /// </summary>
    public void FlipBack()
    {
        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);
        flipCoroutine = StartCoroutine(FlipAnimation(false));
    }

    /// <summary>
    /// Flips the card with smooth animation and sprite swapping.
    /// </summary>
    private IEnumerator FlipAnimation(bool faceUp)
    {
        isLocked = true;
        isFaceUp = faceUp;

        float elapsed = 0f;
        float halfDuration = flipDuration / 2f;

        // First half: rotate to 90 degrees
        while (elapsed < halfDuration)
        {
            float progress = elapsed / halfDuration;
            float angle = Mathf.Lerp(0f, 90f, gameConfig.FlipEase.Evaluate(progress));
            transform.localRotation = Quaternion.Euler(0, angle, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // At 90 degrees, swap sprites
        transform.localRotation = Quaternion.Euler(0, 90, 0);
        cardBackImage.gameObject.SetActive(!isFaceUp);
        cardFrontImage.gameObject.SetActive(isFaceUp);

        // Play flip sound at the moment of swap
        if (audioManager != null)
        {
            audioManager.PlayFlipSound();
        }

        elapsed = 0f;
        // Second half: rotate from 90 to 0 degrees (or 180)
        while (elapsed < halfDuration)
        {
            float progress = elapsed / halfDuration;
            float angle = Mathf.Lerp(90f, 180f, gameConfig.FlipEase.Evaluate(progress));
            transform.localRotation = Quaternion.Euler(0, angle, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset rotation
        transform.localRotation = Quaternion.Euler(0, isFaceUp ? 180f : 0f, 0);
        flipCoroutine = null;
        isLocked = false;

        OnCardFlipped?.Invoke(this);
    }

    /// <summary>
    /// Locks the card to prevent interaction.
    /// </summary>
    public void LockCard()
    {
        isLocked = true;
        isMatched = true;
        if (cardButton != null)
            cardButton.interactable = false;
    }

    /// <summary>
    /// Unlocks the card.
    /// </summary>
    public void UnlockCard()
    {
        isLocked = false;
        if (cardButton != null)
            cardButton.interactable = true;
    }

    /// <summary>
    /// Resets the card to initial face-down state.
    /// </summary>
    public void ResetCard()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
            flipCoroutine = null;
        }

        isFaceUp = false;
        isLocked = false;
        isMatched = false;
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        cardBackImage.gameObject.SetActive(true);
        cardFrontImage.gameObject.SetActive(false);

        if (cardButton != null)
            cardButton.interactable = true;
    }

    /// <summary>
    /// Applies a match animation (scale + fade).
    /// </summary>
    public IEnumerator MatchAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float scale = Mathf.Lerp(1f, 1.2f, progress);
            transform.localScale = originalScale * scale;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0.5f, progress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale * 1.2f;
        if (canvasGroup != null)
            canvasGroup.alpha = 0.5f;
    }

    // Getters
    public int GetCardID() => cardID;
    public int GetMatchValue() => matchValue;
    public string GetCardName() => cardName;
    public bool IsFaceUp() => isFaceUp;
    public bool IsLocked() => isLocked;
    public bool IsMatched() => isMatched;
}
