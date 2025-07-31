using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {
        public static WorldTime Instance { get; private set; }

        public event EventHandler<TimeSpan> WorldTimeChanged;
        public event EventHandler<int> DayChanged;
        
        [Header("Time Settings")]
        [SerializeField] private float _daylength;
        [SerializeField] private float fadeDuration = 2f;

        private const string WarningPanelName = "WarningPanel";
        private const string FadePanelName = "FadePanel";

        private GameObject warningPanel;
        private Image fadePanel;
        
        private TimeSpan _currentTime = new TimeSpan(21, 0, 0);
        private int _currentDay = 1;
        private float _minutelength => _daylength / WorldTimeConstants.MinutesInDay;

        public int CurrentDay => _currentDay;
        public TimeSpan CurrentTime => _currentTime;

        private bool _isPaused = false;
        private const int DaysInMonth = 30;
        
        private bool _isWarningShown = false;
        private bool _isForceEndTriggered = false;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            StartCoroutine(AddMinute());
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FindUIElementsInScene();
        }

        private void FindUIElementsInScene()
        {
            warningPanel = null;
            fadePanel = null;

            GameObject foundWarningObj = GameObject.Find(WarningPanelName);
            if (foundWarningObj != null)
            {
                warningPanel = foundWarningObj;
                warningPanel.SetActive(false);
            }

            GameObject foundFadeObj = GameObject.Find(FadePanelName);
            if (foundFadeObj != null)
            {
                fadePanel = foundFadeObj.GetComponent<Image>();
                if (fadePanel != null)
                {
                    fadePanel.gameObject.SetActive(true);
                    fadePanel.color = new Color(0f, 0f, 0f, 0f);
                }
            }
        }


        private IEnumerator AddMinute()
        {
            while (true)
            {
                if (!_isPaused)
                {
                    _currentTime += TimeSpan.FromMinutes(1);
                    WorldTimeChanged?.Invoke(this, _currentTime);
                    
                    if (_currentTime.Hours == 2 && _currentTime.Minutes == 40 && !_isWarningShown)
                    {
                        _isWarningShown = true;

                        if (warningPanel != null)
                        {
                            warningPanel.SetActive(true);
                        }
                    }
                    
                    if (_currentTime.Hours == 3 && _currentTime.Minutes == 0 && !_isForceEndTriggered)
                    {
                        _isForceEndTriggered = true;
                        SetPaused(true);
                        
                        if (fadePanel != null)
                        {
                            StartCoroutine(FadeAndLoadScene());
                        }
                        else
                        {
                            SceneManager.LoadScene("Harbour");
                        }
                    }
                }
                yield return new WaitForSeconds(_minutelength);
            }
        }
        
        private IEnumerator FadeAndLoadScene()
        {
            float elapsedTime = 0f;
            Color startColor = new Color(0f, 0f, 0f, 0f);
            Color endColor = new Color(0f, 0f, 0f, 1f);

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                fadePanel.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
                yield return null;
            }
            fadePanel.color = endColor;

            SceneManager.LoadScene("Harbour");
        }

        public void SetTimeAndDay(TimeSpan newTime, int newDay)
        {
            _currentTime = newTime;
            _currentDay = newDay;
            _isWarningShown = false;
            _isForceEndTriggered = false;
            WorldTimeChanged?.Invoke(this, _currentTime);
            DayChanged?.Invoke(this, _currentDay);
        }

        public void SetPaused(bool paused)
        {
            _isPaused = paused;
        }

        public void ResetTimeAndAdvanceDay()
        {
            _currentTime = new TimeSpan(21, 0, 0);
            _currentDay++;
            if (_currentDay > DaysInMonth) _currentDay = 1;
            
            _isWarningShown = false;
            _isForceEndTriggered = false;

            if (warningPanel != null) warningPanel.SetActive(false);
            if (fadePanel != null) fadePanel.color = new Color(0f, 0f, 0f, 0f);

            SetPaused(false);

            WorldTimeChanged?.Invoke(this, _currentTime);
            DayChanged?.Invoke(this, _currentDay);
        }
    }
}