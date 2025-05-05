using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelSelectionScreen : MonoBehaviour
{
    internal static LevelSelectionScreen Instance;
    
    [SerializeField] LevelList levelList;
    [SerializeField] LevelSelectTile levelSelectTilePrefab;

    [SerializeField] float flipDelay = 0.1f;
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LevelSelectionManager.Instance.currentLevelList = levelList;
        StartCoroutine(GenerateLevelSelectTiles());
    }

    IEnumerator GenerateLevelSelectTiles()
    {
        List<LevelSelectTile> levelSelectTiles = new List<LevelSelectTile>();
        
        // generate tiles
        for (int i = 0; i < 32; i++)
        {
            foreach (Coordinates coordinates in Coordinates.GetRing(new Coordinates(0,0), i))
            {
                LevelSelectTile tile = Instantiate(levelSelectTilePrefab, coordinates.GetPosition(), Quaternion.LookRotation(-transform.up, transform.forward), transform);
                levelSelectTiles.Add(tile);
            }
        }

        // load or generate level data
        for (int i = 0; i < levelList.levels.Count; i++)
        {
            LevelStatus levelStatus = SaveDataManager.LevelStatusesData.levelStatuses.Find(status => status.levelID.Equals(levelList.levels[i].levelID));
            if (levelStatus == null)
            {
                levelStatus = new LevelStatus();
                SaveDataManager.LevelStatusesData.levelStatuses.Add(levelStatus);
                levelStatus.levelID = levelList.levels[i].levelID;

                // unlock first level by default
                if (i == 1)
                {
                    levelStatus.clearedStatus = LevelClearedStatus.Unlocked;
                }
            }

            // pass data to tile
            levelSelectTiles[i].levelProfile = levelList.levels[i];
            levelSelectTiles[i].levelList = levelList;
            levelSelectTiles[i].clearedStatus = levelStatus.clearedStatus;
        }

        // wait for tiles to spawn in
        yield return null;
        
        // spawn in locked levels right away
        for (int i = 0; i < levelList.levels.Count; i++)
        {
            if (levelSelectTiles[i].clearedStatus <= LevelClearedStatus.Locked)
            {
                levelSelectTiles[i].Initialize();
            }
        }
        
        // spawn in unlocked levels with animation
        for (int i = 0; i < levelList.levels.Count; i++)
        {
            if (levelSelectTiles[i].clearedStatus > LevelClearedStatus.Locked)
            {
                levelSelectTiles[i].Initialize();
                yield return new WaitForSeconds(flipDelay);
            }
        }
    }

    void OnDisable()
    {
        Instance = null;
    }
}
