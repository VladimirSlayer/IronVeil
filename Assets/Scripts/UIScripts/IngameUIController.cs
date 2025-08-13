using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class IngameUIController : MonoBehaviour
{
    public static IngameUIController Instance { get; private set; }

    public GameObject gameplayUI;
    public GameObject pauseMenuUI;
    public GameObject characterMenuUI;
    public GameObject inventoryUI;
    public Image overlayFadeImage;
    public GameObject settingsMenuUI;
    public Slider musicSlider;
    public Slider brightnessSlider;
    public Volume ppv;

    public GameUIState currentState { get; private set; } = GameUIState.Gameplay;

    private Dictionary<GameUIState, System.Action> enterStateActions;
    private Dictionary<GameUIState, System.Action> exitStateActions;


    public void ResumeGame() => SwitchState(GameUIState.Gameplay);

    public void OpenSettings()
    {
        SwitchState(GameUIState.Settings);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        InitializeStateActions();
        var skillsUI = characterMenuUI.GetComponent<CharacterMenuUISwitcher>().skillsMenu.GetComponent<CharacterSkillsUI>();
        if (skillsUI != null)
            skillsUI.InitializeUI();
    }

    private void Start()
    {
        SwitchState(GameUIState.Gameplay); 
        Time.timeScale = 1f;
    }


    void Update()
    {
        if (!GetComponent<DialogueUI>().isDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameUIState.PauseMenu)
                    SwitchState(GameUIState.Gameplay);
                else if (currentState == GameUIState.Gameplay)
                    SwitchState(GameUIState.PauseMenu);
                else if (currentState == GameUIState.Settings)
                    SwitchState(GameUIState.PauseMenu);
                else
                    SwitchState(GameUIState.Gameplay);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                SwitchState(currentState == GameUIState.Inventory ? GameUIState.Gameplay : GameUIState.Inventory);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                SwitchState(currentState == GameUIState.CharacterMenu ? GameUIState.Gameplay : GameUIState.CharacterMenu);
            }
        }
    }

    public void SwitchState(GameUIState newState)
    {
        if (newState == currentState) return;

        exitStateActions[currentState]?.Invoke();
        currentState = newState;
        enterStateActions[newState]?.Invoke();
    }

    private void InitializeStateActions()
    {
        enterStateActions = new Dictionary<GameUIState, System.Action>
        {
            [GameUIState.Gameplay] = EnterGameplay,
            [GameUIState.PauseMenu] = EnterPauseMenu,
            [GameUIState.CharacterMenu] = EnterCharacterMenu,
            [GameUIState.Inventory] = EnterInventory,
            [GameUIState.Settings] = EnterSettings
        };

        exitStateActions = new Dictionary<GameUIState, System.Action>
        {
            [GameUIState.Gameplay] = ExitGameplay,
            [GameUIState.PauseMenu] = ExitPauseMenu,
            [GameUIState.CharacterMenu] = ExitCharacterMenu,
            [GameUIState.Inventory] = ExitInventory,
            [GameUIState.Settings] = ExitSettings
        };
    }

    public void CameraControlling(bool camEnabled)
    {
        PlayerCamController cam = FindObjectOfType<PlayerCamController>();
        if (cam != null)
        {
            cam.camEnabled = camEnabled;
        }
    }

    private void CallPauseOnAll(bool pause)
    {
        foreach (var pausable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (pausable is IPausable handler)
            {
                if (pause) handler.Pause();
                else handler.Resume();
            }
        }
    }


    private void EnterGameplay()
    {
        Time.timeScale = 1f;
        gameplayUI.SetActive(true);
        pauseMenuUI.SetActive(false);
        characterMenuUI.SetActive(false);
        inventoryUI.SetActive(false);
        SetFade(false);
        LockCursor(true);
        CallPauseOnAll(false);
    }

    private void EnterPauseMenu()
    {
        Time.timeScale = 0f;
        gameplayUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        SetFade(true);
        LockCursor(false);

        CameraControlling(false);
        CallPauseOnAll(true);
    }

    private void EnterCharacterMenu()
    {
        Time.timeScale = 0f;
        gameplayUI.SetActive(false);
        characterMenuUI.SetActive(true);
        SetFade(true);
        LockCursor(false);
        CameraControlling(false);
        var skillsUI = characterMenuUI.GetComponent<CharacterMenuUISwitcher>().skillsMenu.GetComponent<CharacterSkillsUI>();
        if (skillsUI != null)
            skillsUI.InitializeUI();
        characterMenuUI.GetComponent<CharacterMenuUISwitcher>().statsEquipMenu.GetComponent<CharacterEquipAndStatsUI>().RefreshUI();
    }

    private void EnterInventory()
    {
        Time.timeScale = 0f;
        gameplayUI.SetActive(false);
        inventoryUI.SetActive(true);
        SetFade(true);
        LockCursor(false);
        CameraControlling(false);
    }

    private void EnterSettings()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
        CameraControlling(false);
    }

    public void ChangeMusicVolumeSlider()
    {
        SoundManager.Instance.SetMusicVolume(musicSlider.value);
    }

    public void ChangeBrightnessSlider(){
        ColorAdjustments colorAdjustments = new ColorAdjustments();
        ppv.profile.TryGet<ColorAdjustments>(out colorAdjustments);
        colorAdjustments.postExposure.value = brightnessSlider.value - 0.3f;
    }

    private void ExitGameplay()
    {
        CameraControlling(true);
    }
    private void ExitPauseMenu()
    {
        pauseMenuUI.SetActive(false);
        CameraControlling(true);
    }
    private void ExitCharacterMenu()
    {
        characterMenuUI.SetActive(false);
        CameraControlling(true);
    }
    private void ExitInventory()
    {
        inventoryUI.SetActive(false);
        CameraControlling(true);
    }
    private void ExitSettings()
    {
        pauseMenuUI.SetActive(true);
        settingsMenuUI.SetActive(false);
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void SetFade(bool enabled)
    {
        if (overlayFadeImage != null)
        {
            overlayFadeImage.gameObject.SetActive(enabled);
            overlayFadeImage.color = new Color(0f, 0f, 0f, 0.4f); 
        }
    }
}
