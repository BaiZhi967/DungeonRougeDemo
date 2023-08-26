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

        //如果节点有父节点或节点是入口节点
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        { 
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // 使用可以从中选择的 RoomNodeType 名称值显示弹出窗口（默认为当前设置的 roomNodeType）
            var selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            var selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];
        }

        

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
        }//右键鼠标
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
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
    /// 处理鼠标右键点击事件
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
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
    
    /// <summary>
    /// 将childID添加到节点（如果已添加节点则返回true，否则返回false）
    /// </summary>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // 判断childID是否可以合法的添加
        // 如果可以则返回true，否则返回false
        if(IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 判断子节点能否被合法的添加到父节点中 如果可以返回true 否则范围false
    /// </summary>
    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;
        // 判断 是否存在BOSS房节点 并且BOSS房节点已经连接
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
                isConnectedBossNodeAlready = true;
        }

        // 如果子节点是BOSS节点 并且BOSS节点已经连接 则返回false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;

        // 如果子节点的类型为None 则返回false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        // 如果已经包含了该子节点 则返回false
        if (childRoomNodeIDList.Contains(childID))
            return false;

        // 如果节点与自身相同 返回false
        if (id == childID)
            return false;

        // 如果子节点是自己的父节点返回false
        if (parentRoomNodeIDList.Contains(childID))
            return false;

        // 如果子节点已经有了父节点返回false
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        // 如果子节点和当前节点都为走廊(Corridor) 返回false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        // 如果子节点不是走廊(Corridor) 并且当前节点不是走廊(Corridor) 返回false
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        // 如果子节点是走廊(Corridor) 并且当前节的子节点数量超过最大子节点数 返回false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.MaxChildCorridors)
            return false;

        // 如果子节点是入口节点 则返回false -入口节点不存在父节点
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        // 如果将房间添加到走廊，请检查该走廊节点是否尚未添加房间
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;



    }


    /// <summary>
    /// 添加parentID到节点（已添加则返回true，否则返回false）
    /// </summary>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }


#endif
}
