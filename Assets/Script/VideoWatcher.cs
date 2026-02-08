using UnityEngine;
using UnityEngine.Video;

public class VideoWatcher : MonoBehaviour
{
    [Header("监视谁？")]
    public VideoPlayer videoPlayer; // 拖入场景里的 Video Player

    [Header("当播放哪个视频时掉粉？")]
    public VideoClip targetClip;    // 拖入那个会导致掉粉的视频文件

    [Header("去触发谁？")]
    public ViewerCountTrigger dropScript; // 拖入挂着掉粉脚本的 Text 物体

    private bool hasTriggered = false;

    void Update()
    {
        // 如果播放器正在播放，且播放的内容正是我们要监视的那个视频
        if (videoPlayer.isPlaying && videoPlayer.clip == targetClip)
        {
            // 如果还没触发过
            if (!hasTriggered)
            {
                dropScript.StartDrop(); // 动手！
                hasTriggered = true;    // 标记为已触发，防止一秒钟触发60次
            }
        }
        else
        {
            // 如果切到别的视频了，重置开关，方便下次再播时还能触发
            if (videoPlayer.clip != targetClip)
            {
                hasTriggered = false;
            }
        }
    }
}