using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PauseManager : MonoBehaviour
{
    [Header("Pause UI")]
    public GameObject pauseMenuUI;

    [Header("Player References")]
    public Transform playerHead;

    [Header("Positioning Settings")]
    public float distanceInFront = 2f;
    public float heightOffset = -0.2f;

    public InputActionAsset inputActions;
    public InputActionProperty uiPressAction;

    private bool isPaused = false;

    void Update()
    {
        if (inputActions == null)
            Debug.LogError("InputActions not assigned to PauseManager.");
        // todo: replace with vr button to pause
        if (Input.GetKeyDown(KeyCode.Escape) || uiPressAction.action.triggered)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        PositionPauseScreen();
            
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // disable vr movement inputs
        inputActions.FindActionMap("XRI Left Locomotion").Disable();
        inputActions.FindActionMap("XRI Right Locomotion").Disable();
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // enable vr inputs
        inputActions.FindActionMap("XRI Left Locomotion").Enable();
        inputActions.FindActionMap("XRI Right Locomotion").Enable();
    }

    private void PositionPauseScreen()
    {
        Vector3 forward = playerHead.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 newPos = playerHead.position + forward * distanceInFront;
        newPos.y += heightOffset;

        pauseMenuUI.transform.position = newPos;
        pauseMenuUI.transform.rotation = Quaternion.LookRotation(forward);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScreen");
    }
}
