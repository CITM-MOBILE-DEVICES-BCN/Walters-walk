using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace Artngame.GLAMOR.VolFx
{

    //public enum EyeAdaptation
    //{
    //    /// <summary>
    //    /// Progressive (smooth) eye adaptation.
    //    /// </summary>
    //    Progressive,

    //    /// <summary>
    //    /// Fixed (instant) eye adaptation.
    //    /// </summary>
    //    Fixed
    //}

    [Serializable, VolumeComponentMenu("VolFx/EyeAdaptGLAMOR")]
    public sealed class EyeAdaptVol : VolumeComponent, IPostProcessComponent
    {

       
       // [Serializable]
        //public sealed class EyeAdaptationParameter : ParameterOverride<EyeAdaptation> { }

        //AUTO
        //[MinMax(1f, 99f), DisplayName("Filtering (%)"), Tooltip("Filters the bright and dark parts of the histogram when computing the average luminance. This is to avoid very dark pixels and very bright pixels from contributing to the auto exposure. Unit is in percent.")]
        public Vector2Parameter filtering = new Vector2Parameter ( new Vector2(50f, 95f) );

        /// <summary>
        /// Minimum average luminance to consider for auto exposure (in EV).
        /// </summary>
       // [Range(LogHistogram.rangeMin, LogHistogram.rangeMax), DisplayName("Minimum (EV)"), Tooltip("Minimum average luminance to consider for auto exposure. Unit is EV.")]
        public FloatParameter minLuminance = new FloatParameter (0f );

        /// <summary>
        /// Maximum average luminance to consider for auto exposure (in EV).
        /// </summary>
        //[Range(LogHistogram.rangeMin, LogHistogram.rangeMax), DisplayName("Maximum (EV)"), Tooltip("Maximum average luminance to consider for auto exposure. Unit is EV.")]
        public FloatParameter maxLuminance = new FloatParameter(0f);

        /// <summary>
        /// Middle-grey value. Use this to compensate the global exposure of the scene.
        /// </summary>
       // [Min(0f), DisplayName("Exposure Compensation"), Tooltip("Use this to scale the global exposure of the scene.")]
        public FloatParameter keyValue = new FloatParameter ( 1f );

        /// <summary>
        /// The type of eye adaptation to use.
        /// </summary>
       // [DisplayName("Type"), Tooltip("Use \"Progressive\" if you want auto exposure to be animated. Use \"Fixed\" otherwise.")]
        public IntParameter adaptationType = new IntParameter (1);

        /// <summary>
        /// The adaptation speed from a dark to a light environment.
        /// </summary>
       // [Min(0f), Tooltip("Adaptation speed from a dark to a light environment.")]
        public FloatParameter speedUp = new FloatParameter ( 2f );

        /// <summary>
        /// The adaptation speed from a light to a dark environment.
        /// </summary>
       // [Min(0f), Tooltip("Adaptation speed from a light to a dark environment.")]
        public FloatParameter speedDown = new FloatParameter (  1f );


        public FloatParameter lowPercent = new FloatParameter ( 0f );
        public FloatParameter highPercent = new FloatParameter (  0f);

        public BoolParameter dynamicKeyValue = new BoolParameter (  true);

        public static GradientValue WhiteClean
        {
            get
            {
                var grad = new UnityEngine.Gradient();
                grad.SetKeys(new []{new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0f, 0f)});
                
                return new GradientValue(grad);
            }
        }
        
        public ClampedFloatParameter m_Intencity = new ClampedFloatParameter(0, 0, 21);
        [HideInInspector]
        public ClampedFloatParameter m_Scatter   = new ClampedFloatParameter(0, 0, 1);
        public CurveParameter        m_Threshold = new CurveParameter(new CurveValue(new AnimationCurve(
                                                                                         new Keyframe(.57f, 0f, 8f, 8f, 0f, 0.1732f),
                                                                                         new Keyframe(1f, 1f, .3f, .3f, .32f, 0.0f))), false);
        [Tooltip("Color replacement for initial EyeAdapt color by threshold evaluation")]
        public GradientParameter     m_Color     = new GradientParameter(WhiteClean, false);
        [HideInInspector]
        public ClampedFloatParameter m_Flicker = new ClampedFloatParameter(1, 0, 1);


        // =======================================================================
        public bool IsActive() => active && m_Intencity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}