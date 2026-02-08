using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(CanvasGroup))] // 自动添加 CanvasGroup 组件
public class ButtonAppearDelay : MonoBehaviour
{
    [Header("设置")]
    public VideoPlayer targetVideo;  // 监视哪个视频
    public float appearTime = 5.0f;  // 视频播放到第几秒时出现

    private CanvasGroup canvasGroup;
    private bool hasAppeared = false;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // 游戏开始时，先把按钮“隐藏”
        HideButton();
    }

    void Update()
    {
        // 如果已经显示了，或者视频没在播放，就不做检测
        if (hasAppeared || targetVideo == null || !targetVideo.isPlaying) return;

        // 检测视频时间
        if (targetVideo.time >= appearTime)
        {
            ShowButton();
        }
    }

    void HideButton()
    {
        canvasGroup.alpha = 0f;          // 透明度设为0 (看不见)
        canvasGroup.interactable = false; // 不可交互 (点不了)
        canvasGroup.blocksRaycasts = false; // 鼠标射线穿透
    }

    void ShowButton()
    {
        canvasGroup.alpha = 1f;          // 透明度设为1 (完全显示)
        canvasGroup.interactable = true;  // 可以点击
        canvasGroup.blocksRaycasts = true; // 阻挡射线

        hasAppeared = true; // 标记为已显示，停止Update里的检测，节省性能
    }
}