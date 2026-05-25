using UnityEngine;
using UnityEngine.Rendering;
using URP = UnityEngine.Rendering.Universal;

namespace Room1Puzzle
{
    /// <summary>
    /// Forces Ultra-quality realtime shadows and reflection probes in Room 1.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class Room1RealtimeLighting : MonoBehaviour
    {
        const int UltraQualityIndex = 5;

        ReflectionProbe[] probes;

        void Awake()
        {
            ApplyUltraQuality();
            ConfigureLights();
            ConfigureReflectionProbes();
            EnableLightProbeGroups();
        }

        void ApplyUltraQuality()
        {
            if (QualitySettings.GetQualityLevel() != UltraQualityIndex)
                QualitySettings.SetQualityLevel(UltraQualityIndex, applyExpensiveChanges: true);

            QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            QualitySettings.shadowDistance = 80f;
            QualitySettings.shadowCascades = 4;
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.VeryHigh;
            QualitySettings.realtimeReflectionProbes = true;
        }

        void ConfigureLights()
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light == null || !light.enabled)
                    continue;

                if (light.type == LightType.Directional)
                {
                    light.shadows = LightShadows.Soft;
                    light.shadowStrength = 1f;
                    light.shadowBias = 0.05f;
                    light.shadowNormalBias = 0.4f;
                    RenderSettings.sun = light;

                    URP.UniversalAdditionalLightData data = light.GetComponent<URP.UniversalAdditionalLightData>();
                    if (data != null)
                        data.softShadowQuality = URP.SoftShadowQuality.High;
                }
                else
                {
                    light.shadows = LightShadows.None;
                }
            }
        }

        void ConfigureReflectionProbes()
        {
            probes = FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);
            foreach (ReflectionProbe probe in probes)
            {
                if (probe == null)
                    continue;

                probe.mode = ReflectionProbeMode.Realtime;
                probe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
                probe.resolution = 512;
                probe.intensity = 1f;
                probe.boxProjection = true;
                probe.hdr = true;
                probe.renderDynamicObjects = true;
                probe.shadowDistance = 80f;
                probe.RenderProbe();
            }

            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.defaultReflectionResolution = 512;
            RenderSettings.reflectionIntensity = 1f;
        }

        static void EnableLightProbeGroups()
        {
            LightProbeGroup[] groups = FindObjectsByType<LightProbeGroup>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (LightProbeGroup group in groups)
            {
                if (group != null && !group.gameObject.activeSelf)
                    group.gameObject.SetActive(true);
            }
        }
    }
}
