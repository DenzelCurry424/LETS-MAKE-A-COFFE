using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    [Header("Configuración del Snap")]
    [Tooltip("Tag del objeto que puede engancharse aquí")]
    public string acceptedTag = "Filter";
    
    [Tooltip("Distancia máxima para hacer snap")]
    public float snapDistance = 0.15f;
    
    [Tooltip("¿Este snap está activo?")]
    public bool isActive = true;
    
    [Tooltip("¿Liberar el objeto automáticamente al interactuar?")]
    public bool allowManualRelease = true;
    
    [Header("Referencias")]
    [Tooltip("Objeto visual que indica el punto de snap")]
    public GameObject snapIndicator;
    
    [Header("Estado")]
    public bool isOccupied = false;
    public GameObject snappedObject = null;
    
    [Header("Eventos")]
    public UnityEngine.Events.UnityEvent onObjectSnapped;
    public UnityEngine.Events.UnityEvent onObjectReleased;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable snappedGrabInteractable;
    
    private void Start()
    {
        if (snapIndicator != null)
            snapIndicator.SetActive(isActive && !isOccupied);
    }
    
    private void Update()
    {
        if (snapIndicator != null)
            snapIndicator.SetActive(isActive && !isOccupied);

        if (isOccupied && snappedObject != null)
        {
            if (!snappedObject.activeInHierarchy)
            {
                Debug.LogError($"{snappedObject.name} fue desactivado!");
                snappedObject.SetActive(true);
            }
        }

        if (allowManualRelease && isOccupied && snappedGrabInteractable != null)
        {
            if (snappedGrabInteractable.isSelected)
            {
                Debug.Log($"Usuario agarró {snappedObject.name}, liberando");
                ReleaseObject();
            }
        }
    }
    
    public bool TrySnap(GameObject obj)
    {
        if (!isActive || isOccupied) return false;
        if (!obj.CompareTag(acceptedTag)) return false;
        
        float distance = Vector3.Distance(transform.position, obj.transform.position);
        if (distance > snapDistance) return false;
        
        SnapObject(obj);
        return true;
    }
    
    private void SnapObject(GameObject obj)
    {
        snappedObject = obj;
        isOccupied = true;

        snappedGrabInteractable = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;
        obj.SetActive(true);

        Debug.Log($"{obj.name} enganchado en {gameObject.name}");

        onObjectSnapped?.Invoke();
    }

    public void ReleaseObject()
    {
        if (snappedObject == null) return;
    
        GameObject obj = snappedObject;
    
        Debug.Log($"Liberando {obj.name} de {gameObject.name}");

        bool userIsGrabbing = snappedGrabInteractable != null && snappedGrabInteractable.isSelected;
        
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null && userIsGrabbing)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            Debug.Log($"Usuario agarró el objeto, física activada");
        }
        else if (rb != null)
        {
            Debug.Log($"Liberación por código, PhysicsEnforcer manejará la física");
        }

        if (!obj.activeInHierarchy)
        {
            Debug.LogWarning($"{obj.name} estaba desactivado, reactivando...");
            obj.SetActive(true);
        }

        snappedObject = null;
        snappedGrabInteractable = null;
        isOccupied = false;
    
        onObjectReleased?.Invoke();
    
        Debug.Log($"{obj.name} liberado del snap");
    }
    
    public void ReleaseObjectSilent()
    {
        if (snappedObject == null) return;
        
        GameObject obj = snappedObject;
        
        obj.transform.SetParent(null);
        
        snappedObject = null;
        snappedGrabInteractable = null;
        isOccupied = false;
        
        onObjectReleased?.Invoke();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.01f);
    }
}