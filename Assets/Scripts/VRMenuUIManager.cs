using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VRMenuUIManager : MonoBehaviour
{
    [Header("Paneles del Menú")]
    [Tooltip("Panel principal con botón INICIAR y MÓDULOS")]
    public GameObject mainMenuPanel;
    
    [Tooltip("Panel de selección de módulos")]
    public GameObject modulesPanel;
    
    [Tooltip("Panel de configuración de audio")]
    public GameObject settingsPanel;
    
    [Header("Referencias del Menú Principal")]
    public Button playButton;
    public Button modulesButton;
    public Button settingsButton;
    public Button exitButton;
    
    [Header("Referencias de Módulos")]
    public Button module1Button;
    public Button module2Button;
    public Button module3Button;
    public Button module4Button;
    public Button backFromModulesButton;
    public TextMeshProUGUI selectedModuleText;
    
    [Header("Referencias de Configuración")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    public Button backFromSettingsButton;
    
    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    
    [Header("Referencias Externas")]
    public VRMenuManager menuManager;
    
    private AudioSource audioSource;
    private int selectedModule = 1; 

    private const string PREF_MASTER_VOLUME = "MasterVolume";
    private const string PREF_MUSIC_VOLUME = "MusicVolume";
    private const string PREF_SFX_VOLUME = "SFXVolume";
    private const string PREF_SELECTED_MODULE = "SelectedModule";

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);
        
        if (modulesButton != null)
            modulesButton.onClick.AddListener(OnModulesButtonClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);

        if (module1Button != null)
            module1Button.onClick.AddListener(() => OnModuleSelected(1));
        
        if (module2Button != null)
            module2Button.onClick.AddListener(() => OnModuleSelected(2));
        
        if (module3Button != null)
            module3Button.onClick.AddListener(() => OnModuleSelected(3));
        
        if (module4Button != null)
            module4Button.onClick.AddListener(() => OnModuleSelected(4));
        
        if (backFromModulesButton != null)
            backFromModulesButton.onClick.AddListener(OnBackFromModules);

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(OnBackFromSettings);

        LoadSettings();

        ShowMainMenu();
        
        Debug.Log("✅ VRMenuUIManager inicializado");
    }

    void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveAllListeners();
        if (modulesButton != null) modulesButton.onClick.RemoveAllListeners();
        if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
        if (exitButton != null) exitButton.onClick.RemoveAllListeners();
        if (module1Button != null) module1Button.onClick.RemoveAllListeners();
        if (module2Button != null) module2Button.onClick.RemoveAllListeners();
        if (module3Button != null) module3Button.onClick.RemoveAllListeners();
        if (module4Button != null) module4Button.onClick.RemoveAllListeners();
        if (backFromModulesButton != null) backFromModulesButton.onClick.RemoveAllListeners();
        if (backFromSettingsButton != null) backFromSettingsButton.onClick.RemoveAllListeners();
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveAllListeners();
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveAllListeners();
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveAllListeners();
    }
    
    void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (modulesPanel != null) modulesPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
    
    void ShowModulesPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (modulesPanel != null) modulesPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        UpdateSelectedModuleDisplay();
    }
    
    void ShowSettingsPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (modulesPanel != null) modulesPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    void OnPlayButtonClicked()
    {
        PlayClickSound();
        Debug.Log($"🎮 Iniciando módulo {selectedModule}");
        
        if (menuManager != null)
        {
            menuManager.StartGame();
        }
        else
        {
            Debug.LogError("❌ MenuManager no asignado!");
        }
    }
    
    void OnModulesButtonClicked()
    {
        PlayClickSound();
        Debug.Log("📚 Abriendo panel de módulos");
        ShowModulesPanel();
    }
    
    void OnSettingsButtonClicked()
    {
        PlayClickSound();
        Debug.Log("⚙️ Abriendo configuración");
        ShowSettingsPanel();
    }
    
    void OnExitButtonClicked()
    {
        PlayClickSound();
        Debug.Log("👋 Saliendo del juego");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void OnModuleSelected(int moduleNumber)
    {
        PlayClickSound();
        selectedModule = moduleNumber;
        PlayerPrefs.SetInt(PREF_SELECTED_MODULE, selectedModule);
        PlayerPrefs.Save();
        
        Debug.Log($"📦 Módulo {moduleNumber} seleccionado");
        UpdateSelectedModuleDisplay();
    }
    
    void OnBackFromModules()
    {
        PlayClickSound();
        ShowMainMenu();
    }
    
    void OnBackFromSettings()
    {
        PlayClickSound();
        ShowMainMenu();
    }

    void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        UpdateVolumeText(masterVolumeText, value);
        PlayerPrefs.SetFloat(PREF_MASTER_VOLUME, value);
    }
    
    void OnMusicVolumeChanged(float value)
    {
        UpdateVolumeText(musicVolumeText, value);
        PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, value);
        Debug.Log($"🎵 Volumen de música: {Mathf.RoundToInt(value * 100)}%");
    }
    
    void OnSFXVolumeChanged(float value)
    {
        if (audioSource != null)
        {
            audioSource.volume = value;
        }
        UpdateVolumeText(sfxVolumeText, value);
        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, value);
        Debug.Log($"🔊 Volumen de SFX: {Mathf.RoundToInt(value * 100)}%");
    }
    
    void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        if (text != null)
        {
            text.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    void LoadSettings()
    {
        float masterVol = PlayerPrefs.GetFloat(PREF_MASTER_VOLUME, 1f);
        float musicVol = PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME, 0.8f);
        float sfxVol = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 1f);
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVol;
            AudioListener.volume = masterVol;
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVol;
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVol;
        }

        selectedModule = PlayerPrefs.GetInt(PREF_SELECTED_MODULE, 1);
        
        Debug.Log($"⚙️ Configuración cargada - Módulo: {selectedModule}");
    }
    
    void UpdateSelectedModuleDisplay()
    {
        if (selectedModuleText != null)
        {
            string moduleName = GetModuleName(selectedModule);
            selectedModuleText.text = $"Módulo Actual: {moduleName}";
        }

        HighlightSelectedModule();
    }
    
    void HighlightSelectedModule()
    {
        ResetButtonColor(module1Button);
        ResetButtonColor(module2Button);
        ResetButtonColor(module3Button);
        ResetButtonColor(module4Button);

        Button selectedButton = null;
        switch (selectedModule)
        {
            case 1: selectedButton = module1Button; break;
            case 2: selectedButton = module2Button; break;
            case 3: selectedButton = module3Button; break;
            case 4: selectedButton = module4Button; break;
        }
        
        if (selectedButton != null)
        {
            ColorBlock colors = selectedButton.colors;
            colors.normalColor = new Color(0.3f, 0.8f, 0.3f); 
            selectedButton.colors = colors;
        }
    }
    
    void ResetButtonColor(Button button)
    {
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            button.colors = colors;
        }
    }
    
    string GetModuleName(int moduleNumber)
    {
        switch (moduleNumber)
        {
            case 1: return "Preparación Básica";
            case 2: return "Arte Latte";
            case 3: return "Métodos de Extracción";
            case 4: return "Servicio al Cliente";
            default: return "Desconocido";
        }
    }

    void PlayClickSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    public void PlayHoverSound()
    {
        if (audioSource != null && buttonHoverSound != null)
        {
            audioSource.PlayOneShot(buttonHoverSound);
        }
    }

    public int GetSelectedModule()
    {
        return selectedModule;
    }
    
    public void SetSelectedModule(int moduleNumber)
    {
        if (moduleNumber >= 1 && moduleNumber <= 4)
        {
            OnModuleSelected(moduleNumber);
        }
    }
}