using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapableObject : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Radio para buscar snap points cercanos")]
    public float snapSearchRadius = 0.2f;
    
    [Tooltip("¿Auto-snap al soltar cerca de un snap point?")]
    public bool autoSnapOnRelease = true;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private SnapPoint currentSnapPoint;
    
    private void Awake()
    {
        Debug.Log($"SnapableObject.Awake en {gameObject.name}");

        var allGrabInteractables = GetComponents<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (allGrabInteractables.Length > 1)
        {
            Debug.LogError($"{gameObject.name} tiene {allGrabInteractables.Length} XRGrabInteractable!");
            Debug.LogError($"   Esto causará problemas. Verifica la configuración del prefab.");
        }

        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (grabInteractable == null)
        {
            Debug.LogError($"{gameObject.name} NO tiene XRGrabInteractable!");
            Debug.LogError($"   Agrega el componente manualmente en el Inspector.");
            return;
        }
        
        Debug.Log($"XRGrabInteractable encontrado en {gameObject.name}");

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
        
        Debug.Log($"Eventos suscritos correctamente");
    }
    
    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
    
    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log($"{gameObject.name} fue agarrado");

        if (currentSnapPoint != null)
        {
            Debug.Log($"   Estaba en snap point {currentSnapPoint.name}, liberando...");
            currentSnapPoint.ReleaseObject();
            currentSnapPoint = null;
        }
    }
    
    private void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log($"{gameObject.name} fue soltado");
        
        if (!autoSnapOnRelease)
        {
            Debug.Log($"   Auto-snap deshabilitado");
            return;
        }

        SnapPoint nearestSnap = FindNearestSnapPoint();
        
        if (nearestSnap != null)
        {
            Debug.Log($"   Snap point cercano encontrado: {nearestSnap.name}");
            
            if (nearestSnap.TrySnap(gameObject))
            {
                currentSnapPoint = nearestSnap;
                Debug.Log($"Enganchado exitosamente en {nearestSnap.name}");
            }
            else
            {
                Debug.LogWarning($"No se pudo enganchar en {nearestSnap.name}");
            }
        }
        else
        {
            Debug.Log($"   No hay snap points cercanos");
        }
    }
    
    private SnapPoint FindNearestSnapPoint()
    {
        SnapPoint[] allSnapPoints = FindObjectsOfType<SnapPoint>();
        SnapPoint nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (SnapPoint snap in allSnapPoints)
        {
            if (!gameObject.CompareTag(snap.acceptedTag)) continue;
            if (!snap.isActive || snap.isOccupied) continue;
            
            float distance = Vector3.Distance(transform.position, snap.transform.position);
            
            if (distance < snapSearchRadius && distance < minDistance)
            {
                nearest = snap;
                minDistance = distance;
            }
        }
        
        return nearest;
    }
    
    public void ForceSnapTo(SnapPoint snapPoint)
    {
        if (snapPoint.TrySnap(gameObject))
        {
            currentSnapPoint = snapPoint;
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, snapSearchRadius);
    }
}