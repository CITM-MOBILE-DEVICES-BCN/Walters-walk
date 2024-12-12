using System;
using System.Linq;
using UnityEngine;

namespace Artngame.GLAMOR.VolFx
{
    [ShaderName("Hidden/VolFx/Sharp")]
    public class SharpPass : VolFxProc.Pass
    {
        private static readonly int s_Directions = Shader.PropertyToID("_Directions");
        private static readonly int s_Step       = Shader.PropertyToID("_Step");
        private static readonly int s_Radius     = Shader.PropertyToID("_Radius");
        private static readonly int s_Rotation   = Shader.PropertyToID("_Rotation");
        private static readonly int stSamples    = Shader.PropertyToID("_Samples");

        private static readonly int   s_Filter = Shader.PropertyToID("_Filter");
        private const           float k_Scale  = 0.014f * 9f;
        
        [CurveRange(0, 0.03f, 1, 1)]
        public  AnimationCurve _response = AnimationCurve.EaseInOut(0, 1, 1, 0.03f);
        //private float     _samples      = 1;
        public  float[] _filter       = Array.Empty<float>();
        private float[] _filterBuffer = new float[8];
        //private float _Radius = 1;

        // =======================================================================
        public override void Init()
        {
            //_validateFilter(_samples);
        }

        private void OnValidate()
        {
           // _validateFilter(_samples);
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<SharpVol>();

            if (settings.IsActive() == false)
                return false;
         
           // if (_samples != settings.m_Samples.value)
            //    _validateFilter(settings.m_Samples.value);
            
            var aspect = settings.m_Aspect.value;
            var xStep = settings.m_Radius.value * (aspect + 1f);
            var yStep = settings.m_Radius.value * (1f - aspect);
            var angle =  Mathf.Deg2Rad * settings.m_Angle.value;
            //mat.SetVector(s_Step, new Vector4((xStep * k_Scale / _samples) * (Screen.width / (float)Screen.height), yStep * k_Scale / _samples, settings.m_Radial.value * k_Scale * 2f / _samples, angle));
            mat.SetFloat("_Samples", settings.m_Samples.value);
            mat.SetFloatArray(s_Filter, _filterBuffer);

            mat.SetFloat(s_Radius, settings.m_Radius.value);

            mat.SetFloat("_lowerThreshold", settings.m_lowerThreshold.value);

            return true;
        }
        
        // =======================================================================
        private void _validateFilter(int samples)
        {
           //_samples = samples;
            //if (_filter.Length != samples)
            //    _filter = new float[samples];

            //// fill and normalize filter, copy to buffer
            //var center = Mathf.CeilToInt(samples * .5f);
            //for (var n = 0; n < center; n++)
            //{
            //    var weight = _response.Evaluate(1f - (n / (float)(center - 1)));
            //    _filter[n]               = weight;
            //    _filter[samples - 1 - n] = weight;
            //}

            var sum   = _filter.Sum();
            var scale = 1f / sum;
            for (var n = 0; n < samples; n++)
            {
                //_filter[n]       *= scale;
                _filterBuffer[n] =  _filter[n];
            }
        }
    }
}