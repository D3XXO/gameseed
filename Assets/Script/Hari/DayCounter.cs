using UnityEngine;
using TMPro;

namespace WorldTime
{
    public class DayCounter : MonoBehaviour
    {
        [SerializeField] private WorldTime _worldTime;
        [SerializeField] private TMP_Text _dayText;

        private void Awake()
        {
            _worldTime.DayChanged += OnDayChanged;
            _dayText.text = $"Night {_worldTime.CurrentDay}";
        }

        private void OnDayChanged(object sender, int day)
        {
            _dayText.text = $"Night {day}";
        }

        private void OnDestroy()
        {
            _worldTime.DayChanged -= OnDayChanged;
        }
    }
}