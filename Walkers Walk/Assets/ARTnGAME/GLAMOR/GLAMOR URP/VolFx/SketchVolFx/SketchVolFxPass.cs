using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.GLAMOR.VolFx
{
    [ShaderName("Hidden/VolFx/EyeAdaptGLAMOR")]
    public class SketchVolFxPass : VolFxProc.Pass
    {        
        //bool m_FirstFrame = true;     
        public void ResetHistory()
        {
           // m_FirstFrame = true;
        }       

        private RenderTarget tmpBufferB;                
        private float            _time;
        private float            _intensity;
        private ProfilingSampler _sampler;

        public override void Init()
        {
            tmpBufferB = new RenderTarget().Allocate($"tmpBufferB");             
            _sampler = new ProfilingSampler(name);            
            // _validateMaterial();
        }

        SketchVolFxVol settings;

        public override bool Validate(Material mat)
        {
            if (settings == null)
            {
                settings = Stack.GetComponent<SketchVolFxVol>();
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
                tmpBufferB = new RenderTarget().Allocate($"tmpBufferB");               
                _validateMaterial();
            }
        }
        void _validateMaterial()
        {
        }      

        Material baseMaterial;

        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData
            , RenderTextureDescriptor descA, bool isRG)
        {
            _sampler.Begin(cmd);

            if (settings == null)
            {
                settings = Stack.GetComponent<SketchVolFxVol>();
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

            if (baseMaterial == null)
            {
                baseMaterial = Resources.Load<Material>("SketchVolFx_URP");//
            }
            if (baseMaterial == null)
            {
                baseMaterial = new Material(Resources.Load<Shader>("SketchVolFx_URP"));
            }

            Camera camera = Camera.main;
            var cam = camera;// GetComponent<Camera>();
            var camtr = cam.transform;

            ////////// SCATTER
            var camPos = camtr.position;
            baseMaterial.SetVector("_CameraWS", camPos);

            baseMaterial.SetTexture("_NoiseTex", settings.noiseTexture.value);





            var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9


        
            tmpBufferB.Get(cmd, in desc);


            GL.ClearWithSkybox(false, camera);
            baseMaterial.SetTexture("_MainTexA", source.rt);
            Utils.Blit(cmd, source, tmpBufferB, baseMaterial, 5);


            //baseMaterial.SetTexture("_MainTexA", tmpBufferB.Handle.rt);

            //WORLD RECONSTRUCT        
            Matrix4x4 camToWorld = Camera.main.cameraToWorldMatrix;
            baseMaterial.SetMatrix("_InverseView", camToWorld);

            if (baseMaterial != null && settings.noiseTexture != null)
            {
                if (settings.noiseTexture.value == null)
                {
                    settings.noiseTexture.value = new Texture2D(1280, 720);
                }
                cmd.SetGlobalTexture("_NoiseTex", settings.noiseTexture.value);
            }
            cmd.SetGlobalVector("_TintColor", new Vector4(settings.TintColor.value.r, settings.TintColor.value.g, settings.TintColor.value.b, 1));
            cmd.SetGlobalTexture("_MainTexB", source);

            baseMaterial.SetFloat("_Blend", settings.m_Intencity.value);
            baseMaterial.SetVector("scaling", settings.scaling.value);

            baseMaterial.SetFloat("_contrast", settings.contrast.value);
            baseMaterial.SetFloat("_brightness", settings.luminance.value);

            Utils.Blit(cmd, source, dest, baseMaterial, 6);


            _sampler.End(cmd);
        }

        public override void Cleanup(CommandBuffer cmd)
        {
        }
    }
}