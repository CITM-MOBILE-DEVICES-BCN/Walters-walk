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

    [Serializable, VolumeComponentMenu("VolFx/VolumetricFog")]
    public sealed class VolumeFogFxVol : VolumeComponent, IPostProcessComponent
    {

        public MaterialParameter fogMaterial = new MaterialParameter(null);

        //VOLUME FOG
        [Range(0f, 1f), Tooltip("SunShafts effect intensity.")]
        public FloatParameter blend = new FloatParameter(1.0f);
        public NoInterpColorParameter _FogColor = new NoInterpColorParameter(Color.white / 2);

        public BoolParameter _useRadialDistance = new BoolParameter(false);
        public BoolParameter _fadeToSkybox = new BoolParameter(true);

        //fog params
        public TextureParameter noiseTexture = new TextureParameter(null);
        public NoInterpFloatParameter _startDistance = new NoInterpFloatParameter(30f);
        public NoInterpFloatParameter _fogHeight = new NoInterpFloatParameter(0.75f);
        public NoInterpFloatParameter _fogDensity = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter _cameraRoll = new NoInterpFloatParameter(0.0f);
        public NoInterpVector4Parameter _cameraDiff = new NoInterpVector4Parameter(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        public NoInterpFloatParameter _cameraTiltSign = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter heightDensity = new NoInterpFloatParameter(1f);

        public NoInterpFloatParameter noiseDensity = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter noiseScale = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter noiseThickness = new NoInterpFloatParameter(1f);
        public NoInterpVector3Parameter noiseSpeed = new NoInterpVector3Parameter(new Vector4(1f, 1f, 1f));
        public NoInterpFloatParameter occlusionDrop = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter occlusionExp = new NoInterpFloatParameter(1f);
        public NoInterpIntParameter noise3D = new NoInterpIntParameter(0);

        public NoInterpFloatParameter startDistance = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter luminance = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter lumFac = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter ScatterFac = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter TurbFac = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter HorizFac = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter turbidity = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter reileigh = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter mieCoefficient = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter mieDirectionalG = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter bias = new NoInterpFloatParameter(1f);
        public NoInterpFloatParameter contrast = new NoInterpFloatParameter(1f);
        public NoInterpColorParameter TintColor = new NoInterpColorParameter(new Color(1, 1, 1, 1));
        public NoInterpVector4Parameter Sun = new NoInterpVector4Parameter(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        public BoolParameter FogSky = new BoolParameter(false);
        public NoInterpFloatParameter ClearSkyFac = new NoInterpFloatParameter(1f);
        public NoInterpVector4Parameter PointL = new NoInterpVector4Parameter(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        public NoInterpVector4Parameter PointLParams = new NoInterpVector4Parameter(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));

        /*
        ////SUN SHAFTS
        public NoInterpIntParameter filterMode = new NoInterpIntParameter(0);
        //public FloatParameter blend = new FloatParameter(1.0f);
        public NoInterpFloatParameter resolutionDivider = new NoInterpFloatParameter(1.0f);
        public NoInterpIntParameter blendChoice = new NoInterpIntParameter(0);
        public NoInterpVector3Parameter sunTransform = new NoInterpVector3Parameter(new Vector3(0f, 0f, 0f)); // Transform sunTransform;
        public NoInterpIntParameter radialBlurIterations = new NoInterpIntParameter(2);
        public NoInterpColorParameter sunColor = new NoInterpColorParameter(Color.white);
        public NoInterpColorParameter sunThreshold = new NoInterpColorParameter(new Color(0.87f, 0.74f, 0.65f));
        public NoInterpFloatParameter sunShaftBlurRadius = new NoInterpFloatParameter(2.5f);
        public NoInterpFloatParameter sunShaftIntensity = new NoInterpFloatParameter(1.15f);
        public NoInterpFloatParameter maxRadius = new NoInterpFloatParameter(0.75f);
        public BoolParameter useDepthTexture = new BoolParameter(true);

        //FBM
        public BoolParameter useNoise = new BoolParameter(false);
        public Texture2DParameter MainTexFBM = new Texture2DParameter(null);
        public Texture2DParameter NoiseTex2 = new Texture2DParameter(null);
        public NoInterpFloatParameter Distort = new NoInterpFloatParameter(0.5f);
        public NoInterpColorParameter HighLight = new NoInterpColorParameter(Color.white);
        public NoInterpColorParameter NoiseColor = new NoInterpColorParameter(Color.white);
        public NoInterpFloatParameter noisePower = new NoInterpFloatParameter(0.5f);
        public NoInterpVector4Parameter brightnessContrast = new NoInterpVector4Parameter(new Vector4(1f, 1f, 1, 1f));
        public NoInterpVector4Parameter noiseCloudSpeed = new NoInterpVector4Parameter(new Vector4(1f, 1f, 1f, 1f));

       



        // [Serializable]
        //public sealed class EyeAdaptationParameter : ParameterOverride<EyeAdaptation> { }

        //AUTO
        //[MinMax(1f, 99f), DisplayName("Filtering (%)"), Tooltip("Filters the bright and dark parts of the histogram when computing the average luminance. This is to avoid very dark pixels and very bright pixels from contributing to the auto exposure. Unit is in percent.")]
       // public Vector2Parameter filtering = new Vector2Parameter ( new Vector2(50f, 95f) );

       // /// <summary>
       // /// Minimum average luminance to consider for auto exposure (in EV).
       // /// </summary>
       //// [Range(LogHistogram.rangeMin, LogHistogram.rangeMax), DisplayName("Minimum (EV)"), Tooltip("Minimum average luminance to consider for auto exposure. Unit is EV.")]
       // public FloatParameter minLuminance = new FloatParameter (0f );

       // /// <summary>
       // /// Maximum average luminance to consider for auto exposure (in EV).
       // /// </summary>
       // //[Range(LogHistogram.rangeMin, LogHistogram.rangeMax), DisplayName("Maximum (EV)"), Tooltip("Maximum average luminance to consider for auto exposure. Unit is EV.")]
       // public FloatParameter maxLuminance = new FloatParameter(0f);

       // /// <summary>
       // /// Middle-grey value. Use this to compensate the global exposure of the scene.
       // /// </summary>
       //// [Min(0f), DisplayName("Exposure Compensation"), Tooltip("Use this to scale the global exposure of the scene.")]
       // public FloatParameter keyValue = new FloatParameter ( 1f );

       // /// <summary>
       // /// The type of eye adaptation to use.
       // /// </summary>
       //// [DisplayName("Type"), Tooltip("Use \"Progressive\" if you want auto exposure to be animated. Use \"Fixed\" otherwise.")]
       // public IntParameter adaptationType = new IntParameter (1);

       // /// <summary>
       // /// The adaptation speed from a dark to a light environment.
       // /// </summary>
       //// [Min(0f), Tooltip("Adaptation speed from a dark to a light environment.")]
       // public FloatParameter speedUp = new FloatParameter ( 2f );

       // /// <summary>
       // /// The adaptation speed from a light to a dark environment.
       // /// </summary>
       //// [Min(0f), Tooltip("Adaptation speed from a light to a dark environment.")]
       // public FloatParameter speedDown = new FloatParameter (  1f );


       // public FloatParameter lowPercent = new FloatParameter ( 0f );
       // public FloatParameter highPercent = new FloatParameter (  0f);

       // public BoolParameter dynamicKeyValue = new BoolParameter (  true);

        public static GradientValue WhiteClean
        {
            get
            {
                var grad = new UnityEngine.Gradient();
                grad.SetKeys(new []{new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0f, 0f)});
                
                return new GradientValue(grad);
            }
        }

         [HideInInspector]
        public ClampedFloatParameter m_Scatter = new ClampedFloatParameter(0, 0, 1);
        public CurveParameter m_Threshold = new CurveParameter(new CurveValue(new AnimationCurve(
                                                                                         new Keyframe(.57f, 0f, 8f, 8f, 0f, 0.1732f),
                                                                                         new Keyframe(1f, 1f, .3f, .3f, .32f, 0.0f))), false);
        [Tooltip("Color replacement for initial EyeAdapt color by threshold evaluation")]
        public GradientParameter m_Color = new GradientParameter(WhiteClean, false);
        */
        public ClampedFloatParameter m_Intencity = new ClampedFloatParameter(0, 0, 1);
       
       // [HideInInspector]
       // public ClampedFloatParameter m_Flicker = new ClampedFloatParameter(1, 0, 1);


        // =======================================================================
        public bool IsActive() => active && m_Intencity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}