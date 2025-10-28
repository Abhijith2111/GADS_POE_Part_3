using UnityEngine;
using UnityEngine.UI; // for Button / Image
using UnityEngine.Events;

#if TMP_PRESENT
using TMPro; // optional - if you use TextMeshPro
#endif

public class MuteAllButton : MonoBehaviour
{
    const string PREF_KEY = "Muted"; // 0 = unmuted, 1 = muted
    const float DEFAULT_VOLUME = 1f;

    [Header("UI (optional)")]
    public Button muteButton;           // assign your Button (optional if using OnClick in inspector)
    public Image buttonIcon;            // optional: change sprite on toggle
    public Sprite iconMuted;
    public Sprite iconUnmuted;
    public Text buttonText;             // optional: Unity UI Text to show "Muted"/"Unmuted"
    // If you use TextMeshPro, uncomment the following line and assign TMP text in inspector:
    // public TextMeshProUGUI buttonTextTMP;

    void Start()
    {
        // Apply saved state on start
        bool isMuted = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
        ApplyMute(isMuted);

        // Hook up button if provided
        if (muteButton != null)
        {
            // Prevent adding multiple listeners if script is reloaded
            muteButton.onClick.RemoveListener(ToggleMute);
            muteButton.onClick.AddListener(ToggleMute);
        }
    }

    public void ToggleMute()
    {
        bool currentlyMuted = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
        ApplyMute(!currentlyMuted);
    }

    void ApplyMute(bool muted)
    {
        // Set global audio on/off
        AudioListener.volume = muted ? 0f : DEFAULT_VOLUME;

        // Save preference
        PlayerPrefs.SetInt(PREF_KEY, muted ? 1 : 0);
        PlayerPrefs.Save();

        // Update icon if assigned
        if (buttonIcon != null && iconMuted != null && iconUnmuted != null)
            buttonIcon.sprite = muted ? iconMuted : iconUnmuted;

        // Update text if assigned
        if (buttonText != null)
            buttonText.text = muted ? "Muted" : "Unmuted";

        // If you use TextMeshPro, uncomment and use:
        // if (buttonTextTMP != null) buttonTextTMP.text = muted ? "Muted" : "Unmuted";
    }

    // Optional public setter if you want to call from other scripts (e.g. a toggle)
    public void SetMuted(bool muted) => ApplyMute(muted);
}