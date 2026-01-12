using UnityEngine;
using UnityEngine.UI; // 必须引用这个才能控制 Slider
using TMPro; // 必须引入这个命名空间
// 
public class SliderController : MonoBehaviour
{
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI mouseSensitivityText;
    public Slider volumnSlider;
    public TextMeshProUGUI volumnText;

    void Start()
    {
        // 设置最小值和最大值
        mouseSensitivitySlider.minValue = 0;
        mouseSensitivitySlider.maxValue = 30;

        // 设置当前数值
        mouseSensitivitySlider.value = 7;

        // 是否只允许整数（适合音量百分比或关卡选择）
        mouseSensitivitySlider.wholeNumbers = true;

        volumnSlider.minValue = 0;
        volumnSlider.maxValue = 1;
        volumnSlider.value = 0.5f;
        volumnSlider.wholeNumbers = false;
    }

    void Awake()
    {
        mouseSensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        volumnSlider.onValueChanged.AddListener(OnVolumnChanged);
    }
    void OnSensitivityChanged(float val)
    {
        Debug.LogFormat("灵敏度已改变为: {0}", val);
        PlayerController.Instance.mouseSensitivity = val;
        mouseSensitivityText.SetText("{0}", val);
        // 在这里应用实际的逻辑，例如：
        // PlayerMovement.Instance.mouseSensitivity = val;
    }
    void OnVolumnChanged(float val)
    {
        Debug.LogFormat("音量已改变为: {0}", val);
        PlayerController.Instance.volumn = val;
        volumnText.SetText("{0}", val);
    }

    // 4. 良好的习惯：在销毁时移除监听
    void OnDestroy()
    {
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        }
    }
}