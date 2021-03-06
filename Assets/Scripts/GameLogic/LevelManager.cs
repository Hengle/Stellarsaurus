﻿using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance = null;
    public bool isLobby;    

    public Transform[] startingPositions;
    public Transform extractionObjectSpawnPosition;
    public ExtractionObjective extractionObjective;
    public ScorePopup scorePopupPrefab;

    [Header("Weapons")]
    [Header("")]
    public Weapon weaponPrefab;
    public float timeBetweenWeaponSpawns;
    public SkyDrops randomWeaponSpawnPositionParameters;
    public bool shouldWeaponsFallFromSky;

    [StringInList(typeof(StringInListHelper), "AllWeaponNames")] public string[] weaponsInThisLevel;

    [Header("Doesn't spawn AI yet")]
    [Header("")]
    [StringInList(typeof(StringInListHelper), "AllAiNames")] public string[] aisInThisLevel;
    public AI aiPrefab;

    public bool debugGhost;
    public bool isClimbLevel;

    [HideInInspector] public List<WeaponType> weaponTypes = new List<WeaponType>();


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (isLobby == false)
        {
            if (weaponsInThisLevel.Length > 0)
            {
                weaponTypes = GameManager.instance.loader.GetWeaponsByNames(weaponsInThisLevel);
                if (shouldWeaponsFallFromSky)
                {
                    StartCoroutine("SpawnWeapons");
                }
            }
        }        
    }

    IEnumerator SpawnWeapons()
    {
        while (true)
        {
            Weapon weapon = Instantiate(weaponPrefab,
                new Vector2(Random.Range(randomWeaponSpawnPositionParameters.minX,
                randomWeaponSpawnPositionParameters.maxX),
                randomWeaponSpawnPositionParameters.y),
                Quaternion.identity);
            weapon.Init(weaponTypes, WeaponSpawnType.FallFromSky);
            yield return new WaitForSeconds(timeBetweenWeaponSpawns);
        }
    }

    public void SpawnExtractionObject ()
    {
        Instantiate(extractionObjective, extractionObjectSpawnPosition.position, Quaternion.identity);
    }
}

[System.Serializable]
public struct SkyDrops
{
    public float minX;
    public float maxX;
    public float y;
}