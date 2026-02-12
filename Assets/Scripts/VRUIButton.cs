using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class VRUIButton : MonoBehaviour
{
    [Header("Acción del Botón")]
    public UnityEvent onButtonClick;
    
    [Header("Feedback Visual")]
    public GameObject visualObject;
    public Vector3 hoverScale = Vector3.one * 1.1f;
    public Vector3 pressScale = Vector3.one * 0.9f;
    
    [Header("Colores (opcional)")]
    public UnityEngine.UI.Image buttonImage;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.7f, 1f, 1f);
    public Color pressColor = new Color(0.5f, 1f, 0.5f);
    
    [Header("Audio")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private Vector3 originalScale;
    private Color originalColor;
    private AudioSource audioSource;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;

        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            collider.size = new Vector3(rect.rect.width, rect.rect.height, 20f);
            collider.center = Vector3.zero;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.5f;

        if (visualObject == null) visualObject = gameObject;
        originalScale = visualObject.transform.localScale;
        
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }

        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
        interactable.selectEntered.AddListener(OnSelectEnter);
        
        Debug.Log($"✅ VRUIButton configurado: {gameObject.name}");
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEnter);
            interactable.hoverExited.RemoveListener(OnHoverExit);
            interactable.selectEntered.RemoveListener(OnSelectEnter);
        }
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        Debug.Log($"🎯 HOVER en botón: {gameObject.name}");

        visualObject.transform.localScale = hoverScale;

        if (buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }

        PlaySound(hoverSound);
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        visualObject.transform.localScale = originalScale;
        
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }

    void OnSelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log($"✅ CLICK en botón: {gameObject.name}");

        visualObject.transform.localScale = pressScale;
        
        if (buttonImage != null)
        {
            buttonImage.color = pressColor;
        }

        PlaySound(clickSound);

        onButtonClick?.Invoke();

        Invoke(nameof(RestoreVisuals), 0.1f);
    }

    void RestoreVisuals()
    {
        if (interactable != null && interactable.isHovered)
        {
            visualObject.transform.localScale = hoverScale;
            if (buttonImage != null)
            {
                buttonImage.color = hoverColor;
            }
        }
        else
        {
            visualObject.transform.localScale = originalScale;
            if (buttonImage != null)
            {
                buttonImage.color = originalColor;
            }
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}