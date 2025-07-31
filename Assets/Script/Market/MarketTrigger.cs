using UnityEngine;
using UnityEngine.SceneManagement;

public class MarketTrigger : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (SceneManager.GetActiveScene().name == "Harbour")
            {
                if (InventoryUI.Instance != null)
                {
                    InventoryUI.Instance.ToggleInventory();
                }
            }
        }
    }
}