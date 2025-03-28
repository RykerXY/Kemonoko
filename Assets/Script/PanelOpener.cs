using UnityEngine;

public class PanelOpener : MonoBehaviour
{
    [Header("Panel Settings")]
    public GameObject settingPanel; // Panel ที่ต้องการเปิด/ปิด
    void Update()
    {
        OpenSettingPanel(settingPanel);
    }

    public void OpenSettingPanel(GameObject panel)
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            panel.SetActive(!panel.activeSelf); // เปิด/ปิด Panel
        }
    }
}
