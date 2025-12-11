using UnityEngine;
using TMPro;

public class EscapeRoomTimer : MonoBehaviour
{
    [Header("UI Root (background panel + texts)")]
    [Tooltip("Parent GameObject that contains the timer background + Day/Clock texts.")]
    public GameObject uiRoot;

    [Header("Text")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI clockText;

    [Header("Timing")]
    [Tooltip("Number of in-game days before failure (visual).")]
    public int maxDays = 7;

    [Tooltip("Total real playtime, in minutes (e.g. 15).")]
    public float totalPlayMinutes = 15f;

    [Tooltip("Debug speed multiplier. 1 = real time, 60 = 1 real second = 1 in-game minute.")]
    public float timeScale = 1f;

    private float _secondsPerDay;
    private float _totalSeconds;
    private bool  _ended  = false;
    private bool  _active = false;   // set true after Level1Complete, false again after password

    [Header("Day Color Lerp")]
    public Color startColor = Color.white;
    public Color endColor   = Color.red;

    [Header("SFX & Screen Fade")]
    public AudioSource audioSource;
    public AudioClip day6AlarmSfx;
    public AudioClip finalAlarmSfx;
    public GameObject blackWall;

    private bool _day5VoPlayed    = false;
    private bool _day6AlarmPlayed = false;
    private bool _finalTriggered  = false;

    private const float FULL_DAY_SECONDS = 24f * 60f * 60f;

    void Awake()
    {
        if (uiRoot != null)
            uiRoot.SetActive(false);
    }

    void OnEnable()
    {
        GameManager.OnLevel1Complete       += HandleLevel1Complete;
        GameManager.OnPasswordPuzzleComplete += HandlePasswordComplete;
    }

    void OnDisable()
    {
        GameManager.OnLevel1Complete       -= HandleLevel1Complete;
        GameManager.OnPasswordPuzzleComplete -= HandlePasswordComplete;
    }

    void Start()
    {
        float totalPlaySeconds = totalPlayMinutes * 60f;
        _secondsPerDay = totalPlaySeconds / Mathf.Max(1, maxDays);
    }

    void HandleLevel1Complete()
    {
        _active = true;
        _totalSeconds = 0f;
        _ended = false;

        // reset flags in case of replay
        _day5VoPlayed = _day6AlarmPlayed = _finalTriggered = false;

        if (uiRoot != null)
            uiRoot.SetActive(true);

        if (blackWall != null)
            blackWall.SetActive(false);
    }

    // 🔒 STOP TIMER WHEN PASSWORD IS ENTERED
    void HandlePasswordComplete()
    {
        _active = false;
        _ended  = true;

        // hide UI if you want it gone after escape
        if (uiRoot != null)
            uiRoot.SetActive(false);

        // stop any timer SFX
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        // make sure fail black wall never appears after success
        if (blackWall != null)
            blackWall.SetActive(false);
    }

    void Update()
    {
        if (!_active || _ended) return;

        _totalSeconds += Time.deltaTime * timeScale;

        float totalDaysFloat = _totalSeconds / _secondsPerDay;
        int currentDay = Mathf.FloorToInt(totalDaysFloat) + 1;

        if (currentDay > maxDays)
        {
            currentDay = maxDays;
            _ended = true;
        }

        float dayRealSeconds = _totalSeconds % _secondsPerDay;
        float fractionThroughDay = Mathf.Clamp01(dayRealSeconds / _secondsPerDay);

        float displayDaySeconds = fractionThroughDay * FULL_DAY_SECONDS;

        int hours   = Mathf.FloorToInt(displayDaySeconds / 3600f);
        int minutes = Mathf.FloorToInt((displayDaySeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(displayDaySeconds % 60f);

        if (dayText != null)
            dayText.text = $"Day {currentDay}/{maxDays}";

        if (clockText != null)
            clockText.text = $"{hours:00}:{minutes:00}:{seconds:00}";

        if (dayText != null)
        {
            float t = Mathf.InverseLerp(1f, maxDays, currentDay);
            dayText.color = Color.Lerp(startColor, endColor, t);
        }

        HandleEvents(currentDay);
    }

    void HandleEvents(int currentDay)
    {
        if (!_active || _ended) return;

        // Day 5 VO
        if (currentDay >= 5 && !_day5VoPlayed)
        {
            _day5VoPlayed = true;
            if (SoundPuzzleVOController.Instance != null)
                SoundPuzzleVOController.Instance.CueTimerDay5Warning();
        }

        // Day 6 SFX
        if (currentDay >= 6 && !_day6AlarmPlayed)
        {
            _day6AlarmPlayed = true;
            PlaySfx(day6AlarmSfx);
        }

        // Day 7 fail
        if (!_finalTriggered && currentDay >= maxDays)
        {
            _finalTriggered = true;

            PlaySfx(finalAlarmSfx);

            if (SoundPuzzleVOController.Instance != null)
                SoundPuzzleVOController.Instance.CueTimerFailDay7();

            if (blackWall != null)
                blackWall.SetActive(true);

            _ended  = true;
            _active = false;
        }
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.Play();
    }

    [ContextMenu("Debug: Start Timer Now")]
    void DebugStartNow()
    {
        _active = true;
        _ended = false;
        if (uiRoot != null) uiRoot.SetActive(true);
    }
}
