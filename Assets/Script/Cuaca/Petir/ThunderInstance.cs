using UnityEngine;

public class ThunderInstance : MonoBehaviour
{
    private ThunderManager manager;

    public void Initialize(ThunderManager manager)
    {
        this.manager = manager;
    }

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.RemoveThunder(gameObject);
        }
    }
}