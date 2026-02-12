using UnityEngine;
using UnityEngine.UI;

public class VRMenuManager : MonoBehaviour
{
    [Header("Puntos de Teletransporte")]
    public Transform menuSpawnPoint;
    public Transform gameStartPoint;
    
    [Header("Referencias")]
    public Transform xrOrigin;
    public GameObject menuCanvas;
    
    [Header("Fade Screen")]
    public Image fadeImage;
    
    [Header("Configuración")]
    public float fadeSpeed = 2f;
    
    [Header("Sonidos")]
    public AudioClip buttonClickSound;
    
    private bool gameStarted = false;
    private AudioSource audioSource;
    private bool isFading = false;

    void Start()
    {
        Debug.Log("=== VR MENU MANAGER INICIADO ===");

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        TeleportToPosition(menuSpawnPoint);

        if (menuCanvas != null)
        {
            menuCanvas.SetActive(true);
        }

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
            Debug.Log("✅ Fade Image configurada (Alpha = 0)");
        }
        
        Debug.Log("📋 Apunta a los botones con el rayo y presiona el gatillo");
    }

    public void StartGame()
    {
        if (gameStarted || isFading)
        {
            Debug.Log("⚠️ Ya está iniciando el juego...");
            return;
        }
        
        Debug.Log("🎮 ========== INICIANDO JUEGO ==========");

        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        gameStarted = true;
        StartCoroutine(TransitionToGame());
    }

    private System.Collections.IEnumerator TransitionToGame()
    {
        isFading = true;

        Debug.Log("🌑 Iniciando FADE TO BLACK...");
        yield return FadeToBlack();
        
        Debug.Log("⬛ Pantalla completamente negra");
        yield return new WaitForSeconds(0.3f);

        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }

        TeleportToPosition(gameStartPoint);
        
        yield return new WaitForSeconds(0.3f);

        Debug.Log("☀️ Iniciando FADE FROM BLACK...");
        yield return FadeFromBlack();
        
        Debug.Log("☕ ¡JUEGO DE BARISMO INICIADO!");
        
        isFading = false;
    }

    private System.Collections.IEnumerator FadeToBlack()
    {
        if (fadeImage == null) yield break;
        
        float alpha = 0f;
        Color color = fadeImage.color;
        
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            color.a = Mathf.Clamp01(alpha);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 1f;
        fadeImage.color = color;
    }

    private System.Collections.IEnumerator FadeFromBlack()
    {
        if (fadeImage == null) yield break;
        
        float alpha = 1f;
        Color color = fadeImage.color;
        
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            color.a = Mathf.Clamp01(alpha);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 0f;
        fadeImage.color = color;
    }

    private void TeleportToPosition(Transform targetPoint)
    {
        if (xrOrigin == null || targetPoint == null)
        {
            Debug.LogError("❌ XR Origin o punto de spawn no asignado!");
            return;
        }

        CharacterController cc = xrOrigin.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Vector3 safePosition = targetPoint.position + Vector3.up * 0.1f;
        xrOrigin.position = safePosition;
        xrOrigin.rotation = targetPoint.rotation;

        Physics.SyncTransforms();
        
        if (cc != null)
        {
            StartCoroutine(ReEnableCharacterController(cc));
        }
        
        Debug.Log($"📍 Teletransportado a: {targetPoint.name}");
    }
    
    private System.Collections.IEnumerator ReEnableCharacterController(CharacterController cc)
    {
        yield return new WaitForFixedUpdate();
        if (cc != null) cc.enabled = true;
    }

    public void ReturnToMenu()
    {
        gameStarted = false;
        
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(true);
        }
        
        TeleportToPosition(menuSpawnPoint);
    }
}