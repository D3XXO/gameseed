using UnityEngine;
using TMPro;
using UnityEngine.UI; // Required for Image component
using WorldTime;
using System.Collections;

namespace WorldTime
{
    public class WeatherForecastUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text weatherText;
        [SerializeField] private Image weatherIcon; // Optional icon display
        [SerializeField] private GameObject warningBadge; // Extra warning element

        [Header("Visuals")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color stormColor = Color.red;
        [SerializeField] private Sprite normalIcon;
        [SerializeField] private Sprite stormIcon;

        private void Start()
        {
            UpdateForecast();
            WorldTime.Instance.DayChanged += OnDayChanged;

            // Keep UI always visible
            if (warningBadge != null)
                warningBadge.SetActive(false);
        }

        private void OnDayChanged(object sender, int day)
        {
            UpdateForecast();
        }

        private void UpdateForecast()
        {
            bool isStormy = CheckForStormsTonight();
            string forecastText = isStormy ? "THUNDERSTORM" : "NORMAL";
            Color textColor = isStormy ? stormColor : normalColor;

            weatherText.text = forecastText;
            weatherText.color = textColor;

            // Update icon if assigned
            if (weatherIcon != null)
            {
                weatherIcon.sprite = isStormy ? stormIcon : normalIcon;
                weatherIcon.color = textColor;
            }

            // Flash warning badge for storms
            if (warningBadge != null)
            {
                warningBadge.SetActive(isStormy);
                if (isStormy)
                    StartCoroutine(FlashWarning());
            }
        }

        private IEnumerator FlashWarning()
        {
            Image badgeImage = warningBadge.GetComponent<Image>();
            while (warningBadge.activeSelf)
            {
                float alpha = Mathf.PingPong(Time.time * 2f, 1f);
                badgeImage.color = new Color(stormColor.r, stormColor.g, stormColor.b, alpha);
                yield return null;
            }
        }

        private bool CheckForStormsTonight()
        {
            return WorldTime.Instance.CurrentDay >= 3 &&
                   Random.value <= DisasterManager.Instance.dailyDisasterChance;
        }

        private void OnDestroy()
        {
            if (WorldTime.Instance != null)
                WorldTime.Instance.DayChanged -= OnDayChanged;
        }
    }
}