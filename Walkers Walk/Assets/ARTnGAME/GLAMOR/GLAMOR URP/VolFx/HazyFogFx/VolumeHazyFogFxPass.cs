using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//https://github.com/microsoft/InsiderDevTour18-MR/blob/master/Assets/3D%20Models/PostProcessing/Runtime/Components/EyeAdaptationComponent.cs#L3
//https://github.com/Unity-Technologies/FPSSample/blob/master/Packages/com.unity.postprocessing/PostProcessing/Runtime/Effects/AutoExposure.cs

namespace Artngame.GLAMOR.VolFx
{
    [ShaderName("Unlit/VolumeFogFx_URP_RG_FBM")]
    public class VolumeHazyFogFxPass : VolFxProc.Pass
    {       
        //bool m_FirstFrame = true;

        public void ResetHistory()
        {
           /// m_FirstFrame = true;
        }

        //SMSS
        const int kMaxIterations = 16;
        RenderTarget[] _blurBuffer1 = new RenderTarget[kMaxIterations];
        RenderTarget[] _blurBuffer2 = new RenderTarget[kMaxIterations];

        int[] _blurBuffer1Width = new int[kMaxIterations];
        int[] _blurBuffer1Height = new int[kMaxIterations];
        float LinearToGamma(float x)
        {
#if UNITY_5_3_OR_NEWER
            return Mathf.LinearToGammaSpace(x);
#else
            if (x <= 0.0031308f)
                return 12.92f * x;
            else
                return 1.055f * Mathf.Pow(x, 1 / 2.4f) - 0.055f;
#endif
        }
        float GammaToLinear(float x)
        {
#if UNITY_5_3_OR_NEWER
            return Mathf.GammaToLinearSpace(x);
#else
            if (x <= 0.04045f)
                return x / 12.92f;
            else
                return Mathf.Pow((x + 0.055f) / 1.055f, 2.4f);
#endif
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
        private RenderTarget tmpBuffer2a;   
        private RenderTarget tmpBufferB;
        private RenderTarget prefiltered;

        private RenderTarget last;
        private RenderTarget basetex;

        private static readonly int s_ValueTex  = Shader.PropertyToID("_ValueTex");
        private static readonly int s_ColorTex  = Shader.PropertyToID("_ColorTex");
        private static readonly int s_Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int s_EyeAdaptTex = Shader.PropertyToID("_EyeAdaptTex");
        private static readonly int s_DownTex   = Shader.PropertyToID("_DownTex");
        private static readonly int s_Blend     = Shader.PropertyToID("_Blend");
                
        private float            _time;        
        private ProfilingSampler _sampler;
        private Texture2D        _valueTex;
        private Texture2D        _colorTex;       
        private float            _scatterLerp;

        //SUN SHAFTS
        public Vector4 cloudsSpeed = new Vector4(1,1,1,1);

        // =======================================================================
        public override void Init()
        { 
            //AUTO
            _rtTMP = new RenderTarget().Allocate($"_rtTMP");

            tmpBuffer2a = new RenderTarget().Allocate($"tmpBuffer2a");
            tmpBufferB = new RenderTarget().Allocate($"tmpBufferB");
            prefiltered = new RenderTarget().Allocate($"prefiltered");
            lrColorB = new RenderTarget().Allocate($"lrColorB");
            last = new RenderTarget().Allocate($"last");
            basetex = new RenderTarget().Allocate($"basetex");

            for (int i = 0; i < kMaxIterations; i++)
            {
                _blurBuffer1[i] = new RenderTarget().Allocate($"_blurBuffer1" + i);
            }
            for (int i = 0; i < kMaxIterations; i++)
            {
                _blurBuffer2[i] = new RenderTarget().Allocate($"_blurBuffer2" + i);
            }

            _sampler = new ProfilingSampler(name);            
            _validateMaterial();
        }

        public override bool Validate(Material mat)
        {
            //var settings = Stack.GetComponent<VolumeHazyFogFxVol>();
            if (settings == null)
            {
                settings = Stack.GetComponent<VolumeHazyFogFxVol>();
            }

            if (settings.IsActive() == false)
                return false;
            
            _time += Time.deltaTime;            
            //mat.SetTexture(s_ValueTex, settings.m_Threshold.value.GetTexture(ref _valueTex));
           // mat.SetTexture(s_ColorTex, settings.m_Color.value.GetTexture(ref _colorTex));
            //_intensity = settings.m_Intencity.value * Mathf.Lerp(1, _flicker.Evaluate(_time / _flickerPeriod), settings.m_Flicker.value);          
            //cloudsSpeed = settings.noiseCloudSpeed.value;
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
                tmpBuffer2a = new RenderTarget().Allocate($"tmpBuffer2a");
                tmpBufferB = new RenderTarget().Allocate($"tmpBufferB");
                prefiltered = new RenderTarget().Allocate($"prefiltered");

                last = new RenderTarget().Allocate($"last");
                basetex = new RenderTarget().Allocate($"basetex");

                for (int i = 0; i < kMaxIterations; i++)
                {
                    _blurBuffer1[i] = new RenderTarget().Allocate($"_blurBuffer1" + i);
                }
                for (int i = 0; i < kMaxIterations; i++)
                {
                    _blurBuffer2[i] = new RenderTarget().Allocate($"_blurBuffer2" + i);
                }

            }
            _validateMaterial();
        }

        private void _validateMaterial()
        {
            if (_material != null)
            {
                //_material.DisableKeyword("_BRIGHTNESS");
                
                //_material.EnableKeyword(_mode switch
                //{
                //    ValueMode.Luma       => "_LUMA",
                //    ValueMode.Brightness => "_BRIGHTNESS",
                //    _                    => throw new ArgumentOutOfRangeException()
                //});

                //if (_EyeAdaptOnly)
                //    _material.EnableKeyword("_EyeAdapt_ONLY");
            }
        }

        VolumeHazyFogFxVol settings;

     

        //SUN SHAFTS
        public void RenderShafts(ScriptableRenderContext context, 
            UnityEngine.Rendering.Universal.RenderingData renderingData, 
            CommandBuffer cmd, 
            RenderTextureDescriptor opaqueDesc,
            Material blitMaterial, RTHandle source, RTHandle destination)
        {
            if (settings == null)
            {
                settings = Stack.GetComponent<VolumeHazyFogFxVol>();
            }

            //VOLUME FOG
            var _material = blitMaterial;//context.propertySheets.Get(Shader.Find("Hidden/InverseProjectVFogLWRP"));
            _material.SetFloat("_DistanceOffset", settings._startDistance.value);
            _material.SetFloat("_Height", settings._fogHeight.value); //v0.1                                                                      
            _material.SetFloat("_cameraRoll", settings._cameraRoll.value);
            _material.SetVector("_cameraDiff", settings._cameraDiff.value);
            _material.SetFloat("_cameraTiltSign", settings._cameraTiltSign.value);

            var mode = RenderSettings.fogMode;
            if (mode == FogMode.Linear)
            {
                var start = RenderSettings.fogStartDistance;
                var end = RenderSettings.fogEndDistance;
                var invDiff = 1.0f / Mathf.Max(end - start, 1.0e-6f);
                _material.SetFloat("_LinearGrad", -invDiff);
                _material.SetFloat("_LinearOffs", end * invDiff);
                _material.DisableKeyword("FOG_EXP");
                _material.DisableKeyword("FOG_EXP2");
            }
            else if (mode == FogMode.Exponential)
            {
                const float coeff = 1.4426950408f; // 1/ln(2)
                var density = RenderSettings.fogDensity;
                _material.SetFloat("_Density", coeff * density * settings._fogDensity.value);
                _material.EnableKeyword("FOG_EXP");
                _material.DisableKeyword("FOG_EXP2");
            }
            else // FogMode.ExponentialSquared
            {
                const float coeff = 1.2011224087f; // 1/sqrt(ln(2))
                var density = RenderSettings.fogDensity;
                _material.SetFloat("_Density", coeff * density * settings._fogDensity.value);
                _material.DisableKeyword("FOG_EXP");
                _material.EnableKeyword("FOG_EXP2");
            }
            if (settings._useRadialDistance.value)
            {
                _material.EnableKeyword("RADIAL_DIST");
            }
            else
            {
                _material.DisableKeyword("RADIAL_DIST");
            }

            if (settings._fadeToSkybox.value)
            {
                _material.DisableKeyword("USE_SKYBOX");
                _material.SetColor("_FogColor", settings._FogColor.value);// RenderSettings.fogColor);//v0.1            
            }
            else
            {
                _material.DisableKeyword("USE_SKYBOX");
                _material.SetColor("_FogColor", settings._FogColor.value);// RenderSettings.fogColor);
            }

            //v0.1
            if (settings.noiseTexture.value == null)
            {
                settings.noiseTexture.value = new Texture2D(1280, 720);
            }
            if (_material != null && settings.noiseTexture != null)
            {
                if (settings.noiseTexture.value == null)
                {
                    settings.noiseTexture.value = new Texture2D(1280, 720);
                }
                _material.SetTexture("_NoiseTex", settings.noiseTexture.value);
            }

            // Calculate vectors towards frustum corners.
            Camera camera = Camera.main;
            var cam = camera;
            var camtr = cam.transform;

            ////////// SCATTER
            var camPos = camtr.position;
            float FdotC = camPos.y - settings._fogHeight.value;
            float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
            //_material.properties.SetMatrix("_FrustumCornersWS", frustumCorners);
            _material.SetVector("_CameraWS", camPos);
            _material.SetVector("_HeightParams", new Vector4(settings._fogHeight.value, FdotC, paramK, settings.heightDensity.value * 0.5f));
            _material.SetVector("_DistanceParams", new Vector4(-Mathf.Max(settings.startDistance.value, 0.0f), 0, 0, 0));
            _material.SetFloat("_NoiseDensity", settings.noiseDensity.value);
            _material.SetFloat("_NoiseScale", settings.noiseScale.value);
            _material.SetFloat("_NoiseThickness", settings.noiseThickness.value);
            _material.SetVector("_NoiseSpeed", settings.noiseSpeed.value);
            _material.SetFloat("_OcclusionDrop", settings.occlusionDrop.value);
            _material.SetFloat("_OcclusionExp", settings.occlusionExp.value);
            _material.SetInt("noise3D", settings.noise3D.value);
            //SM v1.7
            _material.SetFloat("luminance", settings.luminance.value);
            _material.SetFloat("lumFac", settings.lumFac.value);
            _material.SetFloat("Multiplier1", settings.ScatterFac.value);
            _material.SetFloat("Multiplier2", settings.TurbFac.value);
            _material.SetFloat("Multiplier3", settings.HorizFac.value);
            _material.SetFloat("turbidity", settings.turbidity.value);
            _material.SetFloat("reileigh", settings.reileigh.value);
            _material.SetFloat("mieCoefficient", settings.mieCoefficient.value);
            _material.SetFloat("mieDirectionalG", settings.mieDirectionalG.value);
            _material.SetFloat("bias", settings.bias.value);
            _material.SetFloat("contrast", settings.contrast.value);
            _material.SetVector("v3LightDir", settings.Sun.value);//.forward);
            _material.SetVector("_TintColor", new Vector4(settings.TintColor.value.r, settings.TintColor.value.g, settings.TintColor.value.b, 1));//68, 155, 345

            float Foggy = 0;
            if (settings.FogSky.value) //ClearSkyFac
            {
                Foggy = 1;
            }
            _material.SetFloat("FogSky", Foggy);
            _material.SetFloat("ClearSkyFac", settings.ClearSkyFac.value);
            //////// END SCATTER

            //LOCAL LIGHT
            _material.SetVector("localLightPos", new Vector4(settings.PointL.value.x, settings.PointL.value.y, settings.PointL.value.z, settings.PointL.value.w));//68, 155, 345
            _material.SetVector("localLightColor", new Vector4(settings.PointLParams.value.x, settings.PointLParams.value.y, settings.PointLParams.value.z, settings.PointLParams.value.w));//68, 155, 345
            //END LOCAL LIGHT

            //RENDER FINAL EFFECT
            var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
                     
            tmpBufferB.Get(cmd, in opaqueDesc, FilterMode.Trilinear);

            GL.ClearWithSkybox(false, camera);
            _material.SetTexture("_MainTexA", source.rt);
            Utils.Blit(cmd, source, tmpBufferB, _material, 5);


            _material.SetTexture("_MainTexA", tmpBufferB.Handle.rt);

            //WORLD RECONSTRUCT        
            Matrix4x4 camToWorld = Camera.main.cameraToWorldMatrix;
            _material.SetMatrix("_InverseView", camToWorld);

            if (_material != null && settings.noiseTexture != null)
            {
                if (settings.noiseTexture.value == null)
                {
                    settings.noiseTexture.value = new Texture2D(1280, 720);
                }
                cmd.SetGlobalTexture("_NoiseTex", settings.noiseTexture.value);
            }
            cmd.SetGlobalVector("_TintColor", new Vector4(settings.TintColor.value.r, settings.TintColor.value.g, settings.TintColor.value.b, 1));
            cmd.SetGlobalTexture("_MainTexB", source);

            _material.SetFloat("_Blend", settings.m_Intencity.value);

            if (settings.enableWetnessHaze.value)
            {
                //v0.1
                //cmd.Blit(tmpBuffer2a, tmpBuffer2a, _material, 6);//SSMS          
                //Shader.SetGlobalTexture("_FogTex", tmpBuffer2a);
                //_material.SetTexture("_BaseTex", tmpBuffer1);//SSMS
                //cmd.Blit(tmpBuffer1, tmpBuffer2, _material, 19);//SSMS
                if (tmpBuffer2a.Handle.rt == null)
                {
                    tmpBuffer2a.Get(cmd, in opaqueDesc, FilterMode.Trilinear);
                }
                if (lrColorB.Handle.rt == null)
                {
                    lrColorB.Get(cmd, in opaqueDesc, FilterMode.Trilinear);
                }
                

                cmd.SetGlobalTexture("_MainTexB", tmpBuffer2a.Id);
                //dont pass the background image, only fog, then set to global
                Utils.Blit(cmd, source, tmpBuffer2a, _material, 6);
                cmd.SetGlobalTexture("_FogTex", tmpBuffer2a.Id);//
                //_material.SetTexture("_FogTex", tmpBuffer2a.Handle.rt);
                cmd.SetGlobalTexture("_BaseTex", source);               
                Utils.Blit(cmd, tmpBuffer2a, lrColorB, _material, 17);
                //Utils.JustBlitA(cmd,tmpBuffer2a.Handle, destination);
            }
            else
            {
                Utils.Blit(cmd, source, destination, _material, 6);
            }

        }

        //SMSS
        public void renderSSMS(
                ScriptableRenderContext context,
                UnityEngine.Rendering.Universal.RenderingData renderingData,
                CommandBuffer cmd,
                RenderTextureDescriptor opaqueDesc,
                Material blitMaterial, RTHandle source, RTHandle destination
        )
        {
            Material _material = blitMaterial;

            //cmd.SetGlobalTexture("_FogTex", tmpBuffer2a.Id);
            //cmd.SetGlobalTexture("_BaseTex", lrColorB.Id);
            //Utils.Blit(cmd, tmpBuffer2a, destination, _material, 17);
            //return;

            //CAMERA Calculate vectors towards frustum corners.
            Camera camera = Camera.main;
            //if (isForReflections && reflectCamera != null)
            //{
            //    // camera = reflectionc UnityEngine.Rendering.Universal.RenderingData.ca
            //    // ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData
            //    camera = reflectCamera;
            //}
            //if (isForReflections && isForDualCameras) //v1.9.9.7 - Ethereal v1.1.8f
            //{
            //    //if list has members, choose 0 for 1st etc
            //    if (extraCameras.Count > 0 && extraCameraID >= 0 && extraCameraID < extraCameras.Count)
            //    {
            //        camera = extraCameras[extraCameraID];
            //    }
            //}
            //v1.7.1 - Solve editor flickering
            if (Camera.current != null)
            {
                camera = Camera.current;
            }

            //RENDER FINAL EFFECT
            int rtW = opaqueDesc.width;
            int rtH = opaqueDesc.height;
            var format = settings.allowHDR.value ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; 
            //RenderTexture tmpBuffer1 = RenderTexture.GetTemporary(rtW, rtH, 0, format);  //////////////////////////////////////////
            //RenderTexture.active = tmpBuffer1;  //////////////////////////////////////////
            //GL.ClearWithSkybox(false, camera);  //////////////////////////////////////////
            ////v0.1
            //cmd.Blit(source, tmpBuffer1); //v0.1  //////////////////////////////////////////

            var useRGBM = Application.isMobilePlatform;

            // source texture size
            var tw = rtW;// source.width;
            var th = rtH;// source.height;

            // halve the texture size for the low quality mode
            if (!settings._highQuality.value)
            {
                tw /= 2;
                th /= 2;
            }

            // blur buffer format
            var rtFormat = useRGBM ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;

            // determine the iteration count
            var logh = Mathf.Log(th, 2) + settings._radius.value - 8;
            var logh_i = (int)logh;
            var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);

            // update the shader properties
            var lthresh = settings.thresholdLinear.value;
            _material.SetFloat("_Threshold", lthresh);

            var knee = lthresh * settings._softKnee.value + 1e-5f;
            var curve = new Vector3(lthresh - knee, knee * 2, 0.25f / knee);
            _material.SetVector("_Curve", curve);

            var pfo = !settings._highQuality.value && settings._antiFlicker.value;
            _material.SetFloat("_PrefilterOffs", pfo ? -0.5f : 0.0f);

            _material.SetFloat("_SampleScale", 0.5f + logh - logh_i);
            _material.SetVector("_Intensity", settings._intensity.value);

            _material.SetTexture("_FadeTex", settings._fadeRamp.value);
            _material.SetFloat("_BlurWeight", settings._blurWeight.value);
            _material.SetFloat("_Radius", settings._radius.value);
            _material.SetColor("_BlurTint", settings._blurTint.value);

            // prefilter pass
            //var prefiltered = RenderTexture.GetTemporary(tw, th, 0, rtFormat);  //////////////////////////////////////////
            //if (prefiltered.Handle.rt == null)
            //{
                prefiltered.Get(cmd, in opaqueDesc, FilterMode.Bilinear);
            //}

            int offset = 8;
            var pass = settings._antiFlicker.value ? 1 + offset : 0 + offset;

            //TESTER
            //cmd.SetGlobalTexture("_FogTex", tmpBuffer2a.Id);
            //cmd.SetGlobalTexture("_BaseTex", lrColorB.Id);
            //Utils.Blit(cmd, tmpBuffer2a, destination, _material, 17);
            //return;




            //cmd.Blit(tmpBuffer1, prefiltered, _material, pass); //v0.5  //////////////////////////////////////////
            //_material.SetVector("_MainTexB_TexelSize", new Vector4(1 / opaqueDesc.width, 1 / opaqueDesc.height, opaqueDesc.width, opaqueDesc.height));
            cmd.SetGlobalTexture("_MainTexB", source);
            cmd.SetGlobalTexture("_FogTex", tmpBuffer2a.Id);
            Utils.Blit(cmd, source, prefiltered, _material, pass);


            //cmd.SetGlobalTexture("_BaseTex", prefiltered.Id);
            //Utils.Blit(cmd, prefiltered, destination, _material, 18);
            //cmd.SetGlobalTexture("_BaseTex", prefiltered.Id);
            //Utils.Blit(cmd, prefiltered, destination, _material, 18);
            //return;


            // construct a mip pyramid
            // RenderTarget last = prefiltered;
            last.Get(cmd, in opaqueDesc, FilterMode.Bilinear);
            cmd.SetGlobalTexture("_BaseTex", prefiltered.Id);
           // _material.SetVector("_BaseTex_TexelSize", new Vector4(1 / opaqueDesc.width, 1 / opaqueDesc.height, opaqueDesc.width, opaqueDesc.height));
            Utils.Blit(cmd, prefiltered, last, _material, 18);

            //cmd.SetGlobalTexture("_BaseTex", last.Id);
            //Utils.Blit(cmd, last, destination, _material, 18);//
            //return;

            RenderTextureDescriptor descB = opaqueDesc;
            descB.colorFormat = rtFormat;

            //cmd.SetGlobalTexture("_BaseTex", last.Id);
            //Utils.Blit(cmd, last, destination, _material, 18);
            //return;

            for (var level = 0; level < iterations; level++)
            {
                int lastW = descB.width;
                int lastH = descB.height;
                descB.width = descB.width / 2;
                descB.height = descB.height / 2;
                _blurBuffer1Width[level] = descB.width;
                _blurBuffer1Height[level] = descB.height;
                // descB.graphicsFormat = GraphicsFormat.
                //descB.colorFormat = RenderTextureFormat.
                //descB.
                 //_blurBuffer1[level] = RenderTexture.GetTemporary( last.width / 2, last.height / 2, 0, rtFormat);
                 _blurBuffer1[level].Get(cmd, in descB, FilterMode.Bilinear);

                pass = (level == 0) ? (settings._antiFlicker.value ? 3 + offset : 2 + offset) : 4 + offset;

                //cmd.Blit(last, _blurBuffer1[level], _material, pass); //v2.1 Graphics.Blit to cmd.Blit fix flicker in editor time//////////////////////////////////////////
                cmd.SetGlobalTexture("_MainTexB", last.Id);
                _material.SetVector("_MainTexB_TexelSize", new Vector4(1 / lastW, 1 / lastH, lastW, lastH));
                //  _material.SetVector("_MainTexB_TexelSize", new Vector4(1 / (descB.width*2), 1 / (descB.height * 2), descB.width * 2, descB.height * 2));
                Utils.Blit(cmd, last, _blurBuffer1[level], _material, pass);

                //last = _blurBuffer1[level];
               // _material.SetVector("_BaseTex_TexelSize", new Vector4(1 / descB.width, 1 / descB.height, descB.width, descB.height));
                cmd.SetGlobalTexture("_BaseTex", _blurBuffer1[level].Id);
                Utils.Blit(cmd, _blurBuffer1[level], last, _material, 18);
                
                //if(level>3)
               //     break;
            }

           // cmd.SetGlobalTexture("_BaseTex", last.Id);
          //  Utils.Blit(cmd, last, destination, _material, 18);
          // return;

            // upsample and combine loop
            for (var level = iterations - 2; level >= 0; level--)
            {
                descB.width = _blurBuffer1Width[level];// basetex.Handle.rt.width;
                descB.height = _blurBuffer1Height[level]; //basetex.Handle.rt.height;
                basetex.Get(cmd, in descB, FilterMode.Trilinear);
                //RenderTarget basetex = _blurBuffer1[level];
                cmd.SetGlobalTexture("_BaseTex", _blurBuffer1[level].Id);
               // _material.SetVector("_BaseTex_TexelSize", new Vector4(1 / descB.width, 1 / descB.height, descB.width, descB.height));
                Utils.Blit(cmd, _blurBuffer1[level], basetex, _material, 18);

                //_material.SetTexture("_BaseTexA", basetex); //USE another, otherwise BUGS//////////////////////////////////////////
                cmd.SetGlobalTexture("_BaseTexA", basetex.Id);
                //cmd.SetGlobalVector("_BaseTexA_TexelSize", new Vector4(1 / descB.width, 1 / descB.height, descB.width, descB.height));

                //_blurBuffer2[level] = RenderTexture.GetTemporary(basetex.width, basetex.height, 0, rtFormat);  //////////////////////////////////////////
                _blurBuffer2[level].Get(cmd, in descB, FilterMode.Bilinear);


                pass = settings._highQuality.value ? 6 + offset : 5 + offset;
                //cmd.Blit(last, _blurBuffer2[level], _material, pass); //v2.1//////////////////////////////////////////
                cmd.SetGlobalTexture("_MainTexB", last.Id);
                _material.SetVector("_MainTexB_TexelSize", new Vector4(1 / _blurBuffer1Width[level], 1 / _blurBuffer1Height[level], _blurBuffer1Width[level], _blurBuffer1Height[level]));
                Utils.Blit(cmd, last, _blurBuffer2[level], _material, pass);

                //last = _blurBuffer2[level];
                cmd.SetGlobalTexture("_BaseTex", _blurBuffer2[level].Id);
                Utils.Blit(cmd, _blurBuffer2[level], last, _material, 18);
            }

            //cmd.SetGlobalTexture("_BaseTex", last.Id);
            //Utils.Blit(cmd, last, destination, _material, 18);
            //return;

            // finish process
            //_material.SetTexture("_BaseTexA", tmpBuffer1);  //////////////////////////////////////////
            cmd.SetGlobalTexture("_BaseTexA", source);

            pass = settings._highQuality.value ? 8 + offset : 7 + offset;
            //_material.SetTexture("_MainTexA", last);  //////////////////////////////////////////
            cmd.SetGlobalTexture("_MainTexA", last.Id);
            //cmd.SetGlobalVector("_MainTexA_TexelSize", new Vector4(1 / (opaqueDesc.width ), 1 / (opaqueDesc.height ), opaqueDesc.width , opaqueDesc.height ));
            _material.SetVector("_MainTexA_TexelSize", new Vector4(1 / (_blurBuffer1Width[0]), 1 / (_blurBuffer1Height[0]), opaqueDesc.width, opaqueDesc.height));

            //v0.1
            //cmd.Blit(last, source, _material, pass);  //////////////////////////////////////////
            cmd.SetGlobalTexture("_MainTexB", last.Id);
            cmd.SetGlobalTexture("_FogTex", tmpBuffer2a.Id);
            Utils.Blit(cmd, last, destination, _material, pass);

            /*
            context.ExecuteCommandBuffer(cmd);
            // release the temporary buffers
            for (var i = 0; i < kMaxIterations; i++)
            {
                if (_blurBuffer1[i] != null)
                    RenderTexture.ReleaseTemporary(_blurBuffer1[i]);

                if (_blurBuffer2[i] != null)
                    RenderTexture.ReleaseTemporary(_blurBuffer2[i]);

                _blurBuffer1[i] = null;
                _blurBuffer2[i] = null;
            }
            RenderTexture.ReleaseTemporary(prefiltered);
            RenderTexture.ReleaseTemporary(tmpBuffer1);
            */
        }
        //END SSMS

        Material materialSSSS;

        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData
            , RenderTextureDescriptor descA, bool isRG)
        {
            _sampler.Begin(cmd);

            RenderTextureDescriptor desc;
            if (isRG)
            {
                desc = descA;
            }
            else
            {
                desc = renderingData.cameraData.cameraTargetDescriptor;
            }

            desc.colorFormat        = RenderTextureFormat.ARGBFloat;
            desc.depthStencilFormat = GraphicsFormat.None;

            materialSSSS = settings.fogMaterial.value;
            if (materialSSSS == null)
            {
                materialSSSS = Resources.Load<Material>("VolumeHazyFogFx_URP_RG_FBM");
            }
            //materialSSSS = new Material(materialSSSS);//make unique

            if (materialSSSS == null)
            {
                materialSSSS = new Material(Resources.Load<Shader>("VolumeHazyFogFx_URP_RG_FBM"));
            }

            //materialSSSS.SetTexture("_MainTexB", source.rt); 
            //cmd.SetGlobalTexture("_MainTexB", source);
            //Utils.Blit(cmd, source, dest, materialSSSS, 2);
            RenderShafts(context, renderingData, cmd, desc, materialSSSS, source, dest);

            if (settings.enableWetnessHaze.value)
            {
                renderSSMS(context, renderingData, cmd, desc, materialSSSS, source, dest);
            }
            //Utils.JustBlit(cmd, source.rt, dest);
            //cmd.SetGlobalTexture("_MainTexB", source);
            // Utils.Blit(cmd, source, dest, materialSSSS, 5);
            //return;

            //END            

            _sampler.End(cmd);
        }

        public override void Cleanup(CommandBuffer cmd)
        {
        }
    }
}