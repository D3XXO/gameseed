using System;
using UnityEngine;
using TMPro;

namespace WorldTime
{
    public class DayCounter : MonoBehaviour
    {
        [SerializeField] private WorldTime _worldTime;
        [SerializeField] private TMP_Text _dayText;

        private int _currentDay = 1;

        private void Awake()
        {
            _worldTime.WorldTimeChanged += OnTimeChanged;
            UpdateDayDisplay();
        }

        private void OnDestroy()
        {
            _worldTime.WorldTimeChanged -= OnTimeChanged;
        }

        private void OnTimeChanged(object sender, TimeSpan time)
        {
            // Check if it's midnight (00:00)
            if (time.Hours == 0 && time.Minutes == 0)
            {
                _currentDay++;
                UpdateDayDisplay();
            }
        }

        private void UpdateDayDisplay()
        {
            if (_dayText != null)
            {
                _dayText.text = $"Day {_currentDay}";
            }
        }
    }
}