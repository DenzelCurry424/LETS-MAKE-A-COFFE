using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ExtractionButton : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia a la máquina de espresso")]
    public EspressoMachine espressoMachine;
    
    [Header("Materiales")]
    [Tooltip("Material cuando el botón está disponible")]
    public Material availableMaterial;
    
    [Tooltip("Material cuando el botón está deshabilitado")]
    public Material disabledMaterial;
    
    [Header("Efectos Visuales")]
    [Tooltip("Escala cuando el botón es presionado")]
    public float pressedScale = 0.9f;
    
    [Tooltip("Duración de la animación de presión")]
    public float pressAnimationDuration = 0.1f;
    
    private Renderer buttonRenderer;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private Vector3 originalScale;
    private bool isPressed = false;
    private bool canPress = false;
    
    private void Awake()
    {
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer == null)
        {
            Debug.LogError("ExtractionButton necesita un Renderer!");
        }

        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.size = new Vector3(0.05f, 0.05f, 0.02f);
            Debug.LogWarning("Se agregó BoxCollider al botón");
        }

        originalScale = transform.localScale;

        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
        interactable.selectEntered.AddListener(OnPressed);
        
        Debug.Log("ExtractionButton configurado correctamente");
    }

    public void UpdateButtonState(bool canExtract)
    {
        canPress = canExtract;

        if (buttonRenderer != null && availableMaterial != null && disabledMaterial != null)
        {
            buttonRenderer.material = canPress ? availableMaterial : disabledMaterial;
        }

        if (interactable != null)
        {
            interactable.enabled = true;
        }
        
        Debug.Log($"Botón actualizado - Puede presionar: {canPress}");
    }
    
    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (canPress)
        {
            transform.localScale = originalScale * 1.05f;
            Debug.Log("Hover sobre botón");
        }
    }
    
    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (!isPressed)
        {
            transform.localScale = originalScale;
        }
    }
    
    private void OnPressed(SelectEnterEventArgs args)
    {
        Debug.Log("Botón PRESIONADO!");
        
        if (!canPress)
        {
            Debug.LogWarning("Botón no disponible aún");
            return;
        }
        
        if (isPressed)
        {
            Debug.LogWarning("Botón ya fue presionado");
            return;
        }

        isPressed = true;
        transform.localScale = originalScale * pressedScale;

        if (espressoMachine != null)
        {
            espressoMachine.StartExtractionFromButton();
        }
        else
        {
            Debug.LogError("No hay referencia a EspressoMachine!");
        }

        Invoke(nameof(ResetScale), pressAnimationDuration);
    }
    
    private void ResetScale()
    {
        transform.localScale = originalScale;
        isPressed = false;
    }
}