using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace WorldTime
{
    [RequireComponent(typeof(TMP_Text))]

    public class WorldTimeDisplay : MonoBehaviour
    {
        private WorldTime _worldtime;
        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();

            _worldtime = WorldTime.Instance;

            if (_worldtime != null)
            {
                _worldtime.WorldTimeChanged += OnWorldTimeChanged;
            }
        }

        private void OnDestroy()
        {
            if (_worldtime != null)
            {
                _worldtime.WorldTimeChanged -= OnWorldTimeChanged;
            }
        }

        private void OnWorldTimeChanged(object sender, TimeSpan newTime)
        {
            _text.SetText(newTime.ToString(@"hh\:mm"));
        }
    }
}