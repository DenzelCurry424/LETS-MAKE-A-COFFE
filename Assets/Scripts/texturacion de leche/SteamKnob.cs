using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class SteamKnob : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Objeto visual que rota (si es diferente del principal)")]
    public Transform knobVisual;
    
    [Tooltip("Sistema de partículas de vapor")]
    public ParticleSystem steamParticles;
    
    [Tooltip("Audio del vapor")]
    public AudioSource steamSound;
    
    [Header("Configuración de Rotación")]
    [Tooltip("Velocidad de rotación")]
    public float rotationSpeed = 100f;
    
    [Tooltip("Rotación mínima (grados)")]
    public float minRotation = 0f;
    
    [Tooltip("Rotación máxima (grados)")]
    public float maxRotation = 180f;
    
    [Tooltip("Eje de rotación local (X, Y o Z)")]
    public Vector3 rotationAxis = Vector3.forward; 
    
    [Header("Control de Vapor")]
    [Tooltip("Emisión máxima de partículas")]
    public float maxSteamEmission = 50f;
    
    [Tooltip("Volumen máximo del sonido")]
    [Range(0f, 1f)]
    public float maxVolume = 0.8f;
    
    [Header("Estado (Debug)")]
    [SerializeField] private float currentRotation = 0f;
    [SerializeField] private bool isGrabbed = false;
    [SerializeField] private float steamIntensity = 0f;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Vector3 lastHandPosition;
    private Quaternion initialRotation;

    private void Awake()
    {
        // VERIFICACIÓN: La perilla DEBE estar en Default para XR Grab
        int defaultLayer = LayerMask.NameToLayer("Default");
        if (gameObject.layer != defaultLayer)
        {
            Debug.LogWarning($"⚠️ SteamKnob en layer '{LayerMask.LayerToName(gameObject.layer)}' - cambiando a Default...");
            gameObject.layer = defaultLayer;
            Debug.Log("✅ SteamKnob layer cambiado a Default");
        }

        if (knobVisual == null)
        {
            knobVisual = transform;
        }

        initialRotation = knobVisual.localRotation;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            Debug.Log("✅ XRGrabInteractable agregado a la perilla");
        }

        grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Kinematic;
        grabInteractable.trackPosition = false; 
        grabInteractable.trackRotation = true;  

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);

        if (steamParticles != null)
        {
            var emission = steamParticles.emission;
            emission.rateOverTime = 0f;
            
            if (!steamParticles.main.loop)
            {
                var main = steamParticles.main;
                main.loop = true;
            }

            // Solo verificar que las colisiones estén habilitadas y configurar mensajes
            var collision = steamParticles.collision;
            if (collision.enabled)
            {
                collision.sendCollisionMessages = true;
                Debug.Log("✅ Colisiones de vapor detectadas y configuradas");
            }
            
            // Asignar tag de vapor a las partículas si es posible
            if (steamParticles.gameObject != gameObject)
            {
                steamParticles.gameObject.tag = "SteamParticle";
            }
            
            Debug.Log("✅ Sistema de partículas de vapor configurado");
        }
        else
        {
            Debug.LogWarning("⚠️ No se asignó ParticleSystem de vapor!");
        }

        if (steamSound != null)
        {
            steamSound.loop = true;
            steamSound.volume = 0f;
        }
        
        Debug.Log("✅ SteamKnob configurado correctamente");
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
        isGrabbed = true;
        lastHandPosition = args.interactorObject.transform.position;
        
        Debug.Log("🤲 Perilla agarrada");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        
        Debug.Log("✋ Perilla soltada");
    }

    private void Update()
    {
        if (isGrabbed && grabInteractable.isSelected)
        {
            UpdateKnobRotation();
        }
        
        UpdateSteam();
    }

    private void UpdateKnobRotation()
    {
        var interactor = grabInteractable.interactorsSelecting[0];
        Vector3 currentHandPosition = interactor.transform.position;

        Vector3 movement = currentHandPosition - lastHandPosition;

        Vector3 worldAxis = knobVisual.TransformDirection(rotationAxis.normalized);
        Vector3 knobToHand = (currentHandPosition - knobVisual.position).normalized;
        Vector3 tangent = Vector3.Cross(worldAxis, knobToHand).normalized;
        
        float rotationDelta = Vector3.Dot(movement, tangent) * rotationSpeed;

        currentRotation += rotationDelta;
        currentRotation = Mathf.Clamp(currentRotation, minRotation, maxRotation);

        Quaternion targetRotation = initialRotation * Quaternion.AngleAxis(currentRotation, rotationAxis.normalized);
        knobVisual.localRotation = targetRotation;
        
        lastHandPosition = currentHandPosition;
    }

    private void UpdateSteam()
    {
        steamIntensity = Mathf.InverseLerp(minRotation, maxRotation, currentRotation);

        if (steamParticles != null)
        {
            var emission = steamParticles.emission;
            emission.rateOverTime = steamIntensity * maxSteamEmission;

            if (steamIntensity > 0.01f && !steamParticles.isPlaying)
            {
                steamParticles.Play();
                Debug.Log("🌫️ Vapor activado - las partículas pueden vaporizar la leche");
            }
            else if (steamIntensity <= 0.01f && steamParticles.isPlaying)
            {
                steamParticles.Stop();
                Debug.Log("⏹️ Vapor desactivado");
            }
        }

        if (steamSound != null)
        {
            steamSound.volume = steamIntensity * maxVolume;
            
            if (steamIntensity > 0.01f && !steamSound.isPlaying)
            {
                steamSound.Play();
            }
            else if (steamIntensity <= 0.01f && steamSound.isPlaying)
            {
                steamSound.Stop();
            }
        }
    }

    public void ResetKnob()
    {
        currentRotation = 0f;
        steamIntensity = 0f;
        
        if (knobVisual != null)
        {
            knobVisual.localRotation = initialRotation;
        }
        
        if (steamParticles != null)
        {
            steamParticles.Stop();
            var emission = steamParticles.emission;
            emission.rateOverTime = 0f;
        }
        
        if (steamSound != null)
        {
            steamSound.Stop();
        }
        
        Debug.Log("🔄 Perilla reseteada");
    }

    public float GetSteamIntensity()
    {
        return steamIntensity;
    }
    
    public float GetCurrentRotation()
    {
        return currentRotation;
    }

    private void OnDrawGizmosSelected()
    {
        Transform visual = knobVisual != null ? knobVisual : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(visual.position, 0.02f);

        Gizmos.color = Color.blue;
        Vector3 worldAxis = visual.TransformDirection(rotationAxis.normalized);
        Gizmos.DrawRay(visual.position, worldAxis * 0.08f);
        Gizmos.DrawRay(visual.position, -worldAxis * 0.08f);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Vector3 currentDir = visual.TransformDirection(Vector3.right);
            Gizmos.DrawRay(visual.position, currentDir * 0.05f);
        }
    }
}