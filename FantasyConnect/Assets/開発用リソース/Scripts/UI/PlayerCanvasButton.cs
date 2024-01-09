using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCanvasButton : MonoBehaviour
{
    [SerializeField]
    private GameObject m_InventoryCanvas;
    [SerializeField]
    private GameObject m_SettingCanvas;
    [SerializeField]
    PlayerSystem playerSystem;
    [SerializeField]
    PlayerCameraController playerCameraController;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            CloseSettingScreen();
            ActiveInventory();
        }
        if(Input.GetKeyDown(KeyCode.U))
        {
            CloseInventory();
            ActiveSettingScreen();
        }
    }
    public void ActiveInventory()
    {
        Cursor.visible = true;
        playerCameraController.isStop = true;
        playerSystem.isStop = true;
        m_InventoryCanvas.SetActive(true);
    }

    public void CloseInventory()
    {
        Cursor.visible = false;
        playerCameraController.isStop = false;
        playerSystem.isStop = false;
        m_InventoryCanvas.SetActive(false);
    }
    public void ActiveSettingScreen()
    {
        Cursor.visible = true;
        playerCameraController.isStop = true;
        playerSystem.isStop = true;
        m_SettingCanvas.SetActive(true);
    }
    public void CloseSettingScreen()
    {
        Cursor.visible = false;
        playerCameraController.isStop = false;
        playerSystem.isStop = false;
        m_SettingCanvas.SetActive(false);
    }
}
