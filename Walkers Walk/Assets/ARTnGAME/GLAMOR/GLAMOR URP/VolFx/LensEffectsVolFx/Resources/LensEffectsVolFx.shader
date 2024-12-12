// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
Shader "Hidden/LensEffectsVolFx"
{
	Properties
	{
		//SubMul
		//_MainTex ("Texture", 2D) = "white" {}
		_Sub ("Subtract", float) = 0.5
		_Mul ("Multiply", float) = 0.5

		//RadialWarp
		//	_MainTex("Texture", 2D) = "white" {}
		_HaloWidth("Width", float) = .5
		_HaloFalloff("Halo Falloff", float) = 10
		_HaloSub("Halo Subtract", float) = 1

		//LensFlareAberration
		_DisplaceColor("Displacement Color", Color) = (1,1,1,1)

		//GhostFeature
		_NumGhost("Number of Ghosts", int) = 2
		_Displace("Displacement", float) = 0.1
		_Falloff("Falloff", float) = 10

		//GaussianBlur
		_BlurSize("Blur Size", float) = 8
		_Sigma("Sigma", float) = 3
		_Direction("Direction", int) = 0

		//Additive
		_MainTex1("Texture1", 2D) = "white" {}
	}

		HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		//#include "UnityCG.cginc"


		//lensDirtTexture
		TEXTURE2D(_lensDirtTexture);
	SAMPLER(sampler_lensDirtTexture);


		TEXTURE2D(_MainTexA);
	SAMPLER(sampler_MainTexA);

	float4 _MainTexB_TexelSize;
	TEXTURE2D(_MainTexB);
	SAMPLER(sampler_MainTexB);

	TEXTURE2D(_MainTex);
	TEXTURE2D(_ColorBuffer);
	TEXTURE2D(_Skybox);

	SAMPLER(sampler_MainTex);
	SAMPLER(sampler_ColorBuffer);
	SAMPLER(sampler_Skybox);
	float _Blend;

	TEXTURE2D(_CameraDepthTexture);
	SAMPLER(sampler_CameraDepthTexture);
	half4 _CameraDepthTexture_ST;

	half4 _SunColor = half4(0.87, 0.74, 0.65, 1);
	uniform half4 _SunPosition = half4(1, 1, 1, 1);
	uniform half4 _MainTex_TexelSize;
	uniform half4 _MainTexA_TexelSize;
	//uniform half4 _MainTexB_TexelSize;

#define SAMPLES_FLOAT 16.0f
#define SAMPLES_INT 16

	float2 TransformTriangleVertexToUV(float2 vertex)
	{
		float2 uv = (vertex + 1.0) * 0.5;
		return uv;
	}

	struct vert_in
	{
		float4 pos : POSITION;
		float2 uv  : TEXCOORD0;
	};

	struct frag_in
	{
		float2 uv     : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};


	////////////////// STREAKS
	struct appdataSTREAKS
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2fSTREAKS
	{
		float2 texcoord : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};
	v2fSTREAKS VertINV(appdataSTREAKS v)
	{
		v2fSTREAKS o;
		o.vertex = float4((v.vertex.xy - 0.5) * 2, 0.0, 1.0);
		o.texcoord = v.vertex.xy;
		o.texcoord.y = 1 - v.vertex.y;

		return o;
	}
	//Texture2D _MainTex;
	//SamplerState sampler_MainTex;
	Texture2D _HighTex;
	SamplerState sampler_HighTex;
	//float4 _MainTex_TexelSize;
	float _Threshold;
	float _Stretch;float _StretchV;
	float _IntensitySTREAKS;
	half3 _Color;

	float _IntensitySTREAKSV;
	half3 _ColorV;
	float _ThresholdV;

	half4 FragPrefilter(v2fSTREAKS i) : SV_Target
	{

		//return _MainTexA.Sample(sampler_MainTexA, float2(i.texcoord.x, i.texcoord.y)).rgba;

		// Actually this should be 1, but we assume you need more blur...
		const float vscale = 1.5;
		const float dy = _MainTexA_TexelSize.y * vscale / 2;

		float2 uv = i.texcoord;
		half3 c0 = _MainTexA.Sample(sampler_MainTexA, float2(uv.x, uv.y - dy)).rgb;
		half3 c1 = _MainTexA.Sample(sampler_MainTexA, float2(uv.x, uv.y + dy)).rgb;
		half3 c = (c0 + c1) / 2;

		float br = max(c.r, max(c.g, c.b));
		c *= max(0, br - _Threshold) / max(br, 1e-5);

		return half4(c, 1);
	}
		half4 FragPrefilterV(v2fSTREAKS i) : SV_Target
	{

		//return _MainTexA.Sample(sampler_MainTexA, float2(i.texcoord.x, i.texcoord.y)).rgba;

		// Actually this should be 1, but we assume you need more blur...
		const float vscale = 1.5;
		const float dy = _MainTexA_TexelSize.y * vscale / 2;

		float2 uv = i.texcoord;
		half3 c0 = _MainTexA.Sample(sampler_MainTexA, float2(uv.x, uv.y - dy)).rgb;
		half3 c1 = _MainTexA.Sample(sampler_MainTexA, float2(uv.x, uv.y + dy)).rgb;
		half3 c = (c0 + c1) / 2;

		float br = max(c.r, max(c.g, c.b));
		c *= max(0, br - _ThresholdV) / max(br, 1e-5);

		return half4(c, 1);
	}

		// Downsampler
		half4 FragDownsample(v2fSTREAKS i) : SV_Target
	{
		// Actually this should be 1, but we assume you need more blur...
		const float hscale = 1.25;
		const float dx = _MainTexA_TexelSize.x * hscale;

		float2 uv = i.texcoord;
		float u0 = uv.x - dx * 5;
		float u1 = uv.x - dx * 3;
		float u2 = uv.x - dx * 1;
		float u3 = uv.x + dx * 1;
		float u4 = uv.x + dx * 3;
		float u5 = uv.x + dx * 5;

		half3 c0 = _MainTexA.Sample(sampler_MainTexA, float2(u0, uv.y)).rgb;
		half3 c1 = _MainTexA.Sample(sampler_MainTexA, float2(u1, uv.y)).rgb;
		half3 c2 = _MainTexA.Sample(sampler_MainTexA, float2(u2, uv.y)).rgb;
		half3 c3 = _MainTexA.Sample(sampler_MainTexA, float2(u3, uv.y)).rgb;
		half3 c4 = _MainTexA.Sample(sampler_MainTexA, float2(u4, uv.y)).rgb;
		half3 c5 = _MainTexA.Sample(sampler_MainTexA, float2(u5, uv.y)).rgb;

		// Simple box filter
		half3 c = (c0 + c1 + c2 + c3 + c4 + c5) / 6;

		return half4(c, 1);
	}

		// Downsampler
		half4 FragDownsampleV(v2fSTREAKS i) : SV_Target
	{
		// Actually this should be 1, but we assume you need more blur...
		const float hscale = 1.25;
		const float dx = _MainTexA_TexelSize.x * hscale;
		const float dy = _MainTexA_TexelSize.y * hscale;

		float2 uv = i.texcoord;
		float u0 = uv.x - dx * 5;
		float u1 = uv.x - dx * 3;
		float u2 = uv.x - dx * 1;
		float u3 = uv.x + dx * 1;
		float u4 = uv.x + dx * 3;
		float u5 = uv.x + dx * 5;

		float uv0y = uv.y - dy * 5;
		float uv1y = uv.y - dy * 3;
		float uv2y = uv.y - dy * 1;
		float uv3y = uv.y + dy * 1;
		float uv4y = uv.y + dy * 3;
		float uv5y = uv.y + dy * 5;

		half3 c0 = _MainTexA.Sample(sampler_MainTexA, float2(uv.x, uv0y)).rgb;
		half3 c1 = _MainTexA.Sample(sampler_MainTexA, float2(uv.x, uv1y)).rgb;
		half3 c2 = _MainTexA.Sample(sampler_MainTexA, float2(uv.x, uv2y)).rgb;
		half3 c3 = _MainTexA.Sample(sampler_MainTexA, float2(uv.x, uv3y)).rgb;
		half3 c4 = _MainTexA.Sample(sampler_MainTexA, float2(uv.x, uv4y)).rgb;
		half3 c5 = _MainTexA.Sample(sampler_MainTexA, float2(uv.x, uv5y)).rgb;

		// Simple box filter
		half3 c = (c0 + c1 + c2 + c3 + c4 + c5) / 6;

		return half4(c, 1);
	}

		// Upsampler
		half4 FragUpsample(v2fSTREAKS i) : SV_Target
	{
		half3 c0 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 4;
		half3 c1 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 2;
		half3 c2 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 4;
		half3 c3 = _HighTex.Sample(sampler_HighTex, i.texcoord).rgb;
		return half4(lerp(c3, c0 + c1 + c2, _Stretch), 1);
	}
		// UpsamplerV
		half4 FragUpsampleV(v2fSTREAKS i) : SV_Target
	{
		half3 c0 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 4;
		half3 c1 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 2;
		half3 c2 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 4;
		half3 c3 = _HighTex.Sample(sampler_HighTex, i.texcoord).rgb;
		return half4(lerp(c3, c0 + c1 + c2, _StretchV), 1);
	}

		// Final composition
		half4 FragComposition(v2fSTREAKS i) : SV_Target
	{

		half4 lensDirt = SAMPLE_TEXTURE2D(_lensDirtTexture,sampler_lensDirtTexture, i.texcoord);

		half3 c0 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 4;
		half3 c1 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 2;
		half3 c2 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 4;
		half3 c3 = _HighTex.Sample(sampler_HighTex, i.texcoord).rgb;
		half3 cf = (c0 + c1 + c2) * _Color * _IntensitySTREAKS * 5;

		cf *= lensDirt.rgb;
		cf += 40 * lensDirt.rgb * cf + 0.005 * lensDirt.rgb;

		return half4(cf + c3, 1)* _Blend + (1- _Blend)* _HighTex.Sample(sampler_HighTex, i.texcoord);
	}

		sampler2D _BloomTexA;
		float _bloomIntensity;
		float _dirtBloomPower;
		//_lensDirtTexture
		// Final composition Horizontal plus vertical
		half4 FragCompositionHV(v2fSTREAKS i) : SV_Target
	{

		half4 lensDirt = SAMPLE_TEXTURE2D(_lensDirtTexture,sampler_lensDirtTexture, i.texcoord);

		half3 c0 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 4;
		half3 c1 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 2;
		half3 c2 = _MainTexA.Sample(sampler_MainTexA, i.texcoord).rgb / 4;

		half3 c0B = _MainTexB.Sample(sampler_MainTexB, i.texcoord).rgb / 4;
		half3 c1B = _MainTexB.Sample(sampler_MainTexB, i.texcoord).rgb / 2;
		half3 c2B = _MainTexB.Sample(sampler_MainTexB, i.texcoord).rgb / 4;

		half3 c3 = _HighTex.Sample(sampler_HighTex, i.texcoord).rgb;
		half3 cf = (c0 + c1 + c2) * _Color * _IntensitySTREAKS * 5;
		half3 cfV = (c0B + c1B + c2B) * _ColorV * _IntensitySTREAKSV * 5;

		

		//cf *= lensDirt.rgb;
		//cfV *= lensDirt.rgb;
		cf += 10 * lensDirt.rgb * cf;//  +0.0025 * lensDirt.rgb;
		cfV += 10 * lensDirt.rgb * cfV;// +0.0025 * lensDirt.rgb;

		//half4 bloom = abs(pow(tex2D(_BloomTexA, i.texcoord),1.4)- _HighTex.Sample(sampler_HighTex, i.texcoord)) * _bloomIntensity * 1;
		half4 bloom = pow(tex2D(_BloomTexA, i.texcoord), 1.4) * _bloomIntensity* pow(lensDirt,0.75*_dirtBloomPower)*1;


		//return tex2D(_BloomTexA, i.texcoord) * _bloomIntensity;

		float4 outA = (half4(cf + c3/2, 1) + half4(cfV+c3/2, 1)) * _Blend + (1 - _Blend) * _HighTex.Sample(sampler_HighTex, i.texcoord);
		
		return outA + bloom;// *lensDirt;
	}



	////////////////// BLOOM


	frag_in vertBLOOM(vert_in input)
	{
		frag_in output;
		output.vertex = input.pos;
		output.uv = input.uv;

		return output;
	}
	half luma(half3 rgb)
	{
		return dot(rgb.rgb, half3(0.299, 0.587, 0.114));
	}

	half bright(half3 rgb)
	{
		return max(max(rgb.r, rgb.g), rgb.b);
	}
	/*struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}*/

	//sampler2D _MainTex;
	//float4 _MainTex_TexelSize;
	//sampler2D _SourceTex;
	//sampler2D _GhostTex;

	TEXTURE2D(_SourceTex);
	SAMPLER(sampler_SourceTex);
	TEXTURE2D(_GhostTex);
	SAMPLER(sampler_GhostTex);

	//sampler2D _CameraDepthTexture;
	float4 _BloomTint;
	float _Intensity;
	float _BlendFac;
	float _Ghosting;
	float _DistMul;

	static float blurKernel[3] = { -1.5, 0, 1.5 };
	static int kernelSize = 3;

	










	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		half4 screenPos : TEXCOORD1;
		half2 sunScreenPosition : TEXCOORD2;
#if UNITY_UV_STARTS_AT_TOP
		float2 uv1 : TEXCOORD3;
#endif		
	};

	struct v2f_radial {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 blurVector : TEXCOORD1;
		float2 sunScreenPosition: TEXCOORD2;
	};

	struct Varyings
	{
		float2 uv        : TEXCOORD0;
		float4 vertex : SV_POSITION;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	half4x4 _CameraVP[2];
	half3 worldToScreenPositionA(half3 pnt)
	{
		half4x4 camVP = _CameraVP[0];

		half3 result;
		result.x = camVP._m00 * pnt.x + camVP._m01 * pnt.y + camVP._m02 * pnt.z + camVP._m03;
		result.y = camVP._m10 * pnt.x + camVP._m11 * pnt.y + camVP._m12 * pnt.z + camVP._m13;
		result.z = camVP._m20 * pnt.x + camVP._m21 * pnt.y + camVP._m22 * pnt.z + camVP._m23;
		half num = camVP._m30 * pnt.x + camVP._m31 * pnt.y + camVP._m32 * pnt.z + camVP._m33;
		num = 1.0 / num;
		result.x *= num;
		result.y *= num;
		result.z = num;

		result.x = result.x * 0.5 + 0.5;
		result.y = result.y * 0.5 + 0.5;

		return result;
	}

	float2 worldToScreenPosition(float3 pos) {
		pos = normalize(pos - _WorldSpaceCameraPos) * (_ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y)) + _WorldSpaceCameraPos;
		float2 uv = 0;
		float3 toCam = mul(unity_WorldToCamera, pos);
		float camPosZ = toCam.z;
		float height = 2 * camPosZ / unity_CameraProjection._m11;
		float width = _ScreenParams.x / _ScreenParams.y * height;
		uv.x = (toCam.x + width / 2) / width;
		uv.y = (toCam.y + height / 2) / height;
		return uv;
	}

	float Linear01DepthA(float2 uv)
	{
#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		return SAMPLE_TEXTURE2D_ARRAY(_CameraDepthTexture, sampler_CameraDepthTexture, uv, unity_StereoEyeIndex).r;
#else
		return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
#endif

	}
	

	//BLOOM
	float4 frag0(v2f i) : SV_Target
	{
		//return tex2D(_MainTex, float2(i.uv.x, i.uv.y))*2;
		//return 
		// 
		//float4 depth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);
		float4 depth = Linear01Depth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).r, _ZBufferParams);

		//return depth.rrrr;
		depth = saturate(depth * _DistMul);
		depth = depth * _BloomTint;

		float4 ghost = SAMPLE_TEXTURE2D(_GhostTex, sampler_GhostTex, i.uv);

		//return ghost;

		return lerp(depth, ghost, _Ghosting);
	}

		float4 frag1(v2f i) : SV_Target
	{
		float4 col = 0;
		for (int p = 0; p < kernelSize; p++) {
			col += SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, float2(i.uv.x + blurKernel[p] * _MainTexA_TexelSize.x, i.uv.y));
		}
		return col / kernelSize;
	}

		float4 frag2(v2f i) : SV_Target
	{
		float4 col = 0;
		for (int p = 0; p < kernelSize; p++) {
			col += SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, float2(i.uv.x, i.uv.y + blurKernel[p] * _MainTexA_TexelSize.y));
		}
		return col / kernelSize;
	}

		float4 frag3(v2f i) : SV_Target
	{
		float4 bloom = SAMPLE_TEXTURE2D(_MainTexA,sampler_MainTexA, i.uv);
		float4 source = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, i.uv);

		return lerp(source + bloom * _Intensity, bloom * _Intensity , _BlendFac)*_Blend + source * (1-_Blend); // Additive
			// return 1 - (1 - source) * (1 - bloom * _Intensity * _BloomTint); // Screen
	}


	float4 FragGrey(v2f i) : SV_Target
	{
		float4 color = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.uv.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
		return color * 1.5;
	}
		struct Attributes
	{
		float4 positionOS       : POSITION;
		float2 uv               : TEXCOORD0;
	};

	v2f vert(Attributes v) {
		v2f o = (v2f)0;
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);

		o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
		float2 uv = v.uv;

#if !UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif

		o.uv = uv;

#if !UNITY_UV_STARTS_AT_TOP
		o.uv = uv.xy;
		if (_MainTexA_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
#endif	

		o.pos = TransformObjectToHClip(v.positionOS.xyz);

		float3 circle_worldPos = _SunPosition.xyz;
		// this avoids the extra line of code to subtract the world space camera position
		float4 circle_cameraPos = mul(unity_WorldToCamera, float4(circle_worldPos, 1.0));

		// the WorldToCamera matrix is +z forwad, but the projection matrix expects a -z forward view space
		circle_cameraPos.z = -circle_cameraPos.z;

		// transform view space to clip space position
		float4 circle_clipPos = mul(unity_CameraProjection, circle_cameraPos);

		// clip space has a -w to +w range for on screen elements, so divide x and y by w to get a -1 to +1 range
		// then multiply by 0.5 and 0.5 to bring from a -1 to +1 range to 0.0 to 1.0 screen position UV
		float2 circle_screenPos = (circle_clipPos.xy / circle_clipPos.w) * 0.5 + 0.5;

		o.screenPos.xy = circle_screenPos;// ComputeScreenPos(o.pos);
		o.sunScreenPosition = circle_screenPos;//  worldToScreenPosition(_SunPosition.xyz);
		o.screenPos.xy = o.sunScreenPosition;
		o.uv = UnityStereoTransformScreenSpaceTex(v.uv);
		o.uv.y = 1 - o.uv.y;

		return o;
	}
	ENDHLSL



	SubShader
	{
		 ZTest Always Cull Off ZWrite Off
		 
		// No culling or depth
		//Cull Off ZWrite Off ZTest Always

		Pass //0 SubMul
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			//#include "UnityCG.cginc"
			/*struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}*/
			
			//sampler2D _MainTex;
			float _Sub;
			float _Mul;

			float4 frag (v2f i) : SV_Target
			{
				float4 col = SAMPLE_TEXTURE2D(_MainTexA,sampler_MainTexA, i.uv);
				col = max(col-_Sub, 0);
				col *= _Mul;
				return col;
			}
			ENDHLSL
		}

		Pass//1 RadialWarp
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			/*#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}*/

			//sampler2D _MainTex;
			float _HaloWidth;
			float _HaloFalloff;
			float _HaloSub;

			float4 frag(v2f i) : SV_Target
			{
				

				float4 col = float4(0,0,0,0);
				float2 ghostVec = i.uv - .5;

				float2 haloVec = normalize(ghostVec) * -_HaloWidth;
				float weight = length(float2(0.5, 0.5) - (i.uv + haloVec)) / .707;
				weight = pow(1.0 - weight, _HaloFalloff);
				col += SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.uv + haloVec) * weight;

				//return col;

				col = max(0, col - _HaloSub);
				return col;
			}
			ENDHLSL
		}

		Pass//2 LensFlareAberration
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			/*#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}*/

			//sampler2D _MainTex;
			//float4 _DisplaceColor;

			float _ChromaticAberration_Amount;
			//sampler2D _ChromaticAberration_Spectrum;
			int _Distance_Function;
			

			TEXTURE2D(_ChromaticAberration_Spectrum);
			SAMPLER(sampler_ChromaticAberration_Spectrum);

			float4 frag(v2f i) : SV_Target
			{
				float2 coords = 2.0 * i.uv - 1.0;
				float2 end;
				if (_Distance_Function == 0) {
					end = i.uv - coords * _ChromaticAberration_Amount;
				}
				else if (_Distance_Function == 1) {
					end = i.uv - sqrt(length(coords)) * normalize(coords) * _ChromaticAberration_Amount;
				}
				else if (_Distance_Function == 2) {
					end = i.uv - normalize(coords) * _ChromaticAberration_Amount;
				}


				float2 diff = end - i.uv;
				int samples = clamp(int(length(_MainTexB_TexelSize.zw * diff / 2.0)), 3, 16);
				float2 delta = diff / samples;
				float2 pos = i.uv;
				half3 sum = (0.0).xxx, filterSum = (0.0).xxx;


				for (int i = 0; i < samples; i++)
				{
					half t = (i + 0.5) / samples;
					//half3 s = tex2Dlod(_MainTexB, float4(pos, 0, 0)).rgb;
					half3 s = SAMPLE_TEXTURE2D_LOD(_MainTexB,sampler_MainTexB, float4(pos, 0, 0),0).rgb;

					half3 filter = SAMPLE_TEXTURE2D_LOD(_ChromaticAberration_Spectrum, sampler_ChromaticAberration_Spectrum, float4(t, 0,0, 0), 0).rgb;
					//half3 filter = tex2Dlod(_ChromaticAberration_Spectrum, float4(t, 0, 0, 0)).rgb;

					sum += s * filter;
					filterSum += filter;
					pos += delta;
				}

				return float4(sum / filterSum, 1);
			}

			//float4 frag(v2f i) : SV_Target
			//{
			//	//return SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.uv);

			//	//_DisplaceColor("Displacement Color", Color) = (1,1,1,1)
			//	float4 _DisplaceColor = float4(1,1,1,1);

			//	float2 direction = normalize(i.uv - .5);
			//	float r = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.uv + direction * _DisplaceColor.r).r;
			//	float g = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.uv + direction * _DisplaceColor.g).g;
			//	float b = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.uv + direction * _DisplaceColor.b).b;
			//	return float4(r,g,b,1);
			//}
			ENDHLSL
		}

		//GhostFeature
		Pass//3 GhostFeature
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			/*#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}*/

			//sampler2D _MainTex;
			int _NumGhost;
			float _Displace;
			float _Falloff;

			float4 frag(v2f i) : SV_Target
			{
				float4 col = SAMPLE_TEXTURE2D(_MainTexA,sampler_MainTexA, i.uv);
				float2 uv = i.uv - float2(0.5, 0.5);
				for (int k = 3; k < _NumGhost + 3; k++) {
					if (k & 1) {
						col += SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, _Displace * -uv / (k >> 1) + float2(0.5, 0.5));
					}
					else {
						col += SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, uv / (k >> 1) + float2(0.5, 0.5));
						}
				}
				col *= pow(1 - length(uv) / .707, _Falloff);
				return col;
			}
			ENDHLSL
		}

		Pass //4 GaussianBlur
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			/*#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}*/

			//sampler2D _MainTex;
			float _BlurSize;
			float _Sigma;
			int _Direction;

			//float4 _MainTex_TexelSize;

			float g(float x) {
				return pow(2.71829, -x * x / (2 * _Sigma * _Sigma)) / sqrt(2 * 3.141593 * _Sigma * _Sigma);
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 col = float4(0,0,0,0);
				for (int k = -_BlurSize; k <= _BlurSize; k++) {
					col += SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.uv + float2(_Direction * k * _MainTexB_TexelSize.x, (1 - _Direction) * k * _MainTexB_TexelSize.y)) * g(k);
				}
				col.w = 1;
				return col;
			}
			ENDHLSL
		}

			Pass //4a GaussianBlur
			{
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				float _BlurSize;
				float _Sigma;
				int _Direction;

				TEXTURE2D(_MainTexC);
				SAMPLER(sampler_MainTexC);
				float4 _MainTexC_TexelSize;

				float g(float x) {
					return pow(2.71829, -x * x / (2 * _Sigma * _Sigma)) / sqrt(2 * 3.141593 * _Sigma * _Sigma);
				}

				float4 frag(v2f i) : SV_Target
				{
					float4 col = float4(0,0,0,0);
					for (int k = -_BlurSize; k <= _BlurSize; k++) {
						col += SAMPLE_TEXTURE2D(_MainTexC, sampler_MainTexC, i.uv + float2(_Direction * k * _MainTexC_TexelSize.x, (1 - _Direction) * k * _MainTexC_TexelSize.y)) * g(k);
					}
					col.w = 1;
					return col;
				}
				ENDHLSL
			}

		Pass//6 Additive
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

		/*	#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}*/

			//sampler2D _MainTex;
			//sampler2D _MainTex1;
			TEXTURE2D(_MainTex1);
			SAMPLER(sampler_MainTex1);

			float4 frag(v2f i) : SV_Target
			{
				//return  SAMPLE_TEXTURE2D(_MainTex1,sampler_MainTex1, i.uv);
				float4 col = SAMPLE_TEXTURE2D(_MainTexA,sampler_MainTexA, i.uv) + SAMPLE_TEXTURE2D(_MainTexB,sampler_MainTexB, i.uv);
				return col;
			}
			ENDHLSL
		}

				//PASS7 TEST STAGE
				Pass{
						ZTest Always Cull Off ZWrite Off

						HLSLPROGRAM

				/*struct VertexData {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};
			 v2f vert(VertexData v) {
				 v2f o = (v2f)0;
				 UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				 VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
				 o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
				 float2 uv = v.uv;

#if !UNITY_UV_STARTS_AT_TOP
						uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif

						o.uv = uv;
#if !UNITY_UV_STARTS_AT_TOP
						o.uv = uv.xy;
						if (_MainTexA_TexelSize.y < 0)
							o.uv.y = 1 - o.uv.y;
#endif	
						o.pos = TransformObjectToHClip(v.vertex.xyz);
						float3 circle_worldPos = _SunPosition.xyz;
						float4 circle_cameraPos = mul(unity_WorldToCamera, float4(circle_worldPos, 1.0));
						circle_cameraPos.z = -circle_cameraPos.z;
						float4 circle_clipPos = mul(unity_CameraProjection, circle_cameraPos);
						float2 circle_screenPos = (circle_clipPos.xy / circle_clipPos.w) * 0.5 + 0.5;
						o.screenPos.xy = circle_screenPos;
						o.uv = UnityStereoTransformScreenSpaceTex(v.uv);
						o.uv.y = 1 - o.uv.y;
						return o;
					}*/
					float4 FragGreyA(v2f i) : SV_Target
					{	//return float4(1,0,0,1);
						float4 color = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv.xy);
						return color * 1;
					}
					#pragma vertex vert
					#pragma fragment FragGreyA
					ENDHLSL
			}

				// 8: Mask
						Pass //8
					{
						HLSLPROGRAM
						#pragma vertex vert
						#pragma fragment frag0
						//#include "SotCBloom.cginc"
						ENDHLSL
					}

						//9: Blur horizontal
						Pass
					{
						HLSLPROGRAM
						#pragma vertex vert
						#pragma fragment frag1
						//#include "SotCBloom.cginc"
						ENDHLSL
					}

						// 10: Blur vertical
						Pass
					{
						HLSLPROGRAM
						#pragma vertex vert
						#pragma fragment frag2
						//#include "SotCBloom.cginc"
						ENDHLSL
					}

						// 11: Combiner
						Pass
					{
						HLSLPROGRAM
						#pragma vertex vert
						#pragma fragment frag3
						//#include "SotCBloom.cginc"
						ENDHLSL
					}

				/////////////////////////////////////////// BLOOM ////////////////////////////////////////
				//12 BLOOM VOL FX 4 passes
				Pass	// 12
				{
					name "Filter"

					HLSLPROGRAM

					#pragma multi_compile_local _LUMA _BRIGHTNESS _

					#pragma vertex vertBLOOM
					#pragma fragment frag

					//sampler2D    _MainTex;
					sampler2D	 _ValueTex;
					sampler2D	 _ColorTex;

					half4 frag(frag_in i) : SV_Target
					{

						half4 lensDirt = SAMPLE_TEXTURE2D(_lensDirtTexture,sampler_lensDirtTexture, i.uv);

						half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);

						//return col * 3;

						half  val = 0;
						#ifdef _LUMA
										val = luma(col.rgb);
						#endif
						#ifdef _BRIGHTNESS
										val = bright(col.rgb);
						#endif
						// evaluate threshold
						val = tex2D(_ValueTex, half2(val, 0)).r;

						// get color replacement
						half4 tint = tex2D(_ColorTex, half2(val, 0));

						//return lensDirt* lensDirt*lensDirt*10*col;
						//col += 1 * 1 * lensDirt * pow(bright(col.rgb),2)*0.1;
						//col += lensDirt/2;

						return lerp(col * val, col * val * tint, tint.a);
					}

					ENDHLSL
				}

				Pass	// 13
				{
					name "Down Sample"

					HLSLPROGRAM

					#pragma vertex vertBLOOM
					#pragma fragment frag

					//sampler2D    _MainTex;
					//float4		 _MainTex_TexelSize;

					half4 frag(frag_in i) : SV_Target
					{

						//half4 lensDirt = SAMPLE_TEXTURE2D(_lensDirtTexture,sampler_lensDirtTexture, i.uv);

						float4 offset = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1);

						half4 s;
						s = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xy);
						s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zy);
						s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xw);
						s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zw);

						//s -= s * lensDirt*1114* offset.rrrr;


						return s * (1.0 / 4.0);
					}

					ENDHLSL
				}

				Pass	// 14
				{
					name "Up Sample"

					HLSLPROGRAM

					#pragma vertex vertBLOOM
					#pragma fragment frag

					//sampler2D    _MainTex;
					sampler2D    _DownTex;

					//float		 _Blend;
					float4		 _DownTex_TexelSize;
					//float4		 _MainTex_TexelSize;

					half4 frag(frag_in i) : SV_Target
					{

						//half4 lensDirt = SAMPLE_TEXTURE2D(_lensDirtTexture,sampler_lensDirtTexture, i.uv);

						float4 offset = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1);
						half4 s;
						s = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xy);
						s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zy);
						s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xw);
						s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zw);

						s = s * (1.0 / 4);
						half4 down = tex2D(_DownTex, i.uv);



						//s -= s * lensDirt* offset.rrrr*110;

						return s + down * _Blend;
					}

					ENDHLSL
				}

				Pass	// 15
				{
					name "Combine"

					HLSLPROGRAM

					#pragma vertex vertBLOOM
					#pragma fragment frag

					#pragma multi_compile_local _BLOOM_ONLY _

					//sampler2D    _MainTex;
					sampler2D    _BloomTex;

					//float		 _Intensity;

					half4 frag(frag_in i) : SV_Target
					{
						half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
						half4 bloom = tex2D(_BloomTex, i.uv) * _Intensity;

						#ifdef _BLOOM_ONLY
										return bloom;
						#endif
						return col + bloom;
						return half4(col.rgb + bloom.rgb, col.a);
					}

					ENDHLSL
				} //END PASS 15
				/////////////////////////////////////////// END BLOOM ////////////////////////////////////////

				/////////////////////////////////////////// STREAKS ////////////////////////////////////////
					Pass // PASS 16
				{
					HLSLPROGRAM
					#pragma vertex VertINV
					#pragma fragment FragPrefilter
					ENDHLSL
				}
					Pass // PASS 17
				{
					HLSLPROGRAM
					#pragma vertex VertINV
					#pragma fragment FragDownsample
					ENDHLSL
				}
					Pass // PASS 18
				{
					HLSLPROGRAM
					#pragma vertex VertINV
					#pragma fragment FragUpsample
					ENDHLSL
				}
					Pass // PASS 19
				{
					HLSLPROGRAM
					#pragma vertex VertINV
					#pragma fragment FragComposition
					ENDHLSL
				}
					Pass // PASS 20
				{
					HLSLPROGRAM
					#pragma vertex VertINV
					#pragma fragment FragDownsampleV
					ENDHLSL
				}
					Pass // PASS 21
				{
					HLSLPROGRAM
					#pragma vertex VertINV
					#pragma fragment FragCompositionHV
					ENDHLSL
				}
					Pass // PASS 22
				{
					HLSLPROGRAM
					#pragma vertex VertINV
					#pragma fragment FragUpsampleV
					ENDHLSL
				}
						Pass // PASS 23
					{
						HLSLPROGRAM
						#pragma vertex VertINV
						#pragma fragment FragPrefilterV
						ENDHLSL
					}
				/////////////////////////////////////////// END STREAKS ////////////////////////////////////////

	}
}
