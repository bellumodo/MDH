using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ViewerCountTrigger : MonoBehaviour
{
    [Header("UI 组件")]
    public Text viewerText; // 记得把 Text (Legacy) 拖进来

    [Header("设置")]
    public int startValue = 500;
    public int endValue = 320;
    public float duration = 8.0f;

    [Header("格式")]
    public string prefix = "👁 ";
    public string suffix = " WATCHING";

    void Start()
    {
        // 游戏一开始，先重置为初始值，但不开始掉
        if (viewerText != null)
            UpdateText(startValue);
    }

    // 【关键】这个函数是公开的，你可以通过按钮或视频控制器来调用它
    public void StartDrop()
    {
        StopAllCoroutines(); //以此防重复触发
        StartCoroutine(AnimateDrop());
    }

    // 重置人数回 500 (如果需要的话)
    public void ResetCount()
    {
        StopAllCoroutines();
        UpdateText(startValue);
    }

    IEnumerator AnimateDrop()
    {
        float elapsed = 0f;
        int currentStart = startValue; // 从当前设定开始

        // 如果你想接着当前显示的数字掉，可以用下面这行代替上面那行：
        // int.TryParse(viewerText.text.Replace(prefix, "").Replace(suffix, ""), out currentStart);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 平滑插值
            float val = Mathf.Lerp(currentStart, endValue, t);
            UpdateText(Mathf.RoundToInt(val));

            yield return null;
        }
        UpdateText(endValue);
    }

    void UpdateText(int count)
    {
        if (viewerText != null)
        {
            viewerText.text = prefix + count.ToString() + suffix;
            if (count < startValue) viewerText.color = Color.red;
            else viewerText.color = Color.white;
        }
    }
}