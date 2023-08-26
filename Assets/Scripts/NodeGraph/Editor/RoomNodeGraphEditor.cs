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
        if (_currentRoomNodeGraph is not null)
        {
            ProcessEvents(Event.current);
            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }
    
    


    private void ProcessEvents(Event currentEvent)
    {
        // 如果鼠标所在的房间节点为空或当前未被拖动，则获取该房间节点
        if (_currentRoomNode is null || _currentRoomNode.isLeftClickDragging == false)
        {
            _currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // 如果鼠标不在房间节点上
        if (_currentRoomNode is null)
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
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
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
