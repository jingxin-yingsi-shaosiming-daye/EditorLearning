using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 聚焦目标的相机
/// </summary>
public class FollowTrackingCamera : MonoBehaviour
{
    public static FollowTrackingCamera _Instance;


    #region 000 公有变量-------------------------------------------------------------

    // Camera target to look at.
    //相机跟随目标
    [Header("相机聚焦的物体")] public Transform target;

    //相机旋转角度
    [Header("相机的当前旋转角度")] public Vector3 CameraRotation;

    // Exposed vars for the camera position from the target.
    //从目标到摄像机位置的外露vars.
    [Header("相机距离目标度距离")] public float height = 20f;
    [Header("相机距离目标度距离")] public float distance = 20f;

    // Camera limits.
    //相机移动范围
    [Header("相机的移动范围的限制 min")] public float min = 10f;
    [Header("相机的移动范围的限制 max")] public float max = 60;

    // Options.
    //public bool doRotate;
    //相机旋转以及缩放功能开关
    [Header("允许相机放缩")] public bool doZoom;
    [Header("允许相机旋转")] public bool doRotate;
    [Header("允许拖拽")] public bool enableDrag;

    // The movement amount when zooming.缩放时的移动量。
    [Header("相机靠近远离目标单步长度|滚轮滚动一格相机远离靠近目标的距离 长度")]
    public float zoomStep = 30f;

    /// <summary>
    /// 相机当下距离目标的长度与相机上次刷新视图时距离的长度超过5时,才刷新相机文字
    /// </summary>
    [Header("相机刷新位置的最小敏感数值")] public float zoomSpeed = 5f;

    [Header("相机想要到达的高度")] private float heightWanted;
    [Header("相机想要到达的距离")] private float distanceWanted;

    [Header("相机在x轴的速度")] public float xSpeed = 3.0f;
    [Header("相机在y轴的速度")] public float ySpeed = 3.0f;

    [Header("相机在y轴的最小值|相机的最小高度")] public float yMinLimit = -20f;
    [Header("相机在y轴的最大值|相机的最大高度")] public float yMaxLimit = 80f;
    [Header("平滑补间时间")] public float smoothTime = 2f;


    [Header("相机初始角度")] public static float InitAngle = -90;
    [Header("相机当前角度")] public float CurrAngle = 45;
    [Header("相机想要的缩放大小")] public float WantedScale = 20;

    #endregion


    #region 000 私有变量----------------------------------------------------------------------------

    //public float xMinLimit = 30f;
    //public float xMaxLimit = 220f;

    public float distanceMin = 1.5f;
    public float distanceMax = 15f;


    /// <summary>
    /// 相机绕Y轴的旋转值
    /// </summary>
    float rotationYAxis = 230.0f;

    /// <summary>
    /// 相机绕X轴的旋转值
    /// </summary>
    float rotationXAxis = -8.0f;

    /// <summary>
    /// 相机在X轴的速度
    /// </summary>
    float velocityX = 0.0f;

    /// <summary>
    /// 相机在Y轴的速度
    /// </summary>
    float velocityY = 0.0f;

    //两根手指
    /// <summary>
    /// 多点操作 记录第一次触摸
    /// </summary>
    private Touch oldTouch1;

    /// <summary>
    /// 多点操作 记录第二次触摸
    /// </summary>
    private Touch oldTouch2;
    //Vector2 m_screenPos = Vector2.zero; //记录手指触碰的位置

    /// <summary>
    /// 比例因子
    /// </summary>
    float scaleFactor;

    // Result vectors.
    /// <summary>
    /// 缩放后坐标
    /// </summary>
    private Vector3 zoomResult; //缩放后坐标

    /// <summary>
    /// 旋转后四元数
    /// </summary>
    private Quaternion rotationResult;

    /// <summary>
    /// 目标调整位置
    /// </summary>
    private Vector3 targetAdjustedPosition;

    /// <summary>
    /// 自转
    /// </summary>
    private Quaternion rotation;

    #endregion

    #region 001  unity生命周期====================================================================================

    private void Awake()
    {
        _Instance = this;
    }

    void Start()
    {
        init();
        //StartCoroutine(OnMouseDown());
    }

    #endregion

    #region 002 初始化函数===========================================================================================

    void init()
    {
        Position = transform.position;
        rotation = transform.rotation;

        //得到相机欧拉角
        Vector3 angles = transform.eulerAngles;
        //相机绕Y轴转动的角度值
        rotationYAxis = angles.y;
        //相机绕X轴转动的角度值
        rotationXAxis = angles.x;
        print("相机初始位置" + rotationXAxis);
        //print("Y轴数值"+ rotationYAxis);
        //print("X轴数值" + rotationXAxis);

        // Initialise default zoom vals.
        //相机当前高度赋值于目标高度
        heightWanted = height;
        distanceWanted = distance;
        // Setup our default camera.  We set the zoom result to be our default position.
        zoomResult = new Vector3(0f, height, -distance);
    }

    #endregion

    //想要的缩进大小


    #region 004 用于拖拽的属性

    Vector3 oldSameTargetYWorldPosition = Vector3.zero; //记录拖拽时上一帧的的鼠标指针的世界坐标

    bool isMouseDargActioning = false; //是否是按下的第一帧  false表示非按下状态

    Vector2 oldMouseScreenPosition = Vector2.zero; //记录拖拽时上一帧的鼠标指针的相机坐标

    Vector3 ScreenSpace = Vector3.zero; //记录聚焦物体的相机坐标

    #endregion


    #region 003 unity生命周期 LateUpdate==============================================================================

    void LateUpdate()
    {
        if (IsInit == true)
        {
            distanceWanted = WantedScale;
            //Animation.Instance.Tween(InitAngle);
            rotationXAxis = 25;
            //DOTween.To(() => distanceWanted, x => distanceWanted = x, 19, 0.01f);
            DOTween.To(() => rotationXAxis, x => rotationXAxis = x, CurrAngle, 0.5f);
            rotationYAxis = 0;
            IsInit = false;
        }

        // Check target.
        //检测目标是否存在
        if (!target)
        {
            Debug.LogError(
                "This camera has no target, you need to assign a target in the inspector.||该摄像机没有目标，您需要在检查器中分配一个目标");
            return;
        }


        if (doZoom) //处理相机与目标物体距离放缩//相机视角缩放  
        {
            HandleZoom();
        } //处理相机与目标物体距离放缩结束//相机视角缩放结束


        if (doRotate) //处理相机旋转//相机视角旋转
        {
            HandleRotate();
        } //处理相机旋转结束


        if (enableDrag) //----------处理相机拖拽//如果允许拖拽
        {
            HandleDrag();
        } //-------------相机拖拽结束


        Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
        Quaternion rotation = toRotation;
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        //相机跟随
        Vector3 position = rotation * negDistance + target.position;
        //改变相机Rotation，从而旋转相机
        transform.rotation = rotation;


        //将缩放后的坐标作为相机的当前坐标位置
        transform.position = position;
        velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
        velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
    }

    #endregion

    #region 005 相机处理旋转 放缩 拖拽=========================================================================================

    /// <summary>
    /// 处理旋转
    /// </summary>
    private void HandleRotate()
    {
        //print("水平" + Input.GetAxis("Horizontal"));
        //print("竖直" + Input.GetAxis("Vertical"));
        if (Input.touchCount == 1)
        {
            Touch newTouch1 = Input.GetTouch(0);
            //Touch touch = Input.GetTouch(0);
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                oldTouch1 = newTouch1;
                //m_screenPos = touch.position;  
            }

            if (Input.touches[0].phase == TouchPhase.Moved)
            {
                float CX = newTouch1.position.x - oldTouch1.position.x;
                float CY = newTouch1.position.y - oldTouch1.position.y;

                velocityX += xSpeed * CX * 0.02f * Time.deltaTime;
                velocityY += ySpeed * CY * 0.02f * Time.deltaTime;
            }
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) //Input.GetMouseButton(2) || 
        {
            // print("欧拉角"+transform.eulerAngles);
            velocityX += xSpeed * Input.GetAxis("Mouse X") * 0.02f;
            velocityY += ySpeed * Input.GetAxis("Mouse Y") * 0.02f;
        }

        rotationYAxis += velocityX;
        rotationXAxis -= velocityY;
        if (rotationXAxis >= 90)
        {
            rotationXAxis = 90;
        }
        else if (rotationXAxis <= -90)
        {
            rotationXAxis = -90;
        }
    }

    /// <summary>
    /// LateUpdate每帧处理缩放
    /// </summary>
    private void HandleZoom()
    {
        //print(doRotate);
        //if (Input.touchCount <= 0)
        //{
        //    return;
        //}
        float mouseInput;
        if (Input.touchCount > 1)
        {
            Touch newTouch1 = Input.GetTouch(0); //获取第一个触点
            Touch newTouch2 = Input.GetTouch(1); //获取第二个触点
            //第2点刚开始接触屏幕, 只记录，不做处理  
            if (newTouch2.phase == TouchPhase.Began)
            {
                oldTouch2 = newTouch2;
                oldTouch1 = newTouch1;
                //return;
            }

            //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型  
            float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
            float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);
            //两个距离只差，为正表示放大，为负表示缩小
            float offset = newDistance - oldDistance;
            //缩放因子
            scaleFactor = offset / 1000f;

            mouseInput = scaleFactor;

            heightWanted -= zoomStep * mouseInput;
            distanceWanted -= zoomStep * mouseInput;
        }

        // Record our mouse input.  If we zoom add this to our height and distance.
        //记录鼠标滚轮滚动时的变量 并赋值记录
        //mouseInput特性：正常状态为0；滚轮前推一格变为+0.1一次，后拉则变为-0.1一次
        // Input.GetAxis("Mouse ScrollWheel");
        if (Input.touchCount <= 0)
        {
            mouseInput = Input.GetAxis("Mouse ScrollWheel");

            heightWanted -= zoomStep * mouseInput;
            distanceWanted -= zoomStep * mouseInput;
        }
        //print("+++"+mouseInput);

        // Make sure they meet our min/max values.
        //限制相机高度范围 
        heightWanted = Mathf.Clamp(heightWanted, min, max);
        distanceWanted = Mathf.Clamp(distanceWanted, min, max);
        //差值计算，动态修改相机高度值（平滑的变化）
        height = Mathf.Lerp(height, heightWanted, Time.deltaTime * zoomSpeed);
        distance = Mathf.Lerp(distance, distanceWanted, Time.deltaTime * zoomSpeed);

        // Post our result.
        //缩放后坐标
        zoomResult = new Vector3(0f, height, -distance);
    }

    /// <summary>
    /// LateUpdate每帧处理拖拽
    /// </summary>
    private void HandleDrag()
    {
        if (Input.GetMouseButton(2)) //如果鼠标中间被按下
        {
            if (isMouseDargActioning == true) //拖拽标识是true表明上几帧有过   如果是按下的大于第二帧
            {
                Vector2 newMouseScreenPosition = new Vector3(
                    Input.mousePosition.x,
                    Input.mousePosition.y); //记录新的鼠标指针在屏幕中的位置

                

                oldSameTargetYWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(oldMouseScreenPosition.x,
                    oldMouseScreenPosition.y, ScreenSpace.z)); //将上一帧的鼠标屏幕坐标转化为世界坐标

                Vector3 newSameTargetYWorldPosition = Camera.main.ScreenToWorldPoint(
                    new Vector3(newMouseScreenPosition.x, newMouseScreenPosition.y, ScreenSpace.z));

                //Vector3 offSize = newSameTargetYWorldPosition - oldSameTargetYWorldPosition;//反向拖拽
                Vector3 offSize = oldSameTargetYWorldPosition - newSameTargetYWorldPosition; //正向拖拽

               

                target.position += offSize;//聚焦物体执行偏移

                oldMouseScreenPosition = newMouseScreenPosition;//此帧执行完毕  
            }


            else //如果是按下的第一帧
            {
                ScreenSpace = Camera.main.WorldToScreenPoint(target.position);//将聚焦目标转化为相机坐标

                oldMouseScreenPosition = new Vector2(Input.mousePosition.x,
                    Input.mousePosition.y); //记录第一帧时鼠标指针的相机坐标
               

                isMouseDargActioning = true;//标识为正在执行鼠标拖拽
            }
        }
        else //拖拽动作结束
        {
            isMouseDargActioning = false; //将正在拖拽标识为false
        }
    }

    #endregion


    /// <summary>
    /// 夹角
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        //限制相机转动角度 
        return Mathf.Clamp(angle, min, max);
    }

    public void InitPoint()
    {
        heightWanted = max;
        distanceWanted = max;
    }

    public void InitReturn(float a, float b)
    {
        heightWanted = a;
        distanceWanted = b;
    }

    public Vector3 Position; //当前摄像机的位置
    public Vector3 Rotation; //当前摄像机的角度

    public bool IsInit = false;
}