using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance{get;private set;}
    public Transform playerBody;
    public Vector3 initialPosition;
    void Awake(){
        if(Instance==null){
            Instance=this;
        }else{
            Destroy(gameObject);
        }
    }
    [Header("Movement")]
    public float moveSpeed = 5f;
    public CharacterController controller; // 拖入 CharacterController 组件
    [Header("Jump & Gravity")]
    public float gravity = -9.81f;    // 重力加速度
    public float jumpHeight = 1.5f;   // 跳跃高度
    private Vector3 velocity;         // 当前的垂直速度
    private bool isGrounded;          // 是否在地面上
    public Transform gun;

    [Header("Mouse Look")]
    public float mouseSensitivity = 10f;
    public GameObject uis;
    public Transform deathCamera;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        initialPosition=playerBody.position;
        // 自动获取组件（如果没拖的话）
        if (controller == null) controller = GetComponentInParent<CharacterController>();
    }
    public void Reset()
    {
        // 1. 停止所有正在运行的协程（非常重要！防止死亡倒地动画继续执行）
        StopAllCoroutines();

        // 2. 必须先禁用 CharacterController 才能强制修改坐标
        if (controller != null) controller.enabled = false;

        // 3. 重置时间缩放
        Time.timeScale = 1f;

        // 4. 重置位置和旋转
        // 注意：我们将父物体移动到初始位置，将子物体(playerBody)本地旋转归零
        transform.position = initialPosition;
        transform.rotation = Quaternion.identity; // 身体朝向前方
        
        if (playerBody != null)
        {
            playerBody.localRotation = Quaternion.identity;
            playerBody.localPosition = Vector3.zero;
        }

        // 5. 【关键】重置鼠标旋转逻辑变量
        xRotation = 0f;          // 视角水平
        velocity = Vector3.zero; // 垂直速度归零，防止复活后被重力瞬间拍在地上

        // 6. 重新激活 UI 和控制
        uis.SetActive(true);
        
        // 7. 最后重新开启 CharacterController
        if (controller != null) controller.enabled = true;

        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log("玩家已重置，当前位置：" + transform.position);
    }
    public void Clear(){
        uis.SetActive(false);
        controller.enabled = false;
        StartDeathAnimation();
    }
    public void StartDeathAnimation(){

        StartCoroutine(AnimateToTarget(transform, deathCamera.position, deathCamera.rotation, 0.2f));
        StartCoroutine(AnimateToTarget(playerBody, playerBody.position, Quaternion.AngleAxis(90f, Vector3.right), 5f));
    }

    void Update()
    {
        if(!Mouse.current.leftButton.wasPressedThisFrame){
            HandleMouseLook();
        }
        HandleMovement();
    }
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    void HandleMovement()
    {
        // 1. 地面检测
        // CharacterController 有个 isGrounded 属性，但在移动前检测更准确
        isGrounded = controller.isGrounded;
        
        // 如果在地面上且速度在往下掉，重置速度（设置一个小的负值如 -2f 以确保能贴着地走）
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 2. 获取 WASD 输入
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            float moveX = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            float moveY = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
            input = new Vector2(moveX, moveY);
        }

        // 3. 水平移动逻辑
        Vector3 move = playerBody.right * input.x + playerBody.forward * input.y;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 4. 跳跃逻辑
        // 使用公式: v = sqrt(h * -2 * g)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 5. 应用重力
        // 速度随时间增加 (Δv = g * Δt)
        velocity.y += gravity * Time.deltaTime;

        // 6. 执行垂直移动 (Δy = v * Δt)
        controller.Move(velocity * Time.deltaTime);
    }
    [Header("Gun Movement")]
    public float horizontalMovement=0f;
    public float horizontalThreshhold=1f;
    public bool lastDirection=true;
    public float gunMoveScale=0.1f;
    void HandleMouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.1f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.1f;
        bool direction=mouseX>0f;
        if(lastDirection!=direction){
            horizontalMovement=0f;
            Debug.Log("horizontalMovement: "+horizontalMovement);
        }else{
            horizontalMovement+=mouseX;
        }
        lastDirection=direction;
        float gunLastXPosition=gun.localPosition.x;
        gun.localPosition=new Vector3(Mathf.Max(Mathf.Min(gunLastXPosition-horizontalMovement*gunMoveScale,horizontalThreshhold),-horizontalThreshhold),gun.localPosition.y,gun.localPosition.z);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
    public IEnumerator AnimateToTarget(Transform objectToMove, Vector3 targetPos, Quaternion targetRot, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = objectToMove.position;
        Quaternion startRot = objectToMove.rotation;

        while (elapsed < duration)
        {
            // 使用 unscaledDeltaTime 确保在慢动作或暂停时依然平滑
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // 使用 SmoothStep 让开始和结束更平滑（淡入淡出感）
            float curveT = Mathf.SmoothStep(0, 1, t);

            objectToMove.position = Vector3.Lerp(startPos, targetPos, curveT);
            objectToMove.rotation = Quaternion.Slerp(startRot, targetRot, curveT);

            yield return null;
        }

        // 确保最终位置精准对齐
        objectToMove.position = targetPos;
        objectToMove.rotation = targetRot;
    }
}