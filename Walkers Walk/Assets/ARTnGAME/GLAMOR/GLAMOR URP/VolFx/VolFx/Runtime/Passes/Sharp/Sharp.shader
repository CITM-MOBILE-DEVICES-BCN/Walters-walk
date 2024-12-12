Shader "Hidden/VolFx/Sharp"
{
    Properties
    {
        _Step("_Step", Vector) = (0, 0, 0, 0)
        _Radial("Radial", Float) = 0.00
        _Samples("Samples", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 0

        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off

        Pass
        {
            Name "Sharp"
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                        
            float	_Samples;
            float	_Filter[8];
            
            float4 _Step;	// stepX, stepY, radial, angle

	        Texture2D    _MainTex;
	        SamplerState _linear_clamp_sampler;

            //v0.1
            float _Radius;

            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            frag_in vert(vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }
            
            float4 _sample(float2 uv, in const float2 step)
            {
				float4 result = 0;
				uv -= _Samples * .5 * step;
            	
            	// [unroll]
				for (int n = 0; n < _Samples; n ++)
				{
					result += _MainTex.Sample(_linear_clamp_sampler, uv) * _Filter[n];
					uv += step;
				}
            	
            	return result;
            }
            uniform float4 _MainTex_TexelSize;

            float _lowerThreshold;

            half4 frag(frag_in i) : SV_Target
            {
                // Blur calculations
             /*   const float radial = (distance(i.uv, float2(.5, .5)) * _Step.z);

            	const float sx = _Step.x + radial;
            	const float sy = _Step.y + radial;
				const float2 stepX = float2(cos(_Step.w) * sx, sin(_Step.w) * sx);
				const float2 stepY = float2(sin(_Step.w) * sy, cos(_Step.w) * sy);
                
				float2 uv = i.uv - _Samples * .5 * stepX;*/
				float4 result = 0;


                //SHARP
                float4 sharpOrigin = _MainTex.Sample(_linear_clamp_sampler, i.uv); 

                float4 up = _MainTex.Sample(_linear_clamp_sampler, (i.uv + float2(0, _Samples *0.01* _MainTex_TexelSize.y)));
                float4 left = _MainTex.Sample(_linear_clamp_sampler, (i.uv + float2(-_Samples * 0.01 * _MainTex_TexelSize.x, 0)));
                float4 right = _MainTex.Sample(_linear_clamp_sampler, (i.uv + float2(_Samples * 0.01 * _MainTex_TexelSize.x, 0)));
                float4 down = _MainTex.Sample(_linear_clamp_sampler, (i.uv + float2(0, -_Samples * 0.01 * _MainTex_TexelSize.y)));

                // Return edge detection
                result = (1.0 + 4.0 * _Radius * 10) * sharpOrigin - _Radius * (up + left + right + down) * 10;

            	// [unroll]
			/*	for (int n = 0; n < _Samples; n ++)
				{
					result += _sample(uv, stepY) * _Filter[n];
					uv += stepX;
				}*/
                //return result;
            	return clamp(result,_lowerThreshold,10001);
            }
            ENDHLSL
        }
    }
}