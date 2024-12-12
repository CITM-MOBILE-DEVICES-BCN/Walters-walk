using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//https://github.com/microsoft/InsiderDevTour18-MR/blob/master/Assets/3D%20Models/PostProcessing/Runtime/Components/EyeAdaptationComponent.cs#L3
//https://github.com/Unity-Technologies/FPSSample/blob/master/Packages/com.unity.postprocessing/PostProcessing/Runtime/Effects/AutoExposure.cs

namespace Artngame.GLAMOR.VolFx
{



    //[Serializable]
    //public class EyeAdaptationModel// : PostProcessingModel
    //{
    //    public enum EyeAdaptationType
    //    {
    //        Progressive,
    //        Fixed
    //    }

    //    [Serializable]
    //    public struct Settings
    //    {
    //        [Range(1f, 99f), Tooltip("Filters the dark part of the histogram when computing the average luminance to avoid very dark pixels from contributing to the auto exposure. Unit is in percent.")]
    //        public float lowPercent;

    //        [Range(1f, 99f), Tooltip("Filters the bright part of the histogram when computing the average luminance to avoid very dark pixels from contributing to the auto exposure. Unit is in percent.")]
    //        public float highPercent;

    //        [Tooltip("Minimum average luminance to consider for auto exposure (in EV).")]
    //        public float minLuminance;

    //        [Tooltip("Maximum average luminance to consider for auto exposure (in EV).")]
    //        public float maxLuminance;

    //        [Min(0f), Tooltip("Exposure bias. Use this to offset the global exposure of the scene.")]
    //        public float keyValue;

    //        [Tooltip("Set this to true to let Unity handle the key value automatically based on average luminance.")]
    //        public bool dynamicKeyValue;

    //        [Tooltip("Use \"Progressive\" if you want the auto exposure to be animated. Use \"Fixed\" otherwise.")]
    //        public EyeAdaptationType adaptationType;

    //        [Min(0f), Tooltip("Adaptation speed from a dark to a light environment.")]
    //        public float speedUp;

    //        [Min(0f), Tooltip("Adaptation speed from a light to a dark environment.")]
    //        public float speedDown;

    //        [Range(-16, -1), Tooltip("Lower bound for the brightness range of the generated histogram (in EV). The bigger the spread between min & max, the lower the precision will be.")]
    //        public int logMin;

    //        [Range(1, 16), Tooltip("Upper bound for the brightness range of the generated histogram (in EV). The bigger the spread between min & max, the lower the precision will be.")]
    //        public int logMax;

    //        public static Settings defaultSettings
    //        {
    //            get
    //            {
    //                return new Settings
    //                {
    //                    lowPercent = 45f,
    //                    highPercent = 95f,

    //                    minLuminance = -5f,
    //                    maxLuminance = 1f,
    //                    keyValue = 0.25f,
    //                    dynamicKeyValue = true,

    //                    adaptationType = EyeAdaptationType.Progressive,
    //                    speedUp = 2f,
    //                    speedDown = 1f,

    //                    logMin = -8,
    //                    logMax = 4
    //                };
    //            }
    //        }
    //    }

    //    [SerializeField]
    //    Settings m_Settings = Settings.defaultSettings;
    //    public Settings settings
    //    {
    //        get { return m_Settings; }
    //        set { m_Settings = value; }
    //    }

    //    //public override void Reset()
    //    //{
    //    //    m_Settings = Settings.defaultSettings;
    //    //}
    //}


    [ShaderName("Hidden/VolFx/EyeAdaptGLAMOR")]
    public class EyeAdaptPass : VolFxProc.Pass
    {

        //AUTO
        internal static readonly int _Params = Shader.PropertyToID("_Params");
        internal static readonly int _Speed = Shader.PropertyToID("_Speed");
        internal static readonly int _ScaleOffsetRes = Shader.PropertyToID("_ScaleOffsetRes");
        internal static readonly int _ExposureCompensation = Shader.PropertyToID("_ExposureCompensation");
        internal static readonly int _AutoExposure = Shader.PropertyToID("_AutoExposure");
        internal static readonly int _DebugWidth = Shader.PropertyToID("_DebugWidth");
        ComputeShader m_EyeCompute;
        ComputeBuffer m_HistogramBuffer;
        readonly RenderTexture[] m_AutoExposurePool = new RenderTexture[2];
        int m_AutoExposurePingPing;
        RenderTexture m_CurrentAutoExposure;
        RenderTexture m_DebugHistogram;
        static uint[] s_EmptyHistogramBuffer;
        bool m_FirstFrame = true;

        // Don't forget to update 'EyeAdaptation.cginc' if you change these values !
        const int k_HistogramBins = 64;
        const int k_HistogramThreadX = 16;
        const int k_HistogramThreadY = 16;
        //public override bool active
        //{
        //    get
        //    {
        //        return model.enabled
        //               && SystemInfo.supportsComputeShaders
        //               && !context.interrupted;
        //    }
        //}
        public void ResetHistory()
        {
            m_FirstFrame = true;
        }
        //public override void OnEnable()
        //{
        //    m_FirstFrame = true;
        //}
        //public override void OnDisable()
        //{
        //    foreach (var rt in m_AutoExposurePool)
        //        GraphicsUtils.Destroy(rt);

        //    if (m_HistogramBuffer != null)
        //        m_HistogramBuffer.Release();

        //    m_HistogramBuffer = null;

        //    if (m_DebugHistogram != null)
        //        m_DebugHistogram.Release();

        //    m_DebugHistogram = null;
        //}
        Vector4 GetHistogramScaleOffsetRes(float logMin, float logMax, int contextWidth, int contextHeight)
        {
           // var settings = model.settings;
            float diff = logMax - logMin;
            float scale = 1f / diff;
            float offset = -logMin * scale;
            return new Vector4(scale, offset, Mathf.Floor(contextWidth / 1f), Mathf.Floor(contextHeight / 1f));
        }
        private RenderTarget _rtTMP;



        private static readonly int s_ValueTex  = Shader.PropertyToID("_ValueTex");
        private static readonly int s_ColorTex  = Shader.PropertyToID("_ColorTex");
        private static readonly int s_Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int s_EyeAdaptTex = Shader.PropertyToID("_EyeAdaptTex");
        private static readonly int s_DownTex   = Shader.PropertyToID("_DownTex");
        private static readonly int s_Blend     = Shader.PropertyToID("_Blend");
        
        [Range(3, 14)]
        public  int              _samples = 7;
        [CurveRange(0, 0, 1, 3)]
        public AnimationCurve    _scatter = new AnimationCurve(new Keyframe(0.0f, 0.8594512939453125f, 0.0f, 0.1687847524881363f, 0f, .9040920734405518f),
                                                               new Keyframe(1.0f, 2.1807241439819338f, 7.417094707489014f, 1.3360401391983033f, 0.06010228395462036f, 0f));
        public ValueMode         _mode = ValueMode.Luma;
        [CurveRange(0, 0, 1, 1)]
        public  AnimationCurve   _flicker = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(.5f, .77f), new Keyframe(1, 1) }) { postWrapMode = WrapMode.Loop };
        public float             _flickerPeriod = 7f;
        public bool _EyeAdaptOnly;
        
        private float            _time;
        private float            _intensity;
        private ProfilingSampler _sampler;
        private Texture2D        _valueTex;
        private Texture2D        _colorTex;
        
        private RenderTarget[]   _mipDown;
        private RenderTarget[]   _mipUp;
        private float            _scatterLerp;

        public int Samples
        {
            get => _samples;
            set 
            { 
                _samples = value;
                Init();
            }
        }

        // =======================================================================
        public enum ValueMode
        {
            Luma,
            Brightness
        }

        // =======================================================================
        public override void Init()
        {

            //AUTO
            _rtTMP = new RenderTarget().Allocate($"_rtTMP");
            //Debug.Log("Inited");

            _mipDown = new RenderTarget[_samples];
            _mipUp = new RenderTarget[_samples - 1];
            
            for (var n = 0; n < _samples; n++)
                _mipDown[n] = new RenderTarget().Allocate($"EyeAdapt_{name}_down_{n}");
            for (var n = 0; n < _samples - 1; n++)
                _mipUp[n] = new RenderTarget().Allocate($"EyeAdapt_{name}_up_{n}");
            
            _sampler = new ProfilingSampler(name);
            
            _validateMaterial();
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<EyeAdaptVol>();

            if (settings.IsActive() == false)
                return false;
            
            _time += Time.deltaTime;
            
            mat.SetTexture(s_ValueTex, settings.m_Threshold.value.GetTexture(ref _valueTex));
            mat.SetTexture(s_ColorTex, settings.m_Color.value.GetTexture(ref _colorTex));
            _intensity = settings.m_Intencity.value * Mathf.Lerp(1, _flicker.Evaluate(_time / _flickerPeriod), settings.m_Flicker.value);
            _scatterLerp = settings.m_Scatter.value;
            
            return true;
        }

        private void OnValidate()
        {
            if (Application.isPlaying == false)
            {
                //AUTO
                _rtTMP = new RenderTarget().Allocate($"_rtTMP");


                _mipDown = new RenderTarget[_samples];
                _mipUp   = new RenderTarget[_samples - 1];
                
                for (var n = 0; n < _samples; n++)
                    _mipDown[n] = new RenderTarget().Allocate($"EyeAdapt_{name}_down_{n}");
                
                for (var n = 0; n < _samples - 1; n++)
                    _mipUp[n]   = new RenderTarget().Allocate($"EyeAdapt_{name}_up_{n}");
            }

            _validateMaterial();
        }

        private void _validateMaterial()
        {
            if (_material != null)
            {
                _material.DisableKeyword("_BRIGHTNESS");
                _material.DisableKeyword("_LUMA");
                _material.DisableKeyword("_EyeAdapt_ONLY");

                _material.EnableKeyword(_mode switch
                {
                    ValueMode.Luma       => "_LUMA",
                    ValueMode.Brightness => "_BRIGHTNESS",
                    _                    => throw new ArgumentOutOfRangeException()
                });

                if (_EyeAdaptOnly)
                    _material.EnableKeyword("_EyeAdapt_ONLY");
            }
        }


        //public Texture Prepare(CommandBuffer cmd, RenderTextureDescriptor desc, RenderTexture source, Material uberMaterial)
        public RenderTexture Prepare(CommandBuffer cmd, RenderTextureDescriptor desc, RTHandle source, RTHandle dest, ScriptableRenderContext context)
        {
            if (m_CurrentAutoExposure == null)
            {
                m_CurrentAutoExposure = new RenderTexture(desc);
            }
            //Debug.Log("m_CurrentAutoExposure:" + m_CurrentAutoExposure.width);

            var settings = Stack.GetComponent<EyeAdaptVol>();

           // var settings = model.settings;

            // Setup compute
            if (m_EyeCompute == null)
                m_EyeCompute = Resources.Load<ComputeShader>("Shaders/EyeHistogram");

           // Debug.Log(m_EyeCompute.name);

            var material = new Material(Resources.Load<Shader>("Shaders/EyeAdaptationGLAMOR"));// context.materialFactory.Get("Hidden/Post FX/Eye Adaptation");
            material.shaderKeywords = null;

            if (m_HistogramBuffer == null)
                m_HistogramBuffer = new ComputeBuffer(k_HistogramBins, sizeof(uint));

            if (s_EmptyHistogramBuffer == null)
                s_EmptyHistogramBuffer = new uint[k_HistogramBins];

            // Downscale the framebuffer, we don't need an absolute precision for auto exposure and it
            // helps making it more stable
            // Vector4 GetHistogramScaleOffsetRes(float logMin, float logMax, int contextWidth, int contextHeight)
            var scaleOffsetRes = GetHistogramScaleOffsetRes(-9,9,desc.width, desc.height);///CHECK

            //Debug.Log(scaleOffsetRes);

            //var rt = _mipDown[n].Get(cmd, in desc, FilterMode.Bilinear);// context.renderTextureFactory.Get((int)scaleOffsetRes.z, (int)scaleOffsetRes.w, 0, source.format);

            //_mipUp[0].Get(cmd, in desc, FilterMode.Bilinear);
           // Debug.Log("Init1:" + _mipUp[0].Handle.rt.name);

            _rtTMP.Get(cmd, in desc, FilterMode.Bilinear);
            //Debug.Log("Init:" + _rtTMP.Handle.rt.name);
            //Utils.JustBlit(cmd, source.rt, dest);
            //return dest.rt;

            //Graphics.Blit(source, _rtTMP.Handle);
            // Utils.JustBlit(cmd, source, _rtTMP.Handle.rt);
            //Utils.JustBlitA(cmd, source.rt, _rtTMP);
            Utils.JustBlitA(cmd, source.rt, _rtTMP);

           // Utils.JustBlit(cmd, _rtTMP.Handle.rt, dest);
           
            _material.SetFloat(s_Intensity, 1);
            cmd.SetGlobalTexture(s_EyeAdaptTex, source);
            //Utils.Blit(cmd, source, dest, _material, 3);
         

            if (m_AutoExposurePool[0] == null || !m_AutoExposurePool[0].IsCreated())
                m_AutoExposurePool[0] = new RenderTexture(1, 1, 0, RenderTextureFormat.RFloat);

            if (m_AutoExposurePool[1] == null || !m_AutoExposurePool[1].IsCreated())
                m_AutoExposurePool[1] = new RenderTexture(1, 1, 0, RenderTextureFormat.RFloat);

            // Clears the buffer on every frame as we use it to accumulate luminance values on each frame
            m_HistogramBuffer.SetData(s_EmptyHistogramBuffer);

            // Gets a log histogram
            int kernel = m_EyeCompute.FindKernel("KEyeHistogram");
            m_EyeCompute.SetBuffer(kernel, "_Histogram", m_HistogramBuffer);
           // cmd.SetGlobalBuffer("_Histogram", m_HistogramBuffer);
            //cmd.SetBufferData(kernel, "_Histogram", m_HistogramBuffer);

            cmd.SetGlobalTexture("_Source", source);
            //m_EyeCompute.SetTexture(kernel, "_Source", _rtTMP.Handle);
            //cmd.SetGlobalTexture

            

            cmd.SetGlobalVector("_ScaleOffsetRes", scaleOffsetRes);

            m_EyeCompute.SetVector("_ScaleOffsetRes", scaleOffsetRes);

           // Utils.Blit(cmd, _rtTMP.Handle, dest, _material, 3);
          //  Debug.Log(m_EyeCompute);
          //  Debug.Log(kernel);
           // Debug.Log(_rtTMP.Handle);
           // Debug.Log(_rtTMP.Handle.name);
           // Debug.Log(_rtTMP.Handle.rt.name);
           // Debug.Log(_rtTMP.Handle.rt.width);
           // return dest.rt;

            // m_EyeCompute.Dispatch(kernel, Mathf.CeilToInt(_rtTMP.Handle.rt.width / (float)k_HistogramThreadX), Mathf.CeilToInt(_rtTMP.Handle.rt.height / (float)k_HistogramThreadY), 1);
            cmd.DispatchCompute(m_EyeCompute, kernel, Mathf.CeilToInt(desc.width / (float)k_HistogramThreadX), Mathf.CeilToInt(desc.height / (float)k_HistogramThreadY), 1);

          

            // Cleanup
            //         context.renderTextureFactory.Release(rt);

            // Make sure filtering values are correct to avoid apocalyptic consequences
            const float minDelta = 1e-2f;
            settings.highPercent.value = Mathf.Clamp(settings.highPercent.value, 1f + minDelta, 99f);
            settings.lowPercent.value = Mathf.Clamp(settings.lowPercent.value, 1f, settings.highPercent.value - minDelta);

            // Compute auto exposure
            material.SetBuffer("_Histogram", m_HistogramBuffer); // No (int, buffer) overload for SetBuffer ?
            material.SetVector(_Params, new Vector4(settings.lowPercent.value * 0.01f, settings.highPercent.value * 0.01f, 
                Mathf.Exp(settings.minLuminance.value * 0.69314718055994530941723212145818f), 
                Mathf.Exp(settings.maxLuminance.value * 0.69314718055994530941723212145818f)));
            material.SetVector(_Speed, new Vector2(settings.speedDown.value, settings.speedUp.value));
            material.SetVector(_ScaleOffsetRes, scaleOffsetRes);
            material.SetFloat(_ExposureCompensation, settings.keyValue.value);

            if (settings.dynamicKeyValue.value)
                material.EnableKeyword("AUTO_KEY_VALUE");

            if (m_FirstFrame || !Application.isPlaying)
            {
                // We don't want eye adaptation when not in play mode because the GameView isn't
                // animated, thus making it harder to tweak. Just use the final audo exposure value.
                //m_CurrentAutoExposure = m_AutoExposurePool[0];
                Utils.JustBlit(cmd, m_AutoExposurePool[0], m_CurrentAutoExposure);

                cmd.SetGlobalVector("_Speed", new Vector2(settings.speedDown.value, settings.speedUp.value));
                //Debug.Log(new Vector2(settings.speedDown.value, settings.speedUp.value));
                //Graphics.Blit(null, m_CurrentAutoExposure, material, 1);
                Utils.JustBlitB(cmd,null, m_CurrentAutoExposure, material, 1);

                // Copy current exposure to the other pingpong target to avoid adapting from black
               // Graphics.Blit(m_AutoExposurePool[0], m_AutoExposurePool[1]);
                Utils.JustBlit(cmd, m_AutoExposurePool[0], m_AutoExposurePool[1]);
            }
            else
            {
                int pp = m_AutoExposurePingPing;
                var src = m_AutoExposurePool[++pp % 2];
                var dst = m_AutoExposurePool[++pp % 2];
                //material.SetTexture("_MainTex", src);
                cmd.SetGlobalTexture("_MainTexB", src);

                cmd.SetGlobalBuffer("_Histogram", m_HistogramBuffer); // No (int, buffer) overload for SetBuffer ?
                cmd.SetGlobalVector(_Params, new Vector4(settings.lowPercent.value * 0.01f, settings.highPercent.value * 0.01f,
                    Mathf.Exp(settings.minLuminance.value * 0.69314718055994530941723212145818f),
                    Mathf.Exp(settings.maxLuminance.value * 0.69314718055994530941723212145818f)));
                cmd.SetGlobalVector(_Speed, new Vector2(settings.speedDown.value, settings.speedUp.value));
                cmd.SetGlobalVector(_ScaleOffsetRes, scaleOffsetRes);
                //Debug.Log(scaleOffsetRes);
                cmd.SetGlobalFloat(_ExposureCompensation, settings.keyValue.value);

                cmd.SetGlobalVector("_Speed", new Vector2(settings.speedDown.value, settings.speedUp.value));
                //Debug.Log(new Vector2(settings.speedDown.value, settings.speedUp.value));
                // Graphics.Blit(src, dst, material, (int)settings.adaptationType.value);
                //Utils.JustBlitB(cmd, src, dst, material, (int)settings.adaptationType.value);
                material.SetTexture("_MainTexB", src);
                Utils.BlitRT(cmd, src, dst, material, (int)settings.adaptationType.value);

                //Debug.Log("LOOPING");
                m_AutoExposurePingPing = ++pp % 2;
               // m_CurrentAutoExposure = dst;
                Utils.JustBlit(cmd, dst, m_CurrentAutoExposure);

            }

            bool debugMe = false;
            if (debugMe)
            {
                // Generate debug histogram
                //if (context.profile.debugViews.IsModeActive(BuiltinDebugViewsModel.Mode.EyeAdaptation))
                //{
                if (m_DebugHistogram == null || !m_DebugHistogram.IsCreated())
                {
                    m_DebugHistogram = new RenderTexture(256, 128, 0, RenderTextureFormat.ARGB32)
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp
                    };
                }

                material.SetFloat("_DebugWidth", m_DebugHistogram.width);
                //    Graphics.Blit(null, m_DebugHistogram, material, 2);
                Utils.JustBlitB(cmd, source.rt, m_DebugHistogram, material, 2);

                //}
                material.SetTexture("_MainTex", m_DebugHistogram);
                // Utils.JustBlitB(cmd, m_DebugHistogram, m_CurrentAutoExposure, material, 0);
                Utils.JustBlit(cmd, m_DebugHistogram, m_CurrentAutoExposure);
            }

            m_FirstFrame = false;
            return m_CurrentAutoExposure;
        }


        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData
            , RenderTextureDescriptor descA, bool isRG)
        {
            _sampler.Begin(cmd);

            RenderTextureDescriptor desc; //= renderingData.cameraData.cameraTargetDescriptor;

            if (isRG)
            {
                desc = descA;
            }
            else
            {
                desc = renderingData.cameraData.cameraTargetDescriptor;
            }

            desc.colorFormat        = RenderTextureFormat.ARGB32;
            desc.depthStencilFormat = GraphicsFormat.None;

            //Debug.Log(source.name); 
            //Debug.Log(dest.rt.name);
            //Debug.Log(source.rt.name);

            //Utils.JustBlit(cmd, source.rt, dest);

                 m_CurrentAutoExposure =  Prepare( cmd,  desc, source, dest, context);
            //   cmd.Blit( m_CurrentAutoExposure, dest);

           
            //Utils.JustBlit(cmd, source, m_CurrentAutoExposure);
            //return m_CurrentAutoExposure;

            //_material.SetFloat(s_Intensity, 1);
            //cmd.SetGlobalTexture(s_EyeAdaptTex, source);
            //Utils.Blit(cmd, source, dest, _material, 3);
            var material = new Material(Resources.Load<Shader>("Shaders/EyeAdaptationGLAMOR"));
            //Utils.JustBlitB(cmd, m_CurrentAutoExposure, dest, material, 3);
            cmd.SetGlobalTexture("_EyeAdaptTex", m_CurrentAutoExposure);
            cmd.SetGlobalTexture("_MainTexA", source);
            //material.SetTexture("_MainTex", source);
            Utils.Blit(cmd, source, dest, material,3);

       //     Utils.JustBlitB(cmd, m_CurrentAutoExposure, dest, material, 3);


            //       _rtTMP.Get(cmd, in desc, FilterMode.Bilinear);
            //Debug.Log("Init:" + _rtTMP.Handle.rt.name);
            //Graphics.Blit(source, _rtTMP.Handle);
            //      Utils.JustBlitA(cmd, source.rt, _rtTMP);
            //       Utils.JustBlit(cmd, _rtTMP.Handle.rt, dest);
            //Utils.JustBlit(cmd, source.rt, dest);

            //Material _materialA = new Material(Resources.Load<Shader>("Shaders/EyeAdaptation"));
            //Utils.Blit(cmd, source, dest, _materialA, 2);
            //cmd.Blit(source, dest);
            //Utils.JustBlit(cmd, source, dest, _materialA, 2);

            /*
            for (var n = 0; n < _samples - 1; n++)
            {
                desc.width = Mathf.Max(1, desc.width >> 1);
                desc.height = Mathf.Max(1, desc.height >> 1);
                
                _mipDown[n].Get(cmd, in  desc, FilterMode.Bilinear);
                _mipUp[n].Get(cmd, in  desc, FilterMode.Bilinear);
            }
            
            desc.width = Mathf.Max(1, desc.width >> 1);
            desc.height = Mathf.Max(1, desc.height >> 1);
            
            _mipDown[_samples - 1].Get(cmd, in  desc, FilterMode.Bilinear);

            Utils.Blit(cmd, source, _mipDown[0], _material);
                
            for (var n = 1; n < _samples; n++)
                Utils.Blit(cmd, _mipDown[n - 1], _mipDown[n], _material, 1);
            
            var totalBlend = 0f;
            var blend = Mathf.Lerp(_scatter.Evaluate(0f), _scatter.Evaluate(1f), _scatterLerp);
            totalBlend += blend;
            cmd.SetGlobalFloat(s_Blend, blend);
            cmd.SetGlobalTexture(s_DownTex, _mipDown[_samples - 1].Handle.nameID);
            Utils.Blit(cmd, _mipDown[_samples - 2].Handle, _mipUp[_samples - 2].Handle, _material, 2);
            for (var n = _samples - 3; n >= 0; n--)
            {
                var t = n / (float)(_samples - 2);
                blend =  Mathf.Lerp(_scatter.Evaluate(t), _scatter.Evaluate(t), _scatterLerp);
                totalBlend += blend;
                cmd.SetGlobalFloat(s_Blend, blend);
                cmd.SetGlobalTexture(s_DownTex, _mipDown[n].Handle.nameID);
                Utils.Blit(cmd, _mipUp[n + 1].Handle, _mipUp[n].Handle, _material, 2);
            }
            
            _material.SetFloat(s_Intensity, _intensity / totalBlend);
            cmd.SetGlobalTexture(s_EyeAdaptTex, _mipUp[0].Handle);
            Utils.Blit(cmd, source, dest, _material, 3);
            */

            _sampler.End(cmd);
        }

        public override void Cleanup(CommandBuffer cmd)
        {
            foreach (var rt in _mipDown)
                rt.Release(cmd);
            
            foreach (var rt in _mipUp)
                rt.Release(cmd);
        }
    }
}