using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class AutoPlaylistWithFade : MonoBehaviour
{
    [Header("组件设置")]
    public VideoPlayer videoPlayer;   // 你的视频播放器
    public RawImage fadeOverlay;      // 用来做渐变遮罩的黑色图片 (必须是UI RawImage)

    [Header("播放列表")]
    public VideoClip[] playlist;      // 在这里把你要播放的视频按顺序拖进去
    public float fadeDuration = 1.0f; // 渐变需要多长时间 (秒)

    private int currentVideoIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        // 1. 初始化设置
        // 确保遮罩是黑色的，且完全透明，这样能看到第一个视频
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 0);
            fadeOverlay.gameObject.SetActive(true);
        }

        // 2. 绑定视频结束事件
        videoPlayer.loopPointReached += OnVideoFinished;

        // 3. 开始播放第一个视频
        if (playlist.Length > 0)
        {
            PlayVideo(0);
        }
    }

    void PlayVideo(int index)
    {
        if (index >= 0 && index < playlist.Length)
        {
            videoPlayer.clip = playlist[index];
            videoPlayer.Play();
        }
    }

    // 当一个视频播完时自动调用
    void OnVideoFinished(VideoPlayer vp)
    {
        if (!isTransitioning)
        {
            // 开始切换到下一个视频的协程
            StartCoroutine(TransitionToNextVideo());
        }
    }

    IEnumerator TransitionToNextVideo()
    {
        isTransitioning = true;

        // --- 第一步：渐出 (屏幕变黑) ---
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration;
            if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        // 确保完全变黑
        if (fadeOverlay != null) fadeOverlay.color = Color.black;

        // --- 第二步：切换视频资源 ---
        currentVideoIndex++; // 准备播下一个

        // 如果还有视频，就切到下一个；如果没有了，就停止（或者你可以写 currentVideoIndex = 0 来循环）
        if (currentVideoIndex < playlist.Length)
        {
            videoPlayer.Stop();
            videoPlayer.clip = playlist[currentVideoIndex];
            videoPlayer.Prepare();

            // 等待视频准备好 (防止画面还没出来遮罩就没了)
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            videoPlayer.Play();
        }
        else
        {
            Debug.Log("播放列表结束");
            // 这里可以选择由它去，或者做其他逻辑
        }

        // --- 第三步：渐入 (屏幕变亮) ---
        // 稍微停顿一下让黑屏过渡更自然（可选）
        yield return new WaitForSeconds(0.2f);

        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1.0f - (timer / fadeDuration); // 从1变到0
            if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        // 确保完全透明
        if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 0);

        isTransitioning = false;
    }
}