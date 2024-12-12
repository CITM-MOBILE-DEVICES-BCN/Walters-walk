using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Artngame.GLAMOR.VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/LensEffectsVolFx")]
    public sealed class LensEffectsVolFxVol : VolumeComponent, IPostProcessComponent
    {

        public static GradientValue WhiteClean
        {
            get
            {
                var grad = new UnityEngine.Gradient();
                grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }, new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0f, 0f) });

                return new GradientValue(grad);
            }
        }

        //public NoInterpClampedFloatParameter m_Intencity = new NoInterpClampedFloatParameter(0, 0, 21);
       // [HideInInspector]
        public NoInterpClampedFloatParameter m_Scatter = new NoInterpClampedFloatParameter(0, 0, 1); 
        public NoInterpFloatParameter m_dirtBloomPower = new NoInterpFloatParameter(1);
        //[HideInInspector]
        public CurveParameter m_Threshold = new CurveParameter(new CurveValue(new AnimationCurve(
                                                                                         new Keyframe(.57f, 0f, 8f, 8f, 0f, 0.1732f),
                                                                                         new Keyframe(1f, 1f, .3f, .3f, .32f, 0.0f))), false);
        [Tooltip("Color replacement for initial bloom color by threshold evaluation")]
        //[HideInInspector]
        public GradientParameter m_Color = new GradientParameter(WhiteClean, false);
        //[HideInInspector]
        public NoInterpClampedFloatParameter m_Flicker = new NoInterpClampedFloatParameter(1, 0, 1);


        //PENCIL
        //[Tooltip("Gradient Threshold")]
        //public NoInterpClampedFloatParameter gradThreshold = new NoInterpClampedFloatParameter(0.01f, 0.00001f, 0.01f);
        //[Tooltip("Color Threshold")]
        //public NoInterpClampedFloatParameter colorThreshold = new NoInterpClampedFloatParameter(0.8f, 0f, 1f);
        //public NoInterpClampedFloatParameter blendThreshold = new NoInterpClampedFloatParameter(0.8f, -0.3f, 1f);
        //public NoInterpClampedFloatParameter blendScreenThreshold = new NoInterpClampedFloatParameter(0.8f, -0.3f, 1f);
        //public NoInterpClampedFloatParameter sensivity = new NoInterpClampedFloatParameter(10f, 0f, 100f);

        public BoolParameter addBloom = new BoolParameter(true);
        public NoInterpFloatParameter bloomIntensity = new NoInterpFloatParameter(1);

        //STREAKS
        public BoolParameter horizontalStreaks = new BoolParameter(true);
        [Range(0, 5)] 
        public NoInterpFloatParameter threshold = new NoInterpFloatParameter(0.75f);
        [Range(0, 1)] 
        public NoInterpFloatParameter stretch = new NoInterpFloatParameter(0.5f);
        [Range(0, 1)] 
        public NoInterpFloatParameter intensity = new NoInterpFloatParameter(0.25f);
        [ColorUsage(false)] 
        public NoInterpColorParameter tint = new NoInterpColorParameter(new Color(0.55f, 0.55f, 1f));

        public BoolParameter verticalStreaks = new BoolParameter(true);
        [Range(0, 5)]
        public NoInterpFloatParameter thresholdV = new NoInterpFloatParameter(0.75f);
        [Range(0, 1)]
        public NoInterpFloatParameter stretchV = new NoInterpFloatParameter(0.5f);
        [Range(0, 1)]
        public NoInterpFloatParameter intensityV = new NoInterpFloatParameter(0.25f);
        [ColorUsage(false)]
        public NoInterpColorParameter tintV = new NoInterpColorParameter(new Color(0.55f, 0.55f, 1f));

        public Texture2DParameter lensDirtTexture = new Texture2DParameter(null);

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

        public ClampedFloatParameter m_Intencity = new ClampedFloatParameter(0, 0, 4);
       
        public bool IsActive() => active && m_Intencity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}