using UnityEngine;
using TMPro;
using WorldTime;

namespace WorldTime
{
    public class DayCounter : MonoBehaviour
    {
        private WorldTime _worldTime;
        [SerializeField] private TMP_Text _dayText;

        private void Awake()
        {
            _worldTime = WorldTime.Instance;

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
}