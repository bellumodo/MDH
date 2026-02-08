using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;

public class SequentialStoryController_V2 : MonoBehaviour
{
    [Header("核心组件")]
    public VideoPlayer videoPlayer;
    public RawImage fadeOverlay; // 用来做黑屏遮罩的图片

    [Header("全局设置")]
    public float fadeDuration = 1.0f; // 渐入渐出耗时

    [System.Serializable]
    public class StorySegment
    {
        public string note;             // 备注 (例如 "第一章")
        public Button triggerButton;    // 触发按钮
        public VideoClip videoClip;     // 播放的视频
        [Tooltip("设置为0则播放整个视频，设置大于0则在指定秒数停止")]
        public float overrideEndTime;   // 【新功能】自定义结束时间
    }

    public List<StorySegment> storyLine;

    private int currentIndex = 0;
    private bool isPlaying = false;
    private Coroutine currentSequence;

    void Start()
    {
        // 初始化遮罩：先设为透明
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.color = new Color(0, 0, 0, 0);
        }

        // 1. 禁用 VideoPlayer 自带的循环结束检测，改用我们自己的 Update 检测
        videoPlayer.isLooping = false;

        // 2. 初始化第一步
        SetupCurrentStage();
    }

    void Update()
    {
        // 实时检测视频时间
        if (isPlaying && videoPlayer.isPlaying)
        {
            CheckVideoTime();
        }
    }

    void SetupCurrentStage()
    {
        // 隐藏所有按钮
        foreach (var segment in storyLine)
        {
            if (segment.triggerButton != null)
                segment.triggerButton.gameObject.SetActive(false);
        }

        // 显示当前按钮
        if (currentIndex < storyLine.Count)
        {
            StorySegment current = storyLine[currentIndex];
            if (current.triggerButton != null)
            {
                current.triggerButton.gameObject.SetActive(true);

                // 绑定点击事件
                current.triggerButton.onClick.RemoveAllListeners();
                current.triggerButton.onClick.AddListener(() => StartCoroutine(PlaySequence(current)));
            }
        }
    }

    void CheckVideoTime()
    {
        if (currentIndex >= storyLine.Count) return;

        StorySegment current = storyLine[currentIndex];
        float endTime = current.overrideEndTime;

        bool shouldStop = false;

        // 情况A: 设置了自定义结束时间 (例如 10秒)
        if (endTime > 0 && videoPlayer.time >= endTime)
        {
            shouldStop = true;
        }
        // 情况B: 没设置时间 (0)，检测是否自然播完
        else if (endTime <= 0 && (ulong)videoPlayer.frame >= videoPlayer.frameCount - 5)
        {
            shouldStop = true;
        }

        if (shouldStop)
        {
            isPlaying = false; // 停止检测
            StartCoroutine(FinishVideoAndNext());
        }
    }

    // 流程：点击 -> 隐藏按钮 -> 播放视频
    IEnumerator PlaySequence(StorySegment segment)
    {
        // 1. 隐藏按钮
        if (segment.triggerButton != null)
            segment.triggerButton.gameObject.SetActive(false);

        // 2. 准备视频
        videoPlayer.clip = segment.videoClip;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // 3. 播放
        videoPlayer.Play();
        isPlaying = true;
    }

    // 流程：时间到了 -> 渐黑 -> 停止视频 -> 准备下一步 -> 渐亮
    IEnumerator FinishVideoAndNext()
    {
        // --- 1. 渐隐 (屏幕变黑) ---
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration;
            if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        if (fadeOverlay != null) fadeOverlay.color = Color.black;

        // --- 2. 停止视频 ---
        videoPlayer.Stop();

        // --- 3. 切换到下一章 ---
        currentIndex++;
        SetupCurrentStage(); // 这时会把下一个按钮显示出来 (但在黑屏下)

        // 等一小会，让转场不那么生硬
        yield return new WaitForSeconds(0.5f);

        // --- 4. 渐显 (屏幕变亮，显示出下一个按钮和背景) ---
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1.0f - (timer / fadeDuration);
            if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 0);
    }
}