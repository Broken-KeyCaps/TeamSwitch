using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeaponStats))]
public class WeaponStatsEditor : Editor
{
    private WeaponStats _stats;

    private void OnEnable()
    {
        _stats = (WeaponStats)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUIStyle header = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            normal = { textColor = new Color(0.8f, 0.9f, 1f) }
        };
        EditorGUILayout.LabelField("Weapon Presets", header);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Pistol", GUILayout.Height(30))) ApplyPreset_Pistol();
        if (GUILayout.Button("Machine Gun", GUILayout.Height(30))) ApplyPreset_MachineGun();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Shotgun", GUILayout.Height(30))) ApplyPreset_Shotgun();
        if (GUILayout.Button("Burst (3-shot)", GUILayout.Height(30))) ApplyPreset_Burst3();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rocket Launcher", GUILayout.Height(30))) ApplyPreset_RocketLauncher();
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    // ======= PRESETS =======

    private void ApplyPreset_Pistol()
    {
        Undo.RecordObject(_stats, "Apply Pistol Preset");

        _stats.Title = "Pistol";
        _stats.Description = "Standard semi-automatic sidearm.";
        _stats.Damage = 20f;
        _stats.FireRate = 3f;
        _stats.Distance = 75f;
        _stats.IsAutomatic = false;
        _stats.PelletsPerShot = 1;
        _stats.ShotsPerTrigger = 1;
        _stats.PierceCount = 1;
        _stats.MagazineSize = 12;
        _stats.ReloadTime = 1.5f;
        _stats.IsProjectile = false;

        EditorUtility.SetDirty(_stats);
    }

    private void ApplyPreset_MachineGun()
    {
        Undo.RecordObject(_stats, "Apply Machine Gun Preset");

        _stats.Title = "Machine Gun";
        _stats.Description = "Fully automatic rifle with high rate of fire.";
        _stats.Damage = 10f;
        _stats.FireRate = 12f;
        _stats.Distance = 100f;
        _stats.IsAutomatic = true;
        _stats.PelletsPerShot = 1;
        _stats.ShotsPerTrigger = 1;
        _stats.PierceCount = 3;
        _stats.MagazineSize = 40;
        _stats.ReloadTime = 2.5f;
        _stats.IsProjectile = false;

        EditorUtility.SetDirty(_stats);
    }

    private void ApplyPreset_Shotgun()
    {
        Undo.RecordObject(_stats, "Apply Shotgun Preset");

        _stats.Title = "Shotgun";
        _stats.Description = "Close-range weapon firing multiple pellets.";
        _stats.Damage = 8f;
        _stats.FireRate = 1.2f;
        _stats.Distance = 35f;
        _stats.IsAutomatic = false;
        _stats.PelletsPerShot = 8;
        _stats.ShotsPerTrigger = 1;
        _stats.PierceCount = 3;
        _stats.MagazineSize = 6;
        _stats.ReloadTime = 3f;
        _stats.IsProjectile = false;

        EditorUtility.SetDirty(_stats);
    }

    private void ApplyPreset_Burst3()
    {
        Undo.RecordObject(_stats, "Apply 3-Shot Burst Preset");

        _stats.Title = "Burst Rifle";
        _stats.Description = "Fires a burst of three rounds per trigger pull.";
        _stats.Damage = 12f;
        _stats.FireRate = 6f;
        _stats.Distance = 90f;
        _stats.IsAutomatic = false;
        _stats.PelletsPerShot = 1;
        _stats.ShotsPerTrigger = 3;
        _stats.PierceCount = 3;
        _stats.MagazineSize = 30;
        _stats.ReloadTime = 2.2f;
        _stats.IsProjectile = false;

        EditorUtility.SetDirty(_stats);
    }

    private void ApplyPreset_RocketLauncher()
    {
        Undo.RecordObject(_stats, "Apply Rocket Launcher Preset");

        _stats.Title = "Rocket Launcher";
        _stats.Description = "Launches explosive projectiles dealing high area damage.";
        _stats.Damage = 100f;
        _stats.FireRate = 0.8f;
        _stats.Distance = 120f;
        _stats.IsAutomatic = false;
        _stats.PelletsPerShot = 1;
        _stats.ShotsPerTrigger = 1;
        _stats.PierceCount = 3;
        _stats.MagazineSize = 1;
        _stats.ReloadTime = 3.5f;
        _stats.IsProjectile = true;
        _stats.ProjectileSpeed = 40f;
        _stats.ProjectileLifetime = 5f;

        EditorUtility.SetDirty(_stats);
    }
}