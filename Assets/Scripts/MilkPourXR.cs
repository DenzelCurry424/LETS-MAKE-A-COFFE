using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class MilkPourXR : MonoBehaviour
{
    [Header("Referencias")]
    public ParticleSystem milkParticles;
    
    [Header("Configuración de Vertido")]
    [Tooltip("Ángulo mínimo para empezar a verter (grados)")]
    public float minPourAngle = 45f;
    
    [Tooltip("Ángulo máximo de vertido (grados)")]
    public float maxPourAngle = 135f;
    
    [Header("Ejes de Rotación")]
    [Tooltip("Eje principal de inclinación (X, Y o Z)")]
    public RotationAxis mainAxis = RotationAxis.Z;
    
    [Tooltip("Invertir el eje")]
    public bool invertAxis = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private float currentAngle = 0f;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private bool isHeld = false;
    
    public enum RotationAxis { X, Y, Z }

    void Start()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);

        if (milkParticles != null)
        {
            milkParticles.Stop();
            var main = milkParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            Debug.Log("✅ MilkPourXR configurado correctamente");
        }
        else
        {
            Debug.LogError("❌ No se asignó el ParticleSystem de leche!");
        }
    }

    void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrab);
            grab.selectExited.RemoveListener(OnRelease);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
        if (showDebugLogs) Debug.Log("✋ Cartón agarrado");
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        if (milkParticles != null)
            milkParticles.Stop();
        if (showDebugLogs) Debug.Log("🤚 Cartón soltado");
    }

    void Update()
    {
        if (!isHeld || milkParticles == null)
            return;

        Vector3 localRotation = transform.localEulerAngles;
        
        switch (mainAxis)
        {
            case RotationAxis.X:
                currentAngle = localRotation.x;
                break;
            case RotationAxis.Y:
                currentAngle = localRotation.y;
                break;
            case RotationAxis.Z:
                currentAngle = localRotation.z;
                break;
        }

        if (currentAngle > 180f)
            currentAngle -= 360f;

        if (invertAxis)
            currentAngle = -currentAngle;

        bool shouldPour = currentAngle >= minPourAngle && currentAngle <= maxPourAngle;

        if (showDebugLogs && Time.frameCount % 30 == 0)
        {
            Debug.Log($"🔄 Eje {mainAxis}: {currentAngle:F1}° | Vertiendo: {shouldPour}");
        }

        if (shouldPour)
        {
            if (!milkParticles.isPlaying)
            {
                milkParticles.Play();
                if (showDebugLogs) Debug.Log($"✅ VERTIENDO! Ángulo: {currentAngle:F1}°");
            }
        }
        else
        {
            if (milkParticles.isPlaying)
            {
                milkParticles.Stop();
                if (showDebugLogs) Debug.Log($"⏹️ Detenido. Ángulo: {currentAngle:F1}°");
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.cyan;
        Vector3 direction = Vector3.zero;
        
        switch (mainAxis)
        {
            case RotationAxis.X:
                direction = transform.right;
                break;
            case RotationAxis.Y:
                direction = transform.up;
                break;
            case RotationAxis.Z:
                direction = transform.forward;
                break;
        }
        
        if (invertAxis) direction = -direction;
        
        Gizmos.DrawRay(transform.position, direction * 0.5f);
    }
}