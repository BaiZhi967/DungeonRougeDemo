using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Room/RoomNodeGraph")]
public class RoomNodeGraphSO : ScriptableObject
{
     public RoomNodeTypeListSO roomNodeTypeList;
     public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
     public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();
     
     private void Awake()
     {
          LoadRoomNodeDictionary();
     }
     
     /// <summary>
     /// 从房间节点列表中加载房间节点字典
     /// </summary>
     private void LoadRoomNodeDictionary()
     {
          roomNodeDictionary.Clear();

          // 填充字典
          foreach (RoomNodeSO node in roomNodeList)
          {
               roomNodeDictionary[node.id] = node;
          }
     }
     
     #region Editor Code

     // 以下代码只能在 Unity 编辑器中运行
#if UNITY_EDITOR

     [HideInInspector] public RoomNodeSO roomNodeToDrawLineFrom = null;
     [HideInInspector] public Vector2 linePosition;

     // 每次在编辑器中进行更改时重新填充节点字典
     public void OnValidate()
     {
          LoadRoomNodeDictionary();
     }

     public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position)
     {
          roomNodeToDrawLineFrom = node;
          linePosition = position;
     }

#endif

     #endregion Editor Code
    
}
