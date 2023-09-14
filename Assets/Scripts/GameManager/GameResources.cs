using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Audio;

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

    #region Header 关卡列表
    [Space(10)]
    [Header("关卡列表")]
    #endregion

    public RoomNodeTypeListSO roomNodeTypeList;

    #region 选择角色
    [Space(10)]
    [Header("选择角色")]
    #endregion PLAYER SELECTION
    public GameObject playerSelectionPrefab;

    #region Header 玩家
    [Space(10)]
    [Header("玩家")]
    #endregion Header 玩家
    public List<PlayerDetailsSO> playerDetailsList;
    public CurrentPlayerSO currentPlayer;

    #region Header 音乐
    [Space(10)]
    [Header("音乐")]
    #endregion Header 音乐
    public AudioMixerGroup musicMasterMixerGroup;
    public MusicTrackSO mainMenuMusic;
    public AudioMixerSnapshot musicOnFullSnapshot;
    public AudioMixerSnapshot musicLowSnapshot;
    public AudioMixerSnapshot musicOffSnapshot;

    #region Header 音效
    [Space(10)]
    [Header("音效")]
    #endregion 音效
    public AudioMixerGroup soundsMasterMixerGroup;
    public SoundEffectSO doorOpenCloseSoundEffect;
    public SoundEffectSO tableFlip;
    public SoundEffectSO chestOpen;
    public SoundEffectSO healthPickup;
    public SoundEffectSO weaponPickup;
    public SoundEffectSO ammoPickup;

    #region Header 材质
    [Space(10)]
    [Header("材质")]
    #endregion
    public Material dimmedMaterial;
    public Material litMaterial;
    public Shader variableLitShader;
    public Shader materializeShader;

    #region Header 特殊tile
    [Space(10)]
    [Header("特殊tile")]
    #endregion Header 特殊tile
    public TileBase[] enemyUnwalkableCollisionTilesArray;
    public TileBase preferredEnemyPathTile;

    #region Header UI
    [Space(10)]
    [Header("UI")]
    #endregion
    public GameObject heartPrefab;
    public GameObject ammoIconPrefab;
    public GameObject scorePrefab;

    #region Header 箱子
    [Space(10)]
    [Header("箱子")]
    #endregion
    public GameObject chestItemPrefab;
    public Sprite heartIcon;
    public Sprite bulletIcon;

    #region Header 小地图
    [Space(10)]
    [Header("小地图")]
    #endregion
    #region Tooltip
    [Tooltip("小地图 prefab")]
    #endregion
    public GameObject minimapSkullPrefab;


    #region Validation
#if UNITY_EDITOR
    
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(roomNodeTypeList), roomNodeTypeList);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerSelectionPrefab), playerSelectionPrefab);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(playerDetailsList), playerDetailsList);
        HelperUtilities.ValidateCheckNullValue(this, nameof(currentPlayer), currentPlayer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(mainMenuMusic), mainMenuMusic);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundsMasterMixerGroup), soundsMasterMixerGroup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorOpenCloseSoundEffect), doorOpenCloseSoundEffect);
        HelperUtilities.ValidateCheckNullValue(this, nameof(tableFlip), tableFlip);
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestOpen), chestOpen);
        HelperUtilities.ValidateCheckNullValue(this, nameof(healthPickup), healthPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoPickup), ammoPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponPickup), weaponPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(litMaterial), litMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(dimmedMaterial), dimmedMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(variableLitShader), variableLitShader);
        HelperUtilities.ValidateCheckNullValue(this, nameof(materializeShader), materializeShader);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemyUnwalkableCollisionTilesArray), enemyUnwalkableCollisionTilesArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(preferredEnemyPathTile), preferredEnemyPathTile);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicMasterMixerGroup), musicMasterMixerGroup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicOnFullSnapshot), musicOnFullSnapshot);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicLowSnapshot), musicLowSnapshot);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicOffSnapshot), musicOffSnapshot); 
        HelperUtilities.ValidateCheckNullValue(this, nameof(heartPrefab), heartPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoIconPrefab), ammoIconPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(scorePrefab), scorePrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestItemPrefab), chestItemPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(heartIcon), heartIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(bulletIcon), bulletIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(minimapSkullPrefab), minimapSkullPrefab);
    }

#endif
    #endregion
}