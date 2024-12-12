using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.GLAMOR.VolFx
{
    [ShaderName("Hidden/VolFx/RainLite")] // shader name for pass material
    public class RainLitePass : VolFxProc.Pass
    {
        private RenderTarget tmpBufferB;
        RainLiteVol settings;
        //bool m_FirstFrame = true;
        private ProfilingSampler _sampler;
        public void ResetHistory()
        {
            //m_FirstFrame = true;
        }
        public override void Init()
        {
            _sampler = new ProfilingSampler(name);
            tmpBufferB = new RenderTarget().Allocate($"tmpBufferB");
            //_validateMaterial();
        }
        private void OnValidate()
        {
            if (Application.isPlaying == false)
            {
                //AUTO               
                tmpBufferB = new RenderTarget().Allocate($"tmpBufferB");
            }
        }
        // =======================================================================
        public override bool Validate(Material mat)
        {
            // use stack from feature settings
            var settings = Stack.GetComponent<RainLiteVol>();

            // return false if we don't want to execute pass
            if (settings.IsActive() == false)
                return false;

            // setup material before drawing
            mat.SetFloat("_Weight", settings.m_Weight.value);
            return true;
        }

        Material materialSSSS;

        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData
            , RenderTextureDescriptor descA, bool isRG)
        {
            _sampler.Begin(cmd);

            var settings = Stack.GetComponent<RainLiteVol>();

            RenderTextureDescriptor desc;
            if (isRG)
            {
                desc = descA;
            }
            else
            {
                desc = renderingData.cameraData.cameraTargetDescriptor;
            }

            desc.colorFormat = RenderTextureFormat.ARGB32;
            desc.depthStencilFormat = GraphicsFormat.None;


            materialSSSS = settings.rainLiteMaterial.value;
            if (materialSSSS == null)
            {
                materialSSSS = Resources.Load<Material>("RainLite");
            }
            if (materialSSSS == null)
            {
                materialSSSS = new Material(Resources.Load<Shader>("RainLite"));
            }

            //Material materialSSSS = Resources.Load<Material>("RainLite");// //new Material(Resources.Load<Shader>("RainLite"));

            materialSSSS.SetFloat("_Weight", settings.m_Weight.value);

            //Debug.Log(settings.m_Weight.value);

            if (settings.rainMode.value < 3)
            {
                materialSSSS.SetInt("_rainMode", settings.rainMode.value);
            }
            else
            {
                //materialSSSS = Resources.Load<Material>("Outline_WEATHER_LITE_Rain_VOLFX");
                materialSSSS = Resources.Load<Material>("RainLite");

                //Debug.Log(materialSSSS.name);
                //sDebug.Log(materialSSSS.GetFloat("_SnowTexScale"));
                //if (Camera.main != null && dataA.cameraData.camera == Camera.main
                //   && Camera.main.GetComponent<ScreenSpaceRainLITE_SM_URP>() != null)
                //{
                if (Camera.main != null)
                {
                    materialSSSS.SetMatrix("_CamToWorld", Camera.main.cameraToWorldMatrix);
                }
                materialSSSS.SetInt("_rainMode", settings.rainMode.value);
                //cmd.SetGlobalTexture("_CameraColorTexture", source);

                //materialSSSS.SetTexture("_CameraColorTexture", source.rt);
                //materialSSSS = new Material(Resources.Load<Shader>("TESTSGA"));
                //materialSSSS = Resources.Load<Material>("TESTSG");
                //Debug.Log(materialSSSS.name);
                //}
            }

            //materialSSSS.SetTexture("_MainTexB", source.rt);
            //cmd.SetGlobalTexture("_MainTexB", source);
            //Utils.Blit(cmd, source, dest, materialSSSS, 2);
            //RenderShafts(context, renderingData, cmd, desc, materialSSSS, source, dest);

            //cmd.SetGlobalFloat("_Weight", settings.m_Weight.value);
            //Utils.JustBlit(cmd, source.rt, dest);
            Utils.Blit(cmd, source, dest, materialSSSS, 0);

            //Utils.BlitRT(cmd, source, dest, materialSSSS, 0);
            //return;
            //Debug.Log("aaa");
            //END            

            _sampler.End(cmd);
        }
        public override void Cleanup(CommandBuffer cmd)
        {
            tmpBufferB.Release(cmd);
        }

    }
}