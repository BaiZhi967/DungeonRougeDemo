using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeTypeListSO", menuName = "Room/Room Node Type List")]
public class RoomNodeTypeListSO : ScriptableObject
{
    #region Header 房间节点类型列表
    [Space(10)]
    [Header("房间节点类型列表")]
    #endregion
    #region Tooltip
    [Tooltip("该列表应填充游戏的所有 RoomNodeTypeSO - 它用于代替枚举")]
    #endregion
    public List<RoomNodeTypeSO> list;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
#endif
    #endregion
}