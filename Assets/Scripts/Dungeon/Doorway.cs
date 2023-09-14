using UnityEngine;
[System.Serializable]
public class Doorway 
{
    public Vector2Int position;
    public Orientation orientation;
    public GameObject doorPrefab;
    #region Header
    [Header("开始复印的左上角位置")]
    #endregion
    public Vector2Int doorwayStartCopyPosition;
    #region Header
    [Header("要被负责的门的tiles的宽度")]
    #endregion
    public int doorwayCopyTileWidth;
    #region Header
    [Header("要被负责的门的tiles的长度")]
    #endregion
    public int doorwayCopyTileHeight;
    [HideInInspector]
    public bool isConnected = false;
    [HideInInspector]
    public bool isUnavailable = false;
}
