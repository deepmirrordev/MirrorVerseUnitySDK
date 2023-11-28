using MirrorVerse.Options;
using MirrorVerse.UI.RenderFeatures;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MirrorVerse.UI.Renderers
{
    public class ScanLineRenderer : MonoBehaviour
    {
        private enum ScanEffectStatus
        {
            Stoped,
            Playing
        }

        public ScanLineRendererOptions options;
        public GameObject placeholderPlaneObj;

        private Transform _originTransform;
        private ScanEffectStatus _scanEffectStatus = ScanEffectStatus.Stoped;
        private ScanLineRenderFeature _scanLineRenderFeature;

        // Only work with material with ScanLine shader.
        private Material _sharedMaterial;

        private void Awake()
        {
            InitRenderFeature();

            // Turn off the effect initially.
            ToggleEffect(false);
        }

        private void InitRenderFeature()
        {
            var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(0);
            var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
            List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
            foreach (var feature in features)
            {
                if (feature.GetType().Name == "ScanLineRenderFeature")
                {
                    _scanLineRenderFeature = (ScanLineRenderFeature)feature;
                    _sharedMaterial = _scanLineRenderFeature.GetSharedMaterial();
                }
            }
        }

        private void Update()
        {
            if (_originTransform != null && _sharedMaterial != null)
            {
                SetScanOrigin(_originTransform.position);
            }
        }

        private void SetScanOrigin(Vector3 point)
        {
            transform.position = point;
            _sharedMaterial.SetVector("_Origin", transform.position);
        }

        private IEnumerator LoopFrameTask(int frameInterval = 1)
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                if (_scanEffectStatus == ScanEffectStatus.Stoped || !_scanLineRenderFeature.isActive)
                {
                    // Quit if inactive.
                    break;
                }

                if (Time.frameCount % frameInterval == 0)
                {
                    float scanSpeed = _sharedMaterial.GetFloat("_Speed");
                    float scanTimeFactor = _sharedMaterial.GetFloat("_StartTime");
                    float shaderTimeY = Time.time;
                    bool animationFinished = ((shaderTimeY - scanTimeFactor) * scanSpeed) > 1;
                    if (_scanEffectStatus == ScanEffectStatus.Playing && animationFinished)
                    {
                        if (options.loop)
                        {
                            // Continue to next cycle.
                            _sharedMaterial.SetFloat("_StartTime", Time.time);
                        }
                        else
                        {
                            // If not loop, or inactive, Quit.
                            _scanEffectStatus = ScanEffectStatus.Stoped;
                            break;
                        }
                    }
                    else
                    {
                        // Animation continues.
                    }
                }
            }
        }

        public void StartEffect(Transform originTransform = null)
        {
            if (!options.enabled || _sharedMaterial == null || _scanLineRenderFeature == null)
            {
                // Not enabled, return directly.
                return;
            }
            if (originTransform != null)
            {
                _originTransform = originTransform;
            }
            if (_originTransform == null)
            {
                Debug.Log("Origin transform is never set. Cannot start scan line effect.");
                return;
            }
            SetScanOrigin(_originTransform.position);
            _sharedMaterial.SetFloat("_StartTime", Time.time);
            ToggleEffect(true);
            _scanEffectStatus = ScanEffectStatus.Playing;

            StartCoroutine(LoopFrameTask());
        }

        public void StopEffect()
        {
            _scanEffectStatus = ScanEffectStatus.Stoped;
            ToggleEffect(false);
        }

        private void ToggleEffect(bool enabled)
        {
            if (_scanLineRenderFeature != null)
            {
                _scanLineRenderFeature.SetActive(options.enabled && enabled);
            }
            if (placeholderPlaneObj != null)
            {
                placeholderPlaneObj.SetActive(options.enabled && enabled);
            }
        }

        private void OnDestroy()
        {
            ToggleEffect(false);
        }
    }
}
