using UnityEngine;
using UnityEngine.InputSystem;
public class body_controller : MonoBehaviour
{
    float xRotation =0f;
    float mouseSensitivity=100f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        // 1. 计算鼠标增量 (注意：Input System 的 delta 已经受到帧率影响，通常不需要再乘 Time.deltaTime)
        // 如果感觉转得太慢，可以调大 mouseSensitivity 或乘以一个常数
        float mouseX = mouseDelta.x * mouseSensitivity * 0.1f; 
        float mouseY = mouseDelta.y * mouseSensitivity * 0.1f;

        // 2. 处理垂直旋转 (Camera 负责上下看)
        xRotation -= mouseY;
         xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        // 【核心修复】：只修改相机的局部旋转 X 轴
        // 这样就不会干扰到从父物体继承来的 Y 轴旋转
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
    }
}
