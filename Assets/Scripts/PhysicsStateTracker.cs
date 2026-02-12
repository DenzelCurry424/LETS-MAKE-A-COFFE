using UnityEngine;

public class PhysicsStateTracker : MonoBehaviour
{
    [Header("Estado Original (Solo Lectura)")]
    [SerializeField] private bool originalIsKinematic = false;
    [SerializeField] private bool originalUseGravity = true;
    
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            originalIsKinematic = false;
            originalUseGravity = true;
            
            Debug.Log($"📋 PhysicsStateTracker en {gameObject.name}:");
            Debug.Log($"   Física forzada: isKinematic=false, useGravity=true");
        }
        else
        {
            Debug.LogError($"❌ {gameObject.name} no tiene Rigidbody!");
        }
    }

    public void RestoreOriginalPhysics()
    {
        if (rb != null)
        {
            Debug.Log($"🔄 Restaurando física original de {gameObject.name}");
            
            rb.isKinematic = originalIsKinematic;
            rb.useGravity = originalUseGravity;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            Debug.Log($"   isKinematic: {rb.isKinematic}");
            Debug.Log($"   useGravity: {rb.useGravity}");
        }
    }

    public (bool isKinematic, bool useGravity) GetOriginalState()
    {
        return (originalIsKinematic, originalUseGravity);
    }

    public void ForceEnablePhysics()
    {
        if (rb != null)
        {
            Debug.Log($"⚡ Forzando física activa en {gameObject.name}");
            
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}