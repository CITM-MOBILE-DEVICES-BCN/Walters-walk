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
    public class VolumeFogFxPass : VolFxProc.Pass
    {       
        //bool m_FirstFrame = true;

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
        private RenderTarget tmpBufferB;

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

            tmpBufferB = new RenderTarget().Allocate($"tmpBufferB");

            _sampler = new ProfilingSampler(name);            
            _validateMaterial();
        }

        public override bool Validate(Material mat)
        {
            //var settings = Stack.GetComponent<VolumeFogFxVol>();
            if (settings == null)
            {
                settings = Stack.GetComponent<VolumeFogFxVol>();
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
                tmpBufferB = new RenderTarget().Allocate($"tmpBufferB");              
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

        VolumeFogFxVol settings;

        //SUN SHAFTS
        public void RenderShafts(ScriptableRenderContext context, 
            UnityEngine.Rendering.Universal.RenderingData renderingData, 
            CommandBuffer cmd, 
            RenderTextureDescriptor opaqueDesc,
            Material blitMaterial, RTHandle source, RTHandle destination)
        {
            if (settings == null)
            {
                settings = Stack.GetComponent<VolumeFogFxVol>();
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
            var cam = camera;// GetComponent<Camera>();
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



            //          RenderTexture tmpBuffer1 = RenderTexture.GetTemporary(context.width, context.height, 0, format);
            //          RenderTexture.active = tmpBuffer1;
            //format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
            //RenderTextureDescriptor descA1 = new RenderTextureDescriptor(rtW, rtH, format);          
            tmpBufferB.Get(cmd, in opaqueDesc);

            //RenderTexture.active = tmpBufferB.Handle;   

            GL.ClearWithSkybox(false, camera);
            //context.command.BlitFullscreenTriangle(context.source, tmpBuffer1);
            _material.SetTexture("_MainTexA", source.rt);
            //Utils.JustBlitB(cmd, source.rt, tmpBufferB.Handle.rt, _material, 5);
            Utils.Blit(cmd, source, tmpBufferB, _material, 5);


            _material.SetTexture("_MainTexA", tmpBufferB.Handle.rt);

            //WORLD RECONSTRUCT        
            Matrix4x4 camToWorld = Camera.main.cameraToWorldMatrix;// context.camera.cameraToWorldMatrix;
            _material.SetMatrix("_InverseView", camToWorld);

            //Debug.Log("PASS aa");
            //context.command.BlitFullscreenTriangle(context.source, context.destination, _material, 0);
            // Utils.Blit(cmd, source, destination, _material, 7);

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
            //Debug.Log(settings.m_Intencity.value);

            Utils.Blit(cmd, source, destination, _material, 6);

            // Utils.BlitRTA(cmd, source, destination, sheetSHAFTS, 0);

            //RenderTexture.ReleaseTemporary(tmpBuffer1);

            //END RENDER FINAL EFFECT

            //return;



            /*

            if (settings.useNoise.value)
            {
                blitMaterial = new Material(Resources.Load<Shader>("VolumeFogFx_URP_RG_FBM"));
                //Debug.Log(blitMaterial.shader.name);
                blitMaterial.SetTexture("_MainTexFBM", settings.MainTexFBM.value);
                blitMaterial.SetTexture("_Tex2", settings.NoiseTex2.value);
                blitMaterial.SetFloat("_Distort", settings.Distort.value);
                blitMaterial.SetColor("_HighLight", settings.HighLight.value); 
                blitMaterial.SetColor("_noiseColor", settings.NoiseColor.value);
                blitMaterial.SetFloat("_Pow", settings.noisePower.value);
                blitMaterial.SetVector("brightnessContrast", settings.brightnessContrast.value);
                blitMaterial.SetVector("cloudSpeed", cloudsSpeed);// settings.noiseCloudSpeed.value);
                //Debug.Log("cloud speed = " + cloudsSpeed); // settings.Distort.value);
                //Debug.Log("shader cloud speed = " + blitMaterial.GetVector("cloudSpeed"));
            }

            opaqueDesc.depthBufferBits = 0;
            Material sheetSHAFTS = blitMaterial;
            sheetSHAFTS.SetFloat("_Blend", settings.blend.value);
            //Camera camera = Camera.main;
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
    //      RenderTexture m_TemporaryColorTexture = RenderTexture.GetTemporary(opaqueDesc.width, opaqueDesc.height, 0, formatA);
    //      RenderTexture lrDepthBuffer = RenderTexture.GetTemporary(opaqueDesc.width, opaqueDesc.height, 0, formatA);            

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

            if (!settings.useDepthTexture.value && 1==0)
            {
                format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9

                RenderTextureDescriptor descA1 = new RenderTextureDescriptor(rtW, rtH, format);
 //             RenderTexture tmpBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                tmpBufferB.Get(cmd, in descA1);
                         
                RenderTexture.active = tmpBufferB.Handle.rt;
                GL.ClearWithSkybox(false, camera);
                sheetSHAFTS.SetTexture("_Skybox", tmpBufferB.Handle.rt);

                //////         cmd.Blit(source, lrDepthBuffer, sheetSHAFTS, 3);    /////////////
                //Utils.JustBlitA(cmd, source.rt, _rtTMP);
                sheetSHAFTS.SetTexture("_MainTexA", source.rt);
                Utils.JustBlitB(cmd, source.rt, lrDepthBuffer.Handle.rt, sheetSHAFTS, 3);
                //Utils.JustBlit(cmd, m_DebugHistogram, m_CurrentAutoExposure);
                //Utils.BlitRT(cmd, source, destination, blitMaterial, (int)settings.adaptationType.value);

                //              RenderTexture.ReleaseTemporary(tmpBufferB.Handle);                
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
                Matrix4x4 view = renderingData.cameraData.GetViewMatrix(eyeIndex);
                Matrix4x4 proj = renderingData.cameraData.GetProjectionMatrix(eyeIndex);
                cameraMatrices[eyeIndex] = proj * view;
            }    

            sheetSHAFTS.SetMatrixArray("_CameraVP", cameraMatrices);
            sheetSHAFTS.SetVector("_SunPosition", new Vector4(-settings.sunTransform.value.x * 1f + 0, -settings.sunTransform.value.y * 1f, -settings.sunTransform.value.z * 1f, settings.maxRadius.value));//
            
            for (int it2 = 0; it2 < settings.radialBlurIterations.value; it2++)
            {
                RenderTextureDescriptor descA11 = new RenderTextureDescriptor(rtW, rtH);
///             //lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0);
                lrColorB.Get(cmd, in descA11);

                cmd.SetGlobalTexture("_MainTexA", lrDepthBuffer.Id);
                Utils.JustBlitC(cmd, lrDepthBuffer, lrColorB, sheetSHAFTS, 1);
                //cmd.Blit(lrDepthBuffer, lrColorB, sheetSHAFTS, 1);//Blit(cmd, lrDepthBuffer, lrColorB, sheetSHAFTS, 1); //Blit(cmd, lrDepthBuffer.Identifier(), lrColorB, sheetSHAFTS, 1);//v0.1
   //           cmd.ReleaseTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name));//  lrDepthBuffer.id);//v0.1

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
                Utils.BlitRTA(cmd, source, destination, sheetSHAFTS, 0);
                //cmd.Blit(m_TemporaryColorTexture, source, sheetSHAFTS, 0);
            }
            else{
                cmd.SetGlobalTexture("_MainTexA", source);
                Utils.BlitRTA(cmd, source, destination, sheetSHAFTS, 4);//
                //cmd.Blit(m_TemporaryColorTexture, source, sheetSHAFTS, 4);
            }

            */

            //TESTER
            //cmd.SetGlobalTexture("_MainTexB", lrColorB.Id);    ////////////////////////// BASIC, need 1. not be _MainTex and 2. not use Handle.rt but ID !!!
            ////sheetSHAFTS.SetTexture("_MainTexB", lrDepthBuffer);
            //Utils.Blit(cmd, lrDepthBuffer, destination, sheetSHAFTS, 5);
            //return;

            // context.ExecuteCommandBuffer(cmd);
            // CommandBufferPool.Release(cmd);
        }

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

            desc.colorFormat        = RenderTextureFormat.ARGB32;
            desc.depthStencilFormat = GraphicsFormat.None;

            materialSSSS = settings.fogMaterial.value;
            if (materialSSSS == null)
            {
                materialSSSS = Resources.Load<Material>("VolumeFogFx_URP_RG_FBM");
            }
            //materialSSSS = new Material(materialSSSS);//make unique

            if (materialSSSS == null)
            {
                materialSSSS = new Material(Resources.Load<Shader>("VolumeFogFx_URP_RG_FBM"));
            }

            //materialSSSS.SetTexture("_MainTexB", source.rt); 
            //cmd.SetGlobalTexture("_MainTexB", source);
            //Utils.Blit(cmd, source, dest, materialSSSS, 2);
                    RenderShafts(context, renderingData, cmd, desc, materialSSSS, source, dest);

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