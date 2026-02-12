using UnityEngine;

[RequireComponent(typeof(MilkPitcher))]
public class MilkPourer : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Punto desde donde sale la leche (pico de la jarra)")]
    public Transform pourPoint;
    
    [Tooltip("Partículas de leche cayendo")]
    public ParticleSystem milkPourParticles;
    
    [Tooltip("Audio de vertido")]
    public AudioSource pourAudio;
    
    [Header("Configuración")]
    [Tooltip("Ángulo mínimo de inclinación para verter (grados)")]
    public float pourAngle = 45f;
    
    [Tooltip("Tiempo necesario vertiendo para completar (segundos)")]
    public float timeToComplete = 2f;
    
    [Tooltip("Distancia máxima para detectar la taza")]
    public float maxPourDistance = 0.3f;
    
    [Header("Debug")]
    [SerializeField] private bool isPouring = false;
    [SerializeField] private float currentPourTime = 0f;
    [SerializeField] private GameObject targetCup = null;
    
    private MilkPitcher milkPitcher;
    private Rigidbody rb;
    
    private void Start()
    {
        milkPitcher = GetComponent<MilkPitcher>();
        rb = GetComponent<Rigidbody>();
        
        if (pourPoint == null)
        {
            GameObject pourGO = new GameObject("PourPoint");
            pourGO.transform.SetParent(transform);
            pourGO.transform.localPosition = new Vector3(0, 0.1f, 0.05f); 
            pourPoint = pourGO.transform;
            Debug.LogWarning("⚠️ Se creó PourPoint automático - ajusta la posición en el Inspector");
        }
        
        if (milkPourParticles != null)
        {
            milkPourParticles.Stop();
        }
        
        Debug.Log("✅ MilkPourer configurado");
    }
    
    private void Update()
    {
        var grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        bool isGrabbed = grabInteractable != null && grabInteractable.isSelected;
        
        if (!isGrabbed || !milkPitcher.IsTextured())
        {
            if (isPouring)
            {
                StopPouring();
            }
            return;
        }

        float currentAngle = Vector3.Angle(transform.up, Vector3.up);
        bool isTilted = currentAngle > pourAngle;
        
        if (isTilted)
        {
            GameObject cup = FindNearestCup();
            
            if (cup != null)
            {
                if (!isPouring)
                {
                    StartPouring(cup);
                }
                
                currentPourTime += Time.deltaTime;
                
                if (currentPourTime % 0.5f < Time.deltaTime)
                {
                    Debug.Log($"☕ Vertiendo leche... {currentPourTime:F1}/{timeToComplete}s");
                }

                if (currentPourTime >= timeToComplete)
                {
                    CompletePouring();
                }
            }
            else
            {
                if (isPouring)
                {
                    StopPouring();
                }
            }
        }
        else
        {
            if (isPouring)
            {
                StopPouring();
            }
        }
    }
    
    private GameObject FindNearestCup()
    {
        if (pourPoint == null) return null;

        GameObject[] cups = GameObject.FindGameObjectsWithTag("CupWithEspresso");
        
        GameObject nearest = null;
        float nearestDistance = maxPourDistance;
        
        foreach (GameObject cup in cups)
        {
            float distance = Vector3.Distance(pourPoint.position, cup.transform.position);

            bool isBelow = cup.transform.position.y < pourPoint.position.y;
            
            if (distance < nearestDistance && isBelow)
            {
                nearest = cup;
                nearestDistance = distance;
            }
        }
        
        if (nearest != null)
        {
            Debug.Log($"🎯 Taza detectada: {nearest.name} a {nearestDistance:F2}m");
        }
        
        return nearest;
    }
    
    private void StartPouring(GameObject cup)
    {
        isPouring = true;
        targetCup = cup;
        currentPourTime = 0f;

        if (milkPourParticles != null)
        {
            milkPourParticles.Play();
        }

        if (pourAudio != null)
        {
            pourAudio.loop = true;
            pourAudio.Play();
        }
        
        Debug.Log($"☕ Iniciando vertido en {cup.name}");
    }
    
    private void StopPouring()
    {
        isPouring = false;
        currentPourTime = 0f;
        targetCup = null;
        
        if (milkPourParticles != null)
        {
            milkPourParticles.Stop();
        }
        
        if (pourAudio != null)
        {
            pourAudio.Stop();
        }
        
        Debug.Log("⏸️ Vertido detenido");
    }
    
    private void CompletePouring()
    {
        if (targetCup == null) return;
        
        Debug.Log($"✅ Vertido completado en {targetCup.name}");

        CappuccinoCup cappuccino = targetCup.GetComponent<CappuccinoCup>();
        if (cappuccino == null)
        {
            cappuccino = targetCup.AddComponent<CappuccinoCup>();
            Debug.Log("➕ CappuccinoCup añadido a la taza");
        }

        cappuccino.CompleteCappuccino();

        milkPitcher.EmptyMilk();

        StopPouring();
        
        Debug.Log("🎉 ¡CAPPUCCINO COMPLETADO!");
    }
    
    private void OnDrawGizmos()
    {
        if (pourPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pourPoint.position, 0.02f);

            Gizmos.color = new Color(0, 1, 1, 0.2f);
            Gizmos.DrawWireSphere(pourPoint.position, maxPourDistance);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pourPoint.position, Vector3.down * 0.1f);
        }
    }
}