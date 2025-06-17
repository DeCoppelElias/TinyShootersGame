using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGunnerAbility", menuName = "ClassAbilities/GunnerAbility")]
public class GunnerAbility : ClassAbility
{
    [SerializeField]
    private GameObject littleGunnerPrefab;
    private GameObject littleGunner;

    [SerializeField]
    private float duration = 5f;
    public override void PerformAbility(Player player)
    {
        littleGunner.SetActive(true);
        player.StartCoroutine(PerformActionAfterDelay(duration, () => littleGunner.SetActive(false)));
    }

    // Start is called before the first frame update
    public override bool Initialise(Player player)
    {
        littleGunner = Instantiate(littleGunnerPrefab, player.transform);
        littleGunner.GetComponent<LittleGunner>().SetOwner(player);
        littleGunner.SetActive(false);

        return true;
    }

    private IEnumerator PerformActionAfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);

        action();
    }
}
