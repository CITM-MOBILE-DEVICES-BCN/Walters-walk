using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace Artngame.GLAMOR.VolFx
{

    [Serializable, VolumeComponentMenu("VolFx/Volumetric Hazy Fog")]
    public sealed class VolumeHazyFogFxVol : VolumeComponent, IPostProcessComponent
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


        //SMSS
        public BoolParameter enableWetnessHaze = new BoolParameter(false);
        public BoolParameter _highQuality = new BoolParameter(true);
        public BoolParameter _antiFlicker = new BoolParameter(true);
        public BoolParameter allowHDR = new BoolParameter(true);
        public NoInterpFloatParameter thresholdLinear = new NoInterpFloatParameter(0);
        public NoInterpFloatParameter _softKnee = new NoInterpFloatParameter(0.5f);
        public NoInterpFloatParameter _radius = new NoInterpFloatParameter(0f);
        public NoInterpFloatParameter _blurWeight = new NoInterpFloatParameter(1f);
        public NoInterpVector3Parameter _intensity = new NoInterpVector3Parameter(new Vector3(1,0,0));       
        public TextureParameter _fadeRamp = new TextureParameter(null);
        public NoInterpColorParameter _blurTint = new NoInterpColorParameter(Color.white);
    

        public ClampedFloatParameter m_Intencity = new ClampedFloatParameter(0, 0, 1);
       
       // [HideInInspector]
       // public ClampedFloatParameter m_Flicker = new ClampedFloatParameter(1, 0, 1);


        // =======================================================================
        public bool IsActive() => active && m_Intencity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}