using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomInspector
{
    
    [CanEditMultipleObjects]//可以选择多个带有FollowTrackingCamera类型的脚本对象   一块编辑这些对象的FollowTrackingCamera脚本属性
    [CustomEditor(typeof(FollowTrackingCamera))]//为这个类创建自定义编辑器
    public class FollowTrackingCameraCustomInspector : Editor
    {


        private void OnEnable()//当场景中带有目标脚本的游戏对象被选中时执行
        {
            
        }


        private void OnDisable()//1 数值超过范围或销毁时执行   适合做一些代码退出前的清零操作
        {
            
        }
        
        
        
    }
}


