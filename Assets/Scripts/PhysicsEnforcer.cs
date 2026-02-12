using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PhysicsEnforcer : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("¿Forzar física activa cuando no está siendo agarrado ni en snap?")]
    public bool enforcePhysicsWhenFree = true;
    
    [Tooltip("Intervalo de verificación en segundos")]
    public float checkInterval = 0.2f;
    
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    
    private float lastCheckTime = 0f;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (rb == null)
        {
            Debug.LogError($"❌ {gameObject.name} no tiene Rigidbody!");
            enabled = false;
            return;
        }
        
        if (grabInteractable == null)
        {
            Debug.LogError($"❌ {gameObject.name} no tiene XRGrabInteractable!");
            enabled = false;
            return;
        }

        grabInteractable.selectExited.AddListener(OnReleased);
        
        Debug.Log($"✅ PhysicsEnforcer activado en {gameObject.name}");
    }
    
    private void Update()
    {
        if (!enforcePhysicsWhenFree) return;

        if (Time.time - lastCheckTime < checkInterval) return;
        lastCheckTime = Time.time;
        
        CheckAndFixPhysics();
    }
    
    private void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log($"✋ {gameObject.name} fue soltado por el usuario");

        StartCoroutine(DelayedPhysicsCheck());
    }
    
    private System.Collections.IEnumerator DelayedPhysicsCheck()
    {
        yield return new WaitForSeconds(0.3f);

        if (!IsInSnapPoint() && !grabInteractable.isSelected)
        {
            Debug.Log($"🔍 {gameObject.name} quedó libre, verificando física...");
            ForceCorrectPhysics();
        }
        else
        {
            Debug.Log($"🔍 {gameObject.name} está en snap o siendo agarrado, no tocar física");
        }
    }
    
    private void CheckAndFixPhysics()
    {
        if (IsInSnapPoint())
        {
            return;
        }
        
        if (grabInteractable.isSelected)
        {
            return;
        }

        if (rb.isKinematic || !rb.useGravity)
        {
            Debug.LogWarning($"⚠️ {gameObject.name} está libre pero con física incorrecta!");
            Debug.LogWarning($"   isKinematic: {rb.isKinematic} (debería ser false)");
            Debug.LogWarning($"   useGravity: {rb.useGravity} (debería ser true)");
            
            ForceCorrectPhysics();
        }
    }

    private bool IsInSnapPoint()
    {
        SnapPoint[] snapPoints = FindObjectsOfType<SnapPoint>();
        
        foreach (SnapPoint snap in snapPoints)
        {
            if (snap.snappedObject == gameObject && snap.isOccupied)
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void ForceCorrectPhysics()
    {
        if (rb == null) return;
        
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        Debug.Log($"✅ Física forzada en {gameObject.name} (objeto LIBRE):");
        Debug.Log($"   isKinematic: {rb.isKinematic}");
        Debug.Log($"   useGravity: {rb.useGravity}");
    }
    
    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}