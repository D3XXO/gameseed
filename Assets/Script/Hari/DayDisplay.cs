using TMPro;
using UnityEngine;
using WorldTime;

public class DayDisplay : MonoBehaviour
{
    [SerializeField] private WorldTime.WorldTime _worldTime;
    [SerializeField] private TMP_Text _dayText;

    private void Awake()
    {
        _worldTime.DayChanged += OnDayChanged;
        _dayText.text = $"Day {_worldTime.CurrentDay}";
    }

    private void OnDayChanged(object sender, int day)
    {
        _dayText.text = $"Day {day}";
    }

    private void OnDestroy()
    {
        _worldTime.DayChanged -= OnDayChanged;
    }
}