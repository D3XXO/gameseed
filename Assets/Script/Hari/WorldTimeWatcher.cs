using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace WorldTime
{
    public class WorldTimeWatcher : MonoBehaviour
    {
        private WorldTime _worldtime;
        [SerializeField]
        private List<Schedule> _schedule;

        private void Start()
        {
            _worldtime = WorldTime.Instance;

            if (_worldtime != null)
            {
                _worldtime.WorldTimeChanged += CheckSchedule;
            }
        }

        private void OnDestroy()
        {
            if (_worldtime != null)
            {
                _worldtime.WorldTimeChanged -= CheckSchedule;
            }
        }

        private void CheckSchedule(object sender, TimeSpan newTime)
        {
            var schedule =
                _schedule.FirstOrDefault(s =>
                s.hour == newTime.Hours &&
                s.minute == newTime.Minutes);

            schedule?._action?.Invoke();
        }

        [Serializable]
        private class Schedule
        {
            public int hour;
            public int minute;
            public UnityEvent _action;
        }
    }
}