#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using URP = UnityEngine.Rendering.Universal;

namespace Room1Puzzle
{
    public static class Room1LightingSetupEditor
    {
        const int UltraQualityIndex = 5;

        [MenuItem("Room1/Setup Realtime Reflections And Shadows (Ultra)")]
        public static void SetupReflectionsAndShadows()
        {
            QualitySettings.SetQualityLevel(UltraQualityIndex, applyExpensiveChanges: true);
            QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            QualitySettings.shadowDistance = 80f;
            QualitySettings.realtimeReflectionProbes = true;

            int steps = 0;
            steps += EnsureReflectionProbe();
            steps += EnableLightProbeGroup();
            steps += ConfigureDirectionalLight();
            steps += EnsureRuntimeLightingHost();
            steps += ConfigureRenderSettings();

            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            EditorUtility.DisplayDialog(
                "Room 1 Lighting",
                $"Ultra realtime lighting applied ({steps} steps).\n\n" +
                "Press Play — shadows and reflections update every frame.\n" +
                "Quality is set to Ultra.",
                "OK");
        }

        static int EnsureReflectionProbe()
        {
            ReflectionProbe existing = Object.FindFirstObjectByType<ReflectionProbe>();
            if (existing == null)
            {
                GameObject go = new GameObject("Reflection Probe");
                Undo.RegisterCreatedObjectUndo(go, "Create Reflection Probe");
                existing = go.AddComponent<ReflectionProbe>();
                go.transform.position = new Vector3(0f, 1.05f, 0f);
            }

            ConfigureReflectionProbe(existing);
            EditorUtility.SetDirty(existing);
            return 1;
        }

        static void ConfigureReflectionProbe(ReflectionProbe probe)
        {
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
            probe.resolution = 512;
            probe.size = new Vector3(12f, 6f, 12f);
            probe.intensity = 1f;
            probe.blendDistance = 1f;
            probe.boxProjection = true;
            probe.hdr = true;
            probe.renderDynamicObjects = true;
            probe.shadowDistance = 80f;
            probe.importance = 1;
        }

        static int EnableLightProbeGroup()
        {
            LightProbeGroup group = Object.FindFirstObjectByType<LightProbeGroup>(FindObjectsInactive.Include);
            if (group == null)
                return 0;

            if (!group.gameObject.activeSelf)
            {
                Undo.RecordObject(group.gameObject, "Enable Light Probe Group");
                group.gameObject.SetActive(true);
            }

            return 1;
        }

        static int ConfigureDirectionalLight()
        {
            Light light = FindDirectionalLight();
            if (light == null)
                return 0;

            Undo.RecordObject(light, "Configure Directional Light");
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 1f;

            URP.UniversalAdditionalLightData data = light.GetComponent<URP.UniversalAdditionalLightData>();
            if (data != null)
            {
                Undo.RecordObject(data, "Configure Directional Light URP");
                data.softShadowQuality = URP.SoftShadowQuality.High;
            }

            return 1;
        }

        static int EnsureRuntimeLightingHost()
        {
            if (Object.FindFirstObjectByType<Room1RealtimeLighting>() != null)
                return 0;

            GameObject go = new GameObject("Room1RealtimeLighting");
            Undo.RegisterCreatedObjectUndo(go, "Create Room1RealtimeLighting");
            go.AddComponent<Room1RealtimeLighting>();
            return 1;
        }

        static int ConfigureRenderSettings()
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.defaultReflectionResolution = 512;
            RenderSettings.reflectionIntensity = 1f;

            Light sun = FindDirectionalLight();
            if (sun != null)
                RenderSettings.sun = sun;

            return 1;
        }

        static Light FindDirectionalLight()
        {
            foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light != null && light.type == LightType.Directional)
                    return light;
            }

            return null;
        }
    }
}
#endif
