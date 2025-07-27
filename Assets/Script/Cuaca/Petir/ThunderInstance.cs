using UnityEngine;

public class ThunderInstance : MonoBehaviour
{
    private ThunderManager manager;
    private ThunderAttack attack;

    public void Initialize(ThunderManager manager)
    {
        this.manager = manager;
        attack = GetComponent<ThunderAttack>();

        if (attack == null)
        {
            Debug.LogError("ThunderInstance requires ThunderAttack component!");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.RemoveThunder(gameObject);
        }
    }
}