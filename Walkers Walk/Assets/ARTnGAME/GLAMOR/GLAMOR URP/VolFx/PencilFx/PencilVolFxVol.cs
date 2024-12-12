using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Artngame.GLAMOR.VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/PencilVolFx")]
    public sealed class PencilVolFxVol : VolumeComponent, IPostProcessComponent
    {

		[Tooltip("Gradient Threshold")]
		public NoInterpClampedFloatParameter gradThreshold = new NoInterpClampedFloatParameter(0.01f, 0.00001f, 0.01f);
		[Tooltip("Color Threshold")]
		public NoInterpClampedFloatParameter colorThreshold = new NoInterpClampedFloatParameter(0.8f, 0f, 1f);
		public NoInterpClampedFloatParameter blendThreshold = new NoInterpClampedFloatParameter(0.8f, -0.3f, 1f);
		public NoInterpClampedFloatParameter blendScreenThreshold = new NoInterpClampedFloatParameter(0.8f, -0.3f, 1f);
		public NoInterpClampedFloatParameter sensivity = new NoInterpClampedFloatParameter(10f, 0f, 100f);


		//[Range(0, 10)]
		//public NoInterpFloatParameter intensity = new NoInterpFloatParameter(1f);

		//public NoInterpColorParameter bloomTint = new NoInterpColorParameter(Color.white);

		//[Range(1, 25)]
		//public NoInterpIntParameter blurIterations = new NoInterpIntParameter(1);

		//[Range(0, 1)]
		//public NoInterpFloatParameter blendFac = new NoInterpFloatParameter(0.5f);

		//[Range(0, 0.999f)]
		//public NoInterpFloatParameter ghostingAmount = new NoInterpFloatParameter(0.95f);
		//[Tooltip("Just play with this lol.")]

		//[Range(0, 100f)]
		//public NoInterpFloatParameter distanceMultiplier = new NoInterpFloatParameter(1f);

		////[Tooltip("Higher value means lower resolution buffer and therefore better performance. If not using ghosting it causes flickering.")]

		//[Range(1, 16)]
		//public NoInterpIntParameter downSampleFactor = new NoInterpIntParameter(16);

		//[SerializeField]
		//Shader _shader;
		//Material _material;

		public ClampedFloatParameter m_Intencity = new ClampedFloatParameter(0, 0, 1);
       
        public bool IsActive() => active && m_Intencity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}