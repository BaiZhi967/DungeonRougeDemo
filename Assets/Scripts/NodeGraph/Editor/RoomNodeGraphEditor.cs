using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle _roomNodeStyle;

    private static RoomNodeGraphSO _currentRoomNodeGraph;
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

    [MenuItem("Window/Room Node Graph")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow(typeof(RoomNodeGraphEditor));
    }

    private void OnEnable()
    {
        _roomNodeStyle = new GUIStyle();
        _roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        _roomNodeStyle.normal.textColor = Color.white;
        _roomNodeStyle.padding = new RectOffset(NodePadding, NodePadding, NodePadding, NodePadding);
        _roomNodeStyle.border = new RectOffset(NodeBorder, NodeBorder, NodeBorder, NodeBorder);

        _roomNodeTypeListSo = GameResources.Instance.roomNodeTypeList;

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
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
    }

    private void ShowContextMenu(Vector2 MousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("创建房间节点"), false, CreateRoomNode, MousePosition);
        menu.ShowAsContext();
    }

    private void CreateRoomNode(object MousePosition)
    {
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
        foreach (var roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(_roomNodeStyle);
        }

        GUI.changed = true;
    }
}
