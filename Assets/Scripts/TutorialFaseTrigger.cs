using UnityEngine;
using UnityEngine.Events;

public class TutorialFaseTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Referencia al TutorialAudioManager")]
    public TutorialAudioManager tutorialManager;

    [Tooltip("Número de fase que activa este trigger (1-12)")]
    [Range(1, 12)]
    public int numeroFase = 1;

    [Tooltip("¿Activar automáticamente al iniciar?")]
    public bool activarAlInicio = false;

    [Tooltip("¿Activar solo una vez?")]
    public bool activarSoloUnaVez = true;

    [Header("Eventos Opcionales")]
    [Tooltip("Evento que se ejecuta cuando se activa la fase")]
    public UnityEvent onFaseActivada;

    private bool yaActivado = false;

    void Start()
    {

        if (tutorialManager == null)
        {
            tutorialManager = FindObjectOfType<TutorialAudioManager>();
            
            if (tutorialManager == null)
            {
                Debug.LogError($"No se encontró TutorialAudioManager en la escena. Asígnalo manualmente en {gameObject.name}");
            }
        }

        if (activarAlInicio && !yaActivado)
        {
            ActivarFase();
        }
    }

    public void ActivarFase()
    {
        if (activarSoloUnaVez && yaActivado)
        {
            return;
        }

        if (tutorialManager != null)
        {
            tutorialManager.ActivarFase(numeroFase);
            yaActivado = true;
            

            onFaseActivada?.Invoke();
            
            Debug.Log($"{gameObject.name} activó la fase {numeroFase} del tutorial");
        }
        else
        {
            Debug.LogError($"TutorialAudioManager no asignado en {gameObject.name}");
        }
    }

    public void ReiniciarTrigger()
    {
        yaActivado = false;
    }

    public void OnInteraccionCompletada()
    {
        ActivarFase();
    }
}