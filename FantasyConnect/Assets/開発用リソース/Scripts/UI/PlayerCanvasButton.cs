using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerCanvasButton : MonoBehaviour
{
    [SerializeField]
    private InputActionReference SettingButton;
    [SerializeField]
    private InputActionReference InventoryButton;
    public MainController mainController;

    [SerializeField]
    private GameObject m_InventoryCanvas;
    [SerializeField]
    private GameObject m_SettingCanvas;
    [SerializeField]
    PlayerSystem playerSystem;
    [SerializeField]
    PlayerCameraController playerCameraController;

    public static bool isPaused = false;
    private void Start()
    {
        isPaused = false;
        mainController = new MainController();
        mainController.Enable();
        SetInput();
    }
    private void SetInput()
    {
        if (InventoryButton != null)
        {
            InventoryButton.action.started += SetInventoryScreen;
        }
        if (SettingButton != null)
        {
            SettingButton.action.started += SetSettingScreen;
        }
    }
    private void Update()
    {
        if (PlayerSystem.isHitStop)
            return;
        if (Input.GetKeyDown(KeyCode.I))
        {
            CloseSettingScreen();
            ActiveInventory();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            CloseInventory();
            ActiveSettingScreen();
        }
    }
    public void ActiveInventory()
    {
        isPaused = true;
        Cursor.visible = true;
        playerCameraController.isStop = true;
        playerSystem.isStop = true;
        m_InventoryCanvas.SetActive(true);
    }

    public void CloseInventory()
    {
        isPaused = false;
        Cursor.visible = false;
        playerCameraController.isStop = false;
        playerSystem.isStop = false;
        m_InventoryCanvas.SetActive(false);
    }
    public void ActiveSettingScreen()
    {
        isPaused = true;
        Cursor.visible = true;
        playerCameraController.isStop = true;
        playerSystem.isStop = true;
        m_SettingCanvas.SetActive(true);
    }
    public void CloseSettingScreen()
    {
        isPaused = false;
        Cursor.visible = false;
        playerCameraController.isStop = false;
        playerSystem.isStop = false;
        m_SettingCanvas.SetActive(false);
    }

    private void SetSettingScreen(InputAction.CallbackContext context)
    {
        CloseSettingScreen();
        ActiveInventory();
    }
    private void SetInventoryScreen(InputAction.CallbackContext context)
    {
        CloseInventory();
        ActiveSettingScreen();
    }
}
