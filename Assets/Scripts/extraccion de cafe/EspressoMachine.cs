using UnityEngine;

public class EspressoMachine : MonoBehaviour
{           
    [Header("Referencias de Snap Points")]
    [Tooltip("Snap point para el filtro prensado")]
    public SnapPoint filterSnapPoint;
    
    [Tooltip("Snap point para la taza")]
    public SnapPoint cupSnapPoint;
    
    [Header("Botón de Extracción")]
    [Tooltip("Script del botón de extracción")]
    public ExtractionButton extractionButton;
    
    [Header("Partículas y Efectos")]
    [Tooltip("Partículas de espresso cayendo")]
    public ParticleSystem espressoParticles;
    
    [Tooltip("Sólido de líquido en la taza (desactivado al inicio)")]
    public GameObject espressoLiquid;
    
    [Tooltip("Audio de extracción")]
    public AudioSource extractionSound;
    
    [Header("Configuración")]
    [Tooltip("Duración de la extracción en segundos")]
    public float extractionDuration = 15f;

    private bool isExtracting = false;
    private float extractionTimer = 0f;
    private bool canExtract = false;
    
    private void Start()
    {
        if (espressoLiquid != null)
            espressoLiquid.SetActive(false);
    
        if (espressoParticles != null)
            espressoParticles.Stop();
    
        if (filterSnapPoint != null)
        {
            filterSnapPoint.onObjectSnapped.AddListener(OnSnapChanged);
            filterSnapPoint.onObjectReleased.AddListener(OnSnapChanged);
        }
    
        if (cupSnapPoint != null)
        {
            cupSnapPoint.onObjectSnapped.AddListener(OnSnapChanged);
            cupSnapPoint.onObjectReleased.AddListener(OnSnapChanged);
        }
    
        if (extractionButton == null)
        {
            Debug.LogError("No se asignó el ExtractionButton!");
        }
    
        CheckConditions();
    }
    
    private void Update()
    {
        if (isExtracting)
        {
            extractionTimer += Time.deltaTime;
            
            if (extractionTimer % 1f < 0.1f)
            {
                Debug.Log($"Extrayendo espresso... {extractionTimer:F1}/{extractionDuration:F1}s");
            }
            
            if (extractionTimer >= extractionDuration)
            {
                FinishExtraction();
            }
        }
    }
    
    private void OnSnapChanged()
    {
        CheckConditions();
    }
    
    private void CheckConditions()
    {
        bool filterReady = filterSnapPoint != null && 
                          filterSnapPoint.isOccupied && 
                          filterSnapPoint.snappedObject != null;
        
        bool filterCorrectTag = false;
        if (filterReady)
        {
            string filterTag = filterSnapPoint.snappedObject.tag;
            filterCorrectTag = filterTag == "FilterPressed";
            Debug.Log($"Filtro: {filterReady}, Tag: {filterTag}, Correcto: {filterCorrectTag}");
        }
        
        bool cupReady = cupSnapPoint != null && 
                       cupSnapPoint.isOccupied && 
                       cupSnapPoint.snappedObject != null;
        
        bool cupCorrectTag = false;
        if (cupReady)
        {
            string cupTag = cupSnapPoint.snappedObject.tag;
            cupCorrectTag = cupTag == "Cup";
            Debug.Log($"Taza: {cupReady}, Tag: {cupTag}, Correcto: {cupCorrectTag}");
        }
        
        canExtract = filterCorrectTag && cupCorrectTag && !isExtracting;
        
        Debug.Log($"Puede extraer: {canExtract}");
        
        UpdateButtonState();
    }
    
    private void UpdateButtonState()
    {
        if (extractionButton != null)
        {
            extractionButton.UpdateButtonState(canExtract);
        }
    
        string estado = canExtract ? "LISTO" : "NO LISTO";
        Debug.Log($"Estado extracción: {estado}");
    }
    
    public void StartExtractionFromButton()
    {
        Debug.Log("StartExtractionFromButton llamado");
        
        if (canExtract && !isExtracting)
        {
            StartExtraction();
        }
        else
        {
            if (!canExtract)
                Debug.LogWarning("No se puede extraer: condiciones no cumplidas");
            if (isExtracting)
                Debug.LogWarning("Ya se está extrayendo");
        }
    }

    private void StartExtraction()
    {
        isExtracting = true;
        extractionTimer = 0f;
        canExtract = false;
        
        UpdateButtonState();
        
        if (espressoParticles != null)
        {
            espressoParticles.Play();
            Debug.Log("Partículas de espresso activadas");
        }
        else
        {
            Debug.LogWarning("No hay partículas asignadas");
        }
        
        if (extractionSound != null)
        {
            extractionSound.loop = true;
            extractionSound.Play();
            Debug.Log("Sonido de extracción iniciado");
        }
        
        Debug.Log("¡INICIANDO EXTRACCIÓN DE ESPRESSO!");
    }
    
    private void FinishExtraction()
    {
        isExtracting = false;
        
        if (espressoParticles != null)
        {
            espressoParticles.Stop();
        }
        
        if (extractionSound != null)
        {
            extractionSound.Stop();
        }
        
        if (espressoLiquid != null)
        {
            espressoLiquid.SetActive(true);
            Debug.Log("Líquido de espresso activado en la taza");
        }
        else
        {
            Debug.LogWarning("No hay líquido asignado");
        }
        
        if (cupSnapPoint != null && cupSnapPoint.snappedObject != null)
        {
            cupSnapPoint.snappedObject.tag = "CupWithEspresso";

            Rigidbody cupRb = cupSnapPoint.snappedObject.GetComponent<Rigidbody>();
            if (cupRb != null)
            {
                Debug.Log($"Taza tiene Rigidbody - isKinematic: {cupRb.isKinematic}");
            }
        }
        
        Debug.Log("¡ESPRESSO COMPLETADO!");
        Debug.Log("¡Espresso listo! Puedes tomar tu taza.");
    }
}