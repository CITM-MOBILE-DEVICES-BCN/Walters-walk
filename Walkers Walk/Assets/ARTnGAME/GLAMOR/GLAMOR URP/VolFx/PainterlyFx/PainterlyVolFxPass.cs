using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//https://github.com/microsoft/InsiderDevTour18-MR/blob/master/Assets/3D%20Models/PostProcessing/Runtime/Components/EyeAdaptationComponent.cs#L3
//https://github.com/Unity-Technologies/FPSSample/blob/master/Packages/com.unity.postprocessing/PostProcessing/Runtime/Effects/AutoExposure.cs

namespace Artngame.GLAMOR.VolFx
{

    [ShaderName("Hidden/VolFx/EyeAdaptGLAMOR")]
    public class PainterlyVolFxPass : VolFxProc.Pass
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
       // bool m_FirstFrame = true;

        // Don't forget to update 'EyeAdaptation.cginc' if you change these values !
        const int k_HistogramBins = 64;
        const int k_HistogramThreadX = 16;
        const int k_HistogramThreadY = 16;
     
        public void ResetHistory()
        {
           // m_FirstFrame = true;
        }
       
        Vector4 GetHistogramScaleOffsetRes(float logMin, float logMax, int contextWidth, int contextHeight)
        {
           // var settings = model.settings;
            float diff = logMax - logMin;
            float scale = 1f / diff;
            float offset = -logMin * scale;
            return new Vector4(scale, offset, Mathf.Floor(contextWidth / 1f), Mathf.Floor(contextHeight / 1f));
        }

        private RenderTarget _rtTMP;

        //SUN SHAFTS
        private RenderTarget lrColorB;
        private RenderTarget m_TemporaryColorTexture;
        private RenderTarget lrDepthBuffer;
        private RenderTarget tmpBuffer;


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


        //SUN SHAFTS
        public Vector4 cloudsSpeed = new Vector4(1,1,1,1);


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
            lrColorB = new RenderTarget().Allocate($"lrColorB");
            m_TemporaryColorTexture = new RenderTarget().Allocate($"m_TemporaryColorTexture");
            lrDepthBuffer = new RenderTarget().Allocate($"lrDepthBuffer");
            tmpBuffer = new RenderTarget().Allocate($"tmpBuffer");
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
            //var settings = Stack.GetComponent<PainterlyVolFxVol>();
            if (settings == null)
            {
                settings = Stack.GetComponent<PainterlyVolFxVol>();
            }

            if (settings.IsActive() == false)
                return false;
            
            _time += Time.deltaTime;
            
            //mat.SetTexture(s_ValueTex, settings.m_Threshold.value.GetTexture(ref _valueTex));
            //mat.SetTexture(s_ColorTex, settings.m_Color.value.GetTexture(ref _colorTex));
            //_intensity = settings.m_Intencity.value * Mathf.Lerp(1, _flicker.Evaluate(_time / _flickerPeriod), settings.m_Flicker.value);
            //_scatterLerp = settings.m_Scatter.value;
            //cloudsSpeed = settings.noiseCloudSpeed.value;//
            return true;
        }

        private void OnValidate()
        {
            if (Application.isPlaying == false)
            {
                //AUTO
                _rtTMP = new RenderTarget().Allocate($"_rtTMP");
                lrColorB = new RenderTarget().Allocate($"lrColorB");
                m_TemporaryColorTexture = new RenderTarget().Allocate($"m_TemporaryColorTexture");
                lrDepthBuffer = new RenderTarget().Allocate($"lrDepthBuffer");
                tmpBuffer = new RenderTarget().Allocate($"tmpBuffer");

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


        PainterlyVolFxVol settings;

        /*
        //SUN SHAFTS
        public void RenderShafts(ScriptableRenderContext context, 
            UnityEngine.Rendering.Universal.RenderingData renderingData, 
            CommandBuffer cmd, 
            RenderTextureDescriptor opaqueDesc,
            Material blitMaterial, RTHandle source, RTHandle destination, bool isRG)
        {
            if (settings == null)
            {
                settings = Stack.GetComponent<PainterlyVolFxVol>();
            }

            if (settings.useNoise.value)
            {
                blitMaterial = new Material(Resources.Load<Shader>("SunShaftsSRP_FORWARD_URP_RG_FBM"));
                //Debug.Log(blitMaterial.shader.name);
                blitMaterial.SetTexture("_MainTexFBM", settings.MainTexFBM.value);
                blitMaterial.SetTexture("_Tex2", settings.NoiseTex2.value);
                blitMaterial.SetFloat("_Distort", settings.Distort.value);
                blitMaterial.SetColor("_HighLight", settings.HighLight.value); 
                blitMaterial.SetColor("_noiseColor", settings.NoiseColor.value);
                blitMaterial.SetFloat("_Pow", settings.noisePower.value);
                blitMaterial.SetVector("brightnessContrast", settings.brightnessContrast.value);

                blitMaterial.SetVector("cloudSpeed", settings.noiseCloudSpeed.value);
            }

            blitMaterial.SetFloat("_Blend", settings.m_Intencity.value*1);
            //Debug.Log(settings.m_Intencity.value*1);

            opaqueDesc.depthBufferBits = 0;
            Material sheetSHAFTS = blitMaterial;
            //sheetSHAFTS.SetFloat("_Blend", settings.blend.value);
            Camera camera = Camera.main;
            if (settings.useDepthTexture.value)
            {
                camera.depthTextureMode |= DepthTextureMode.Depth;
            }
            Vector3 v = Vector3.one * 0.5f;
            if (settings.sunTransform != Vector3.zero)
            {
                v = Camera.main.WorldToViewportPoint(settings.sunTransform.value);
            }
            else
            {
                v = new Vector3(0.5f, 0.5f, 0.0f);
            }

            //v0.1
            int rtW = (int)(opaqueDesc.width/ settings.resolutionDivider.value);//context.width; //source.width / divider;
            int rtH = (int)(opaqueDesc.height / settings.resolutionDivider.value);// context.width; //source.height / divider;

            var formatA = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
    //        RenderTexture m_TemporaryColorTexture = RenderTexture.GetTemporary(opaqueDesc.width, opaqueDesc.height, 0, formatA);
    //        RenderTexture lrDepthBuffer = RenderTexture.GetTemporary(opaqueDesc.width, opaqueDesc.height, 0, formatA);
                

            FilterMode filterMode = FilterMode.Point;
            if (settings.filterMode.value == 1)
            {
                filterMode = FilterMode.Bilinear;
            }
            if (settings.filterMode.value == 2)
            {
                filterMode = FilterMode.Trilinear;
            }

            //          cmd.GetTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name), opaqueDesc,filterMode);
            m_TemporaryColorTexture.Get(cmd, in opaqueDesc);
            lrDepthBuffer.Get(cmd, in opaqueDesc, filterMode);

            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * settings.sunShaftBlurRadius.value);
            sheetSHAFTS.SetVector("_SunThreshold", settings.sunThreshold.value);

            ////////   cmd.Blit(source, m_TemporaryColorTexture); //KEEP BACKGROUND
           // sheetSHAFTS.SetTexture("_MainTexA", source.rt);
            //cmd.SetGlobalTexture("_MainTexA", source.rt);
            Utils.JustBlitA(cmd, source.rt, m_TemporaryColorTexture);
                        


            if (!settings.useDepthTexture.value && 1==1)
            {
                var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9

                RenderTextureDescriptor descA1 = new RenderTextureDescriptor(rtW, rtH, format);
 //             RenderTexture tmpBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                tmpBuffer.Get(cmd, in descA1);
                         
                RenderTexture.active = tmpBuffer.Handle.rt;
                GL.ClearWithSkybox(false, camera);
                sheetSHAFTS.SetTexture("_Skybox", tmpBuffer.Handle.rt);

                //////         cmd.Blit(source, lrDepthBuffer, sheetSHAFTS, 3);    /////////////
                //Utils.JustBlitA(cmd, source.rt, _rtTMP);
                sheetSHAFTS.SetTexture("_MainTexA", source.rt);
                Utils.JustBlitB(cmd, source.rt, lrDepthBuffer.Handle.rt, sheetSHAFTS, 3);
                //Utils.JustBlit(cmd, m_DebugHistogram, m_CurrentAutoExposure);
                //Utils.BlitRT(cmd, source, destination, blitMaterial, (int)settings.adaptationType.value);

                //              RenderTexture.ReleaseTemporary(tmpBuffer.Handle);                
            }
            else
            {
///////        //cmd.Blit(source, lrDepthBuffer, sheetSHAFTS, 2);
                sheetSHAFTS.SetTexture("_MainTexA", source.rt);
                //Utils.JustBlitB(cmd, source.rt, lrDepthBuffer.Handle.rt, sheetSHAFTS, 2);
                Utils.Blit(cmd, source, lrDepthBuffer, sheetSHAFTS, 2);
                //Utils.Blit(cmd, source, lrDepthBuffer, sheetSHAFTS, 2);
                //RenderTexture.active = null;
            }
            //Debug.Log(source.rt.width);

           
            //TEST TEST TEST
            //Utils.JustBlitB(cmd, source.rt, destination, sheetSHAFTS, 0);
            // Utils.JustBlit(cmd, source.rt, destination);
            //return;///


            settings.radialBlurIterations.value = Mathf.Clamp(settings.radialBlurIterations.value, 1, 4);

            float ofs = settings.sunShaftBlurRadius.value * (1.0f / 768.0f);
            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
                        
            int eyeCount = 1;
            Matrix4x4[] cameraMatrices = new Matrix4x4[2];
            for (int eyeIndex = 0; eyeIndex < eyeCount; eyeIndex++)
            {
                
                if (isRG)
                {
                    cameraMatrices[eyeIndex] = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix;
                }
                else
                {
                    Matrix4x4 view = renderingData.cameraData.GetViewMatrix(eyeIndex);
                    Matrix4x4 proj = renderingData.cameraData.GetProjectionMatrix(eyeIndex);
                    cameraMatrices[eyeIndex] = proj * view;
                }
            }          
          

            sheetSHAFTS.SetMatrixArray("_CameraVP", cameraMatrices);
            sheetSHAFTS.SetVector("_SunPosition", new Vector4(-settings.sunTransform.value.x * 1f + 0, -settings.sunTransform.value.y * 1f, -settings.sunTransform.value.z * 1f, settings.maxRadius.value));//
            
            for (int it2 = 0; it2 < settings.radialBlurIterations.value; it2++)
            {
                RenderTextureDescriptor descA11 = new RenderTextureDescriptor(rtW, rtH);
///            //lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0);
                lrColorB.Get(cmd, in descA11);

                cmd.SetGlobalTexture("_MainTexA", lrDepthBuffer.Id);
                Utils.JustBlitC(cmd, lrDepthBuffer, lrColorB, sheetSHAFTS, 1);
                //cmd.Blit(lrDepthBuffer, lrColorB, sheetSHAFTS, 1);//Blit(cmd, lrDepthBuffer, lrColorB, sheetSHAFTS, 1); //Blit(cmd, lrDepthBuffer.Identifier(), lrColorB, sheetSHAFTS, 1);//v0.1
   //             cmd.ReleaseTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name));//  lrDepthBuffer.id);//v0.1

                ofs = settings.sunShaftBlurRadius.value * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

                //            cmd.GetTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name), opaqueDesc, filterMode);   //    lrDepthBuffer.id, opaqueDesc, filterMode);   //v0.1 
                lrDepthBuffer.Get(cmd, in opaqueDesc, filterMode);

                cmd.SetGlobalTexture("_MainTexA", lrColorB.Id);
                Utils.JustBlitC(cmd, lrColorB, lrDepthBuffer, sheetSHAFTS, 1);
                //cmd.Blit(lrColorB, lrDepthBuffer, sheetSHAFTS, 1); //Blit(cmd, lrColorB, lrDepthBuffer.Identifier(), sheetSHAFTS, 1);//v0.1
   //             RenderTexture.ReleaseTemporary(lrColorB);  //v0.1

                ofs = settings.sunShaftBlurRadius.value * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            }

            ////TESTER
            //cmd.SetGlobalTexture("_MainTexB", m_TemporaryColorTexture.Id);    ////////////////////////// BASIC, need 1. not be _MainTex and 2. not use Handle.rt but ID !!!
            ////sheetSHAFTS.SetTexture("_MainTexB", lrDepthBuffer);
            //Utils.Blit(cmd, m_TemporaryColorTexture, destination, sheetSHAFTS, 5);
            //return;

            if (v.z >= 0.0f)
            {
                sheetSHAFTS.SetVector("_SunColor", new Vector4(settings.sunColor.value.r, settings.sunColor.value.g, settings.sunColor.value.b, settings.sunColor.value.a) * settings.sunShaftIntensity.value);
            }
            else
            {
                sheetSHAFTS.SetVector("_SunColor", Vector4.zero); // no backprojection !
            }
            cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer.Id);


            //cmd.Blit(m_TemporaryColorTexture, source, sheetSHAFTS, (settings.screenBlendMode.value == BlitSunShaftsSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);
            if (settings.blendChoice.value == 0)
            {
                cmd.SetGlobalTexture("_MainTexA", source);
                //Utils.BlitRTA(cmd, source, destination.rt, sheetSHAFTS, 0);
                Utils.Blit(cmd, source, destination, sheetSHAFTS, 0);
                //cmd.Blit(m_TemporaryColorTexture, source, sheetSHAFTS, 0);
            }
            else{
                cmd.SetGlobalTexture("_MainTexA", source);
                //Utils.BlitRTA(cmd, source, destination.rt, sheetSHAFTS, 4);//
                Utils.Blit(cmd, source, destination, sheetSHAFTS, 4);//
                //cmd.Blit(m_TemporaryColorTexture, source, sheetSHAFTS, 4);
            }

            //TESTER
            //cmd.SetGlobalTexture("_MainTexB", lrColorB.Id);    ////////////////////////// BASIC, need 1. not be _MainTex and 2. not use Handle.rt but ID !!!
            ////sheetSHAFTS.SetTexture("_MainTexB", lrDepthBuffer);
            //Utils.Blit(cmd, lrDepthBuffer, destination, sheetSHAFTS, 5);
            //return;

        }
        */

        RenderTarget structureTensor;
        RenderTarget eigenvectors1;
        RenderTarget eigenvectors2;
        Material baseMaterial;

        RenderTarget[] kuwaharaPasses;
        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData
            , RenderTextureDescriptor descA, bool isRG)
        {
            _sampler.Begin(cmd);

            if (settings == null)
            {
                settings = Stack.GetComponent<PainterlyVolFxVol>();
            }

            RenderTextureDescriptor desc; //= renderingData.cameraData.cameraTargetDescriptor;

            if (isRG)
            {
                desc = descA;
            }
            else
            {
                desc = renderingData.cameraData.cameraTargetDescriptor;
            }

            desc.colorFormat = RenderTextureFormat.DefaultHDR;
            desc.depthStencilFormat = GraphicsFormat.None;

            //PainterlyVolFx_URP_RG_FBM
            //Material baseMaterial = Resources.Load<Material>("PainterlyVolFx_URP_RG_FBM");

            if (baseMaterial == null)
            {
                baseMaterial = Resources.Load<Material>("PainterlyVolFx_URP_RG_FBM");
            }
            if (baseMaterial == null)
            {
                baseMaterial = new Material(Resources.Load<Shader>("PainterlyVolFx_URP_RG_FBM"));
            }

            //var baseMaterialA = new Material(Resources.Load<Shader>("SunShaftsSRP_FORWARD_URP_RG"));

            /*
            //PAINTING
            baseMaterial.SetInt("_KernelSize", settings.kernelSize.value);
            //Graphics.Blit(src, dst, baseMaterial);

            //DRAWING
            bool isOffset = (Time.time % settings.shiftCycleTime.value) < (settings.shiftCycleTime.value / 2.0f);
            if (settings.drawingTex != null)
            {
                baseMaterial.SetTexture("_DrawingTex", settings.drawingTex.value);
            }
            baseMaterial.SetFloat("_OverlayOffset", isOffset ? 0.5f : 0.0f);
            baseMaterial.SetFloat("_Strength", settings.strength.value);
            baseMaterial.SetFloat("_Tiling", settings.tiling.value);
            baseMaterial.SetFloat("_Smudge", settings.smudge.value);
            baseMaterial.SetFloat("_DepthThreshold", settings.depthThreshold.value);
            // Graphics.Blit(src, dst, baseMaterial);
            */


            FilterMode filterMode = FilterMode.Bilinear;

            baseMaterial.SetFloat("_Blend", settings.m_Intencity.value);

            //KAWAHARA
            if (settings.KawaharaType.value == 0)
            {
                baseMaterial.SetInt("_kernelSizeKAWAHARA", settings.kernelSizeKAWAHARA.value);
                baseMaterial.SetInt("_MinKernelSize", settings.minKernelSize.value);
                baseMaterial.SetInt("_AnimateSize", settings.animateKernelSize.value ? 1 : 0);
                baseMaterial.SetFloat("_SizeAnimationSpeed", settings.sizeAnimationSpeed.value);
                baseMaterial.SetFloat("_NoiseFrequency", settings.noiseFrequency.value);
                baseMaterial.SetInt("_AnimateOrigin", settings.animateKernelOrigin.value ? 1 : 0);

                kuwaharaPasses = new RenderTarget[settings.passes.value];  //RenderTexture[] kuwaharaPasses = new RenderTexture[settings.passes.value];
                                
                for (int i = 0; i < settings.passes.value; ++i)
                {
                    if (kuwaharaPasses[i] == null)
                    {
                        kuwaharaPasses[i] = new RenderTarget().Allocate($"kuwaharaPasses" + i);  //kuwaharaPasses[i] = RenderTexture.GetTemporary(desc.width, desc.height, 0, desc.colorFormat);
                    }
                        kuwaharaPasses[i].Get(cmd, in desc, filterMode);
                }
                cmd.SetGlobalTexture("_MainTexA", source);
                Utils.Blit(cmd, source, kuwaharaPasses[0], baseMaterial, 3);

                for (int i = 1; i < settings.passes.value; ++i)
                {
                    cmd.SetGlobalTexture("_MainTexA", kuwaharaPasses[i - 1].Id);
                    Utils.Blit(cmd, kuwaharaPasses[i - 1], kuwaharaPasses[i], baseMaterial, 3);
                }
                cmd.SetGlobalTexture("_MainTexA", kuwaharaPasses[settings.passes.value - 1].Id);
                cmd.SetGlobalTexture("_ColorBuffer", source);
                Utils.Blit(cmd, kuwaharaPasses[settings.passes.value - 1], dest, baseMaterial, 0);
            }else
            //END KAWAHARA
            //GENERALIZED KAWAHARA
            if (settings.KawaharaType.value == 1)
            {
                baseMaterial.SetInt("_KernelSize", settings.kernelSizeKAWAHARA.value);
                baseMaterial.SetInt("_N", 8);
                baseMaterial.SetFloat("_Q", settings.sharpness.value);
                baseMaterial.SetFloat("_Hardness", settings.hardness.value);
                baseMaterial.SetFloat("_ZeroCrossing", settings.zeroCrossing.value);
                baseMaterial.SetFloat("_Zeta", settings.useZeta.value ? settings.zeta.value : 2.0f / (settings.kernelSizeKAWAHARA.value / 2.0f));

                kuwaharaPasses = new RenderTarget[settings.passes.value]; //RenderTexture[] kuwaharaPasses = new RenderTexture[settings.passes.value];

                for (int i = 0; i < settings.passes.value; ++i)
                {
                    kuwaharaPasses[i] = new RenderTarget().Allocate($"kuwaharaPasses" + i);  ////kuwaharaPasses[i] = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                    kuwaharaPasses[i].Get(cmd, in desc, filterMode);
                }
                //Graphics.Blit(source, kuwaharaPasses[0], baseMaterial);
                cmd.SetGlobalTexture("_MainTexA", source);
                Utils.Blit(cmd, source, kuwaharaPasses[0], baseMaterial, 4);

                for (int i = 1; i < settings.passes.value; ++i)
                {
                    //Graphics.Blit(kuwaharaPasses[i - 1], kuwaharaPasses[i], baseMaterial);
                    cmd.SetGlobalTexture("_MainTexA", kuwaharaPasses[i - 1].Id);
                    Utils.Blit(cmd, kuwaharaPasses[i - 1], kuwaharaPasses[i], baseMaterial, 4);
                }
                //Graphics.Blit(kuwaharaPasses[settings.passes.value - 1].value, destination);
                cmd.SetGlobalTexture("_MainTexA", kuwaharaPasses[settings.passes.value - 1].Id);
                cmd.SetGlobalTexture("_ColorBuffer", source);
                Utils.Blit(cmd, kuwaharaPasses[settings.passes.value - 1].Handle, dest, baseMaterial, 0);
            }
            else
            //GENERALIZED KAWAHARA

            //ANISOTROPIC KAWAHARA
            if (settings.KawaharaType.value == 2)
            {
                baseMaterial.SetInt("_KernelSize", settings.kernelSizeKAWAHARA.value);
                baseMaterial.SetInt("_N", 8);
                baseMaterial.SetFloat("_Q", settings.sharpness.value);
                baseMaterial.SetFloat("_Hardness", settings.hardness.value);
                baseMaterial.SetFloat("_Alpha", settings.alpha.value);
                baseMaterial.SetFloat("_ZeroCrossing", settings.zeroCrossing.value);
                baseMaterial.SetFloat("_Zeta", settings.useZeta.value ? settings.zeta.value : 2.0f / 2.0f / (settings.kernelSizeKAWAHARA.value / 2.0f));

                //RenderTarget structureTensor = new RenderTarget().Allocate($"structureTensor");
                if (structureTensor == null)
                {
                    structureTensor = new RenderTarget().Allocate($"structureTensor");
                }
                if (structureTensor.Handle.rt == null)
                {
                    structureTensor.Get(cmd, in desc, filterMode);
                }
                //var structureTensor = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                cmd.SetGlobalTexture("_MainTexA", source);
                Utils.Blit(cmd, source, structureTensor, baseMaterial, 5);
                //Graphics.Blit(source, structureTensor, baseMaterial, 0);

                //RenderTarget eigenvectors1 = new RenderTarget().Allocate($"eigenvectors1");
                if (eigenvectors1 == null)
                {
                    eigenvectors1 = new RenderTarget().Allocate($"eigenvectors1");
                }
                if (eigenvectors1.Handle.rt == null)
                {
                    eigenvectors1.Get(cmd, in desc, filterMode);
                }
                //var eigenvectors1 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                cmd.SetGlobalTexture("_MainTexA", structureTensor.Id);
                Utils.Blit(cmd, structureTensor, eigenvectors1, baseMaterial, 6);
                //Graphics.Blit(structureTensor, eigenvectors1, baseMaterial, 1);

                //RenderTarget eigenvectors2 = new RenderTarget().Allocate($"eigenvectors2");
                if (eigenvectors2 == null)
                {
                    eigenvectors2 = new RenderTarget().Allocate($"eigenvectors2");
                    //Debug.Log("EIGEN 2");
                }
                if (eigenvectors2.Handle.rt == null)
                {
                    eigenvectors2.Get(cmd, in desc, filterMode);
                }
                //var eigenvectors2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                cmd.SetGlobalTexture("_MainTexA", eigenvectors1.Id);
                //baseMaterial.SetTexture("_MainTexA", eigenvectors1.Handle.rt);
                Utils.Blit(cmd, eigenvectors1, eigenvectors2, baseMaterial, 7);
                //Graphics.Blit(eigenvectors1, eigenvectors2, baseMaterial, 2);

                cmd.SetGlobalTexture("_TFM", eigenvectors2.Id);
                //baseMaterial.SetTexture("_TFM", eigenvectors2);

                //////TEST
                //cmd.SetGlobalTexture("_MainTexA", eigenvectors2.Id);
                //Utils.Blit(cmd, eigenvectors2.Handle, dest, baseMaterial, 0);
                //return;
                if (kuwaharaPasses == null || kuwaharaPasses.Length != settings.passes.value)
                {
                    kuwaharaPasses = new RenderTarget[settings.passes.value];// RenderTexture[] kuwaharaPasses = new RenderTexture[settings.passes.value];

                    for (int i = 0; i < settings.passes.value; ++i)
                    {
                        //kuwaharaPasses[i] = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                        kuwaharaPasses[i] = new RenderTarget().Allocate($"kuwaharaAnisoPasses" + i);
                        //kuwaharaPasses[i].Get(cmd, in desc, filterMode);
                    }
                }

                for (int i = 0; i < settings.passes.value; ++i)
                {
                    //kuwaharaPasses[i] = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                    //kuwaharaPasses[i] = new RenderTarget().Allocate($"kuwaharaAnisoPasses" + i);
                    if (kuwaharaPasses[i].Handle.rt == null)
                    {
                        kuwaharaPasses[i].Get(cmd, in desc, filterMode);
                    }
                }

                cmd.SetGlobalTexture("_MainTexA", source);
                Utils.Blit(cmd, source, kuwaharaPasses[0], baseMaterial, 8);
                //Graphics.Blit(source, kuwaharaPasses[0], baseMaterial, 3);

                for (int i = 1; i < settings.passes.value; ++i)
                {
                    //Graphics.Blit(kuwaharaPasses[i - 1], kuwaharaPasses[i], baseMaterial, 3);
                    cmd.SetGlobalTexture("_MainTexA", kuwaharaPasses[i - 1].Id);
                    Utils.Blit(cmd, kuwaharaPasses[i - 1], kuwaharaPasses[i], baseMaterial, 8);
                }
                cmd.SetGlobalTexture("_MainTexA", kuwaharaPasses[settings.passes.value - 1].Id);
                cmd.SetGlobalTexture("_ColorBuffer", source);
                Utils.Blit(cmd, kuwaharaPasses[settings.passes.value - 1].Handle, dest, baseMaterial, 0);
                //Graphics.Blit(kuwaharaPasses[settings.passes - 1], destination);
            }
            else
            {
                cmd.SetGlobalTexture("_MainTexA", source);
                Utils.Blit(cmd, source, dest, baseMaterial, 9);
            }
            //ANISOTROPIC KAWAHARA
            //cmd.SetGlobalTexture("_MainTexA", source);
            //Utils.Blit(cmd, source, dest, baseMaterial, 8);

            //Debug.Log(baseMaterial.shader.name);
            //baseMaterial.SetInt("_kernelSizeKAWAHARA", settings.kernelSizeKAWAHARA.value);
            //     cmd.SetGlobalTexture("_MainTexA", source);
            //baseMaterial.SetTexture("_MainTexA", source.rt);
            //    Utils.Blit(cmd, source, dest, baseMaterial, 3);
            //Utils.Blit(cmd, source, dest, baseMaterialA, 1);
            //Utils.JustBlit(cmd, source.rt, dest.rt);
            // return;


            /*
            for (int i = 0; i < settings.passes.value; ++i)
            {
                //kuwaharaPasses[i] = RenderTexture.GetTemporary(desc.width, desc.height, 0, desc.colorFormat);
                kuwaharaPasses[i] = new RenderTarget().Allocate($"kuwaharaPasses"+i);
                kuwaharaPasses[i].Get(cmd, in desc, filterMode);
            }
            cmd.SetGlobalTexture("_MainTexA",source);
            //Utils.BlitRTA(cmd, source, kuwaharaPasses[0].Handle, baseMaterial, 3);
            //Utils.JustBlitB(cmd, source.rt, kuwaharaPasses[0].Handle.rt, baseMaterial, 3);
            Utils.Blit(cmd, source, kuwaharaPasses[0], baseMaterial, 3);
            //Graphics.Blit(source, kuwaharaPasses[0], baseMaterial);
    
    //TESTER
           // cmd.SetGlobalTexture("_MainTexA", kuwaharaPasses[0].Id);
            //        Utils.Blit(cmd, kuwaharaPasses[0].Handle, dest, baseMaterial,0);//
            //Utils.BlitNoMat(cmd, kuwaharaPasses[0].Handle, dest);
           // return;

            for (int i = 1; i < settings.passes.value; ++i)
            {
                cmd.SetGlobalTexture("_MainTexA", kuwaharaPasses[i - 1].Id);
                //Graphics.Blit(kuwaharaPasses[i - 1], kuwaharaPasses[i], baseMaterial);
                //Utils.JustBlitB(cmd, kuwaharaPasses[i - 1].Handle.rt, kuwaharaPasses[0].Handle.rt, baseMaterial, 3);
                Utils.Blit(cmd, kuwaharaPasses[i - 1], kuwaharaPasses[i], baseMaterial, 3);
            }
            cmd.SetGlobalTexture("_MainTexA", kuwaharaPasses[settings.passes.value - 1].Id);
            //Utils.JustBlitA(cmd, kuwaharaPasses[settings.passes.value - 1].Handle, dest);
            //Utils.JustBlitA(cmd, kuwaharaPasses[settings.passes.value - 1].Handle.rt, dest.rt);
            Utils.Blit(cmd, kuwaharaPasses[settings.passes.value - 1].Handle, dest, baseMaterial, 0);


            // Graphics.Blit(kuwaharaPasses[settings.passes.value - 1], dest);
            for (int i = 0; i < settings.passes.value; ++i)
            {
                //RenderTexture.ReleaseTemporary(kuwaharaPasses[i].Handle);
            }
            //END KAWAHARA
            //Debug.Log(baseMaterial.name);
            //Debug.Log(baseMaterial.shader.name);
           // cmd.SetGlobalTexture("_MainTexA", kuwaharaPasses[0].Id);
           // Utils.Blit(cmd, source, dest, baseMaterial, 3);
           */




            //var materialSSSS = new Material(Resources.Load<Shader>("SunShaftsSRP_FORWARD_URP_RG"));

            ////Debug.Log(settings.m_Intencity.value);
            //if (settings.m_Intencity.value > 0.65f)
            //{
            //    //Utils.Blit(cmd, source, dest, materialSSSS, 2);
            //    RenderShafts(context, renderingData, cmd, desc, materialSSSS, source, dest, isRG);
            //}
            //else
            //{
            //    RenderShafts(context, renderingData, cmd, desc, materialSSSS, source, dest, isRG);
            //    //Utils.JustBlit(cmd, source.rt, dest.rt);
            //}

            _sampler.End(cmd);
        }

        public override void Cleanup(CommandBuffer cmd)
        {
            foreach (var rt in _mipDown)
                rt.Release(cmd);

            foreach (var rt in _mipUp)
                rt.Release(cmd);


            //AUTO
            _rtTMP.Release(cmd);
            lrColorB.Release(cmd);
            m_TemporaryColorTexture.Release(cmd);
            lrDepthBuffer.Release(cmd);
            tmpBuffer.Release(cmd);
        }
    }
}