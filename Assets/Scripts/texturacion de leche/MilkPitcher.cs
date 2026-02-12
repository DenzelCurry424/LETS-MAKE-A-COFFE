using UnityEngine;

public class MilkPitcher : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("GameObject del sólido de leche dentro de la jarra (desactivado al inicio)")]
    public GameObject milkSolid;
    
    [Tooltip("GameObject del sólido de leche texturizada (diferente color/material) - OPCIONAL")]
    public GameObject texturedMilkSolid;
    
    [Header("Configuración de Llenado")]
    [Tooltip("Tiempo que debe recibir leche para llenar (segundos)")]
    public float timeToFill = 5f;
    
    [Tooltip("Audio de la leche cayendo (opcional)")]
    public AudioSource pourSound;
    
    [Header("Materiales de Texturización")]
    [Tooltip("Material de leche normal (blanco)")]
    public Material normalMilkMaterial;
    
    [Tooltip("Material de leche texturizada (espumosa)")]
    public Material texturedMilkMaterial;
    
    [Header("Configuración de Vaporización")]
    [Tooltip("Tiempo que debe recibir vapor para texturizar (segundos)")]
    public float timeToTexturize = 3f;
    
    [Tooltip("Audio de vaporización (opcional)")]
    public AudioSource texturizingSound;
    
    [Header("Estado")]
    [SerializeField] private float currentFillTime = 0f;
    [SerializeField] private float currentTexturizeTime = 0f;
    [SerializeField] private bool isFilled = false;
    [SerializeField] private bool isTextured = false;
    [SerializeField] private bool isReceivingMilk = false;
    [SerializeField] private bool isReceivingSteam = false;
    
    private void Start()
    {
        // VERIFICACIÓN: La jarra DEBE estar en Default para XR Grab
        int defaultLayer = LayerMask.NameToLayer("Default");
        if (gameObject.layer != defaultLayer)
        {
            Debug.LogWarning($"⚠️ ADVERTENCIA: La jarra está en layer '{LayerMask.LayerToName(gameObject.layer)}'");
            Debug.LogWarning($"   XR Grab Interactable requiere layer 'Default'. Cambiando automáticamente...");
            gameObject.layer = defaultLayer;
            Debug.Log("✅ Layer cambiado a Default - XR Grab funcionará correctamente");
        }
        else
        {
            Debug.Log("✅ Jarra en layer Default - XR Grab OK");
        }
        
        if (milkSolid != null)
        {
            milkSolid.SetActive(false);
            Debug.Log("✅ Sólido de leche desactivado al inicio");
            ConfigureSingleMilkSolid(milkSolid);
        }
        else
        {
            Debug.LogError("❌ No se asignó el GameObject milkSolid en el Inspector!");
        }

        if (texturedMilkSolid != null)
        {
            texturedMilkSolid.SetActive(false);
            Debug.Log("✅ Sólido de leche texturizada desactivado al inicio");
            ConfigureSingleMilkSolid(texturedMilkSolid);
        }
        
        // Configurar tag inicial
        gameObject.tag = "Pitcher";
    }

    private void ConfigureSingleMilkSolid(GameObject solid)
    {
        if (solid == null) return;

        Collider[] colliders = solid.GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
            Debug.Log($"🔧 Collider desactivado en {solid.name}");
        }

        Collider[] childColliders = solid.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in childColliders)
        {
            col.enabled = false;
            Debug.Log($"🔧 Collider hijo desactivado en {col.gameObject.name}");
        }

        Rigidbody rb = solid.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
            Debug.Log($"🔧 Rigidbody eliminado de {solid.name}");
        }
    }
    
    private void Update()
    {
        // Proceso de llenado con leche
        if (isReceivingMilk && !isFilled)
        {
            currentFillTime += Time.deltaTime;

            if (pourSound != null && !pourSound.isPlaying)
            {
                pourSound.Play();
            }

            if (currentFillTime % 1f < Time.deltaTime)
            {
                Debug.Log($"🥛 Llenando jarra... {currentFillTime:F1}/{timeToFill}s");
            }

            if (currentFillTime >= timeToFill)
            {
                FillPitcher();
            }
        }
        else if (!isReceivingMilk && pourSound != null && pourSound.isPlaying)
        {
            pourSound.Stop();
        }

        // Proceso de vaporización de leche
        if (isReceivingSteam && isFilled && !isTextured)
        {
            currentTexturizeTime += Time.deltaTime;

            if (texturizingSound != null && !texturizingSound.isPlaying)
            {
                texturizingSound.Play();
            }

            if (currentTexturizeTime % 1f < Time.deltaTime)
            {
                Debug.Log($"🌫️ Vaporizando leche... {currentTexturizeTime:F1}/{timeToTexturize}s");
            }

            if (currentTexturizeTime >= timeToTexturize)
            {
                TexturizeMilk();
            }
        }
        else if (!isReceivingSteam && texturizingSound != null && texturizingSound.isPlaying)
        {
            texturizingSound.Stop();
        }

        // Reset de flags
        isReceivingMilk = false;
        isReceivingSteam = false;
    }

    // Llamado por MilkPitcherDetector cuando detecta partículas de leche
    public void OnMilkParticleDetected()
    {
        if (!isFilled)
        {
            isReceivingMilk = true;
        }
    }

    // Llamado cuando detecta vapor de la lanceta
    public void OnSteamDetected()
    {
        if (isFilled && !isTextured)
        {
            isReceivingSteam = true;
        }
    }

    private void FillPitcher()
    {
        if (isFilled) return;
        
        isFilled = true;

        if (milkSolid != null)
        {
            milkSolid.SetActive(true);
            Debug.Log("✅ ¡JARRA LLENA! Sólido de leche activado");
        }

        if (pourSound != null)
        {
            pourSound.Stop();
        }

        gameObject.tag = "PitcherWithMilk";
        
        Debug.Log("🎯 ¡Jarra lista para texturizar!");
    }

    public void TexturizeMilk()
    {
        if (!isFilled)
        {
            Debug.LogWarning("⚠️ No se puede texturizar: jarra no está llena");
            return;
        }
        
        if (isTextured)
        {
            Debug.LogWarning("⚠️ La leche ya está texturizada");
            return;
        }
        
        isTextured = true;

        // Cambiar material si usamos el mismo sólido
        if (milkSolid != null && texturedMilkMaterial != null && texturedMilkSolid == null)
        {
            Renderer renderer = milkSolid.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = texturedMilkMaterial;
                Debug.Log("✅ Material cambiado a leche texturizada");
            }
        }

        // O intercambiar sólidos si tenemos uno específico para texturizada
        if (texturedMilkSolid != null)
        {
            if (milkSolid != null)
            {
                milkSolid.SetActive(false);
            }
            texturedMilkSolid.SetActive(true);
            Debug.Log("✅ Sólido intercambiado a leche texturizada");
        }

        if (texturizingSound != null)
        {
            texturizingSound.Stop();
        }

        gameObject.tag = "PitcherWithTexturedMilk";
        
        Debug.Log("🌫️ ¡LECHE TEXTURIZADA! Lista para hacer cappuccino");
    }

    public void EmptyMilk()
    {
        if (milkSolid != null)
        {
            milkSolid.SetActive(false);
        }
        
        if (texturedMilkSolid != null)
        {
            texturedMilkSolid.SetActive(false);
        }
        
        isFilled = false;
        isTextured = false;
        currentFillTime = 0f;
        currentTexturizeTime = 0f;
        
        gameObject.tag = "Pitcher";
        
        Debug.Log("🗑️ Jarra vaciada");
    }

    public void EmptyPitcher()
    {
        EmptyMilk();
    }

    public bool IsFilled()
    {
        return isFilled;
    }

    public bool IsTextured()
    {
        return isTextured;
    }

    public bool IsEmpty()
    {
        return !isFilled;
    }
}