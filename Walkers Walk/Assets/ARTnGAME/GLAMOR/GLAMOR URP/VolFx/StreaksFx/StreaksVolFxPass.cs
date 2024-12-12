using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.GLAMOR.VolFx
{
    [ShaderName("Hidden/StreaksVolFx")]
    public class StreaksVolFxPass : VolFxProc.Pass
    {        
        StreaksVolFxVol settings;

        private static readonly int s_ValueTex = Shader.PropertyToID("_ValueTex");
        private static readonly int s_ColorTex = Shader.PropertyToID("_ColorTex");
        private static readonly int s_Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int s_BloomTex = Shader.PropertyToID("_BloomTex");
        private static readonly int s_DownTex = Shader.PropertyToID("_DownTex");
        private static readonly int s_Blend = Shader.PropertyToID("_Blend");

        internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
        internal static readonly int HighTex = Shader.PropertyToID("_HighTex");
        internal static readonly int Threshold = Shader.PropertyToID("_Threshold");
        internal static readonly int Stretch = Shader.PropertyToID("_Stretch");
        internal static readonly int Intensity = Shader.PropertyToID("_IntensitySTREAKS");
        internal static readonly int Color = Shader.PropertyToID("_Color");

        //STREAKS
        const int MaxMipLevel = 16;
        private RenderTarget[] _rtMipDown;
        private RenderTarget[] _rtMipUp;
        RenderTarget lastRT;
        RenderTarget bloomOut;
        int[] _mipWidth;

        //int[] _rtMipDown;
        //int[] _rtMipUp;        
        //public StreakPass(RenderPassEvent renderPassEvent) : base(renderPassEvent)
        //{
        //    _mipWidth = new int[MaxMipLevel];
        //    _rtMipDown = new int[MaxMipLevel];
        //    _rtMipUp = new int[MaxMipLevel];

        //    for (var i = 0; i < MaxMipLevel; i++)
        //    {
        //        _rtMipDown[i] = Shader.PropertyToID("_MipDown" + i);
        //        _rtMipUp[i] = Shader.PropertyToID("_MipUp" + i);
        //    }
        //}
        //static class ShaderPropertyID
        //{
        //    internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
        //    internal static readonly int HighTex = Shader.PropertyToID("_HighTex");
        //    internal static readonly int Threshold = Shader.PropertyToID("_Threshold");
        //    internal static readonly int Stretch = Shader.PropertyToID("_Stretch");
        //    internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        //    internal static readonly int Color = Shader.PropertyToID("_Color");
        //}





        [Range(3, 14)]
        public int _samples = 7;
        [CurveRange(0, 0, 1, 3)]
        public AnimationCurve _scatter = new AnimationCurve(new Keyframe(0.0f, 0.8594512939453125f, 0.0f, 0.1687847524881363f, 0f, .9040920734405518f),
                                                               new Keyframe(1.0f, 2.1807241439819338f, 7.417094707489014f, 1.3360401391983033f, 0.06010228395462036f, 0f));
        public ValueMode _mode = ValueMode.Luma;
        [CurveRange(0, 0, 1, 1)]
        public AnimationCurve _flicker = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(.5f, .77f), new Keyframe(1, 1) }) { postWrapMode = WrapMode.Loop };
        public float _flickerPeriod = 7f;
        public bool _bloomOnly;

        private float _time;
        private float _intensity;
        private ProfilingSampler _sampler;
        private Texture2D _valueTex;
        private Texture2D _colorTex;

        private RenderTarget[] _mipDown;
        private RenderTarget[] _mipUp;
        private float _scatterLerp;

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
            //STREAKS
            bloomOut = new RenderTarget().Allocate($"bloomOut");
            lastRT = new RenderTarget().Allocate($"lastRT");
            _mipWidth = new int[MaxMipLevel];
            _rtMipDown = new RenderTarget[MaxMipLevel];
            _rtMipUp = new RenderTarget[MaxMipLevel];
            for (var n = 0; n < MaxMipLevel; n++)
            {
                _rtMipDown[n] = new RenderTarget().Allocate($"streaks_{name}_down_{n}");
            }
            for (var n = 0; n < MaxMipLevel; n++)
            {
                _rtMipUp[n] = new RenderTarget().Allocate($"streaks_{name}_up_{n}");
            }

            _mipDown = new RenderTarget[_samples];
            _mipUp = new RenderTarget[_samples - 1];

            for (var n = 0; n < _samples; n++)
                _mipDown[n] = new RenderTarget().Allocate($"bloom_{name}_down_{n}");
            for (var n = 0; n < _samples - 1; n++)
                _mipUp[n] = new RenderTarget().Allocate($"bloom_{name}_up_{n}");

            _sampler = new ProfilingSampler(name);

            _validateMaterial();
        }

        public override bool Validate(Material mat)
        {
            if (settings == null)
            {
                settings = Stack.GetComponent<StreaksVolFxVol>();
            }

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
                //STREAKS
                bloomOut = new RenderTarget().Allocate($"bloomOut");
                lastRT = new RenderTarget().Allocate($"lastRT");
                _mipWidth = new int[MaxMipLevel];
                _rtMipDown = new RenderTarget[MaxMipLevel];
                _rtMipUp = new RenderTarget[MaxMipLevel];
                for (var n = 0; n < MaxMipLevel; n++)
                {
                    _rtMipDown[n] = new RenderTarget().Allocate($"streaks_{name}_down_{n}");
                }
                for (var n = 0; n < MaxMipLevel; n++)
                {
                    _rtMipUp[n] = new RenderTarget().Allocate($"streaks_{name}_up_{n}");
                }

                //BLOOM
                _mipDown = new RenderTarget[_samples];
                _mipUp = new RenderTarget[_samples - 1];

                for (var n = 0; n < _samples; n++)
                {
                    _mipDown[n] = new RenderTarget().Allocate($"bloom_{name}_down_{n}");
                }
                for (var n = 0; n < _samples - 1; n++)
                {
                    _mipUp[n] = new RenderTarget().Allocate($"bloom_{name}_up_{n}");
                }
            }

            if (_materialA == null)
            {
                _materialA = Resources.Load<Material>("StreaksVolFx");
            }
            if (_materialA == null)
            {
                _materialA = new Material(Resources.Load<Shader>("StreaksVolFx"));
            }//

            _validateMaterial();
        }
        void _validateMaterial()
        {
            if (_materialA != null)
            {
                _materialA.DisableKeyword("_BRIGHTNESS");
                _materialA.DisableKeyword("_LUMA");
                _materialA.DisableKeyword("_BLOOM_ONLY");

                _materialA.EnableKeyword(_mode switch
                {
                    ValueMode.Luma => "_LUMA",
                    ValueMode.Brightness => "_BRIGHTNESS",
                    _ => throw new ArgumentOutOfRangeException()
                });

                if (_bloomOnly)
                    _materialA.EnableKeyword("_BLOOM_ONLY");
            }
        }      

        Material _materialA;//

        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData
            , RenderTextureDescriptor descA, bool isRG)
        {
            _sampler.Begin(cmd);

            if (settings == null)
            {
                settings = Stack.GetComponent<StreaksVolFxVol>();
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
                _materialA = Resources.Load<Material>("StreaksVolFx");
            }
            if (_materialA == null)
            {
                _materialA = new Material(Resources.Load<Shader>("StreaksVolFx"));
            }

            //BLOOMING           
            //FilterMode filterMode = FilterMode.Bilinear;
            // RenderTextureDescriptor descB = desc;
            //descB.width = descB.width / settings.downSampleFactor.value;
            //descB.height = descB.height / settings.downSampleFactor.value;
            //// Create RTs
            //if (bloomRT == null)
            //{
            //    bloomRT = new RenderTarget().Allocate($"bloomRT");
            //    bloomRT.Get(cmd, in descB, filterMode);
            //}
            //if (bloomRT2 == null)
            //{
            //    bloomRT2 = new RenderTarget().Allocate($"bloomRT2");
            //    bloomRT2.Get(cmd, in desc, filterMode);
            //}
            //if (ghostRT == null)
            //{
            //    ghostRT = new RenderTarget().Allocate($"ghostRT");
            //    ghostRT.Get(cmd, in desc, filterMode);
            //}
            //else
            //{
            //    ghostRT.Get(cmd, in desc, filterMode);
            //}

            //bloomRT.Get(cmd, in descB, filterMode);
            //bloomRT2.Get(cmd, in desc, filterMode);

            //// bloomRT = RenderTexture.GetTemporary(desc.width / settings.downSampleFactor.value, desc.height / settings.downSampleFactor.value, 0, RenderTextureFormat.DefaultHDR);
            ////  bloomRT2 = RenderTexture.GetTemporary(bloomRT.width, bloomRT.height, 0, RenderTextureFormat.DefaultHDR);

            ////  if (ghostRT == null) { 
            ////     ghostRT = new RenderTexture(bloomRT.width, bloomRT.height, 0, RenderTextureFormat.DefaultHDR); 
            ////  }

            //// Pass data to the shader
            ////_materialA.SetTexture("_GhostTex", ghostRT);
            //_materialA.SetFloat("_Blend", settings.m_Intencity.value);

            ////_materialA.SetTexture("_SourceTex", source);////////////////////
            //cmd.SetGlobalTexture("_SourceTex", source);
            //cmd.SetGlobalTexture("_GhostTex", ghostRT.Id);

            //_materialA.SetColor("_BloomTint", settings.bloomTint.value);
            //_materialA.SetFloat("_Intensity", settings.intensity.value);
            //_materialA.SetFloat("_Ghosting", settings.ghostingAmount.value);
            //_materialA.SetFloat("_BlendFac", settings.blendFac.value);
            //_materialA.SetFloat("_DistMul", settings.distanceMultiplier.value);

            //int offset = 8;
            //// Mask creation and blurring
            ////Graphics.Blit(source, bloomRT, _material, 0);
            //Utils.Blit(cmd, source, bloomRT, _materialA, 0 + offset);

            //for (int i = 0; i < settings.blurIterations.value; i++)
            //{
            //    cmd.SetGlobalTexture("_MainTexA", bloomRT.Id);
            //    Utils.Blit(cmd, bloomRT, bloomRT2, _materialA, 1 + offset);
            //    //Graphics.Blit(bloomRT, bloomRT2, _material, 1);

            //    cmd.SetGlobalTexture("_MainTexA", bloomRT2.Id);
            //    Utils.Blit(cmd, bloomRT2, bloomRT, _materialA, 2 + offset);
            //    // Graphics.Blit(bloomRT2, bloomRT, _material, 2);
            //}

            ////cmd.SetGlobalTexture("_MainTexA", bloomRT.Id);
            //// Utils.Blit(cmd, bloomRT, dest, _materialA, 7);
            //// return;

            //// Copy to ghosting RT
            //cmd.SetGlobalTexture("_MainTexA", bloomRT.Id);
            //Utils.Blit(cmd, bloomRT, ghostRT, _materialA, 7);
            ////Graphics.Blit(bloomRT, ghostRT);

            //// Combine with source texture
            //cmd.SetGlobalTexture("_MainTexA", bloomRT2.Id);
            //cmd.SetGlobalTexture("_SourceTex", source);


            //Utils.Blit(cmd, bloomRT2, dest, _materialA, 3 + offset);
            ////Graphics.Blit(bloomRT, dest, _material, 3);

            //// Cleanup
            ////RenderTexture.ReleaseTemporary(bloomRT);
            ////RenderTexture.ReleaseTemporary(bloomRT2);
            ///


            //_materialA.SetFloat("_GradThreshold", settings.gradThreshold.value);
            //_materialA.SetFloat("_ColorThreshold", settings.colorThreshold.value);
            //_materialA.SetFloat("_Sensivity", settings.sensivity.value);
            //_materialA.SetFloat("_blendThreshold", settings.blendThreshold.value);
            //_materialA.SetFloat("_blendScreenThreshold", settings.blendScreenThreshold.value);

            //_materialA.SetFloat("_Blend", settings.m_Intencity.value);


            //END FLARES

            /*
            //BLOOM
            RenderTextureDescriptor descB = desc;
            int bloomOffset = 12;

            for (var n = 0; n < _samples - 1; n++)
            {
                descB.width = Mathf.Max(1, descB.width >> 1);
                descB.height = Mathf.Max(1, descB.height >> 1);

                _mipDown[n].Get(cmd, in descB, FilterMode.Bilinear);
                _mipUp[n].Get(cmd, in descB, FilterMode.Bilinear);
            }

            descB.width = Mathf.Max(1, descB.width >> 1);
            descB.height = Mathf.Max(1, descB.height >> 1);

            _mipDown[_samples - 1].Get(cmd, in descB, FilterMode.Bilinear);

            Utils.Blit(cmd, source, _mipDown[0], _materialA, 0 + bloomOffset);

            for (var n = 1; n < _samples; n++)
            {
                Utils.Blit(cmd, _mipDown[n - 1], _mipDown[n], _materialA, 1 + bloomOffset);
            }

            var totalBlend = 0f;
            var blend = Mathf.Lerp(_scatter.Evaluate(0f), _scatter.Evaluate(1f), _scatterLerp);
            totalBlend += blend;
            cmd.SetGlobalFloat(s_Blend, blend);
            cmd.SetGlobalTexture(s_DownTex, _mipDown[_samples - 1].Handle.nameID);
            Utils.Blit(cmd, _mipDown[_samples - 2].Handle, _mipUp[_samples - 2].Handle, _materialA, 2 + bloomOffset);
            for (var n = _samples - 3; n >= 0; n--)
            {
                var t = n / (float)(_samples - 2);
                blend = Mathf.Lerp(_scatter.Evaluate(t), _scatter.Evaluate(t), _scatterLerp);
                totalBlend += blend;
                cmd.SetGlobalFloat(s_Blend, blend);
                cmd.SetGlobalTexture(s_DownTex, _mipDown[n].Handle.nameID);
                Utils.Blit(cmd, _mipUp[n + 1].Handle, _mipUp[n].Handle, _materialA, 2 + bloomOffset);
            }

            _materialA.SetFloat(s_Intensity, _intensity / totalBlend);
            cmd.SetGlobalTexture(s_BloomTex, _mipUp[0].Handle);
            Utils.Blit(cmd, source, dest, _materialA, 3 + bloomOffset);
            */
            //bloomOut.Get(cmd, in desc, FilterMode.Bilinear);
            //END BLOOM



            //return;

            //////////////////////////// STREAKS /////////////////////////////

            int streakOffset = 16;
            //ref var cameraData = ref renderingData.cameraData;
            var w = desc.width;  // cameraData.camera.scaledPixelWidth;
            var h = desc.height; //cameraData.camera.scaledPixelHeight;

            //Debug.Log(desc.width);
            //Debug.Log(Camera.main.scaledPixelWidth);

            _materialA.SetFloat(Threshold, settings.threshold.value);
            _materialA.SetFloat(Stretch, settings.stretch.value);
            _materialA.SetFloat(Intensity, settings.intensity.value);
            _materialA.SetColor(Color, settings.tint.value);

            // Calculate the mip widths.
            _mipWidth[0] = w;
            for (var i = 1; i < MaxMipLevel; i++)
            {
                _mipWidth[i] = _mipWidth[i - 1] / 2;
            }

            // Apply the prefilter and store into MIP 0.
            var height = h / 2;
           

            RenderTextureDescriptor descS = desc;
            descS.width = _mipWidth[0];
            descS.height = height;
            _rtMipDown[0].Get(cmd, in descS, FilterMode.Bilinear);
            //cmd.GetTemporaryRT(_rtMipDown[0], _mipWidth[0], height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);
            cmd.SetGlobalTexture("_MainTexA", source);
            Utils.Blit(cmd, source, _rtMipDown[0], _materialA, 0 + streakOffset);
            // cmd.Blit(source, _rtMipDown[0], _materialA, 0 + streakOffset);

            //TESTER
          //  cmd.SetGlobalTexture("_MainTexA", _rtMipDown[0].Id);
          //  Utils.Blit(cmd, _rtMipDown[0], dest, _materialA, 7);
           // return;


            // Build the MIP pyramid.
            var level = 1;
            for (; level < MaxMipLevel && _mipWidth[level] > 7; level++)
            {
                descS.width = _mipWidth[level];
                descS.height = height;
                _rtMipDown[level].Get(cmd, in descS, FilterMode.Bilinear);
                //cmd.GetTemporaryRT(_rtMipDown[level], _mipWidth[level], height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);

                cmd.SetGlobalTexture("_MainTexA", _rtMipDown[level - 1].Id);
                //cmd.Blit(_rtMipDown[level - 1], _rtMipDown[level], _materialA, 1 + streakOffset);
                Utils.Blit(cmd, _rtMipDown[level - 1], _rtMipDown[level], _materialA, 1 + streakOffset);

             
            }
            // MIP 0 is not needed at this point.
            //           cmd.ReleaseTemporaryRT(_rtMipDown[level]);
           


            // Upsample and combine.
            //var lastRT = _rtMipDown[--level];
            descS.width = _mipWidth[--level];
            descS.height = height;
            lastRT.Get(cmd, in descS, FilterMode.Bilinear);
            cmd.SetGlobalTexture("_MainTexA", _rtMipDown[level].Id);
            Utils.Blit(cmd, _rtMipDown[level], lastRT, _materialA, 7); //JUST BLIT

        

            for (level--; level >= 1; level--)
            {
                descS.width = _mipWidth[level];
                descS.height = height;
                _rtMipUp[level].Get(cmd, in descS, FilterMode.Bilinear);
                //cmd.GetTemporaryRT(_rtMipUp[level], _mipWidth[level], height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);

                cmd.SetGlobalTexture("_MainTexA", lastRT.Id);
                cmd.SetGlobalTexture(HighTex, _rtMipDown[level].Id);
                //cmd.Blit(lastRT, _rtMipUp[level], _materialA, 2 + streakOffset);
                Utils.Blit(cmd, lastRT, _rtMipUp[level], _materialA, 2 + streakOffset);

                //cmd.ReleaseTemporaryRT(_rtMipDown[level]);
                //cmd.ReleaseTemporaryRT(lastRT);
                //lastRT = _rtMipUp[level];
                //descS.width = _mipWidth[level];
                //descS.height = height;
                lastRT.Release(cmd);
                lastRT.Get(cmd, in descS, FilterMode.Bilinear);
                cmd.SetGlobalTexture("_MainTexA", _rtMipUp[level].Id);
                Utils.Blit(cmd, _rtMipUp[level], lastRT, _materialA, 7); //JUST BLIT

                //if (level == 2)
                //{
                //    cmd.SetGlobalTexture("_MainTexA", lastRT.Id);
                //    Utils.Blit(cmd, lastRT, dest, _materialA, 7);
                //    //Debug.Log(level);
                //    return;
                //}
            }

            _materialA.SetFloat("_Blend", settings.m_Intencity.value);

            // Final composition.
            cmd.SetGlobalTexture("_MainTexA", lastRT.Id);
            cmd.SetGlobalTexture(HighTex, source);
            //           cmd.Blit(lastRT, dest, _materialA, 3 + streakOffset);
            //           cmd.ReleaseTemporaryRT(lastRT);
            Utils.Blit(cmd, lastRT, dest, _materialA, 3 + streakOffset);
            ////////////////////////// END STREAKS ////////////////////////////


            //     cmd.SetGlobalTexture("_MainTexA", source);
            //cmd.Blit(source, destination, material);
            //    Utils.Blit(cmd, source, dest, _materialA, 12);

            //cmd.SetGlobalTexture("_MainTexA", source);
            // Utils.Blit(cmd, source, dest, _materialA,7);

            _sampler.End(cmd);
        }

        public override void Cleanup(CommandBuffer cmd)
        {
            //ghostRT.Release(cmd);
            //_rtTMP.Release(cmd);
            //bloomRT.Release(cmd);
            //bloomRT2.Release(cmd);
        }
    }
}