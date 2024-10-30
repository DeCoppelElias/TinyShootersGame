﻿using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSniperAbility", menuName = "ClassAbilities/SniperAbility")]
public class SniperAbility : ClassAbility
{
    private GameStateManager gameStateManager;
    private ShootingAbility shootingAbility;

    [SerializeField]
    private int abilityDuration = 3;
    [SerializeField]
    private float effectDuration = 0.5f;
    
    private IEnumerator PerformActionAfterRealDelay(float delay, Action action)
    {
        yield return new WaitForSecondsRealtime(delay);

        action();
    }

    public override bool Initialise(Player player)
    {
        this.gameStateManager = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();
        this.shootingAbility = player.GetComponent<ShootingAbility>();

        if (gameStateManager == null || shootingAbility == null) return false;
        return true;
    }

    public override void PerformAbility(Player player)
    {
        gameStateManager.SlowDownTime(0.05f, effectDuration, abilityDuration);
        shootingAbility.workWithRealTime = true;

        player.StartCoroutine(PerformActionAfterRealDelay((2 * effectDuration) + abilityDuration, () => shootingAbility.workWithRealTime = false));
    }
}
