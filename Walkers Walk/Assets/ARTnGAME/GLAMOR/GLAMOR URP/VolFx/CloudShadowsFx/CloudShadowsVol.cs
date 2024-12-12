using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.GLAMOR.VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Cloud Shadows")]
    public sealed class CloudShadowsVol : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter m_Weight = new ClampedFloatParameter(0, 0, 1);
        public NoInterpIntParameter rainMode = new NoInterpIntParameter(0);
        public NoInterpVector3Parameter sunTransform = new NoInterpVector3Parameter(new Vector3(0f, 0f, 0f));
        public Texture2DParameter cloudTexture = new Texture2DParameter(null);
        public NoInterpVector2Parameter noiseCloudSpeed = new NoInterpVector2Parameter(new Vector3(0f, 0f, 0f));

        public NoInterpColorParameter cloudShadowColor = new NoInterpColorParameter(Color.black);
        public NoInterpVector3Parameter cloudShadowScale = new NoInterpVector3Parameter(new Vector3(1f, 1f, 1f));

        //public ObjectParameter<Transform> sunTransform = new ObjectParameter<Transform>(null);

        // =======================================================================
        public bool IsActive() => active && m_Weight.value > 0;

        public bool IsTileCompatible() => false;
    }
}