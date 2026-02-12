using UnityEngine;
using System.Collections.Generic;

public class MilkPitcherDetector : MonoBehaviour
{
    private MilkPitcher milkPitcher;
    
    [Header("Configuración de Detección")]
    [Tooltip("Solo detectar objetos en estos layers")]
    public LayerMask particleLayers = -1; 
    
    [Tooltip("Ignorar el cartón de leche")]
    public bool ignoreCarton = true;
    
    [Header("Debug")]
    [SerializeField] private int milkDetectionCount = 0;
    [SerializeField] private int steamDetectionCount = 0;
    [SerializeField] private bool showDebugLogs = true;

    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    
    private void Start()
    {
        milkPitcher = GetComponentInParent<MilkPitcher>();
        
        if (milkPitcher == null)
        {
            Debug.LogError("❌ MilkPitcherDetector no encontró MilkPitcher!");
            Debug.LogError($"   GameObject actual: {gameObject.name}");
            Debug.LogError($"   Padre: {transform.parent?.name ?? "NULL"}");
        }
        else
        {
            Debug.Log($"✅ MilkPitcherDetector encontró MilkPitcher en: {milkPitcher.gameObject.name}");
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (!col.isTrigger)
            {
                Debug.LogWarning($"⚠️ Detector '{gameObject.name}' - Collider debe ser Trigger. Corrigiendo...");
                col.isTrigger = true;
            }
            else
            {
                Debug.Log($"✅ Detector configurado como Trigger");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ MilkPitcherDetector no tiene collider. Creando uno...");
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.radius = 0.05f;
            capsule.height = 0.1f;
            capsule.center = Vector3.zero;
            Debug.Log("✅ CapsuleCollider Trigger creado automáticamente");
        }

        if (transform.parent == null)
        {
            Debug.LogError("❌❌❌ CRÍTICO: MilkPitcherDetector NO debe estar en el objeto raíz!");
            Debug.LogError("    Debe ser un HIJO de la jarra. Crea un objeto vacío como hijo.");
        }

        if (transform.parent != null)
        {
            Collider parentCollider = transform.parent.GetComponent<Collider>();
            if (parentCollider != null && parentCollider.isTrigger)
            {
                Debug.LogError("❌ ERROR: El collider del PADRE (jarra) está marcado como Trigger!");
                Debug.LogError("    Esto impide que XR Grab funcione. Desmarca 'Is Trigger' en el collider de la jarra.");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (milkPitcher == null) return;

        // Ignorar el cartón de leche
        if (ignoreCarton)
        {
            if (other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() != null ||
                other.GetComponent<MilkPourXR>() != null ||
                other.name.ToLower().Contains("carton"))
            {
                return;
            }
        }

        // Filtrar por layer si está configurado
        if (particleLayers != -1 && ((1 << other.gameObject.layer) & particleLayers) == 0)
        {
            return;
        }
        
        if (showDebugLogs && (milkDetectionCount + steamDetectionCount) < 5)
        {
            Debug.Log($"🔍 Colisión con: {other.name} | Layer: {LayerMask.LayerToName(other.gameObject.layer)} | Tag: {other.tag}");
        }

        // Detectar partículas de LECHE
        bool isMilkParticle = other.name.ToLower().Contains("particle") || 
                              other.name.ToLower().Contains("milk") ||
                              other.CompareTag("MilkParticle");
        
        if (isMilkParticle)
        {
            milkPitcher.OnMilkParticleDetected();
            milkDetectionCount++;

            if (showDebugLogs && milkDetectionCount % 30 == 0)
            {
                Debug.Log($"🥛 Partículas de LECHE detectadas ({milkDetectionCount} veces)");
            }
        }

        // Detectar partículas de VAPOR
        bool isSteamParticle = other.name.ToLower().Contains("steam") || 
                               other.name.ToLower().Contains("vapor") ||
                               other.CompareTag("SteamParticle");
        
        if (isSteamParticle)
        {
            milkPitcher.OnSteamDetected();
            steamDetectionCount++;

            if (showDebugLogs && steamDetectionCount % 30 == 0)
            {
                Debug.Log($"🌫️ Partículas de VAPOR detectadas ({steamDetectionCount} veces)");
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (milkPitcher == null) return;

        ParticleSystem ps = other.GetComponent<ParticleSystem>();
        
        if (ps == null)
        {
            if (showDebugLogs && (milkDetectionCount + steamDetectionCount) == 0)
            {
                Debug.LogWarning($"⚠️ OnParticleCollision llamado pero {other.name} no tiene ParticleSystem");
            }
            return;
        }

        int numCollisionEvents = ps.GetCollisionEvents(gameObject, collisionEvents);
        
        if (showDebugLogs && (milkDetectionCount + steamDetectionCount) == 0)
        {
            Debug.Log($"💥 Partículas de {other.name} colisionaron: {numCollisionEvents} eventos");
            Debug.Log($"   Sistema de partículas: {ps.name}");
            Debug.Log($"   Layer del sistema: {LayerMask.LayerToName(ps.gameObject.layer)}");
        }

        // Detectar partículas de LECHE
        bool isMilkParticle = other.name.ToLower().Contains("milk") || 
                              other.CompareTag("MilkParticle") ||
                              ps.name.ToLower().Contains("milk");
        
        if (isMilkParticle && numCollisionEvents > 0)
        {
            for (int i = 0; i < numCollisionEvents; i++)
            {
                milkPitcher.OnMilkParticleDetected();
            }
            
            milkDetectionCount += numCollisionEvents;
            
            if (showDebugLogs && milkDetectionCount % 30 == 0)
            {
                Debug.Log($"🥛 Partícula de LECHE colisionó ({milkDetectionCount} veces total)");
            }
            return;
        }

        // Detectar partículas de VAPOR
        bool isSteamParticle = other.name.ToLower().Contains("steam") || 
                               other.name.ToLower().Contains("vapor") ||
                               other.CompareTag("SteamParticle") ||
                               ps.name.ToLower().Contains("steam") ||
                               ps.name.ToLower().Contains("vapor");
        
        if (isSteamParticle && numCollisionEvents > 0)
        {
            for (int i = 0; i < numCollisionEvents; i++)
            {
                milkPitcher.OnSteamDetected();
            }
            
            steamDetectionCount += numCollisionEvents;
            
            if (showDebugLogs && steamDetectionCount % 30 == 0)
            {
                Debug.Log($"🌫️ Partícula de VAPOR colisionó ({steamDetectionCount} veces total)");
            }
            return;
        }

        // Si no es ni leche ni vapor
        if (showDebugLogs && (milkDetectionCount + steamDetectionCount) < 5)
        {
            Debug.Log($"ℹ️ Ignorando partículas de {other.name} (no es leche ni vapor)");
        }
    }
    
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
            else if (col is CapsuleCollider capsule)
            {
                Gizmos.DrawSphere(capsule.center, capsule.radius);
            }
        }
    }
}