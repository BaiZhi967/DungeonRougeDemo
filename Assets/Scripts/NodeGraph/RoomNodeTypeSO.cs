using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Room/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    #region Header
    [Header("编辑器中是否可见")]
    #endregion Header
    public bool displayInNodeGraphEditor = true;
    #region Header
    [Header("是否为走廊类型")]
    #endregion Header
    public bool isCorridor;
    #region Header
    [Header("走廊朝向 南北")]
    #endregion Header
    public bool isCorridorNS;
    #region Header
    [Header("走廊朝向 东西")]
    #endregion Header
    public bool isCorridorEW;
    #region Header
    [Header("是否为入口类型")]
    #endregion Header
    public bool isEntrance;
    #region Header
    [Header("是否为BOSS房间类型")]
    #endregion Header
    public bool isBossRoom;
    #region Header
    [Header("是否为None(未分配房间类型)")]
    #endregion Header
    public bool isNone;

    #region 验证
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}