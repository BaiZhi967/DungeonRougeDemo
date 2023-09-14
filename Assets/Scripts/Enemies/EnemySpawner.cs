using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawner : SingletonMonobehaviour<EnemySpawner>
{
    private int enemiesToSpawn;
    private int currentEnemyCount;
    private int enemiesSpawnedSoFar;
    private int enemyMaxConcurrentSpawnNumber;
    private Room currentRoom;
    private RoomEnemySpawnParameters roomEnemySpawnParameters;

    private void OnEnable()
    {
        
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {

        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    /// <summary>
    /// 房间更换事件
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        enemiesSpawnedSoFar = 0;
        currentEnemyCount = 0;

        currentRoom = roomChangedEventArgs.room;

        MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);


        if (currentRoom.roomNodeType.isCorridorEW || currentRoom.roomNodeType.isCorridorNS || currentRoom.roomNodeType.isEntrance)
            return;

        if (currentRoom.isClearedOfEnemies) return;


        enemiesToSpawn = currentRoom.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel());

        roomEnemySpawnParameters = currentRoom.GetRoomEnemySpawnParameters(GameManager.Instance.GetCurrentDungeonLevel());

        if (enemiesToSpawn == 0)
        {
 
            currentRoom.isClearedOfEnemies = true;

            return;
        }

        enemyMaxConcurrentSpawnNumber = GetConcurrentEnemies();


        MusicManager.Instance.PlayMusic(currentRoom.battleMusic, 0.2f, 0.5f);

        currentRoom.instantiatedRoom.LockDoors();


        SpawnEnemies();
    }

    /// <summary>
    /// 生成敌人
    /// </summary>
    private void SpawnEnemies()
    {

        if (GameManager.Instance.gameState == GameState.bossStage)
        {
            GameManager.Instance.previousGameState = GameState.bossStage;
            GameManager.Instance.gameState = GameState.engagingBoss;
        }


        else if(GameManager.Instance.gameState == GameState.playingLevel)
        {
            GameManager.Instance.previousGameState = GameState.playingLevel;
            GameManager.Instance.gameState = GameState.engagingEnemies;
        }

        StartCoroutine(SpawnEnemiesRoutine());
    }

    /// <summary>
    /// 生成敌人协程
    /// </summary>
    private IEnumerator SpawnEnemiesRoutine()
    {
        Grid grid = currentRoom.instantiatedRoom.grid;

        
        RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(currentRoom.enemiesByLevelList);

        
        if (currentRoom.spawnPositionArray.Length > 0)
        {
           
            for (int i = 0; i < enemiesToSpawn; i++)
            {
               
                while (currentEnemyCount >= enemyMaxConcurrentSpawnNumber)
                {
                    yield return null;
                }

                Vector3Int cellPosition = (Vector3Int)currentRoom.spawnPositionArray[Random.Range(0, currentRoom.spawnPositionArray.Length)];

                
                CreateEnemy(randomEnemyHelperClass.GetItem(), grid.CellToWorld(cellPosition));

                yield return new WaitForSeconds(GetEnemySpawnInterval());
            }
        }
    }

    /// <summary>
    /// 随机生成间隔
    /// </summary>
    private float GetEnemySpawnInterval()
    {
        return (Random.Range(roomEnemySpawnParameters.minSpawnInterval, roomEnemySpawnParameters.maxSpawnInterval));
    }

    /// <summary>
    /// 获取生成怪物数
    /// </summary>
    private int GetConcurrentEnemies()
    {
        return (Random.Range(roomEnemySpawnParameters.minConcurrentEnemies, roomEnemySpawnParameters.maxConcurrentEnemies));
    }

    /// <summary>
    /// 指定位置生成敌人
    /// </summary>
    private void CreateEnemy(EnemyDetailsSO enemyDetails, Vector3 position)
    {

        enemiesSpawnedSoFar++;
        currentEnemyCount++;
        DungeonLevelSO dungeonLevel = GameManager.Instance.GetCurrentDungeonLevel();
        GameObject enemy = Instantiate(enemyDetails.enemyPrefab, position, Quaternion.identity, transform);
        enemy.GetComponent<Enemy>().EnemyInitialization(enemyDetails, enemiesSpawnedSoFar, dungeonLevel);
        enemy.GetComponent<DestroyedEvent>().OnDestroyed += Enemy_OnDestroyed;

    }

    /// <summary>
    /// 敌人被销毁事件
    /// </summary>
    private void Enemy_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {

        destroyedEvent.OnDestroyed -= Enemy_OnDestroyed;

        currentEnemyCount--;

        StaticEventHandler.CallPointsScoredEvent(destroyedEventArgs.points);

        if (currentEnemyCount <= 0 && enemiesSpawnedSoFar == enemiesToSpawn)
        {
            currentRoom.isClearedOfEnemies = true;

            if (GameManager.Instance.gameState == GameState.engagingEnemies)
            {
                GameManager.Instance.gameState = GameState.playingLevel;
                GameManager.Instance.previousGameState = GameState.engagingEnemies;
            }

            else if (GameManager.Instance.gameState == GameState.engagingBoss)
            {
                GameManager.Instance.gameState = GameState.bossStage;
                GameManager.Instance.previousGameState = GameState.engagingBoss;
            }


            currentRoom.instantiatedRoom.UnlockDoors(Settings.doorUnlockDelay);

            MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);

            StaticEventHandler.CallRoomEnemiesDefeatedEvent(currentRoom);
        }
    }

}