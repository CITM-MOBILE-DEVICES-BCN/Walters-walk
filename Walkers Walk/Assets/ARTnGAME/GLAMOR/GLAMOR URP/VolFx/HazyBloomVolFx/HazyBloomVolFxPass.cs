using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.GLAMOR.VolFx
{
    [ShaderName("Hidden/VolFx/EyeAdaptGLAMOR")]
    public class HazyBloomVolFxPass : VolFxProc.Pass
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

        public override void Init()
        {
            ghostRT = new RenderTarget().Allocate($"ghostRT");
            _rtTMP = new RenderTarget().Allocate($"_rtTMP");             
            _sampler = new ProfilingSampler(name);            
            // _validateMaterial();
        }

        HazyBloomVolFxVol settings;

        public override bool Validate(Material mat)
        {
            if (settings == null)
            {
                settings = Stack.GetComponent<HazyBloomVolFxVol>();
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
                ghostRT = new RenderTarget().Allocate($"ghostRT");
                _rtTMP = new RenderTarget().Allocate($"_rtTMP");               
                _validateMaterial();
            }
        }
        void _validateMaterial()
        {
        }      

        Material _materialA;


        RenderTarget bloomRT;
        RenderTarget bloomRT2;
        RenderTarget ghostRT;



        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData
            , RenderTextureDescriptor descA, bool isRG)
        {
            _sampler.Begin(cmd);

            if (settings == null)
            {
                settings = Stack.GetComponent<HazyBloomVolFxVol>();
            }

            RenderTextureDescriptor desc;
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

            if (_materialA == null)
            {
                _materialA = Resources.Load<Material>("HazyBloomVolFx");
            }
            if (_materialA == null)
            {
                _materialA = new Material(Resources.Load<Shader>("HazyBloomVolFx"));
            }

            //BLOOMING           
            FilterMode filterMode = FilterMode.Bilinear;
            RenderTextureDescriptor descB = desc;
            descB.width = descB.width / settings.downSampleFactor.value;
            descB.height = descB.height / settings.downSampleFactor.value;
            // Create RTs
            if (bloomRT == null)
            {
                bloomRT = new RenderTarget().Allocate($"bloomRT");
                bloomRT.Get(cmd, in descB, filterMode);
            }
            if (bloomRT2 == null)
            {
                bloomRT2 = new RenderTarget().Allocate($"bloomRT2");
                bloomRT2.Get(cmd, in desc, filterMode);
            }
            if (ghostRT == null)
            {
                ghostRT = new RenderTarget().Allocate($"ghostRT");
                ghostRT.Get(cmd, in desc, filterMode);
            }
            else
            {
                ghostRT.Get(cmd, in desc, filterMode);
            }

            bloomRT.Get(cmd, in descB, filterMode);
            bloomRT2.Get(cmd, in desc, filterMode);

            // bloomRT = RenderTexture.GetTemporary(desc.width / settings.downSampleFactor.value, desc.height / settings.downSampleFactor.value, 0, RenderTextureFormat.DefaultHDR);
            //  bloomRT2 = RenderTexture.GetTemporary(bloomRT.width, bloomRT.height, 0, RenderTextureFormat.DefaultHDR);

            //  if (ghostRT == null) { 
            //     ghostRT = new RenderTexture(bloomRT.width, bloomRT.height, 0, RenderTextureFormat.DefaultHDR); 
            //  }

            // Pass data to the shader
            //_materialA.SetTexture("_GhostTex", ghostRT);
            _materialA.SetFloat("_Blend", settings.m_Intencity.value);

            //_materialA.SetTexture("_SourceTex", source);////////////////////
            cmd.SetGlobalTexture("_SourceTex", source);
            cmd.SetGlobalTexture("_GhostTex", ghostRT.Id);

            _materialA.SetColor("_BloomTint", settings.bloomTint.value);
            _materialA.SetFloat("_Intensity", settings.intensity.value);
            _materialA.SetFloat("_Ghosting", settings.ghostingAmount.value);
            _materialA.SetFloat("_BlendFac", settings.blendFac.value);
            _materialA.SetFloat("_DistMul", settings.distanceMultiplier.value);

            int offset = 8;
            // Mask creation and blurring
            //Graphics.Blit(source, bloomRT, _material, 0);
            Utils.Blit(cmd, source, bloomRT, _materialA, 0 + offset);

            



            for (int i = 0; i < settings.blurIterations.value; i++)
            {
                cmd.SetGlobalTexture("_MainTexA", bloomRT.Id);
                Utils.Blit(cmd, bloomRT, bloomRT2, _materialA, 1 + offset);
                //Graphics.Blit(bloomRT, bloomRT2, _material, 1);

                cmd.SetGlobalTexture("_MainTexA", bloomRT2.Id);
                Utils.Blit(cmd, bloomRT2, bloomRT, _materialA, 2 + offset);
                // Graphics.Blit(bloomRT2, bloomRT, _material, 2);
            }


            //cmd.SetGlobalTexture("_MainTexA", bloomRT.Id);
           // Utils.Blit(cmd, bloomRT, dest, _materialA, 7);
           // return;

            // Copy to ghosting RT
            cmd.SetGlobalTexture("_MainTexA", bloomRT.Id);
            Utils.Blit(cmd, bloomRT, ghostRT, _materialA, 7);
            //Graphics.Blit(bloomRT, ghostRT);

            // Combine with source texture
            cmd.SetGlobalTexture("_MainTexA", bloomRT2.Id);
            cmd.SetGlobalTexture("_SourceTex", source);
            Utils.Blit(cmd, bloomRT2, dest, _materialA, 3 + offset);
            //Graphics.Blit(bloomRT, dest, _material, 3);

            // Cleanup
            //RenderTexture.ReleaseTemporary(bloomRT);
            //RenderTexture.ReleaseTemporary(bloomRT2);

            _sampler.End(cmd);
        }

        public override void Cleanup(CommandBuffer cmd)
        {
        }
    }
}