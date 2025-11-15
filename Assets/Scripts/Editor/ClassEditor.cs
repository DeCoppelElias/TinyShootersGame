using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

[CustomEditor(typeof(PlayerStats))]
public class ClassEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get a reference to the Class scriptable object
        PlayerStats playerClass = (PlayerStats)target;

        serializedObject.Update();

        // Draw default fields for general settings
        EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
        playerClass.className = EditorGUILayout.TextField("Class Name", playerClass.className);
        playerClass.animatorController = (AnimatorController)EditorGUILayout.ObjectField("Animation Controller", playerClass.animatorController, typeof(AnimatorController), false);
        playerClass.UISprite = (Sprite)EditorGUILayout.ObjectField("UI sprite", playerClass.UISprite, typeof(Sprite), false);
        playerClass.maxHealth = EditorGUILayout.FloatField("Max Health", playerClass.maxHealth);
        playerClass.health = EditorGUILayout.FloatField("Health", playerClass.health);
        playerClass.damage = EditorGUILayout.FloatField("Damage", playerClass.damage);
        playerClass.normalMoveSpeed = EditorGUILayout.FloatField("Normal Move Speed", playerClass.normalMoveSpeed);
        playerClass.shootingMoveSpeed = EditorGUILayout.FloatField("Shooting Move Speed", playerClass.shootingMoveSpeed);
        playerClass.invulnerableDuration = EditorGUILayout.FloatField("Invulnerable Duration", playerClass.invulnerableDuration);
        playerClass.contactDamage = EditorGUILayout.FloatField("Contact Damage", playerClass.contactDamage);
        playerClass.contactHitCooldown = EditorGUILayout.FloatField("Contact Hit Cooldown", playerClass.contactHitCooldown);

        playerClass.classAbility = (ClassAbility)EditorGUILayout.ObjectField("Class Ability", playerClass.classAbility, typeof(ClassAbility), false);

        // Draw the Shootability section if the player class has the ability
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shootability Variables", EditorStyles.boldLabel);
        playerClass.hasShootAbility = EditorGUILayout.Toggle("Has Shoot Ability", playerClass.hasShootAbility);

        if (playerClass.hasShootAbility)
        {
            playerClass.attackCooldown = EditorGUILayout.FloatField("Attack Cooldown", playerClass.attackCooldown);
            playerClass.range = EditorGUILayout.FloatField("Range", playerClass.range);
            playerClass.pierce = EditorGUILayout.IntField("Pierce", playerClass.pierce);
            playerClass.totalSplit = EditorGUILayout.FloatField("Total Split", playerClass.totalSplit);
            playerClass.totalFan = EditorGUILayout.FloatField("Total Fan", playerClass.totalFan);
            playerClass.bulletSize = EditorGUILayout.FloatField("Bullet Size", playerClass.bulletSize);
            playerClass.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", playerClass.bulletSpeed);
            playerClass.splitOnHit = EditorGUILayout.Toggle("Split On Hit", playerClass.splitOnHit);
            playerClass.splitAmount = EditorGUILayout.IntField("Split Amount", playerClass.splitAmount);
            playerClass.splitRange = EditorGUILayout.FloatField("Split Range", playerClass.splitRange);
            playerClass.splitBulletSize = EditorGUILayout.FloatField("Split Bullet Size", playerClass.splitBulletSize);
            playerClass.splitBulletSpeed = EditorGUILayout.FloatField("Split Bullet Speed", playerClass.splitBulletSpeed);
            playerClass.splitDamagePercentage = EditorGUILayout.FloatField("Split Damage Percentage", playerClass.splitDamagePercentage);
        }

        // Draw the Dashability section if the player class has the ability
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dashability Variables", EditorStyles.boldLabel);
        playerClass.hasDashAbility = EditorGUILayout.Toggle("Has Dash Ability", playerClass.hasDashAbility);

        if (playerClass.hasDashAbility)
        {
            playerClass.dashCooldown = EditorGUILayout.IntField("Dash Cooldown", playerClass.dashCooldown);
            playerClass.dashDuration = EditorGUILayout.FloatField("Dash Duration", playerClass.dashDuration);
            playerClass.chargeDuration = EditorGUILayout.FloatField("Charge Duration", playerClass.chargeDuration);
            playerClass.dashSpeed = EditorGUILayout.FloatField("Dash Speed", playerClass.dashSpeed);
        }

        // Upgrades Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Upgrades", EditorStyles.boldLabel);
        SerializedProperty upgradesProp = serializedObject.FindProperty("upgrades");
        EditorGUILayout.PropertyField(upgradesProp, true);

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}
