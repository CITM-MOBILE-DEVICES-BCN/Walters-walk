using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Artngame.GLAMOR.VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/SketchVolFx")]
    public sealed class SketchVolFxVol : VolumeComponent, IPostProcessComponent
    {        
        public ClampedFloatParameter m_Intencity = new ClampedFloatParameter(0, 0, 1);

        public TextureParameter noiseTexture = new TextureParameter(null);

        public NoInterpColorParameter TintColor = new NoInterpColorParameter(new Color(1, 1, 1, 1));
        public NoInterpFloatParameter contrast = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter luminance = new NoInterpFloatParameter(1f);

        public NoInterpVector4Parameter scaling = new NoInterpVector4Parameter(new Vector4(1,1,1,1));

        public bool IsActive() => active && m_Intencity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}