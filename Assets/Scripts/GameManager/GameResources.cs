using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }
    
    #region Header 房间
    [Space(10)]
    [Header("房间")]
    #endregion
    #region Tooltip
    [Tooltip("放入房间类型到 RoomNodeTypeListSO")]
    #endregion

    public RoomNodeTypeListSO roomNodeTypeList;
}