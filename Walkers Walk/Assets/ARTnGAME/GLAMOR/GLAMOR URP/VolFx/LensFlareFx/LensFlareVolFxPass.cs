using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.GLAMOR.VolFx
{
    [ShaderName("Hidden/VolFx/EyeAdaptGLAMOR")]
    public class LensFlareVolFxPass : VolFxProc.Pass
    {        
        //bool m_FirstFrame = true;     
        public void ResetHistory()
        {
            //m_FirstFrame = true;
        }       

        private RenderTarget _rtTMP;
                
        private float            _time;
        private float            _intensity;
        private ProfilingSampler _sampler;
        //private Texture2D        _valueTex;        
        //private RenderTarget[]   _mipDown;     

        public override void Init()
        {  
            //AUTO
            _rtTMP = new RenderTarget().Allocate($"_rtTMP");       

            //_mipDown = new RenderTarget[_samples];      
            //for (var n = 0; n < _samples; n++)
            //    _mipDown[n] = new RenderTarget().Allocate($"EyeAdapt_{name}_down_{n}");
            
            _sampler = new ProfilingSampler(name);            
            // _validateMaterial();
        }

        LensFlareVolFxVol settings;

        public override bool Validate(Material mat)
        {
            //var settings = Stack.GetComponent<LensFlareVolFxVol>();
            if (settings == null)
            {
                settings = Stack.GetComponent<LensFlareVolFxVol>();
            }

            if (settings.IsActive() == false)
                return false;
            
            //_time += Time.deltaTime;
            return true;
        }

        private void OnValidate()
        {
            if (Application.isPlaying == false)
            {
                _rtTMP = new RenderTarget().Allocate($"_rtTMP");               
                _validateMaterial();
            }
        }
        void _validateMaterial()
        {
            //if (_material != null)          
            //    if (_EyeAdaptOnly)
            //        _material.EnableKeyword("_EyeAdapt_ONLY");
            //}
        }      

        Material material;
        // RenderTarget structureTensor;
        // RenderTarget[] kuwaharaPasses;

        RenderTarget downsampled;
        RenderTarget ghosts;
        RenderTarget radialWarped;
        RenderTarget added;
        RenderTarget aberration;
        RenderTarget blur;
        RenderTarget blur1;

        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData
            , RenderTextureDescriptor descA, bool isRG)
        {
            _sampler.Begin(cmd);

            if (settings == null)
            {
                settings = Stack.GetComponent<LensFlareVolFxVol>();
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
            //Material material = Resources.Load<Material>("PainterlyVolFx_URP_RG_FBM");
            if (material == null)
            {
                material = Resources.Load<Material>("AllFlarePassesVolFx");
            }
            if (material == null)
            {
                material = new Material(Resources.Load<Shader>("AllFlarePassesVolFx"));
            }

            //PAINTING           
            FilterMode filterMode = FilterMode.Bilinear;

            //material = new Material(Shader.Find("Hidden/SubMul"));
            //ghostMaterial = new Material(Shader.Find("Hidden/GhostFeature"));
            //radialWarpMaterial = new Material(Shader.Find("Hidden/RadialWarp"));
            //additiveMaterial = new Material(Shader.Find("Hidden/Additive"));
            //aberrationMaterial = new Material(Shader.Find("Hidden/ChromaticAberration"));
            //blurMaterial = new Material(Shader.Find("Hidden/GaussianBlur"));

            //RenderTarget downsampled;
            //RenderTarget ghosts;
            //RenderTarget radialWarped;
            //RenderTarget added;
            //RenderTexture aberration;
            //RenderTexture blur;
            //RenderTexture blur1;

            RenderTextureDescriptor descDOWN = desc;
            descDOWN.width = desc.width >> settings.Downsample.value; //Screen.width >> settings.Downsample.value
            descDOWN.height = desc.height >> settings.Downsample.value;

            if (downsampled == null)
            {
                downsampled = new RenderTarget().Allocate($"downsampled");
                downsampled.Get(cmd, in descDOWN, filterMode);
            }
            if (ghosts == null)
            {
                descDOWN.width = desc.width >> settings.Downsample.value; //Screen.width >> settings.Downsample.value
                descDOWN.height = desc.height >> settings.Downsample.value;
                ghosts = new RenderTarget().Allocate($"ghosts");
                ghosts.Get(cmd, in descDOWN, filterMode);
            }
            if (radialWarped == null)
            {
                radialWarped = new RenderTarget().Allocate($"radialWarped");
                radialWarped.Get(cmd, in desc, filterMode);
            }
            if (added == null)
            {
                added = new RenderTarget().Allocate($"added");
                added.Get(cmd, in desc, filterMode);
            }
            if (aberration == null)
            {
                aberration = new RenderTarget().Allocate($"aberration");
                aberration.Get(cmd, in desc, filterMode);
            }
            if (blur == null)
            {
                blur = new RenderTarget().Allocate($"blur");
                blur.Get(cmd, in desc, filterMode);
            }
            if (blur1 == null)
            {
                blur1 = new RenderTarget().Allocate($"blur1");
                blur1.Get(cmd, in desc, filterMode);
            }

            if (downsampled.Handle.rt == null)
            {
                downsampled.Get(cmd, in descDOWN, filterMode);
            }
            if (ghosts.Handle.rt == null)
            {
                ghosts.Get(cmd, in descDOWN, filterMode);
            }

            if (radialWarped.Handle.rt == null)
            {
                radialWarped.Get(cmd, in desc, filterMode);
            }
            if (added.Handle.rt == null)
            {
                added.Get(cmd, in desc, filterMode);
            }
            if (aberration.Handle.rt == null)
            {
                aberration.Get(cmd, in desc, filterMode);
            }
            if (blur.Handle.rt == null)
            {
                blur.Get(cmd, in desc, filterMode);
            }
            if (blur1.Handle.rt == null)
            {
                blur1.Get(cmd, in desc, filterMode);
            }

            //material
            material.SetFloat("_Sub", settings.Subtract.value);
            material.SetFloat("_Mul", settings.Multiply.value);
            cmd.SetGlobalTexture("_MainTexA", source);
            //    RenderTexture downsampled = RenderTexture.GetTemporary(Screen.width >> settings.Downsample.value, Screen.height >> settings.Downsample.value, 0, RenderTextureFormat.DefaultHDR);
            //Graphics.Blit(source.rt, downsampled.Handle.rt, material, 0);
            Utils.Blit(cmd, source, downsampled, material, 0);

           

            //ghostMaterial
            //    RenderTexture ghosts = RenderTexture.GetTemporary(Screen.width >> settings.Downsample.value, Screen.height >> settings.Downsample.value, 0, RenderTextureFormat.DefaultHDR);
            material.SetInt("_NumGhost", settings.NumberOfGhosts.value);
            material.SetFloat("_Displace", settings.Displacement.value);
            material.SetFloat("_Falloff", settings.Falloff.value);
            cmd.SetGlobalTexture("_MainTexA", downsampled.Id);
         //   Graphics.Blit(downsampled.Handle.rt, ghosts.Handle.rt, material, 3);
            Utils.Blit(cmd, downsampled, ghosts, material, 3);
                     
            //radialWarpMaterial
            //    RenderTexture radialWarped = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.DefaultHDR);            
            material.SetFloat("_HaloFalloff", settings.HaloFalloff.value);
            material.SetFloat("_HaloWidth", settings.HaloWidth.value);
            //Debug.Log(settings.HaloSubtract.value);
            material.SetFloat("_HaloSub", settings.HaloSubtract.value*1);
            cmd.SetGlobalFloat("_HaloSub", settings.HaloSubtract.value * 1);
            cmd.SetGlobalTexture("_MainTexB", source);
            //Graphics.Blit(source.rt, radialWarped.Handle.rt, material, 1);
            Utils.Blit(cmd, source, radialWarped, material, 1);                     

            //additiveMaterial
            //material.SetTexture("_MainTex1", radialWarped.Handle.rt);
            cmd.SetGlobalTexture("_MainTexA", ghosts.Id);
            cmd.SetGlobalTexture("_MainTexB", radialWarped.Id);
            //   RenderTexture added = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.DefaultHDR);
            //Graphics.Blit(ghosts.Handle.rt, added.Handle.rt, material, 5);
            Utils.Blit(cmd, ghosts, added, material, 6);

       


            //aberrationMaterial
            //     RenderTexture aberration = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.DefaultHDR);          
            //material.SetTexture("_ChromaticAberration_Spectrum", settings.chromaticAberrationSpectrum.value);
            material.SetFloat("_ChromaticAberration_Amount", settings.chromaticAberration.value);
            material.SetInt("_Distance_Function", (int)settings.chromaticAberrationDistanceFunction.value);
            cmd.SetGlobalTexture("_MainTexB", added.Id);
            cmd.SetGlobalTexture("_ChromaticAberration_Spectrum", settings.chromaticAberrationSpectrum.value);
            // Graphics.Blit(added.Handle.rt, aberration.Handle.rt, material, 2);
            Utils.Blit(cmd, added, aberration, material, 2);

           

            //blurMaterial
            //     RenderTexture blur = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.DefaultHDR);
            //     RenderTexture blur1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.DefaultHDR);
            material.SetInt("_BlurSize", settings.BlurSize.value);
            material.SetFloat("_Sigma", settings.Sigma.value);
            material.SetInt("_Direction", 1);
            cmd.SetGlobalTexture("_MainTexB", aberration.Id);
       //     Graphics.Blit(aberration.Handle.rt, blur.Handle.rt, material, 4);
            Utils.Blit(cmd, aberration, blur, material, 4);
            material.SetInt("_Direction", 0);
            cmd.SetGlobalTexture("_MainTexC", blur.Id);
            //     Graphics.Blit(blur.Handle.rt, blur1.Handle.rt, material, 4);
            Utils.Blit(cmd, blur, blur1, material, 5);

            /////////TEST
            //cmd.SetGlobalTexture("_MainTexA", blur1.Id);
            //cmd.SetGlobalTexture("_MainTexA", blur1.Id);
            //Utils.Blit(cmd, blur1, dest, material, 6);
            //return;

            //additiveMaterial
            material.SetFloat("_Blend", settings.m_Intencity.value);
            // material.SetTexture("_MainTex1", blur1.Handle.rt);
            cmd.SetGlobalTexture("_MainTexA", source);
            cmd.SetGlobalTexture("_MainTexB", blur1.Id);
            //Graphics.Blit(source.rt, dest.rt, material, 5);
            Utils.Blit(cmd, source, dest, material,6);

            

            /*
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
                Utils.Blit(cmd, kuwaharaPasses[settings.passes.value - 1].Handle, dest, baseMaterial, 0);
                //Graphics.Blit(kuwaharaPasses[settings.passes - 1], destination);
            }
            */
            _sampler.End(cmd);
        }

        public override void Cleanup(CommandBuffer cmd)
        {
            //foreach (var rt in _mipDown)
            //    rt.Release(cmd);

            //RenderTexture.ReleaseTemporary(downsampled);
            //RenderTexture.ReleaseTemporary(ghosts);
            //RenderTexture.ReleaseTemporary(radialWarped);
            //RenderTexture.ReleaseTemporary(added);
            //RenderTexture.ReleaseTemporary(aberration);
            //RenderTexture.ReleaseTemporary(blur);
            //RenderTexture.ReleaseTemporary(blur1);

            downsampled.Release(cmd);
            ghosts.Release(cmd);
            radialWarped.Release(cmd);
            added.Release(cmd);
            aberration.Release(cmd);
            blur.Release(cmd);
            blur1.Release(cmd);
        }
    }
}