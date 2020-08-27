using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMove : MonoBehaviour
{
    
    
    Vector3 oldSameTargetYWorldPosition = Vector3.zero;
    
    bool isMouseDownActioning = false;//是否是按下的第一帧  false表示非按下状态

     void Update()
     {

         



         if (Input.GetMouseButton(0))//如果被按下
         {
             Vector3 ScreenSpace = Camera.main.WorldToScreenPoint(transform.position);
             
             if (isMouseDownActioning == false) //如果是按下的第一帧
             {
                 oldSameTargetYWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                     Input.mousePosition.y, ScreenSpace.z)); //记录第一帧
                 Debug.Log("老: "+ oldSameTargetYWorldPosition);

                 isMouseDownActioning = true;
                 return;
             }
             else //如果不是按下的第一帧
             {
                 Vector3 newSameTargetYWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                     Input.mousePosition.y, ScreenSpace.z)); //记录新的

                 Debug.Log("新: "+ newSameTargetYWorldPosition);

                 this.transform.position += newSameTargetYWorldPosition - oldSameTargetYWorldPosition;

                 oldSameTargetYWorldPosition = newSameTargetYWorldPosition;
             }

         }
         else
         {
             isMouseDownActioning = false;
         }
    }

    

}
