using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralToolkitTCT
{
    /// <summary>
    /// Serializable Renderer properties
    /// </summary>
    [Serializable]
    public class RendererPropertiesTCT
    {
        public LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes;
        public GameObject lightProbeProxyVolumeOverride = null;
        public ReflectionProbeUsage reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
        public Transform probeAnchor = null;
        public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
        public bool receiveShadows = true;
        public MotionVectorGenerationMode motionVectorGenerationMode = MotionVectorGenerationMode.Object;
    }
}