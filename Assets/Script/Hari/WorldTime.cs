using System;
using System.Collections;
using UnityEngine;

namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {
        public event EventHandler<TimeSpan> WorldTimeChanged;
        public event EventHandler<int> DayChanged;

        [SerializeField]
        private float _daylength; //in seconds

        private TimeSpan _currentTime;
        private int _currentDay = 1;
        private float _minutelength => _daylength / WorldTimeConstants.MinutesInDay;

        public int CurrentDay => _currentDay;
        private bool _isPaused = false; // Tambahkan variabel untuk status pause

        private void Start()
        {
            StartCoroutine(AddMinute());
        }

        private IEnumerator AddMinute()
        {
            while (true) // Gunakan loop infinite dengan yield return
            {
                if (!_isPaused) // Hanya tambahkan waktu jika tidak pause
                {
                    _currentTime += TimeSpan.FromMinutes(1);
                    WorldTimeChanged?.Invoke(this, _currentTime);

                    if (_currentTime.TotalMinutes >= WorldTimeConstants.MinutesInDay)
                    {
                        _currentTime = TimeSpan.Zero;
                        _currentDay++;
                        DayChanged?.Invoke(this, _currentDay);
                    }
                }

                yield return new WaitForSeconds(_minutelength);
            }
        }

        // Tambahkan metode untuk mengontrol pause
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
        }
    }
}