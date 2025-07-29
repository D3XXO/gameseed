using TMPro;
using UnityEngine;
using WorldTime;

public class DayDisplay : MonoBehaviour
{
    private WorldTime.WorldTime _worldTime;
    [SerializeField] private TMP_Text _dayText;

    private void Awake()
    {
        _worldTime = WorldTime.WorldTime.Instance;

        if (_worldTime != null)
        {
            _worldTime.DayChanged += OnDayChanged;
            _dayText.text = $"Night {_worldTime.CurrentDay}";
        }
    }

    private void OnDayChanged(object sender, int day)
    {
        _dayText.text = $"Night {day}";
    }

    private void OnDestroy()
    {
        if (_worldTime != null)
        {
            _worldTime.DayChanged -= OnDayChanged;
        }
    }
}