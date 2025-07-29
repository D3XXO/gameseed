using System;
using System.Collections;
using UnityEngine;

namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {
        public static WorldTime Instance { get; private set; }

        public event EventHandler<TimeSpan> WorldTimeChanged;
        public event EventHandler<int> DayChanged;

        public event EventHandler WorldTimeEndNight;

        [SerializeField]
        private float _daylength; //in seconds

        private TimeSpan _currentTime = new TimeSpan(21, 0, 0);
        private int _currentDay = 1;
        private float _minutelength => _daylength / WorldTimeConstants.MinutesInDay;

        public int CurrentDay => _currentDay;
        public TimeSpan CurrentTime => _currentTime;

        private bool _isPaused = false;
        // --- BARU: Flag untuk memastikan notifikasi akhir malam hanya muncul sekali ---
        private bool _isNightEndNotified = false;
        private const int DaysInMonth = 30;

        private void Awake()
        {
            Debug.Log($"WorldTime Awake: Instance Status: {(Instance != null ? Instance.gameObject.name + "(active)" : "NULL")}. This Object: {gameObject.name} ({this.GetType().Name})");

            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"WorldTime: Destroying duplicate instance on {gameObject.name}. An instance already exists: {Instance.gameObject.name}.");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log($"WorldTime: Setting {gameObject.name} as the singleton instance and DontDestroyOnLoad.");
            }
        }

        private void Start()
        {
            WorldTimeChanged?.Invoke(this, _currentTime);
            DayChanged?.Invoke(this, _currentDay);

            if (Instance == this)
            {
                StartCoroutine(AddMinute());
            }
            else
            {
                Debug.LogWarning("WorldTime Start: This instance is not the singleton. Coroutine will not start for " + gameObject.name + ".");
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

                    // --- PERBAIKAN LOGIKA 03:00 ---
                    // Jika waktu mencapai 03:00 dan notifikasi belum ditampilkan
                    if (_currentTime.Hours == 3 && _currentTime.Minutes == 0 && !_isNightEndNotified)
                    {
                        SetPaused(true); // Hentikan waktu
                        _isNightEndNotified = true; // Set flag agar notifikasi tidak muncul berulang
                        Debug.Log("WorldTime: Reached 03:00 AM. Time paused. Triggering end night notification.");
                        WorldTimeEndNight?.Invoke(this, EventArgs.Empty); // Panggil event
                    }
                    // Logika pergantian hari akan dipindahkan ke tempat lain,
                    // yaitu setelah pemain memilih untuk pulang atau dipaksa pulang.
                }

                yield return new WaitForSeconds(_minutelength);
            }
        }

        public void SetTimeAndDay(TimeSpan newTime, int newDay)
        {
            _currentTime = newTime;
            _currentDay = newDay;
            // --- BARU: Reset flag notifikasi saat waktu diatur ulang (misal: setelah load game) ---
            _isNightEndNotified = false;

            WorldTimeChanged?.Invoke(this, _currentTime);
            DayChanged?.Invoke(this, _currentDay);
        }

        public void SetPaused(bool paused)
        {
            _isPaused = paused;
            Debug.Log($"WorldTime Paused state set to: {_isPaused} for {gameObject.name}.");
        }

        // --- BARU: Metode untuk melanjutkan waktu setelah 03:00 (jika pemain menolak pulang) ---
        public void ContinueTimeAfterNightEnd()
        {
            _isNightEndNotified = true; // Pastikan tetap true agar notifikasi tidak muncul lagi
            SetPaused(false); // Lanjutkan waktu
            Debug.Log("WorldTime: Time continued after night end notification.");
        }

        // --- BARU: Metode untuk mereset waktu dan mengganti hari (dipanggil setelah pulang ke Harbour) ---
        public void ResetTimeAndAdvanceDay()
        {
            _currentTime = new TimeSpan(21, 0, 0); // Atur kembali ke 21:00 (awal malam)
            _currentDay++; // Majukan hari
            if (_currentDay > DaysInMonth)
            {
                _currentDay = 1;
            }
            _isNightEndNotified = false; // Reset flag untuk malam berikutnya
            SetPaused(false); // Pastikan waktu tidak ter-pause saat memulai malam baru
            Debug.Log($"WorldTime: Time reset to {_currentTime.ToString(@"hh\:mm")} and day advanced to {_currentDay}.");

            WorldTimeChanged?.Invoke(this, _currentTime);
            DayChanged?.Invoke(this, _currentDay);
        }


        private void OnDestroy()
        {
            if (Instance == this)
            {
                Debug.LogWarning($"WorldTime OnDestroy: The singleton instance ({gameObject.name}) is being destroyed! This should generally not happen if DontDestroyOnLoad is working as expected. Check other scripts for explicit Destroy() calls targeting {gameObject.name} or its parent.");
                Instance = null;
            }
            else
            {
                Debug.Log($"WorldTime OnDestroy: A non-singleton instance ({gameObject.name}) is being destroyed (as expected).");
            }
        }
    }
}