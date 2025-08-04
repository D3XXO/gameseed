using UnityEngine;
using UnityEngine.UI;

public class PanelBlock : MonoBehaviour
{
    [Header("Panel dan Objek")]
    public GameObject[] blockingPanels;
    public GameObject[] objectsToBlock;

    void Update()
    {
        bool anyPanelActive = false;
        foreach (GameObject panel in blockingPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                anyPanelActive = true;
                break;
            }
        }
        
        foreach (GameObject obj in objectsToBlock)
        {
            if (obj == null) continue;
            
            Button btn = obj.GetComponent<Button>();
            if (btn != null) btn.interactable = !anyPanelActive;
            
            Collider2D col = obj.GetComponent<Collider2D>();
            if (col != null) col.enabled = !anyPanelActive;
        }
    }

    void Awake()
    {
        foreach (GameObject panel in blockingPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}