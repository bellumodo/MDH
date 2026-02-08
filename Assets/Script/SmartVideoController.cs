using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;

public class SmartVideoController : MonoBehaviour
{
    [Header("核心组件")]
    public VideoPlayer videoPlayer;
    public RawImage fadeOverlay; // 黑色遮罩 (UI RawImage)

    [System.Serializable]
    public class VideoStage
    {
        public string note;             // 备注
        public VideoClip clip;          // 视频片段
        public Button triggerButton;    // 交互按钮

        [Tooltip("按钮在视频播放到第几秒时出现？\n0 = 一开始就出现")]
        public float showButtonTime = 0f;

        [Tooltip("自动切换时间 (秒)：\n0 = 【不自动切换】视频播完停在最后一帧，必须按按钮才切\n>0 = 播放到该时间点自动切")]
        public float autoSwitchTime = 0f;
    }

    [Header("剧情列表")]
    public List<VideoStage> stageList;

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.color = Color.clear;
        }

        if (stageList.Count > 0)
        {
            PlayStage(0);
        }
    }

    void Update()
    {
        if (isTransitioning || currentIndex >= stageList.Count) return;

        VideoStage current = stageList[currentIndex];

        // --- 1. 按钮出现时间控制 ---
        if (current.triggerButton != null)
        {
            if (!current.triggerButton.gameObject.activeSelf && current.showButtonTime > 0)
            {
                if (videoPlayer.time >= current.showButtonTime)
                {
                    current.triggerButton.gameObject.SetActive(true);
                }
            }
        }

        // --- 2. 视频播放状态控制 ---

        // 情况A: 设定了具体时间自动切
        if (current.autoSwitchTime > 0)
        {
            if (videoPlayer.time >= current.autoSwitchTime)
            {
                GoToNextVideo();
            }
        }
        // 情况B: 没设定时间 (0)，意思是播完就停住等按钮
        else
        {
            // 如果视频正在播放，且已经到了最后几帧
            if (videoPlayer.isPlaying && videoPlayer.frameCount > 0)
            {
                // 检测是否播放到了最后 (保留最后2帧作为缓冲)
                if ((ulong)videoPlayer.frame >= videoPlayer.frameCount - 2)
                {
                    videoPlayer.Pause(); // 【关键】暂停！这样画面就定格了
                    // 注意：这里不要 Stop()，Stop会黑屏
                }
            }
        }
    }

    void PlayStage(int index)
    {
        currentIndex = index;
        VideoStage stage = stageList[index];

        // 1. 播放视频
        videoPlayer.Stop();
        videoPlayer.clip = stage.clip;

        // 【关键修改】我们要定格，所以永远不要循环
        videoPlayer.isLooping = false;

        videoPlayer.Play();

        // 2. 设置按钮状态
        HideAllButtons();

        if (stage.triggerButton != null)
        {
            bool shouldShowImmediately = (stage.showButtonTime <= 0);
            stage.triggerButton.gameObject.SetActive(shouldShowImmediately);

            stage.triggerButton.onClick.RemoveAllListeners();
            stage.triggerButton.onClick.AddListener(OnBtnClick);
        }
    }

    void OnBtnClick()
    {
        if (!isTransitioning)
        {
            GoToNextVideo();
        }
    }

    void GoToNextVideo()
    {
        HideAllButtons();

        if (currentIndex + 1 < stageList.Count)
        {
            StartCoroutine(TransitionRoutine(currentIndex + 1));
        }
        else
        {
            Debug.Log("游戏流程结束");
            // 结束后暂停在最后一帧，而不是黑屏Stop
            videoPlayer.Pause();
        }
    }

    void HideAllButtons()
    {
        foreach (var stage in stageList)
        {
            if (stage.triggerButton != null)
                stage.triggerButton.gameObject.SetActive(false);
        }
    }

    IEnumerator TransitionRoutine(int nextIndex)
    {
        isTransitioning = true;
        float duration = 0.5f;

        // 渐黑
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, timer / duration);
            yield return null;
        }
        if (fadeOverlay != null) fadeOverlay.color = Color.black;

        // 切换
        PlayStage(nextIndex);

        // 渐亮
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 1f - (timer / duration));
            yield return null;
        }
        if (fadeOverlay != null) fadeOverlay.color = Color.clear;

        isTransitioning = false;
    }
}