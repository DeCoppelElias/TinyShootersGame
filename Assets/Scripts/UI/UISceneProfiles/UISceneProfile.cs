using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI/Scene Profile")]
public class UISceneProfile : ScriptableObject
{
    [Header("Shared UI")]
    public List<GameObject> sharedUIPrefabs;

    [Header("Player UI")]
    public List<GameObject> playerUIPrefabs;
}
