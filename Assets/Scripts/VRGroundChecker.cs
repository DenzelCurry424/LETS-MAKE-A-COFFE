using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class VRGroundChecker : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Fuerza de gravedad aplicada")]
    public float gravity = 9.81f;
    
    [Tooltip("Distancia para detectar el suelo")]
    public float groundCheckDistance = 0.2f;
    
    [Tooltip("Layers que se consideran suelo")]
    public LayerMask groundLayers = -1;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public Color debugRayColor = Color.green;
    
    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            Debug.LogError("❌ No se encontró CharacterController!");
            enabled = false;
            return;
        }

        characterController.minMoveDistance = 0f;
        characterController.skinWidth = 0.08f;
        
        Debug.Log("✅ VRGroundChecker inicializado");
    }

    void Update()
    {
        CheckGround();
        ApplyGravity();
    }

    void CheckGround()
    {
        Vector3 rayStart = transform.position;
        float rayDistance = (characterController.height / 2f) + groundCheckDistance;
        
        isGrounded = Physics.Raycast(
            rayStart, 
            Vector3.down, 
            rayDistance, 
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
        
        if (showDebugInfo)
        {
            Debug.DrawRay(rayStart, Vector3.down * rayDistance, isGrounded ? Color.green : Color.red);
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        characterController.Move(velocity * Time.deltaTime);
        
        if (showDebugInfo && !isGrounded)
        {
            Debug.Log($"⚠️ Jugador NO está en el suelo! Velocidad Y: {velocity.y:F2}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        if (characterController != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayStart = transform.position;
            float rayDistance = (characterController.height / 2f) + groundCheckDistance;
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * rayDistance);
            Gizmos.DrawWireSphere(rayStart + Vector3.down * rayDistance, 0.1f);
        }
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }
}