using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class Settings
{
    #region 像素
    public const float pixelsPerUnit = 16f;
    public const float tileSizePixels = 16f;
    #endregion

    #region 关卡构建
    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion

    #region 房间设置
    public const float fadeInTime = 0.5f; // 淡入时间
    public const int maxChildCorridors = 3; // 房间最多的走廊数量
    public const float doorUnlockDelay = 1f;
    #endregion


    #region Animator 参数
    // Animator 参数 - Player
    public static int aimUp = Animator.StringToHash("aimUp");
    public static int aimDown = Animator.StringToHash("aimDown");
    public static int aimUpRight = Animator.StringToHash("aimUpRight");
    public static int aimUpLeft = Animator.StringToHash("aimUpLeft");
    public static int aimRight = Animator.StringToHash("aimRight");
    public static int aimLeft = Animator.StringToHash("aimLeft");
    public static int isIdle = Animator.StringToHash("isIdle");
    public static int isMoving = Animator.StringToHash("isMoving");
    public static int rollUp = Animator.StringToHash("rollUp");
    public static int rollRight = Animator.StringToHash("rollRight");
    public static int rollLeft = Animator.StringToHash("rollLeft");
    public static int rollDown = Animator.StringToHash("rollDown");
    public static int flipUp = Animator.StringToHash("flipUp");
    public static int flipRight = Animator.StringToHash("flipRight");
    public static int flipLeft = Animator.StringToHash("flipLeft");
    public static int flipDown = Animator.StringToHash("flipDown");
    public static int use = Animator.StringToHash("use");
    public static float baseSpeedForPlayerAnimations = 8f;

    // Animator 参数 - Enemy
    public static float baseSpeedForEnemyAnimations = 3f;


    // Animator 参数 - Door
    public static int open = Animator.StringToHash("open");

    // Animator 参数 - 装饰物
    public static int destroy = Animator.StringToHash("destroy");
    public static String stateDestroyed = "Destroyed";

    #endregion

    #region Tags
    public const string playerTag = "Player";
    public const string playerWeapon = "playerWeapon";
    #endregion

    #region 音频相关
    public const float musicFadeOutTime = 0.5f;  
    public const float musicFadeInTime = 0.5f;  
    #endregion

    #region 开火
    public const float useAimAngleDistance = 3.5f;
    #endregion

    #region Astar 寻路相关
    public const int defaultAStarMovementPenalty = 40;
    public const int preferredPathAStarMovementPenalty = 1;
    public const int targetFrameRateToSpreadPathfindingOver = 60;
    public const float playerMoveDistanceToRebuildPath = 3f;
    public const float enemyPathRebuildCooldown = 2f;

    #endregion

    #region 敌人
    public const int defaultEnemyHealth = 20;
    #endregion

    #region UI
    public const float uiHeartSpacing = 16f;
    public const float uiAmmoIconSpacing = 4f;
    #endregion

    #region 触碰伤害
    public const float contactDamageCollisionResetDelay = 0.5f;
    #endregion

    #region 排行榜
    public const int numberOfHighScoresToSave = 100;
    #endregion
}
