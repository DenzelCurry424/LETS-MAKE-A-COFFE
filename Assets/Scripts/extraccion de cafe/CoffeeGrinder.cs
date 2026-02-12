using UnityEngine;

public class CoffeeGrinder : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Snap point donde se coloca el filtro")]
    public SnapPoint filterSnapPoint;
    
    [Tooltip("Sistema de partículas de café molido")]
    public ParticleSystem grinderParticles;
    
    [Tooltip("GameObject del sólido de café molido (desactivado al inicio)")]
    public GameObject groundCoffeeSolid;
    
    [Header("Configuración")]
    [Tooltip("Duración del molido en segundos")]
    public float grindDuration = 10f;
    
    [Tooltip("Audio del molido")]
    public AudioSource grindSound;
    
    private bool isGrinding = false;
    private float grindTimer = 0f;
    private GameObject currentFilter;
    
    private void Start()
    {
        if (groundCoffeeSolid != null)
            groundCoffeeSolid.SetActive(false);

        if (grinderParticles != null)
            grinderParticles.Stop();

        if (filterSnapPoint != null)
        {
            filterSnapPoint.onObjectSnapped.AddListener(OnFilterSnapped);
        }
    }
    
    private void Update()
    {
        if (isGrinding)
        {
            grindTimer += Time.deltaTime;
            
            if (grindTimer >= grindDuration)
            {
                FinishGrinding();
            }
        }
    }

    private void OnFilterSnapped()
    {
        currentFilter = filterSnapPoint.snappedObject;

        if (currentFilter != null && currentFilter.CompareTag("Filter"))
        {
            StartGrinding();
        }
    }

    private void StartGrinding()
    {
        isGrinding = true;
        grindTimer = 0f;

        if (filterSnapPoint != null)
            filterSnapPoint.allowManualRelease = false;

        if (grinderParticles != null)
        {
            grinderParticles.Play();
        }

        if (grindSound != null)
        {
            grindSound.loop = true;
            grindSound.Play();
        }
        
        Debug.Log("Iniciando molido de café...");
        
    }

    private void FinishGrinding()
    {
        isGrinding = false;
        
        if (grinderParticles != null)
        {
            grinderParticles.Stop();
        }
        
        if (grindSound != null)
        {
            grindSound.Stop();
        }

        if (groundCoffeeSolid != null)
        {
            groundCoffeeSolid.SetActive(true);
        }

        if (currentFilter != null)
        {
            currentFilter.tag = "FilterWithCoffee";
            Debug.Log("Filtro ahora contiene café molido");
        }

        if (filterSnapPoint != null)
            filterSnapPoint.allowManualRelease = true;
        
        Debug.Log("Puedes agarrar el filtro ahora");

    }
}