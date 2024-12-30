using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
    private GameObject enemy;
    private float startTime = 0;

    private GameObject explanationUI;
    private Text explanationTitle;
    private Text doneText;
    private Text explanationSubTitle;

    private TutorialUIManager tutorialUIManager;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        tutorialUIManager = GameObject.Find("TutorialUIManager").GetComponent<TutorialUIManager>();

        explanationUI = GameObject.Find("ExplanationUI");
        explanationTitle = explanationUI.transform.Find("TitleContainer").Find("Title").GetComponent<Text>();
        doneText = explanationUI.transform.Find("TitleContainer").Find("Done").GetComponent<Text>();
        doneText.text = "";
        explanationSubTitle = explanationUI.transform.Find("SubTitle").GetComponent<Text>();
        tutorialState = TutorialState.Explanation;

        StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
        {
            ToMovement();
        }));
    }

    private void Update()
    {
        if (player.health < player.maxHealth / 2) player.health = player.maxHealth;

        if (tutorialState == TutorialState.Movement)
        {
            if (player.GetComponent<Rigidbody2D>().velocity != Vector2.zero)
            {
                tutorialState = TutorialState.Pause;
                doneText.text = "Done!";
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    doneText.text = "";
                    ToShooting();
                }));
            }
        }
        else if (tutorialState == TutorialState.Shoot)
        {
            if (player.GetComponent<ShootingAbility>().shooting)
            {
                tutorialState = TutorialState.Pause;
                doneText.text = "Done!";
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    doneText.text = "";
                    ToDash();
                }));
            }
        }
        else if (tutorialState == TutorialState.Dash)
        {
            if (player.GetComponent<DashAbility>().Dashing())
            {
                tutorialState = TutorialState.Pause;
                doneText.text = "Done!";
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    doneText.text = "";
                    ToReflect();
                }));
            }
        }
        else if (tutorialState == TutorialState.Reflect)
        {
            if (player.GetComponent<ReflectShieldAbility>().IsReflecting())
            {
                tutorialState = TutorialState.Pause;
                doneText.text = "Done!";
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    doneText.text = "";
                    ToEnemyTest();
                }));
            }
        }
        else if (tutorialState == TutorialState.Combat)
        {
            if (enemy == null && Time.time - startTime > 3)
            {
                tutorialState = TutorialState.Pause;
                doneText.text = "Done!";
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    doneText.text = "";
                    ToUpgrade();
                }));
            }
        }
        else if (tutorialState == TutorialState.Upgrading)
        {
            if (enemy == null && Time.time - startTime > 3)
            {
                tutorialState = TutorialState.Pause;
                doneText.text = "Done!";
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    doneText.text = "";
                    ToClassAbility();
                }));
            }
        }
        else if (tutorialState == TutorialState.ClassAbility)
        {
            if (player.GetComponent<AbilityBehaviour>().OnCooldown())
            {
                tutorialState = TutorialState.Pause;
                doneText.text = "Done!";
                StartCoroutine(PerformAfterDelay(tutorialStepDelay, () =>
                {
                    doneText.text = "";
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

        explanationTitle.text = "Movement (1/7)";
        explanationSubTitle.text = "You can move your character (the blue tank) by using WASD (Keyboard)\n or the left stick (Gamepad).";
    }

    private void ToShooting()
    {
        tutorialState = TutorialState.Shoot;

        explanationTitle.text = "Shooting (2/7)";
        explanationSubTitle.text = "You can shoot by aiming with your mouse and shooting with left click (Keyboard)\n or aiming with the right stick and shooting with right trigger (Gamepad).";
    }

    private void ToDash()
    {
        tutorialState = TutorialState.Dash;
        tutorialUIManager.EnableAbilityUI();

        explanationTitle.text = "Dashing (3/7)";
        explanationSubTitle.text = "Dashing will perform a quick movement in the direction you are moving.\n" +
            "While dashing, you deal high damage when bumping into enemies and you are invulnerable.\n" +
            "Dashing has a cooldown which can be seen at the bottom of the screen.\n" + 
            "You can dash with your character by moving and dashing with left shift (Keyboard)\n" +
            "or with left trigger (Gamepad).";
    }

    private void ToReflect()
    {
        tutorialState = TutorialState.Reflect;

        explanationTitle.text = "Reflect Shield (4/7)";
        explanationSubTitle.text = "The reflect shield will reflect enemy bullets.\n" +
            "The reflect shield has a cooldown which can be seen at the bottom of the screen.\n" +
            "You can enable your reflect shield by pressing space (Keyboard)\n " +
            "or with left shoulder (Gamepad).";
    }
    private void ToEnemyTest()
    {
        tutorialState = TutorialState.Combat;
        tutorialUIManager.EnableScoreUI();

        explanationTitle.text = "Combat (5/7)";
        explanationSubTitle.text = "Test your skills against a real enemy!\n" +
            "Killing an enemy will reward your with a score. This score can be seen on the top right of the screen.\n" +
            "When enemies hit your character (either melee or with bullets), your character will lose health.\n" +
            "If your health reaches zero, you lose!";

        CreateRandomEnemyForTutorial();
        startTime = Time.time;
    }
    private void ToUpgrade()
    {
        tutorialState = TutorialState.Upgrading;

        explanationTitle.text = "Upgrading (6/7)";
        explanationSubTitle.text = "From time to time, you will be able to upgrade your character!\n" +
            "You can then choose between different classes, each with their own advantages and dissadvantages!";

        tutorialUIManager.EnableUpgradeUI();

        CreateRandomEnemyForTutorial();
        startTime = Time.time;
    }

    private void ToClassAbility()
    {
        tutorialState = TutorialState.ClassAbility;

        explanationTitle.text = "Class Abilities (7/7)";
        explanationSubTitle.text = "Each class has a unique class ability. This ability has a cooldown which can be seen at the bottom of the screen.\n" +
            "You can use this class ability by pressing the right mouse button (Keyboard)\n" +
            "or by using the right shoulder (Gamepad).";

        CreateRandomEnemyForTutorial();
        startTime = Time.time;
    }

    private void ToEndless()
    {
        tutorialState = TutorialState.Endless;

        explanationTitle.text = "Endless";
        explanationSubTitle.text = "You have completed the tutorial!\n" +
            "You can leave by pressing ESC (Keyboard) or Start (Gamepad) and clicking the main menu button.\n" +
            "You can also stay and practise some more against the infinite enemies. Have fun!";
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
