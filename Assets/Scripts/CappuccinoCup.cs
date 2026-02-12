using UnityEngine;

public class CappuccinoCup : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Sólido de espresso (desactivar al completar)")]
    public GameObject espressoLiquid;
    
    [Tooltip("Sólido de cappuccino (activar al completar)")]
    public GameObject cappuccinoLiquid;
    
    [Header("Efectos")]
    [Tooltip("Partículas de mezcla (opcional)")]
    public ParticleSystem mixingParticles;
    
    [Tooltip("Audio de mezcla (opcional)")]
    public AudioSource mixingSound;
    
    [Header("Estado")]
    [SerializeField] private bool isCompleted = false;
    
    private void Start()
    {
        if (espressoLiquid == null)
        {
            Transform liquid = transform.Find("EspressoLiquid");
            if (liquid != null)
            {
                espressoLiquid = liquid.gameObject;
            }
            else
            {
                foreach (Transform child in transform)
                {
                    if (child.gameObject.activeSelf && child.name.ToLower().Contains("espresso"))
                    {
                        espressoLiquid = child.gameObject;
                        break;
                    }
                }
            }
        }

        if (cappuccinoLiquid == null)
        {
            Transform cappuccino = transform.Find("CappuccinoLiquid");
            if (cappuccino != null)
            {
                cappuccinoLiquid = cappuccino.gameObject;
            }
        }

        if (cappuccinoLiquid == null && espressoLiquid != null)
        {
            cappuccinoLiquid = Instantiate(espressoLiquid, espressoLiquid.transform.parent);
            cappuccinoLiquid.name = "CappuccinoLiquid";

            Renderer renderer = cappuccinoLiquid.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                Color espressoColor = renderer.material.color;
                Color cappuccinoColor = Color.Lerp(espressoColor, Color.white, 0.5f);
                renderer.material.color = cappuccinoColor;
                
                Debug.Log("✅ Sólido de cappuccino creado automáticamente");
            }
            
            cappuccinoLiquid.SetActive(false);
        }

        if (cappuccinoLiquid != null)
        {
            cappuccinoLiquid.SetActive(false);
        }
    }

    public void CompleteCappuccino()
    {
        if (isCompleted) return;
        
        isCompleted = true;

        if (espressoLiquid != null)
        {
            espressoLiquid.SetActive(false);
            Debug.Log("❌ Espresso ocultado");
        }

        if (cappuccinoLiquid != null)
        {
            cappuccinoLiquid.SetActive(true);
            Debug.Log("✅ Cappuccino mostrado");
        }
        else
        {
            Debug.LogWarning("⚠️ No hay sólido de cappuccino asignado");
        }

        if (mixingParticles != null)
        {
            mixingParticles.Play();
        }
        
        if (mixingSound != null)
        {
            mixingSound.Play();
        }

        gameObject.tag = "CupWithCappuccino";
        
        Debug.Log("☕ ¡CAPPUCCINO COMPLETADO!");
        Debug.Log("🎨 ¡Listo para servir o hacer arte latte!");
    }

    public void ResetCup()
    {
        isCompleted = false;
        
        if (espressoLiquid != null)
        {
            espressoLiquid.SetActive(false);
        }
        
        if (cappuccinoLiquid != null)
        {
            cappuccinoLiquid.SetActive(false);
        }
        
        gameObject.tag = "Cup";
        
        Debug.Log("🔄 Taza reseteada");
    }
    
    public bool IsCompleted() => isCompleted;
}