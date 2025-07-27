using System;
using System.Collections;
using UnityEngine;

namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {
        public event EventHandler<TimeSpan> WorldTimeChanged;
        public event EventHandler<int> DayChanged; // New event for day change

        [SerializeField]
        private float _daylength; //in seconds

        private TimeSpan _currentTime;
        private int _currentDay = 1; // Day counter starts at 1
        private float _minutelength => _daylength / WorldTimeConstants.MinutesInDay;

        public int CurrentDay => _currentDay; // Public property to access current day

        private void Start()
        {
            StartCoroutine(AddMinute());
        }

        private IEnumerator AddMinute()
        {
            _currentTime += TimeSpan.FromMinutes(1);
            WorldTimeChanged?.Invoke(this, _currentTime);

            // Check if a new day has started
            if (_currentTime.TotalMinutes >= WorldTimeConstants.MinutesInDay)
            {
                _currentTime = TimeSpan.Zero;
                _currentDay++;
                DayChanged?.Invoke(this, _currentDay);
            }

            yield return new WaitForSeconds(_minutelength);
            StartCoroutine(AddMinute());
        }
    }
}