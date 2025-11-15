using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    private enum TutorialState { Explanation, Movement, Shoot, Dash, Reflect, Combat, Upgrading, ClassAbility, Endless, Pause }
    [Header("Tutorial Settings")]
    [SerializeField] private TutorialState tutorialState = TutorialState.Explanation;
    [SerializeField] private float tutorialStepDelay = 5f;
    [SerializeField] private List<GameObject> tutorialEnemies;
    [SerializeField] private Tilemap warningTilemap;
    [SerializeField] private Tile warningTile;

    private Player player;
    private PlayerUIManager playerUIManager;

    private GameObject enemy;
    private float startTime = 0;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        playerUIManager = player.GetComponentInChildren<PlayerUIManager>();
        tutorialState = TutorialState.Explanation;

        StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
        {
            ToMovement();
        }));

        StartCoroutine(PerformAfterDelay(0.1f, () =>
        {
            playerUIManager.Disable<AbilityUI>();
            SharedUIManager.Instance.Disable<ScoreUI>();
        }));
    }

    private void Update()
    {
        if (player.Health < player.MaxHealth / 2) player.Health = player.MaxHealth;

        if (tutorialState == TutorialState.Shoot)
        {
            if (player.GetComponent<ShootingAbility>().Shooting)
            {
                tutorialState = TutorialState.Pause;
                SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().EnableDoneText();
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().DisableDoneText();
                    ToDash();
                }));
            }
        }
        else if (tutorialState == TutorialState.Dash)
        {
            if (player.GetComponent<DashAbility>().Dashing())
            {
                tutorialState = TutorialState.Pause;
                SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().EnableDoneText();
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().DisableDoneText();
                    ToReflect();
                }));
            }
        }
        else if (tutorialState == TutorialState.Reflect)
        {
            if (player.GetComponent<ReflectShieldAbility>().IsReflecting())
            {
                tutorialState = TutorialState.Pause;
                SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().EnableDoneText();
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().DisableDoneText();
                    ToEnemyTest();
                }));
            }
        }
        else if (tutorialState == TutorialState.Combat)
        {
            if (enemy == null && Time.time - startTime > 3)
            {
                tutorialState = TutorialState.Pause;
                SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().EnableDoneText();
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().DisableDoneText();
                    ToUpgrade();
                }));
            }
        }
        else if (tutorialState == TutorialState.Upgrading)
        {
            if (enemy == null && Time.time - startTime > 3)
            {
                tutorialState = TutorialState.Pause;
                SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().EnableDoneText();
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().DisableDoneText();
                    ToClassAbility();
                }));
            }
        }
        else if (tutorialState == TutorialState.ClassAbility)
        {
            if (player.GetComponent<AbilityBehaviour>().OnCooldown())
            {
                tutorialState = TutorialState.Pause;
                SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().EnableDoneText();
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().DisableDoneText();
                    ToEndless();
                }));
            }
            else if (enemy == null && Time.time - startTime > 3)
            {
                CreateRandomEnemyForTutorial();
                startTime = Time.time;
            }
        }
        else if (tutorialState == TutorialState.Endless)
        {
            if (enemy == null && Time.time - startTime > 3)
            {
                CreateRandomEnemyForTutorial();
                startTime = Time.time;
            }
        }
    }

    private void ToMovement()
    {
        tutorialState = TutorialState.Movement;

        var explanationTitle = "Movement (1/7)";
        var explanationSubTitle = "You can move your character (the blue tank) by using WASD (Keyboard)\n or the left stick (Gamepad).";
        SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().SetTitle(explanationTitle, explanationSubTitle);

        void OnMoveCallback()
        {
            tutorialState = TutorialState.Pause;
            SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().EnableDoneText();
            StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
            {
                SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().DisableDoneText();
                ToShooting();
            }));
            player.GetComponent<PlayerController>().RemoveOnMoveCallback(OnMoveCallback);
        }

        player.GetComponent<PlayerController>().AddOnMoveCallback(OnMoveCallback);
    }



    private void ToShooting()
    {
        tutorialState = TutorialState.Shoot;

        var explanationTitle = "Shooting (2/7)";
        var explanationSubTitle = "You can shoot by aiming with your mouse and shooting with left click (Keyboard)\n or aiming with the right stick and shooting with right trigger (Gamepad).";
        SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().SetTitle(explanationTitle, explanationSubTitle);
    }

    private void ToDash()
    {
        tutorialState = TutorialState.Dash;
        playerUIManager.Enable<AbilityUI>();

        var explanationTitle = "Dashing (3/7)";
        var explanationSubTitle = "Dashing will perform a quick movement in the direction you are moving.\n" +
            "While dashing, you deal high damage when bumping into enemies and you are invulnerable.\n" +
            "Dashing has a cooldown which can be seen at the bottom of the screen.\n" + 
            "You can dash with your character by moving and dashing with left shift (Keyboard)\n" +
            "or with left trigger (Gamepad).";
        SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().SetTitle(explanationTitle, explanationSubTitle);
    }

    private void ToReflect()
    {
        tutorialState = TutorialState.Reflect;

        var explanationTitle = "Reflect Shield (4/7)";
        var explanationSubTitle = "The reflect shield will reflect enemy bullets.\n" +
            "The reflect shield has a cooldown which can be seen at the bottom of the screen.\n" +
            "You can enable your reflect shield by pressing space (Keyboard)\n " +
            "or with left shoulder (Gamepad).";
        SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().SetTitle(explanationTitle, explanationSubTitle);
    }
    private void ToEnemyTest()
    {
        tutorialState = TutorialState.Combat;
        SharedUIManager.Instance.Enable<ScoreUI>();

        var explanationTitle = "Combat (5/7)";
        var explanationSubTitle = "Test your skills against a real enemy!\n" +
            "Killing an enemy will reward your with a score. This score can be seen on the top right of the screen.\n" +
            "When enemies hit your character (either melee or with bullets), your character will lose health.\n" +
            "If your health reaches zero, you lose!";
        SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().SetTitle(explanationTitle, explanationSubTitle);

        CreateRandomEnemyForTutorial();
        startTime = Time.time;
    }
    private void ToUpgrade()
    {
        tutorialState = TutorialState.Upgrading;

        var explanationTitle = "Upgrading (6/7)";
        var explanationSubTitle = "From time to time, you will be able to upgrade your character!\n" +
            "You can then choose between different classes, each with their own advantages and dissadvantages!";
        SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().SetTitle(explanationTitle, explanationSubTitle);

        playerUIManager.Enable<UpgradeUI>();

        CreateRandomEnemyForTutorial();
        startTime = Time.time;
    }

    private void ToClassAbility()
    {
        tutorialState = TutorialState.ClassAbility;

        var explanationTitle = "Class Abilities (7/7)";
        var explanationSubTitle = "Each class has a unique class ability. This ability has a cooldown which can be seen at the bottom of the screen.\n" +
            "You can use this class ability by pressing the right mouse button (Keyboard)\n" +
            "or by using the right shoulder (Gamepad).";
        SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().SetTitle(explanationTitle, explanationSubTitle);

        CreateRandomEnemyForTutorial();
        startTime = Time.time;
    }

    private void ToEndless()
    {
        tutorialState = TutorialState.Endless;

        var explanationTitle = "Endless";
        var explanationSubTitle = "You have completed the tutorial!\n" +
            "You can leave by pressing ESC (Keyboard) or Start (Gamepad) and clicking the main menu button.\n" +
            "You can also stay and practise some more against the infinite enemies. Have fun!";
        SharedUIManager.Instance.GetUIElement<TutorialExplanationUI>().SetTitle(explanationTitle, explanationSubTitle);
    }

    private IEnumerator PerformAfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);

        action();
    }

    private void CreateRandomEnemyForTutorial()
    {
        int index = UnityEngine.Random.Range(0, tutorialEnemies.Count);
        GameObject enemyPrefab = tutorialEnemies[index];

        CreateEnemy(enemyPrefab, new Vector3(3.5f, 0.5f, 0));
    }

    private void CreateEnemy(GameObject prefab, Vector3 spawnLocation)
    {
        warningTilemap.SetTile(Vector3Int.FloorToInt(spawnLocation), warningTile);
        StartCoroutine(CreateEnemyAfterDelay(prefab, spawnLocation, 2));
    }

    private IEnumerator CreateEnemyAfterDelay(GameObject prefab, Vector3 spawnLocation, int delay)
    {
        yield return new WaitForSeconds(delay);

        enemy = Instantiate(prefab, spawnLocation, Quaternion.identity);
        warningTilemap.SetTile(Vector3Int.FloorToInt(spawnLocation), null);
    }
}
