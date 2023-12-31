using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle _roomNodeStyle;
    private GUIStyle _roomNodeSelectedStyle;
    private static RoomNodeGraphSO _currentRoomNodeGraph;
    private Vector2 _graphOffset;
    private Vector2 _graphDrag;
    private RoomNodeSO _currentRoomNode = null;
    private RoomNodeTypeListSO _roomNodeTypeListSo = null;
    
    //Node边框大小
    private const float NodeWidth = 160f;
    private const float NodeHeight = 75f;
    private const int NodePadding = 25;
    private const int NodeBorder = 12;
    
    // 连线属性
    private const float ConnectingLineWidth = 3f;
    private const float ConnectingLineArrowSize = 6f;
    
    // 网格属性
    private const float GridLarge = 100f;
    private const float GridSmall = 25f;

    [MenuItem("Window/Room Node Graph")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow(typeof(RoomNodeGraphEditor));
    }

    private void OnEnable()
    {
        // 监视器选择更改事件
        Selection.selectionChanged += InspectorSelectionChanged;
        
        //默认节点样式
        _roomNodeStyle = new GUIStyle();
        _roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        _roomNodeStyle.normal.textColor = Color.white;
        _roomNodeStyle.padding = new RectOffset(NodePadding, NodePadding, NodePadding, NodePadding);
        _roomNodeStyle.border = new RectOffset(NodeBorder, NodeBorder, NodeBorder, NodeBorder);
        // 默认选中节点样式
        _roomNodeSelectedStyle = new GUIStyle();
        _roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        _roomNodeSelectedStyle.normal.textColor = Color.white;
        _roomNodeSelectedStyle.padding = new RectOffset(NodePadding, NodePadding, NodePadding, NodePadding);
        _roomNodeSelectedStyle.border = new RectOffset(NodeBorder, NodeBorder, NodeBorder, NodeBorder);


        _roomNodeTypeListSo = GameResources.Instance.roomNodeTypeList;

    }
    
    private void OnDisable()
    {
        // 监视器选择更改事件
        Selection.selectionChanged -= InspectorSelectionChanged;
    }
    
    /// <summary>
    /// 如果在检查器中双击房间节点图可编写脚本的对象资源，则绘制房间节点图编辑器窗口
    /// </summary>
    [OnOpenAsset(0)]  
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            OpenWindow();

            _currentRoomNodeGraph = roomNodeGraph;
            
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 绘制编辑器GUI
    /// </summary>
    private void OnGUI()
    {
        //如果已选择 RoomNodeGraphSO 类型的可编写脚本的对象，则处理
        if (_currentRoomNodeGraph is not null)
        {
            //绘制网格
            DrawBackgroundGrid(GridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(GridLarge, 0.3f, Color.gray);
            
            // 如果被拖动则画线
            DrawDraggedLine();
            //处理事件
            ProcessEvents(Event.current);
            // 绘制两个房间节点间的线
            DrawRoomConnections();
            //绘制房间节点
            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }
    
    /// <summary>
    /// 绘制网格背景
    /// </summary>
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        _graphOffset += _graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(_graphOffset.x % gridSize, _graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset,
                new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, 
                new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
        }

        Handles.color = Color.white;

    }

    
    private void DrawDraggedLine()
    {
        if (_currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            //从节点位置绘制一条线
            Handles.DrawBezier(_currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, _currentRoomNodeGraph.linePosition, 
                _currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, _currentRoomNodeGraph.linePosition,
                Color.white, null, ConnectingLineWidth);
        }
    }
    


    private void ProcessEvents(Event currentEvent)
    {
        // 重置偏移量
        _graphDrag = Vector2.zero;
        
        // 如果鼠标所在的房间节点为空或当前未被拖动，则获取该房间节点
        if (_currentRoomNode is null || _currentRoomNode.isLeftClickDragging == false)
        {
            _currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // 如果鼠标不在房间节点上或者我们当前正在从房间节点拖动一条线，然后处理图形事件
        if (_currentRoomNode is null || _currentRoomNodeGraph.roomNodeToDrawLineFrom is not null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        // else 处理房间节点事件
        else
        {
            // 处理房间节点事件
            _currentRoomNode.ProcessEvents(currentEvent);
        }
    }


    /// <summary>
    /// 流程图事件
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            //鼠标按下
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            // 鼠标松开
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // 鼠标拖拽
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);

                break;
            default:
                break;
            
        }
    }
    
    /// <summary>
    ///  检查鼠标是否位于房间节点上 - 如果是，则返回房间节点，否则返回 null
    /// </summary>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = _currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (_currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return _currentRoomNodeGraph.roomNodeList[i];
            }
        }

        return null;
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //右键 - 打开菜单
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        // 左键
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// 打开菜单
    /// </summary>
    /// <param name="MousePosition"></param>
    private void ShowContextMenu(Vector2 MousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("创建房间节点"), false, CreateRoomNode, MousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("选择所有的节点"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("删除选中房间节点的连接"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("删除选中的房间节点"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }

    private void CreateRoomNode(object MousePosition)
    {
        //如果当前没有房间节点，则创建入口节点
        if (_currentRoomNodeGraph.roomNodeList.Count <= 0)
        {
            CreateRoomNode(new Vector2(100f,100f),_roomNodeTypeListSo.list.Find(x => x.isEntrance));
        }
        CreateRoomNode(MousePosition,_roomNodeTypeListSo.list.Find(x => x.isNone));
    }
    private void CreateRoomNode(object MousePosition,RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)MousePosition;

        // 创建房间节点可编写脚本的对象资产
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // 将房间节点添加到当前房间节点图房间节点列表
        _currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // 设置房间节点值
        roomNode.Initialise(new Rect(mousePosition, new Vector2(NodeWidth, NodeHeight)), _currentRoomNodeGraph, roomNodeType);

        // 将房间节点添加到房间节点图可编写脚本的对象资产数据库
        AssetDatabase.AddObjectToAsset(roomNode, _currentRoomNodeGraph);

        AssetDatabase.SaveAssets();
        // 更新节点字典
        _currentRoomNodeGraph.OnValidate();
        
    }
    
    /// <summary>
    /// 删除选中的房间节点
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        // 遍历所有的节点
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            //如果节点被选中并且不是入口节点
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                // 迭代遍历节点的子节点
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // 检索子房间节点
                    RoomNodeSO childRoomNode = _currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode is not null)
                    {
                        // 从子房间节点中删除parentID
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // 迭代遍历节点的父节点
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    // 移除父节点
                    RoomNodeSO parentRoomNode = _currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode is not null)
                    {
                        // 从子节点中删除parentID
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // 删除队列里面的所有节点
        while (roomNodeDeletionQueue.Count > 0)
        {
            // 从队列中取出一个节点
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            // 从字典中移除节点
            _currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            // 从列表中移除节点
            _currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // 在资源里删除节点
            DestroyImmediate(roomNodeToDelete, true);

            // 保存删除操作
            AssetDatabase.SaveAssets();

        }
    }

    /// <summary>
    /// 删除选中的节点之间的联系
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        // 迭代遍历全部节点
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    // 获取子节点
                    RoomNodeSO childRoomNode = _currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    // 如果子节点被选中了
                    if (childRoomNode is not null && childRoomNode.isSelected)
                    {
                        // 从父节点中移除子节点
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        // 从子节点中移除父节点
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // 清空选中节点
        ClearAllSelectedRoomNodes();
    }
    
    /// <summary>
    ///  清楚选中状态
    /// </summary>
    private void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }
    
    /// <summary>
    /// 选择所有的节点
    /// </summary>
    private void SelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    /// <summary>
    /// 处理鼠标抬起事件
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // 如果松开鼠标右键并当前拖动一条线
        if (currentEvent.button == 1 && _currentRoomNodeGraph.roomNodeToDrawLineFrom is not null)
        {
            // 判断是否在一个节点上
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode is not null)
            {
                // 如果可以的话将其设置为父房间节点的子节点（如果可以添加）
                if (_currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    // 在子房间节点设置父ID
                    roomNode.AddParentRoomNodeIDToRoomNode(_currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }


    /// <summary>
    /// 处理鼠标拖拽事件
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // 处理右键拖动事件-画线
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }// 处理鼠标左键拖动事件 -  移动房间节点 和 网格
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }

    }

    /// <summary>
    /// 处理右键拖动事件-画线
    /// </summary>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (_currentRoomNodeGraph.roomNodeToDrawLineFrom is not null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }
    
    /// <summary>
    /// 处理鼠标左键拖动事件 -  移动房间节点 和 网格
    /// </summary>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        _graphDrag = dragDelta;

        for (int i = 0; i < _currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            _currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }

        GUI.changed = true;
    }

    /// <summary>
    /// 从房间节点拖动连接线
    /// </summary>
    public void DragConnectingLine(Vector2 delta)
    {
        _currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// 从房间节点清除线拖动
    /// </summary>
    private void ClearLineDrag()
    {
        _currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        _currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// 在窗口中绘制房间节点之间的连接
    /// </summary>
    private void DrawRoomConnections()
    {
        // 循环遍历所有房间节点
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                // 循环遍历子房间节点
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // 从字典中获取子房间节点
                    if (_currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, _currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 绘制父房间节点和子房间节点之间的连接线
    /// </summary>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        //获取线的起始位置和结束位置
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;
        
        // 计算线的中间位置
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        // 线从起始位置到结束位置向量
        Vector2 direction = endPosition - startPosition;

        // 计算从中点开始的标准化垂直位置
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * ConnectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * ConnectingLineArrowSize;

        // 计算箭头的中点偏移位置
        Vector2 arrowHeadPoint = midPosition + direction.normalized * ConnectingLineArrowSize;

        // 画箭头
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1,
            Color.white, null, ConnectingLineArrowSize);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2,
            Color.white, null, ConnectingLineArrowSize);

        // 画线
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, ConnectingLineArrowSize);

        GUI.changed = true;
    }
    
    /// <summary>
    /// 绘制节点
    /// </summary>
    private void DrawRoomNodes()
    {
        //遍历全部的房间节点并绘制
        foreach (var roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(_roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(_roomNodeStyle);
            }
            
        }

        GUI.changed = true;
    }
    
    /// <summary>
    /// 检查器中的选择已更改
    /// </summary>
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            _currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}
