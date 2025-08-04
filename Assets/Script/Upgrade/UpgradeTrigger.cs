using UnityEngine;
using UnityEngine.UI;

public class UpgradeTrigger : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;

    void Start()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(!upgradePanel.activeSelf);
            }
        }
    }
}