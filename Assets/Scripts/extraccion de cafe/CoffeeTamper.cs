using UnityEngine;

public class CoffeeTamper : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Snap point donde se engancha el tamper (en el filtro)")]
    public SnapPoint tamperSnapPoint;
    
    [Header("Configuración")]
    [Tooltip("Tiempo que debe estar el tamper para prensar")]
    public float pressTime = 2f;
    
    [Tooltip("Audio del prensado")]
    public AudioSource pressSound;
    
    [Tooltip("Partículas de compactación (opcional)")]
    public ParticleSystem compactParticles;
    
    private bool isPressing = false;
    private float pressTimer = 0f;
    private GameObject currentFilter;
    private GameObject currentTamper;
    private bool hasPressed = false;
    
    private void Start()
    {
        if (tamperSnapPoint != null)
        {
            tamperSnapPoint.onObjectSnapped.AddListener(OnTamperSnapped);
        }
    }
    
    private void Update()
    {
        if (isPressing && !hasPressed)
        {
            pressTimer += Time.deltaTime;
            
            if (pressTimer >= pressTime)
            {
                CompletePressing();
            }
        }
    }
    
    private void OnTamperSnapped()
    {
        currentTamper = tamperSnapPoint.snappedObject;
        
        if (currentTamper != null && currentTamper.CompareTag("Tamper"))
        {
            Transform filterTransform = tamperSnapPoint.transform.parent;
            if (filterTransform != null)
            {
                currentFilter = filterTransform.gameObject;

                if (currentFilter.CompareTag("FilterWithCoffee"))
                {
                    Debug.Log($"Tamper detectado en filtro con café. Iniciando prensado...");
                    StartPressing();
                }
                else
                {
                    Debug.LogWarning($"El filtro no tiene café. Tag actual: {currentFilter.tag}");
                }
            }
        }
    }
    
    private void StartPressing()
    {
        isPressing = true;
        pressTimer = 0f;
        hasPressed = false;

        if (tamperSnapPoint != null)
            tamperSnapPoint.allowManualRelease = false;

        if (pressSound != null)
        {
            pressSound.Play();
        }

        if (compactParticles != null)
        {
            compactParticles.Play();
        }
        
        Debug.Log("Prensando café...");
    }
    
    private void CompletePressing()
    {
        isPressing = false;
        hasPressed = true;

        if (compactParticles != null)
        {
            compactParticles.Stop();
        }

        if (currentFilter != null)
        {
            currentFilter.tag = "FilterPressed";
            Debug.Log("Café prensado completado. Filtro tag: FilterPressed");
        }

        if (tamperSnapPoint != null)
            tamperSnapPoint.allowManualRelease = true;
        
        Debug.Log("Puedes retirar el tamper ahora");
    }
}