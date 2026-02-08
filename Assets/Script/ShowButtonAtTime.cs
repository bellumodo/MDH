using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ShowButtonAtTime : MonoBehaviour
{
    [Header("1. 控制哪个按钮？")]
    public Button targetButton;

    [Header("2. 触发条件")]
    [Tooltip("当这个视频播放时...")]
    public VideoClip targetClip;

    [Tooltip("...播放到第几秒显示按钮？")]
    public float showTime = 3.0f;

    // 内部私有变量，用来存找到的播放器
    private VideoPlayer autoFoundPlayer;

    void Start()
    {
        // 1. 隐藏按钮
        if (targetButton != null)
        {
            targetButton.gameObject.SetActive(false);
        }
        else
        {
            // 如果你把脚本挂在按钮自己身上，且没拖Target Button，那就默认控制自己
            targetButton = GetComponent<Button>();
            if (targetButton != null) targetButton.gameObject.SetActive(false);
        }

        // 2. 自动寻找场景里的播放器
        autoFoundPlayer = FindObjectOfType<VideoPlayer>();
    }

    void Update()
    {
        // --- 第一步：如果还没找到播放器，继续找 ---
        if (autoFoundPlayer == null)
        {
            autoFoundPlayer = FindObjectOfType<VideoPlayer>();
            return; // 还没找到，这帧先不干活
        }

        // --- 第二步：核心逻辑 ---

        // 如果播放器里没有视频，或者播的不是我们想要的那个 Clip
        if (autoFoundPlayer.clip != targetClip)
        {
            // 确保按钮隐藏
            if (targetButton != null && targetButton.gameObject.activeSelf)
                targetButton.gameObject.SetActive(false);
            return;
        }

        // 如果播的是对的视频，检查时间
        if (autoFoundPlayer.time >= showTime)
        {
            if (targetButton != null && !targetButton.gameObject.activeSelf)
            {
                targetButton.gameObject.SetActive(true);
            }
        }
        else
        {
            // 时间没到 (比如视频重播了)
            if (targetButton != null && targetButton.gameObject.activeSelf)
                targetButton.gameObject.SetActive(false);
        }
    }
}