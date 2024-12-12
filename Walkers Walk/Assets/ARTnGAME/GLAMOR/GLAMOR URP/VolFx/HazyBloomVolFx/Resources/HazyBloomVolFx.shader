// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
Shader "Hidden/HazyBloomVolFx"
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

#define SAMPLES_FLOAT 16.0f
#define SAMPLES_INT 16

	float2 TransformTriangleVertexToUV(float2 vertex)
	{
		float2 uv = (vertex + 1.0) * 0.5;
		return uv;
	}




	////////////////// BLOOM
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

	}
}
