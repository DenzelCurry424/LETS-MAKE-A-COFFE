using UnityEngine;

public class MilkTexturizer : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Script de la perilla de vapor")]
    public SteamKnob steamKnob;
    
    [Header("Configuración")]
    [Tooltip("Tiempo necesario de texturización (segundos)")]
    public float texturizingTime = 20f;
    
    [Tooltip("Intensidad mínima de vapor requerida (0-1)")]
    [Range(0f, 1f)]
    public float minimumSteamIntensity = 0.3f;
    
    [Header("Debug")]
    [SerializeField] private bool isTexturizing = false;
    [SerializeField] private float currentTime = 0f;
    [SerializeField] private MilkPitcher currentPitcher = null;
    
    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = 0.15f;
            sphere.isTrigger = true;
            Debug.LogWarning("⚠️ Se agregó SphereCollider al texturizador");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
        
        if (steamKnob == null)
        {
            steamKnob = GetComponentInParent<SteamKnob>();
            if (steamKnob == null)
            {
                Debug.LogError("❌ No se encontró SteamKnob!");
            }
        }
        
        Debug.Log("✅ MilkTexturizer configurado");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        MilkPitcher pitcher = other.GetComponent<MilkPitcher>();
        if (pitcher == null)
        {
            pitcher = other.GetComponentInParent<MilkPitcher>();
        }
        
        if (pitcher != null && pitcher.IsFilled() && !pitcher.IsTextured())
        {
            currentPitcher = pitcher;
            Debug.Log("🥛 Jarra detectada en zona de texturización");
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (currentPitcher == null) return;

        float steamIntensity = steamKnob != null ? steamKnob.GetSteamIntensity() : 0f;
        
        if (steamIntensity >= minimumSteamIntensity)
        {
            if (!isTexturizing)
            {
                isTexturizing = true;
                Debug.Log("🌫️ ¡Iniciando texturización de leche!");
            }
            
            currentTime += Time.deltaTime;

            if (currentTime % 1f < Time.deltaTime)
            {
                Debug.Log($"🌫️ Texturizando... {currentTime:F1}/{texturizingTime}s");
            }

            if (currentTime >= texturizingTime)
            {
                CompleteTexturizing();
            }
        }
        else
        {
            if (isTexturizing)
            {
                Debug.Log("⚠️ Vapor insuficiente - pausando texturización");
                isTexturizing = false;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (currentPitcher != null)
        {
            MilkPitcher pitcher = other.GetComponent<MilkPitcher>();
            if (pitcher == null)
            {
                pitcher = other.GetComponentInParent<MilkPitcher>();
            }
            
            if (pitcher == currentPitcher)
            {
                Debug.Log("🥛 Jarra retirada de la zona de texturización");
                ResetTexturizing();
            }
        }
    }
    
    private void CompleteTexturizing()
    {
        if (currentPitcher != null)
        {
            currentPitcher.TexturizeMilk();
            Debug.Log("✅ ¡LECHE TEXTURIZADA COMPLETADA!");
        }
        
        ResetTexturizing();
    }
    
    private void ResetTexturizing()
    {
        isTexturizing = false;
        currentTime = 0f;
        currentPitcher = null;
    }
    
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = isTexturizing ? new Color(1, 0.5f, 0, 0.5f) : new Color(0, 1, 1, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}