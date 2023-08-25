using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle _roomNodeStyle;
    
    //Node边框大小
    private const float NodeWidth = 160f;
    private const float NodeHeight = 75f;
    private const int NodePadding = 25;
    private const int NodeBorder = 12;

    [MenuItem("Window/Room Node Graph")]
    public static void ShowWindow()
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

    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(new Vector2(100,100), new Vector2(NodeWidth,NodeHeight)),_roomNodeStyle);
        EditorGUILayout.LabelField("Node 1");
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(new Vector2(300,300), new Vector2(NodeWidth,NodeHeight)),_roomNodeStyle);
        EditorGUILayout.LabelField("Node 2");
        GUILayout.EndArea();
    }
}
