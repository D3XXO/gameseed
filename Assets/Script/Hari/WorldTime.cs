using System;
using System.Collections;
using UnityEngine;

namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {

        public event EventHandler<TimeSpan> WorldTimeChanged;

        [SerializeField]
        private float _daylength; //in seconds
        private TimeSpan _currentTime;
        private float _minutelength => _daylength / WorldTimeConstants.MinutesInDay;

        private void Start()
        {
            StartCoroutine(AddMinute());

        }
        private IEnumerator AddMinute() // kalo error ada di coroutine, di add minute ada routine harusnya
        {
            _currentTime += TimeSpan.FromMinutes(1);
            WorldTimeChanged?.Invoke(this, _currentTime);
            yield return new WaitForSeconds(_minutelength);
            StartCoroutine(AddMinute());
        }
    }
}