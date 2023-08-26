using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNode_", menuName = "Room/RoomNode")]
public class RoomNodeSO : ScriptableObject 
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    
    // 以下代码只能在 Unity 编辑器中运行
#if UNITY_EDITOR

    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    /// <summary>
    /// 初始化节点
    /// </summary>
    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        // 加载房间节点类型列表
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }
    
    /// <summary>
    /// 使用nodestyle绘制节点
    /// </summary>
    public void Draw(GUIStyle nodeStyle)
    {
        //  绘制节点框
        GUILayout.BeginArea(rect, nodeStyle);

        // 启动区域以检测弹出选择更改
        EditorGUI.BeginChangeCheck();

        // 使用可以从中选择的 RoomNodeType 名称值显示弹出窗口（默认为当前设置的 roomNodeType）
        int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

        roomNodeType = roomNodeTypeList.list[selection];

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();
    }

    /// <summary>
    /// 使用要显示的可选择的房间节点类型填充字符串数组
    /// </summary>
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }
    /// <summary>
    /// 处理节点事件
    /// </summary>
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            // 鼠标点击事件
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            // 鼠标松开事件
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // 鼠标拖拽事件
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 处理鼠标点击事件
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // 左键点击
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }

    }

    /// <summary>
    /// 处理鼠标左键点击事件
    /// </summary>
    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        // 切换被选中的节点
        if (isSelected == true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }
    }

    /// <summary>
    /// 鼠标松开事件
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // 左键松开
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    /// <summary>
    /// 鼠标左键松开事件
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    /// <summary>
    /// 鼠标拖拽事件
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // 鼠标左键拖拽
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }


    /// <summary>
    /// 处理鼠标左键拖拽事件
    /// </summary>
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// 拖拽节点
    /// </summary>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }


#endif
}
