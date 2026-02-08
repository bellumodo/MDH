using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] // Ensures this script is only added to Buttons
public class ButtonSoundEffect : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip clickSound; // Drag your MP3/WAV file here
    [Range(0f, 1f)]
    public float volume = 1.0f;  // Adjust volume (0.0 to 1.0)

    private AudioSource audioSource;
    private Button button;

    void Start()
    {
        // 1. Get the Button component
        button = GetComponent<Button>();

        // 2. Add an AudioSource component automatically to this object
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false; // Prevent playing sound immediately on start

        // 3. Add the click listener
        if (button != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    void PlaySound()
    {
        // Check if a sound file is assigned
        if (clickSound != null && audioSource != null)
        {
            // PlayOneShot is better for UI because it allows overlapping sounds
            // (e.g., if you click fast, it won't cut off the previous sound)
            audioSource.PlayOneShot(clickSound, volume);
        }
        else
        {
            Debug.LogWarning("No Audio Clip assigned to button: " + gameObject.name);
        }
    }
}