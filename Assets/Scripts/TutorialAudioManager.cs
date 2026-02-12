using UnityEngine;
using System.Collections;

public class TutorialAudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Audio Clips - Tutorial Phases")]
    [Tooltip("1. Audio cuando el jugador toma el filtro")]
    public AudioClip audio1_TomarFiltro;
    
    [Tooltip("2. Audio cuando pone el filtro en la molienda")]
    public AudioClip audio2_FiltroEnMolienda;
    
    [Tooltip("3. Audio cuando toma el filtro y lo pone en la balanza")]
    public AudioClip audio3_FiltroEnBalanza;
    
    [Tooltip("4. Audio cuando toma el tamper y prensa el café")]
    public AudioClip audio4_UsarTamper;
    
    [Tooltip("5. Audio cuando coloca el filtro en la cafetera")]
    public AudioClip audio5_FiltroEnCafetera;
    
    [Tooltip("6. Audio cuando pone la taza en la cafetera")]
    public AudioClip audio6_TazaEnCafetera;
    
    [Tooltip("7. Audio cuando presiona el botón de extracción")]
    public AudioClip audio7_IniciarExtraccion;
    
    [Tooltip("8. Audio cuando vierte leche en la jarra")]
    public AudioClip audio8_VertirLeche;
    
    [Tooltip("9. Audio cuando purga la lanceta")]
    public AudioClip audio9_PurgarLanceta;
    
    [Tooltip("10a. Audio cuando inicia la vaporización")]
    public AudioClip audio10a_InicioVaporizacion;
    
    [Tooltip("10b. Audio cuando termina la vaporización")]
    public AudioClip audio10b_FinVaporizacion;
    
    [Tooltip("11. Audio cuando vierte la leche en la taza")]
    public AudioClip audio11_VertirLecheEnTaza;
    
    [Tooltip("12. Audio final de la experiencia")]
    public AudioClip audio12_Final;

    [Header("Settings")]
    [Tooltip("Volumen de los audios (0-1)")]
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Tooltip("Delay antes de reproducir cada audio (segundos)")]
    public float delayBeforeAudio = 0.5f;

    private int currentPhase = 0;
    private bool isPlayingAudio = false;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.volume = volume;
        audioSource.playOnAwake = false;
    }

    public void ActivarSiguienteFase()
    {
        currentPhase++;
        ReproducirAudioFase(currentPhase);
    }

    public void ActivarFase(int numeroFase)
    {
        currentPhase = numeroFase;
        ReproducirAudioFase(numeroFase);
    }

    private void ReproducirAudioFase(int fase)
    {
        if (isPlayingAudio)
        {
            StopAllCoroutines();
        }

        AudioClip clipToPlay = null;

        switch (fase)
        {
            case 1:
                clipToPlay = audio1_TomarFiltro;
                break;
            case 2:
                clipToPlay = audio2_FiltroEnMolienda;
                break;
            case 3:
                clipToPlay = audio3_FiltroEnBalanza;
                break;
            case 4:
                clipToPlay = audio4_UsarTamper;
                break;
            case 5:
                clipToPlay = audio5_FiltroEnCafetera;
                break;
            case 6:
                clipToPlay = audio6_TazaEnCafetera;
                break;
            case 7:
                clipToPlay = audio7_IniciarExtraccion;
                break;
            case 8:
                clipToPlay = audio8_VertirLeche;
                break;
            case 9:
                clipToPlay = audio9_PurgarLanceta;
                break;
            case 10:
                StartCoroutine(ReproducirAudiosVaporizacion());
                return;
            case 11:
                clipToPlay = audio11_VertirLecheEnTaza;
                break;
            case 12:
                clipToPlay = audio12_Final;
                break;
            default:
                Debug.LogWarning($"Fase {fase} no existe en el tutorial");
                return;
        }

        if (clipToPlay != null)
        {
            StartCoroutine(ReproducirAudioConDelay(clipToPlay));
        }
        else
        {
            Debug.LogWarning($"No hay audio asignado para la fase {fase}");
        }
    }

    private IEnumerator ReproducirAudioConDelay(AudioClip clip)
    {
        isPlayingAudio = true;
        
        yield return new WaitForSeconds(delayBeforeAudio);
        
        audioSource.clip = clip;
        audioSource.Play();
        
        Debug.Log($"Reproduciendo audio de la fase {currentPhase}: {clip.name}");
        
        yield return new WaitForSeconds(clip.length);
        
        isPlayingAudio = false;
    }

    private IEnumerator ReproducirAudiosVaporizacion()
    {
        isPlayingAudio = true;

        if (audio10a_InicioVaporizacion != null)
        {
            yield return new WaitForSeconds(delayBeforeAudio);
            audioSource.clip = audio10a_InicioVaporizacion;
            audioSource.Play();
            Debug.Log($"Reproduciendo audio fase 10a: {audio10a_InicioVaporizacion.name}");
            yield return new WaitForSeconds(audio10a_InicioVaporizacion.length);
        }

        yield return new WaitForSeconds(0.3f);

        if (audio10b_FinVaporizacion != null)
        {
            audioSource.clip = audio10b_FinVaporizacion;
            audioSource.Play();
            Debug.Log($"Reproduciendo audio fase 10b: {audio10b_FinVaporizacion.name}");
            yield return new WaitForSeconds(audio10b_FinVaporizacion.length);
        }

        isPlayingAudio = false;
    }

    public void DetenerAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        StopAllCoroutines();
        isPlayingAudio = false;
    }

    public void ReiniciarTutorial()
    {
        DetenerAudio();
        currentPhase = 0;
    }

    public int ObtenerFaseActual()
    {
        return currentPhase;
    }

    public bool EstaReproduciendoAudio()
    {
        return isPlayingAudio;
    }

    public void CambiarVolumen(float nuevoVolumen)
    {
        volume = Mathf.Clamp01(nuevoVolumen);
        audioSource.volume = volume;
    }
}