using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.GLAMOR.VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Screen Rain Lite")]
    public sealed class RainLiteVol : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter m_Weight = new ClampedFloatParameter(0, 0, 1);
        public IntParameter rainMode = new IntParameter(0);
        public MaterialParameter rainLiteMaterial = new MaterialParameter(null);

        // =======================================================================
        public bool IsActive() => active && m_Weight.value > 0;

        public bool IsTileCompatible() => false;
    }
}