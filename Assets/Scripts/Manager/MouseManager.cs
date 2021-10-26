using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

//拖拽的方式 添加事件
////必须序列化才能在Inspector面板中看到Event
//[System.Serializable]
//public class EventVector3 : UnityEvent<Vector3> { }

//代码的方式添加事件

public class MouseManager : Singleton<MouseManager>
{
    //记录碰撞信息
    RaycastHit hitInfo;
    //事件对象
    public event Action<Vector3> OnMouseClicked;
    public event Action<GameObject> OnEnemyClicked;
    //鼠标 贴图
    public Texture2D point, doorway, attack, target, arrow;

    protected virtual new void Awake()
    {
        base.Awake();
        //DontDestroyOnLoad(this);
    }

    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    
   
    void SetCursorTexture()
    {
        //每一帧都会向鼠标指向的位置发射射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //当射线发生碰撞，记录碰撞信息
        if (Physics.Raycast(ray, out hitInfo))
        {
            
            switch (hitInfo.collider.tag)
            {
                case "Ground":
                    //修改鼠标贴图
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    //修改鼠标贴图
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
            }
        }
    }

    void MouseControl()
    {
        //当射线发生碰撞，并且鼠标右键按下
        if (Input.GetMouseButtonDown(1) && hitInfo.collider != null)
        {
            //当碰撞到的物体的标签为“Ground”，且事件不为空
            //if (hitInfo.collider.CompareTag("Ground") && OnMouseClicked != null)
            //{
            //    //添加到这个事件的函数都会被执行
            //    //调用事件函数，将玩家移动到发生碰撞的位置
            //    OnMouseClicked.Invoke(hitInfo.point);
                
            //}
            if (hitInfo.collider.CompareTag("Ground"))
            {
                //添加到这个事件的函数都会被执行
                //调用事件函数，将玩家移动到发生碰撞的位置
                OnMouseClicked?.Invoke(hitInfo.point);//上面的简写，先判断OnMouseClicked是否为空，如果为空不执行
            }
            //当玩家点中的物体时Enemy时 调用事件
            if (hitInfo.collider.CompareTag("Enemy"))
            {
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
        }
    }
}
