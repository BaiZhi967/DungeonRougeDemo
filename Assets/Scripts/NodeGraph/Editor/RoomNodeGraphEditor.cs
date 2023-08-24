using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    [MenuItem("Window/Room Node Graph")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(RoomNodeGraphEditor));
    }
}
