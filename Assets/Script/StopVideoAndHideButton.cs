using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StopVideoAndHideButton : MonoBehaviour
{
    [Header("Assignments")]
    public VideoPlayer videoPlayer;  // The video player component
    public GameObject screenObject;  // The RawImage object (the blue screen)
    public Button myButton;          // The button itself

    void Start()
    {
        // Automatically listen for the click event
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        // 1. Stop the video immediately
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        // 2. Hide the screen (RawImage)
        if (screenObject != null)
        {
            screenObject.SetActive(false);
        }

        // 3. Hide the button itself immediately
        if (myButton != null)
        {
            myButton.gameObject.SetActive(false);
        }
    }
}