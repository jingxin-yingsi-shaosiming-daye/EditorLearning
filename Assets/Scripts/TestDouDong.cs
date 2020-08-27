using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDouDong : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    #region 004 用于拖拽的属性

    public Transform target;

    Vector3 oldSameTargetYWorldPosition = Vector3.zero;

    Vector2 oldMouseScreenPosition = Vector2.zero;


    bool isMouseDownActioning = false; //是否是按下的第一帧  false表示非按下状态
    private bool enableDrag = true;


    Vector3 ScreenSpace = new Vector3(0, 0, 0);

    #endregion

    // Update is called once per frame
    void LateUpdate()
    {
        //----------相机拖拽开始
        if (enableDrag) //如果允许拖拽
        {
            if (Input.GetMouseButton(2)) //如果被按下
            {
                if (isMouseDownActioning == true) //如果是按下的大于第二帧
                {
                    Vector2 newMouseScreenPosition = new Vector3(
                        Input.mousePosition.x,
                        Input.mousePosition.y);//记录新的
                    
                    
                    

                    Debug.Log("新的鼠标指针的屏幕坐标: " + newMouseScreenPosition);
                    
                    oldMouseScreenPosition =Camera.main.ScreenToWorldPoint(new Vector3(oldMouseScreenPosition.x,
                        oldMouseScreenPosition.y, ScreenSpace.z));

                    Vector3 newSameTargetYWorldPosition = Camera.main.ScreenToWorldPoint(
                        new Vector3(newMouseScreenPosition.x, newMouseScreenPosition.y, ScreenSpace.z));

                    //Vector3 offSize = newSameTargetYWorldPosition - oldSameTargetYWorldPosition;//反向
                    Vector3 offSize = oldSameTargetYWorldPosition - newSameTargetYWorldPosition; //正向

                    Debug.Log("old position: " + oldSameTargetYWorldPosition + "  new position: " +
                              newSameTargetYWorldPosition);

                    Debug.Log("偏移: " + offSize);

                    target.position += offSize;

                    oldMouseScreenPosition = newMouseScreenPosition; //将新的赋值给老的
                }


                else //如果是按下的第一帧
                {
                    ScreenSpace = Camera.main.WorldToScreenPoint(target.position);

                    oldMouseScreenPosition = new Vector3(Input.mousePosition.x,
                        Input.mousePosition.y); //记录第一帧
                    Debug.Log("老: " + oldSameTargetYWorldPosition);

                    isMouseDownActioning = true;
                }
            }
            else //拖拽动作结束
            {
                isMouseDownActioning = false; //将正在拖拽标识为false
            }
        } //-------------相机拖拽结束
    }
}