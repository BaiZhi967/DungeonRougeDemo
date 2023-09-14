using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{
    #region Header 游戏对象
    [Space(10)]
    [Header("游戏对象")]
    #endregion Header 游戏对象

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private TextMeshProUGUI messageTextTMP;
    [SerializeField] private CanvasGroup canvasGroup;

    #region Header 地牢关卡
    [Space(10)]
    [Header("地牢关卡")]
    #endregion



    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;
    [SerializeField] private int currentDungeonLevelListIndex = 0;
    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetailsSO playerDetails;
    private Player player;

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;
    private long gameScore;
    private int scoreMultiplier;
    private InstantiatedRoom bossRoom;
    private bool isFading = false;

    protected override void Awake()
    {
    
        base.Awake();
        //获取玩家信息
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        // 初始化玩家
        InstantiatePlayer();

    }

    /// <summary>
    /// 初始化玩家
    /// </summary>
    private void InstantiatePlayer()
    {
        
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);
        player = playerGameObject.GetComponent<Player>();
        player.Initialize(playerDetails);

    }

    private void OnEnable()
    {
        //事件订阅
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnPointsScored += StaticEventHandler_OnPointsScored;
        StaticEventHandler.OnMultiplier += StaticEventHandler_OnMultiplier;
        player.destroyedEvent.OnDestroyed += Player_OnDestroyed;
    }

    private void OnDisable()
    {

        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnPointsScored -= StaticEventHandler_OnPointsScored;
        StaticEventHandler.OnMultiplier -= StaticEventHandler_OnMultiplier;
        player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;

    }

    /// <summary>
    /// 房间变化事件
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);
    }

    /// <summary>
    /// 房间敌人全部击杀
    /// </summary>
    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        RoomEnemiesDefeated();
    }

    /// <summary>
    /// 分数事件
    /// </summary>
    private void StaticEventHandler_OnPointsScored(PointsScoredArgs pointsScoredArgs)
    {
        // 加分
        gameScore += pointsScoredArgs.points * scoreMultiplier;

        // 呼叫成绩改变事件
        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    /// <summary>
    /// 乘胜追击事件
    /// </summary>
    private void StaticEventHandler_OnMultiplier(MultiplierArgs multiplierArgs)
    {
        if (multiplierArgs.multiplier)
        {
            scoreMultiplier++;
        }
        else
        {
            scoreMultiplier--;
        }

        
        scoreMultiplier = Mathf.Clamp(scoreMultiplier, 1, 30);

        // 分数改变事件
        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    /// <summary>
    /// 玩家销毁事件
    /// </summary>
    private void Player_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        previousGameState = gameState;
        gameState = GameState.gameLost;
    }

    
    private void Start()
    {
        previousGameState = GameState.gameStarted;
        gameState = GameState.gameStarted;

       
        gameScore = 0;
        scoreMultiplier = 1;

        // 淡入淡出
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));
    }


    private void Update()
    {
        HandleGameState();

        //// Test
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    gameState = GameState.gameStarted;
        //}

    }

    /// <summary>
    /// 游戏状态改变
    /// </summary>
    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:

                PlayDungeonLevel(currentDungeonLevelListIndex);
                gameState = GameState.playingLevel;
                RoomEnemiesDefeated();

                break;
            case GameState.playingLevel:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    DisplayDungeonOverviewMap();
                }

                break;
            case GameState.engagingEnemies:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }

                break;
            case GameState.dungeonOverviewMap:

                if (Input.GetKeyUp(KeyCode.Tab))
                {
                    DungeonMap.Instance.ClearDungeonOverViewMap();
                }

                break;
            case GameState.bossStage:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    DisplayDungeonOverviewMap();
                }

                break;
            case GameState.engagingBoss:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }

                break;

            case GameState.levelCompleted:
                StartCoroutine(LevelCompleted());
                break;
            case GameState.gameWon:

                if (previousGameState != GameState.gameWon)
                    StartCoroutine(GameWon());

                break;
            case GameState.gameLost:

                if (previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines();
                    StartCoroutine(GameLost());
                }

                break;
            case GameState.restartGame:

                RestartGame();

                break;
            case GameState.gamePaused:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }
                break;
        }

    }

    /// <summary>
    /// 设置玩家当前所在房间
    /// </summary>
    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;

        //// Debug
        //Debug.Log(room.prefab.name.ToString());
    }

    /// <summary>
    /// 房间怪物击杀
    /// </summary>
    private void RoomEnemiesDefeated()
    {
        bool isDungeonClearOfRegularEnemies = true;
        bossRoom = null;
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            if (keyValuePair.Value.roomNodeType.isBossRoom)
            {
                bossRoom = keyValuePair.Value.instantiatedRoom;
                continue;
            }

            if (!keyValuePair.Value.isClearedOfEnemies)
            {
                isDungeonClearOfRegularEnemies = false;
                break;
            }
        }

        if ((isDungeonClearOfRegularEnemies && bossRoom == null) || (isDungeonClearOfRegularEnemies && bossRoom.room.isClearedOfEnemies))
        {

            if (currentDungeonLevelListIndex < dungeonLevelList.Count - 1)
            {
                gameState = GameState.levelCompleted;
            }
            else
            {
                gameState = GameState.gameWon;
            }
        }
        else if (isDungeonClearOfRegularEnemies)
        {
            gameState = GameState.bossStage;

            StartCoroutine(BossStage());
        }

    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGameMenu()
    {
        if (gameState != GameState.gamePaused)
        {
            pauseMenu.SetActive(true);
            GetPlayer().playerControl.DisablePlayer();

            previousGameState = gameState;
            gameState = GameState.gamePaused;
        }
        else if (gameState == GameState.gamePaused)
        {
            pauseMenu.SetActive(false);
            GetPlayer().playerControl.EnablePlayer();

            gameState = previousGameState;
            previousGameState = GameState.gamePaused;

        }
    }

    /// <summary>
    /// 显示地牢小地图
    /// </summary>
    private void DisplayDungeonOverviewMap()
    {
        
        if (isFading)
            return;

        DungeonMap.Instance.DisplayDungeonOverViewMap();
    }


    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        // 根据关卡等级创建地牢
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if (!dungeonBuiltSucessfully)
        {
            Debug.LogError("无法正确创建地牢");
        }

        StaticEventHandler.CallRoomChangedEvent(currentRoom);
        player.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f, (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);
        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(player.gameObject.transform.position);
        StartCoroutine(DisplayDungeonLevelText());

        //// ** Test
        //RoomEnemiesDefeated();
    }

    /// <summary>
    /// 显示关卡难度
    /// </summary>
    private IEnumerator DisplayDungeonLevelText()
    {
        
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

        GetPlayer().playerControl.DisablePlayer();

        string messageText = "关卡难度 LEVEL " + (currentDungeonLevelListIndex + 1).ToString() + "\n\n" + dungeonLevelList[currentDungeonLevelListIndex].levelName.ToUpper();

        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 2f));

        GetPlayer().playerControl.EnablePlayer();

        yield return StartCoroutine(Fade(1f, 0f, 2f, Color.black));

    }

    /// <summary>
    /// 屏幕Msg
    /// </summary>
    private IEnumerator DisplayMessageRoutine(string text, Color textColor, float displaySeconds)
    {
        
        messageTextTMP.SetText(text);
        messageTextTMP.color = textColor;

        
        if (displaySeconds > 0f)
        {
            float timer = displaySeconds;

            while (timer > 0f && !Input.GetKeyDown(KeyCode.Return))
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            while (!Input.GetKeyDown(KeyCode.Return))
            {
                yield return null;
            }
        }

        yield return null;

        messageTextTMP.SetText("");
    }

    /// <summary>
    /// 进入Boss阶段
    /// </summary>
    private IEnumerator BossStage()
    {
        // 解锁Boss房间
        bossRoom.gameObject.SetActive(true);
        bossRoom.UnlockDoors(0f);

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        // Boss提示
        yield return StartCoroutine(DisplayMessageRoutine("很好  " + GameResources.Instance.currentPlayer.playerName + "!  你已经击杀完全部怪物了\n\n现在请找到BOSS，并击败他....祝你好运!", Color.white, 5f));

        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

    }

    /// <summary>
    /// 关卡完成提示
    /// </summary>
    private IEnumerator LevelCompleted()
    {

        gameState = GameState.playingLevel;
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        yield return StartCoroutine(DisplayMessageRoutine("恭喜你 " + GameResources.Instance.currentPlayer.playerName + "! \n\n你通过了这一层地牢", Color.white, 5f));
        yield return StartCoroutine(DisplayMessageRoutine("收集你的战利品 ....然后按下返回键\n\n去挑战更深的地牢吧", Color.white, 5f));

        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));
        while (!Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }

        yield return null;
        currentDungeonLevelListIndex++;

        PlayDungeonLevel(currentDungeonLevelListIndex);
    }

    /// <summary>
    /// Fade
    /// </summary>
    public IEnumerator Fade(float startFadeAlpha, float targetFadeAlpha, float fadeSeconds, Color backgroundColor)
    {
        isFading = true;

        Image image = canvasGroup.GetComponent<Image>();
        image.color = backgroundColor;

        float time = 0;

        while (time <= fadeSeconds)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startFadeAlpha, targetFadeAlpha, time / fadeSeconds);
            yield return null;
        }

        isFading = false;

    }


    /// <summary>
    /// 游戏胜利
    /// </summary>
    private IEnumerator GameWon()
    {
        previousGameState = GameState.gameWon;

        
        GetPlayer().playerControl.DisablePlayer();

        int rank = HighScoreManager.Instance.GetRank(gameScore);

        string rankText;

        // Test if the score is in the rankings
        if (rank > 0 && rank <= Settings.numberOfHighScoresToSave)
        {
            rankText = "你的成绩 " + rank.ToString("#0") + " 在排行榜上 " + Settings.numberOfHighScoresToSave.ToString("#0");

            string name = GameResources.Instance.currentPlayer.playerName;

            if (name == "")
            {
                name = playerDetails.playerCharacterName.ToUpper();
            }

            
            HighScoreManager.Instance.AddScore(new Score() { playerName = name, levelDescription = "LEVEL " + (currentDungeonLevelListIndex + 1).ToString() + " - " + GetCurrentDungeonLevel().levelName.ToUpper(), playerScore = gameScore }, rank);


        }
        else
        {
            rankText = "你的成绩没有登上排行榜 " + Settings.numberOfHighScoresToSave.ToString("#0");
        }

        
        yield return new WaitForSeconds(1f);

        
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        
        yield return StartCoroutine(DisplayMessageRoutine("很好 " + GameResources.Instance.currentPlayer.playerName + "! 你通过了所有的地牢", Color.white, 3f));

        yield return StartCoroutine(DisplayMessageRoutine("你的分数 " + gameScore.ToString("###,###0") + "\n\n" + rankText, Color.white, 4f));

        yield return StartCoroutine(DisplayMessageRoutine("按返回回到主界面", Color.white, 0f));

        
        gameState = GameState.restartGame;
    }

    /// <summary>
    /// 游戏失败
    /// </summary>
    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;

        
        GetPlayer().playerControl.DisablePlayer();


        
        int rank = HighScoreManager.Instance.GetRank(gameScore);
        string rankText;

        
        if (rank > 0 && rank <= Settings.numberOfHighScoresToSave)
        {
            
            rankText = "你的成绩 " + rank.ToString("#0") + " 在排行榜上 " + Settings.numberOfHighScoresToSave.ToString("#0");

            string name = GameResources.Instance.currentPlayer.playerName;

            if (name == "")
            {
                name = playerDetails.playerCharacterName.ToUpper();
            }

            
            HighScoreManager.Instance.AddScore(new Score() { playerName = name, levelDescription = "LEVEL " + (currentDungeonLevelListIndex + 1).ToString() + " - " + GetCurrentDungeonLevel().levelName.ToUpper(), playerScore = gameScore }, rank);
        }
        else
        {
            rankText = "你的成绩没有登上排行榜 " + Settings.numberOfHighScoresToSave.ToString("#0");
        }


        
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));
        Enemy[] enemyArray = GameObject.FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemyArray)
        {
            enemy.gameObject.SetActive(false);
        }


        yield return StartCoroutine(DisplayMessageRoutine("很可惜 " + GameResources.Instance.currentPlayer.playerName + "! 你殒命于地牢之中", Color.white, 2f));
        yield return StartCoroutine(DisplayMessageRoutine("你的分数 " + gameScore.ToString("###,###0") + "\n\n" + rankText, Color.white, 4f));
        yield return StartCoroutine(DisplayMessageRoutine("按返回回到主界面", Color.white, 0f));

        gameState = GameState.restartGame;
    }

    /// <summary>
    /// 回到主界面
    /// </summary>
    private void RestartGame()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// 获取玩家
    /// </summary>
    public Player GetPlayer()
    {
        return player;
    }


    /// <summary>
    /// 玩家小地图icon
    /// </summary>
    public Sprite GetPlayerMiniMapIcon()
    {
        return playerDetails.playerMiniMapIcon;
    }


    /// <summary>
    /// 获取玩家当前所在房间
    /// </summary>
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// 获取当前关卡
    /// </summary>
    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }


    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(pauseMenu), pauseMenu);
        HelperUtilities.ValidateCheckNullValue(this, nameof(messageTextTMP), messageTextTMP);
        HelperUtilities.ValidateCheckNullValue(this, nameof(canvasGroup), canvasGroup);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }

#endif

    #endregion Validation

}