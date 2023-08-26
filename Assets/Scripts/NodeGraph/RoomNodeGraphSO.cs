using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Room/RoomNodeGraph")]
public class RoomNodeGraphSO : ScriptableObject
{
     public RoomNodeTypeListSO roomNodeTypeList;
     public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
     public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();
    
}
