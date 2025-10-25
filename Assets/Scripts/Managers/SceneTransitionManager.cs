using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    public int Level { get; set; }

    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(this);
    }
}
