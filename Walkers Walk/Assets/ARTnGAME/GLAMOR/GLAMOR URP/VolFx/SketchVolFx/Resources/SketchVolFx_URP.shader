Shader "Unlit/SketchVolFx_URP"
{

	Properties
	{
		//[HideInInspector] _MainTex("Base (RGB)", 2D) = "white" {}
		//[HideInInspector]_ColorBuffer("Base (RGB)", 2D) = "white" {}

		//[HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
		//_Delta("Line Thickness", Range(0.0005, 0.0025)) = 0.001
		//[Toggle(RAW_OUTLINE)]_Raw("Outline Only", Float) = 0
		//[Toggle(POSTERIZE)]_Poseterize("Posterize", Float) = 0
		//_PosterizationCount("Count", int) = 8

		//_SunThreshold("sun thres", Color) = (0.87, 0.74, 0.65,1)
		//_SunColor("sun color", Color) = (1.87, 1.74, 1.65,1)
		//_BlurRadius4("blur", Color) = (0.00325, 0.00325, 0,0)
		//_SunPosition("sun pos", Vector) = (111, 11,339, 11)

		//NOISE
		_MainTexFBM("Texture", 2D) = "white" {}
		_Tex2("_Tex2", 2D) = "white" {}
		//_MaskTex("_MaskTex", 2D) = "white" {}
		_Distort("_Distort", Float) = 0.5
		_HighLight("_HighLight", Color) = (1,1,1,1)
		_noiseColor("_Color", Color) = (1,1,1,1)
		_Pow("_Pow", Float) = 0.5
		brightnessContrast("Brightness-Contrast ", Vector) = (1, 1, 1, 1)
			//_CloudSpeed("Cloud Speed", Vector) = (0.001, 0, 0, 0)
			cloudSpeed("Increase cloud Speed ", Vector) = (1, 1, 1, 1)

			//IMPOSTOR LIGHTS
			//v1.9.9.1
			lightsArrayLength("lightsArrayLength", Int) = 0

	}

		HLSLINCLUDE
			//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl" //unity 2018.3
//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl" 
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		//#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/SurfaceInput.hlsl"
		//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		//#include "PostProcessing/Shaders/StdLib.hlsl" //unity 2018.1-2
		//#include "UnityCG.cginc"

			//FOG
//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Colors.hlsl"

//#include "ClassicNoise3D.hlsl"



		//IMPOSTOR LIGHTS		
		//v1.9.9.1
			int lightsArrayLength = 0;
		float4 _LightsArrayPos[128];//position and power
		float4 _LightsArrayDir[128];//direction (0,0,0 for point) and falloff
		float4 _LightsArrayColor[128];


		//v0.2
		float _Distort;
		float4 _HighLight;
		float4 _noiseColor;
		float _Pow;
		float4 brightnessContrast;
		//sampler2D _MainTexFBM;
		TEXTURE2D(_MainTexFBM);
		SAMPLER(sampler_MainTexFBM);
		float4 _MainTexFBM_ST;
		//sampler2D _Tex2;
		TEXTURE2D(_Tex2);
		SAMPLER(sampler_Tex2);
		float4 _Tex2_ST;
		//sampler2D _MaskTex;
		//float4 _MaskTex_ST;
		//float4 _CloudSpeed;
		float4 cloudSpeed;

		float4 _SunColor;
		float4 _SunPosition;
		float4 _SunThreshold;
		float3 _BlurRadius4;

		TEXTURE2D(_MainTexA);
		SAMPLER(sampler_MainTexA);

		TEXTURE2D(_MainTex);
		TEXTURE2D(_ColorBuffer);
		TEXTURE2D(_Skybox);

		SAMPLER(sampler_MainTex);
		SAMPLER(sampler_ColorBuffer);
		SAMPLER(sampler_Skybox);
		float _Blend;
		float _brightness;
		float _contrast;

		//sampler2D _MainTex;
		//sampler2D _ColorBuffer;
		//sampler2D _Skybox;
		//sampler2D_float _CameraDepthTexture;
		TEXTURE2D(_CameraDepthTexture);
		SAMPLER(sampler_CameraDepthTexture);
		half4 _CameraDepthTexture_ST;

		/*half4 _SunThreshold = half4(0.87, 0.74, 0.65, 1);

		half4 _SunColor = half4(0.87, 0.74, 0.65, 1);
		uniform half4 _BlurRadius4 = half4(2.5 / 768, 2.5 / 768, 0.0, 0.0);
		uniform half4 _SunPosition = half4(1, 1, 1, 1);*/
		uniform half4 _MainTex_TexelSize;
		uniform half4 _MainTexA_TexelSize;

#define SAMPLES_FLOAT 16.0f
#define SAMPLES_INT 16

		// Vertex manipulation
		float2 TransformTriangleVertexToUV(float2 vertex)
		{
			float2 uv = (vertex + 1.0) * 0.5;
			return uv;
		}

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

		//sampler2D _MainTexA
		TEXTURE2D(_MainTexB);
		SAMPLER(sampler_MainTexB);

		float4 FragGrey(v2f i) : SV_Target
		{
			float4 color = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.uv.xy);//
			// color = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, float2(i.uv.x,1-i.uv.y));
			half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
			//float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.0721750));
			//color.rgb = lerp(color.rgb, luminance.xxx, _Blend.xxx);
			//return color/2 + colorB/2;
			return color * 1.5;
		}

			half4 fragScreen(v2f i) : SV_Target{

						UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		//half4 colorA = tex2D(_MainTex, i.uv.xy);
		half4 colorA = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv.xy); // half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
	#if !UNITY_UV_STARTS_AT_TOP
																			 ///half4 colorB = tex2D(_ColorBuffer, i.uv1.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);//v0.2 //i.uv1.xy);//v0.2
	#else
																			 //half4 colorB = tex2D(_ColorBuffer, i.uv.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);//v1.1
	#endif
		half4 depthMask = saturate(colorB * _SunColor);
		return  1.0f - (1.0f - colorA) * (1.0f - depthMask);//colorA * 5.6;// 1.0f - (1.0f - colorA) * (1.0f - depthMask);
		}


			half4 fragAdd(v2f i) : SV_Target{

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		//half4 colorA = tex2D(_MainTex, i.uv.xy);
		half4 colorA = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv.xy);
#if !UNITY_UV_STARTS_AT_TOP
		//half4 colorB = tex2D(_ColorBuffer, i.uv1.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy); //v0.1 - i.uv1.xy
#else
		//half4 colorB = tex2D(_ColorBuffer, i.uv.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
#endif
		half4 depthMask = saturate(colorB * _SunColor);







		////// NOISE
		//Get the colors at the right point on the first texture
		half4 col = SAMPLE_TEXTURE2D(_MainTexFBM, sampler_MainTexFBM, i.uv.xy * _MainTexFBM_ST.xy + _MainTexFBM_ST.zw); //tex2D(_MainTexFBM, i.uv.xy * _MainTexFBM_ST.xy + _MainTexFBM_ST.zw);

		//Use that to create an offset for the second texture
		//float2 offset = float2(_Distort*(col.x - .5), _Distort*(col.y - .5));
		//float2 offset = float2(_Distort * (col.x - .5), _Distort * (col.y - .5)) + float2(_Time.y * 0.001 * cloudSpeed.x, _Time.y * 0.001 * cloudSpeed.y);
		float2 offset = float2(_Distort * (col.x - .5), _Distort * (col.y - .5)) + float2(_Time.y * 0.25 * cloudSpeed.x, _Time.y * 0.011 * cloudSpeed.y) * 1;

		//Get the colors from the second texture, using the offset to distort the image
		half4 col2 = SAMPLE_TEXTURE2D(_Tex2, sampler_Tex2, i.uv.xy * _MainTexFBM_ST.xy + _MainTexFBM_ST.zw + offset); //tex2D(_Tex2, i.uv.xy * _MainTexFBM_ST.xy + _MainTexFBM_ST.zw + offset);

		//return pow(depthMask,3.5);
		return  colorA + brightnessContrast.x * 0.45 * depthMask + brightnessContrast.y * 0.75 * col.r * col2.r * pow(depthMask, 3.5 * brightnessContrast.z);//brightnessContrast

		return  colorA + depthMask + col.r * col2.r * depthMask * 1;
		//Create a circular mask: if we're close to the edge the value is 0
		//If we're by the center the value is 1
		//By multipling the final alpha by this, we mask the edges of the box
		float radA = max(1 - max(length(half2(.5, .5) - i.uv.xy) - .25, 0) / .25, 0);

		//Get the mask color from our mask texture
		//half4 mask = tex2D(_MaskTex, i.uv.xy * _MaskTex_ST.xy + _MaskTex_ST.zw);

		//Add the color portion : apply the gradient from the highlight to the color
		//To the gray value from the blue channel of the distorted noise
		float3 final_color = lerp(_HighLight, _noiseColor, col2.b * .5).rgb;

		//calculate the final alpha value:
		//First combine several of the distorted noises together
		float final_alpha = col2.a * col2.g * col.b;

		//Apply the a combination of two tendril masks
		//final_alpha *= mask.g * mask.r;//

		//Apply the circular mask
		final_alpha *= radA;

		//Raise it to a power to dim it a bit 
		//it should be between 0 and 1, so the higher the power
		//the more transparent it becomes
		final_alpha = pow(final_alpha, _Pow);

		//Finally, makes sure its never more than 90% opaque
		final_alpha = min(final_alpha, .9);

		//v0.1
		final_color = pow(final_color, 5);

		//We're done! Return the final pixel color!
		float4 finalOUT = float4(final_color, final_alpha);

		finalOUT = pow(finalOUT, brightnessContrast.y) * brightnessContrast.x;



		/*
		float3 pos = (i.pos - _WorldSpaceCameraPos) * 1;
		float2 newUVA = float2(0, 0);
		newUVA.x = 0.5 + atan2(pos.z, pos.x) / (PI * 2);
		newUVA.y = 0.5 - asin(pos.y) / PI;

		float2 dx = ddx(newUVA);
		float2 dy = ddy(newUVA);
		float2 du = float2(dx.x, dy.x);
		du -= (abs(du) > 0.5f) * sign(du);
		dx.x = du.x;
		dy.x = du.y;

		// In case you want to rotate your view using the texture x-offset.
		newUVA.x += _MainTexFBM_ST.z;

		// Sample the texture with our calculated UV & seam fixup.
	//	float4 col = SAMPLE_TEXTURE2D_GRAD(_MainTexFBM, sampler_MainTexFBM, newUVA, dx, dy);// tex2Dgrad(_MainTexFBM, newUVA, dx, dy);//



		float2 UVs = newUVA;// i.uv.xy;// -_WorldSpaceCameraPos.xz * 1.4;
		//sunScreenPosition

		///// NOISE
				//Get the colors at the right point on the first texture
		half4 col = SAMPLE_TEXTURE2D(_MainTexFBM, sampler_MainTexFBM, UVs * _MainTexFBM_ST.xy + _MainTexFBM_ST.zw); //tex2D(_MainTexFBM, i.uv.xy * _MainTexFBM_ST.xy + _MainTexFBM_ST.zw);

		//Use that to create an offset for the second texture
		//float2 offset = float2(_Distort*(col.x - .5), _Distort*(col.y - .5));
		//float2 offset = float2(_Distort * (col.x - .5), _Distort * (col.y - .5)) + float2(_Time.y * 0.001 * cloudSpeed.x, _Time.y * 0.001 * cloudSpeed.y);
		float2 offset = float2(_Distort * (col.x - .5), _Distort * (col.y - .5)) + float2(_Time.y * 0.101 * 1, _Time.y * 0.011 * 1);

		col = SAMPLE_TEXTURE2D(_MainTexFBM, sampler_MainTexFBM, UVs * _MainTexFBM_ST.xy + _MainTexFBM_ST.zw + offset);
		col = SAMPLE_TEXTURE2D_GRAD(_MainTexFBM, sampler_MainTexFBM, UVs + offset, dx, dy);
		return colorA + col * depthMask;
	//
		//Get the colors from the second texture, using the offset to distort the image
		//half4 col2 = SAMPLE_TEXTURE2D(_Tex2, sampler_Tex2, UVs * _MainTexFBM_ST.xy + _MainTexFBM_ST.zw + offset); //tex2D(_Tex2, i.uv.xy * _MainTexFBM_ST.xy + _MainTexFBM_ST.zw + offset);
		float4 col2 = SAMPLE_TEXTURE2D_GRAD(_Tex2, sampler_Tex2, UVs + offset, dx, dy);
		//float4 col2 = SAMPLE_TEXTURE2D(_Tex2, sampler_Tex2, newUVA);

		//Create a circular mask: if we're close to the edge the value is 0
		//If we're by the center the value is 1
		//By multipling the final alpha by this, we mask the edges of the box
		float radA = max(1 - max(length(half2(.5, .5) - UVs) - .25, 0) / .25, 0);

		//Get the mask color from our mask texture
		//half4 mask = tex2D(_MaskTex, i.uv.xy * _MaskTex_ST.xy + _MaskTex_ST.zw);

		//Add the color portion : apply the gradient from the highlight to the color
		//To the gray value from the blue channel of the distorted noise
		float3 final_color = lerp(_HighLight, _noiseColor, col2.b * .5).rgb;

		//calculate the final alpha value:
		//First combine several of the distorted noises together
		float final_alpha = col2.a * col2.g * col.b;

		//Apply the a combination of two tendril masks
		//final_alpha *= mask.g * mask.r;//

		//Apply the circular mask
		final_alpha *= radA;

		//Raise it to a power to dim it a bit
		//it should be between 0 and 1, so the higher the power
		//the more transparent it becomes
		final_alpha = pow(final_alpha, _Pow);

		//Finally, makes sure its never more than 90% opaque
		final_alpha = min(final_alpha, .9);

		//v0.1
		final_color = pow(final_color, 5);

		//We're done! Return the final pixel color!
		float4 finalOUT = float4(final_color, final_alpha);

		finalOUT = pow(finalOUT, brightnessContrast.y) * brightnessContrast.x;
		*/
		return depthMask;
		return  colorA + 0.12 * depthMask + 0.7 * finalOUT * pow(depthMask,5);











		return 1 * colorA + depthMask;
		}

			struct Attributes
		{
			float4 positionOS       : POSITION;
			float2 uv               : TEXCOORD0;
		};

		v2f vert(Attributes v) {//v2f vert(AttributesDefault v) { //appdata_img v) {
								//v2f o;
			v2f o = (v2f)0;
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			//VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
			//o.pos = vertexInput.positionCS;
			//o.uv = v.uv;
			//Varyings output = (Varyings)0;
			//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
			VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
			//output.vertex = vertexInput.positionCS;
			//output.uv = input.uv;
			//return output;


			//o.pos = UnityObjectToClipPos(v.vertex);
			//	o.pos = float4(v.vertex.xy, 0.0, 1.0);
			//	float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

			o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
			float2 uv = v.uv;

			//o.uv = uv;// v.texcoord.xy;

			//o.uv1 = uv.xy;



			//// NEW 1
			//o.pos = float4(v.positionOS.xy, 0.0, 1.0);
			//uv = TransformTriangleVertexToUV(v.positionOS.xy);

#if !UNITY_UV_STARTS_AT_TOP
			uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
			//uv.y = 1-uv.y;
#endif

			o.uv = uv;// v.texcoord.xy;

#if !UNITY_UV_STARTS_AT_TOP
			o.uv = uv.xy;//o.uv1 = uv.xy;
			if (_MainTexA_TexelSize.y < 0)
				o.uv.y = 1 - o.uv.y;//o.uv1.y = 1 - o.uv1.y;
#endif	



			o.pos = TransformObjectToHClip(v.positionOS.xyz);


			float3 circle_worldPos = _SunPosition.xyz;
			// this avoids the extra line of code to subtract the world space camera position
			float4 circle_cameraPos = mul(unity_WorldToCamera, float4(circle_worldPos, 1.0));

			// if behind the camera, ignore
			//if (circle_cameraPos.z <= 0.0)
			//	return tex;

			// the WorldToCamera matrix is +z forwad, but the projection matrix expects a -z forward view space
			circle_cameraPos.z = -circle_cameraPos.z;

			// transform view space to clip space position
			float4 circle_clipPos = mul(unity_CameraProjection, circle_cameraPos);

			// clip space has a -w to +w range for on screen elements, so divide x and y by w to get a -1 to +1 range
			// then multiply by 0.5 and 0.5 to bring from a -1 to +1 range to 0.0 to 1.0 screen position UV
			float2 circle_screenPos = (circle_clipPos.xy / circle_clipPos.w) * 0.5 + 0.5;

			//circle_screenPos.y = 1 - circle_screenPos.y;

			o.screenPos.xy = circle_screenPos;// ComputeScreenPos(o.pos);







			o.sunScreenPosition = circle_screenPos;//  worldToScreenPosition(_SunPosition.xyz);
			o.screenPos.xy = o.sunScreenPosition;
			o.uv = UnityStereoTransformScreenSpaceTex(v.uv);

			/*#if UNITY_UV_STARTS_AT_TOP
					if (_ColorTexture_TexelSize.y < 0)
					{
						o.uv.y = 1 - o.uv.y;
					}
			#endif*/
			o.uv.y = 1 - o.uv.y;

			return o;
		}


		v2f_radial vert_radial(Attributes v) {//v2f_radial vert_radial(AttributesDefault v) { //appdata_img v) {
			//v2f_radial o;

			v2f_radial o = (v2f_radial)0;
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			////		o.pos = UnityObjectToClipPos(v.vertex);

			//o.pos = float4(v.vertex.xyz,1);
			//o.pos = float4(v.vertex.xy, 0.0, 1.0);
			//float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

			VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
			o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
			float2 uv = v.uv;
			//output.vertex = vertexInput.positionCS;

			//uv = TransformTriangleVertexToUV(vertexInput.positionCS.xy);

#if !UNITY_UV_STARTS_AT_TOP
		//uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif


//uv.y += 0.5;
//uv.y *= 1.25;



			o.uv.xy = uv;//v.texcoord.xy;
						 //o.blurVector = (_SunPosition.xy - v.texcoord.xy) * _BlurRadius4.xy;
			//o.uv1 = uv.xy;
			//o.uv.y = 1 - o.uv.y;
			//uv.y = 1 - uv.y;
			//o.uv.y = 1 - o.uv.y;
			//_SunPosition.y = _SunPosition.y*0.5 + 0.5;
			//_SunPosition.x = _SunPosition.x*0.5 + 0.5;
			//uv.x += 0.5;
			//uv.x *= 0.5+0.5;
			//uv.y *= 1.2;
			//uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
			o.blurVector = (_SunPosition.xy - uv.xy) * _BlurRadius4.xy;

			o.pos = TransformObjectToHClip(v.positionOS.xyz);


			float3 circle_worldPos = _SunPosition.xyz;
			// this avoids the extra line of code to subtract the world space camera position
			float4 circle_cameraPos = mul(unity_WorldToCamera, float4(circle_worldPos, 1.0));

			// if behind the camera, ignore
			//if (circle_cameraPos.z <= 0.0)
			//	return tex;

			// the WorldToCamera matrix is +z forwad, but the projection matrix expects a -z forward view space
			circle_cameraPos.z = -circle_cameraPos.z;

			// transform view space to clip space position
			float4 circle_clipPos = mul(unity_CameraProjection, circle_cameraPos);

			// clip space has a -w to +w range for on screen elements, so divide x and y by w to get a -1 to +1 range
			// then multiply by 0.5 and 0.5 to bring from a -1 to +1 range to 0.0 to 1.0 screen position UV
			float2 circle_screenPos = (circle_clipPos.xy / circle_clipPos.w) * 0.5 + 0.5;
			//circle_screenPos.y = 1 - circle_screenPos.y;


			half2 sunScreenPosition = circle_screenPos;// worldToScreenPosition(_SunPosition.xyz + _WorldSpaceCameraPos * 0);
			o.sunScreenPosition = sunScreenPosition.xy;// ComputeScreenPos(o.pos);//    sunScreenPosition;

			o.uv = UnityStereoTransformScreenSpaceTex(v.uv);



			o.blurVector = (sunScreenPosition.xy - o.uv.xy) * _BlurRadius4.xy;

			return o;
		}

		half4 frag_radial(v2f_radial i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			half4 color = half4(0,0,0,0);
			for (int j = 0; j < SAMPLES_INT; j++)
			{
				//half4 tmpColor = tex2D(_MainTex, i.uv.xy);
				half4 tmpColor = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv.xy);
				color += tmpColor;
				i.uv.xy += i.blurVector;
			}
			return color / SAMPLES_FLOAT;
		}

			half TransformColor(half4 skyboxValue) {
			return dot(max(skyboxValue.rgb - _SunThreshold.rgb, half3(0, 0, 0)), half3(1, 1, 1)); // threshold and convert to greyscale
		}

		half4 frag_depth(v2f i) : SV_Target{

			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		#if !UNITY_UV_STARTS_AT_TOP
		//float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv1.xy);
		float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy), _ZBufferParams); //v0.1 URP i.uv1.xy
#else
		//float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
		float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy), _ZBufferParams);
#endif

		//half4 tex = tex2D(_MainTex, i.uv.xy);
		half4 tex = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv.xy);
		//depthSample = Linear01Depth(depthSample, _ZBufferParams);

		//i.uv.x += 0.5;
		//i.uv.x *= 1.25;

		// consider maximum radius
	#if !UNITY_UV_STARTS_AT_TOP
		half2 vec = i.sunScreenPosition.xy - i.uv.xy;  // _SunPosition.xy - i.uv.xy; //i.uv1.xy;
	#else
		half2 vec = i.sunScreenPosition.xy - i.uv.xy; //_SunPosition.xy - i.uv.xy;
	#endif
		half dist = saturate(_SunPosition.w - length(vec.xy));

		half4 outColor = 0;

		// consider shafts blockers
		//if (depthSample > 0.99)
		//if (depthSample > 0.103)
		if (depthSample > 1 - 0.018) {//if (depthSample < 0.018) {
			//outColor = TransformColor(tex) * dist;
		}





#if !UNITY_UV_STARTS_AT_TOP
		if (depthSample < 0.018) {
			outColor = TransformColor(tex) * dist;
		}
#else
		if (depthSample > 1 - 0.018) {
			outColor = TransformColor(tex) * dist;
		}
#endif

		return outColor * 1;
		}

			//inline half Luminance(half3 rgb)
			//{
				//return dot(rgb, unity_ColorSpaceLuminance.rgb);
			//	return dot(rgb, rgb);
			//}

			half4 frag_nodepth(v2f i) : SV_Target{

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

						#if !UNITY_UV_STARTS_AT_TOP
		//float4 sky = (tex2D(_Skybox, i.uv1.xy));
		float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.uv.xy);
#else
		//float4 sky = (tex2D(_Skybox, i.uv.xy));
		float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.uv.xy); //i.uv1.xy;
#endif




		//float4 tex = (tex2D(_MainTex, i.uv.xy));
		half4 tex = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv.xy);


		//i.uv.x += 0.5;
		//i.uv.x *= 1.25;
		//sky = float4(0.3, 0.05, 0.05,  1);
		/// consider maximum radius
#if !UNITY_UV_STARTS_AT_TOP
		half2 vec = _SunPosition.xy - i.uv.xy;
#else
		half2 vec = _SunPosition.xy - i.uv.xy;//i.uv1.xy;
#endif
		half dist = saturate(_SunPosition.w - length(vec));

		half4 outColor = 0;

		// find unoccluded sky pixels
		// consider pixel values that differ significantly between framebuffer and sky-only buffer as occluded


		if (Luminance(abs(sky.rgb - tex.rgb)) < 0.2) {
			outColor = TransformColor(tex) * dist;
			//outColor = TransformColor(sky) * dist;
		}

		return outColor * 1;
		}



			/////// VOLUME FOG



		//float4x4 unity_CameraInvProjection;

		//TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		//TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
		//TEXTURE2D_SAMPLER2D(_NoiseTex, sampler_NoiseTex);

		TEXTURE2D(_NoiseTex);
		SAMPLER(sampler_NoiseTex);

		//MINE
//#pragma multi_compile FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile _ RADIAL_DIST
#pragma multi_compile _ USE_SKYBOX
		float _DistanceOffset;
		float _Density;
		float _LinearGrad;
		float _LinearOffs;
		float _Height;
		float _cameraRoll;
		//WORLD RECONSTRUCT	
		//float4x4 _InverseView;
		//float4x4 _camProjection;	////TO REMOVE
		// Fog/skybox information
		half4 _FogColor;
		samplerCUBE _SkyCubemap;
		half4 _SkyCubemap_HDR;
		half4 _SkyTint;
		half _SkyExposure;
		float _SkyRotation;
		float4 _cameraDiff;
		float _cameraTiltSign;

		float _NoiseDensity;
		float _NoiseScale;
		float3 _NoiseSpeed;
		float _NoiseThickness;
		float _OcclusionDrop;
		float _OcclusionExp;
		int noise3D = 0;

		// Applies one of standard fog formulas, given fog coordinate (i.e. distance)
		half ComputeFogFactorA(float coord)
		{
			float fog = 0.0;
#if FOG_LINEAR
			// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
			fog = coord * _LinearGrad + _LinearOffs;
#elif FOG_EXP
			// factor = exp(-density*z)
			fog = _Density * coord;
			fog = exp2(-fog);
#else // FOG_EXP2
			// factor = exp(-(density*z)^2)
			fog = _Density * coord;
			fog = exp2(-fog * fog);
#endif
			return saturate(fog);
		}

		// Distance-based fog
		float ComputeDistance(float3 ray, float depth)
		{
			float dist;
#if RADIAL_DIST
			dist = length(ray * depth);
#else
			dist = depth * _ProjectionParams.z;
#endif
			// Built-in fog starts at near plane, so match that by
			// subtracting the near value. Not a perfect approximation
			// if near plane is very large, but good enough.
			dist -= _ProjectionParams.y;
			return dist;
		}

		////LOCAL LIGHT
		float4 localLightColor;
		float4 localLightPos;

		/////////////////// SCATTER
		bool doDistance;
		bool doHeight;
		// Distance-based fog

		uniform float4 _CameraWS;
		//SM v1.7
		uniform float luminance, Multiplier1, Multiplier2, Multiplier3, bias, lumFac, contrast, turbidity;
		//uniform float mieDirectionalG = 0.7,0.913; 
		float mieDirectionalG;
		float mieCoefficient;//0.054
		float reileigh;


		//SM v1.7
		uniform sampler2D _ColorRamp;
		uniform float _Close;
		uniform float _Far;
		uniform float3 v3LightDir;		// light source
		uniform float FogSky;
		float4 _TintColor; //float3(680E-8, 1550E-8, 3450E-8);
		uniform float ClearSkyFac;
		uniform float4 _HeightParams;

		// x = start distance
		uniform float4 _DistanceParams;

		int4 _SceneFogMode; // x = fog mode, y = use radial flag
		float4 _SceneFogParams;
#ifndef UNITY_APPLY_FOG
		//half4 unity_FogColor;
		half4 unity_FogDensity;
#endif	


		uniform float e = 2.71828182845904523536028747135266249775724709369995957;
		uniform float pi = 3.141592653589793238462643383279502884197169;
		uniform float n = 1.0003;
		uniform float N = 2.545E25;
		uniform float pn = 0.035;
		uniform float3 lambda = float3(680E-9, 550E-9, 450E-9);
		uniform float3 K = float3(0.686, 0.678, 0.666);//const vec3 K = vec3(0.686, 0.678, 0.666);
		uniform float v = 4.0;
		uniform float rayleighZenithLength = 8.4E3;
		uniform float mieZenithLength = 1.25E3;
		uniform float EE = 1000.0;
		uniform float sunAngularDiameterCos = 0.999956676946448443553574619906976478926848692873900859324;
		// 66 arc seconds -> degrees, and the cosine of that
		float cutoffAngle = 3.141592653589793238462643383279502884197169 / 1.95;
		float steepness = 1.5;
		// Linear half-space fog, from https://www.terathon.com/lengyel/Lengyel-UnifiedFog.pdf
		float ComputeHalfSpace(float3 wsDir)
		{
			//float4 _HeightParams = float4(1,1,1,1);

			//wsDir.y = wsDir.y - abs(11.2*_cameraDiff.x);// -0.4;// +abs(11.2*_cameraDiff.x);

			float3 wpos = _CameraWS.xyz + wsDir; // _CameraWS + wsDir;

			float FH = _HeightParams.x;
			float3 C = _CameraWS.xyz;
			float3 V = wsDir;
			float3 P = wpos;
			float3 aV = _HeightParams.w * V;
			float FdotC = _HeightParams.y;
			float k = _HeightParams.z;
			float FdotP = P.y - FH;
			float FdotV = wsDir.y;
			float c1 = k * (FdotP + FdotC);
			float c2 = (1 - 2 * k) * FdotP;
			float g = min(c2, 0.0);
			g = -length(aV) * (c1 - g * g / abs(FdotV + 1.0e-5f));
			return g;
		}

		//SM v1.7
		float3 totalRayleigh(float3 lambda) {
			float pi = 3.141592653589793238462643383279502884197169;
			float n = 1.0003; // refraction of air
			float N = 2.545E25; //molecules per air unit volume 								
			float pn = 0.035;
			return (8.0 * pow(pi, 3.0) * pow(pow(n, 2.0) - 1.0, 2.0) * (6.0 + 3.0 * pn)) / (3.0 * N * pow(lambda, float3(4.0, 4.0, 4.0)) * (6.0 - 7.0 * pn));
		}

		float rayleighPhase(float cosTheta)
		{
			return (3.0 / 4.0) * (1.0 + pow(cosTheta, 2.0));
		}

		float3 totalMie(float3 lambda, float3 K, float T)
		{
			float pi = 3.141592653589793238462643383279502884197169;
			float v = 4.0;
			float c = (0.2 * T) * 10E-18;
			return 0.434 * c * pi * pow((2.0 * pi) / lambda, float3(v - 2.0, v - 2.0, v - 2.0)) * K;
		}

		float hgPhase(float cosTheta, float g)
		{
			float pi = 3.141592653589793238462643383279502884197169;
			return (1.0 / (4.0 * pi)) * ((1.0 - pow(g, 2.0)) / pow(abs(1.0 - 2.0 * g * cosTheta + pow(g, 2.0)), 1.5));
		}

		float sunIntensity(float zenithAngleCos)
		{
			float cutoffAngle = 3.141592653589793238462643383279502884197169 / 1.95;//pi/
			float steepness = 1.5;
			float EE = 1000.0;
			return EE * max(0.0, 1.0 - exp(-((cutoffAngle - acos(zenithAngleCos)) / steepness)));
		}

		float logLuminance(float3 c)
		{
			return log(c.r * 0.2126 + c.g * 0.7152 + c.b * 0.0722);
		}

		float3 tonemap(float3 HDR)
		{
			float Y = logLuminance(HDR);
			float low = exp(((Y * lumFac + (1.0 - lumFac)) * luminance) - bias - contrast / 2.0);
			float high = exp(((Y * lumFac + (1.0 - lumFac)) * luminance) - bias + contrast / 2.0);
			float3 ldr = (HDR.rgb - low) / (high - low);
			return float3(ldr);
		}

		/////////////////// END SCATTER


		half _Opacity;

		struct VaryingsA
		{
			float4 position : SV_Position;
			float2 texcoord : TEXCOORD0;
			float3 ray : TEXCOORD1;
			float2 uvFOG : TEXCOORD2;
			float4 interpolatedRay : TEXCOORD3;
		};

		// Vertex shader that procedurally outputs a full screen triangle
		VaryingsA Vertex(uint vertexID : SV_VertexID)
		{
			// Render settings
			float far = _ProjectionParams.z;
			float2 orthoSize = unity_OrthoParams.xy;
			float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic

			// Vertex ID -> clip space vertex position
			float x = (vertexID != 1) ? -1 : 3;
			float y = (vertexID == 2) ? -3 : 1;
			float3 vpos = float3(x, y, 1.0);

			// Perspective: view space vertex position of the far plane
			float3 rayPers = mul(unity_CameraInvProjection, vpos.xyzz * far).xyz;
			//rayPers.y = rayPers.y - abs(_cameraDiff.x * 15111);

			// Orthographic: view space vertex position
			float3 rayOrtho = float3(orthoSize * vpos.xy, 0);

			VaryingsA o;
			o.position = float4(vpos.x, -vpos.y, 1, 1);
			o.texcoord = (vpos.xy + 1) / 2;
			o.ray = lerp(rayPers, rayOrtho, isOrtho);

			//MINE
			float3 vA = vpos;
			float deg = _cameraRoll;
			float alpha = deg * 3.14 / 180.0;
			float sina, cosa;
			sincos(alpha, sina, cosa);
			float2x2 m = float2x2(cosa, -sina, sina, cosa);

			float3 tmpV = float3(mul(m, vA.xy), vA.z).xyz;
			float2 uvFOG = TransformTriangleVertexToUV(tmpV.xy);
			o.uvFOG = uvFOG.xy;

			half index = vpos.z;
			o.interpolatedRay.xyz = vpos;  // _FrustumCornersWS[(int)index];
			o.interpolatedRay.w = index;

			return o;
		}

		float3 ComputeViewSpacePosition(VaryingsA input, float z)
		{
			// Render settings
			float near = _ProjectionParams.y;
			float far = _ProjectionParams.z;
			float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic

			// Z buffer sample        

			// Far plane exclusion
#if !defined(EXCLUDE_FAR_PLANE)
			float mask = 1;
#elif defined(UNITY_REVERSED_Z)
			float mask = z > 0;
#else
			float mask = z < 1;
#endif

			//FOG
			//float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy), _ZBufferParams);

			// Perspective: view space position = ray * depth
			float3 vposPers = input.ray * Linear01Depth(z, _ZBufferParams);


			// Orthographic: linear depth (with reverse-Z support)
#if defined(UNITY_REVERSED_Z)
			float depthOrtho = -lerp(far, near, z);
#else
			float depthOrtho = -lerp(near, far, z);
#endif

			// Orthographic: view space position
			float3 vposOrtho = float3(input.ray.xy, depthOrtho);

			// Result: view space position
			return lerp(vposPers, vposOrtho, isOrtho) * mask;
		}

		half4 VisualizePosition(VaryingsA input, float3 pos)
		{
			const float grid = 5;
			const float width = 3;

			pos *= grid;

			// Detect borders with using derivatives.
			float3 fw = fwidth(pos);
			half3 bc = saturate(width - abs(1 - 2 * frac(pos)) / fw);

			// Frequency filter
			half3 f1 = smoothstep(1 / grid, 2 / grid, fw);
			half3 f2 = smoothstep(2 / grid, 4 / grid, fw);
			bc = lerp(lerp(bc, 0.5, f1), 0, f2);

			// Blend with the source color.
			half4 c = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, input.texcoord);
			c.rgb = SRGBToLinear(lerp(LinearToSRGB(c.rgb), bc, 0.5));

			return c;
		}





		////// END VOLUME FOG



		ENDHLSL

			//		SubShader
			//	{
			//		//Cull Off ZWrite Off ZTest Always
			//
			//			Pass
			//		{
			//			HLSLPROGRAM
			//
			//#pragma vertex VertDefault
			//#pragma fragment Frag
			//
			//			ENDHLSL
			//		}
			//	}
			Subshader{
			//Tags{ "RenderType" = "Opaque" }
			Pass{ //PASS 0 
				ZTest Always Cull Off ZWrite Off

				HLSLPROGRAM

				#pragma vertex vert
				#pragma fragment fragScreen

				ENDHLSL
			}
			Pass{ //PASS 1
				ZTest Always Cull Off ZWrite Off

				HLSLPROGRAM

				#pragma vertex vert_radial
				#pragma fragment frag_radial

				ENDHLSL
			}

			Pass{//PASS 2
				ZTest Always Cull Off ZWrite Off

				HLSLPROGRAM

				#pragma vertex vert
				#pragma fragment frag_depth

				ENDHLSL
			}

			Pass{//PASS 3
				ZTest Always Cull Off ZWrite Off

				HLSLPROGRAM

				#pragma vertex vert
				#pragma fragment frag_nodepth

				ENDHLSL
			}

			Pass{//PASS 4
				ZTest Always Cull Off ZWrite Off

				HLSLPROGRAM

				#pragma vertex vert
				#pragma fragment fragAdd

				ENDHLSL
			}


			//PASS5
			Pass{
				ZTest Always Cull Off ZWrite Off

				HLSLPROGRAM

				#pragma vertex vert
				#pragma fragment FragGrey

				ENDHLSL
			}

			///// VOLUME FOG
				/////////// PASS 6
		Pass
		{
			HLSLPROGRAM

			#pragma vertex Vertex
			#pragma fragment Fragment

			float4x4 _InverseView;

			float2 WorldToScreenPos(float3 pos) {
				pos = normalize(pos - _WorldSpaceCameraPos) * (_ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y)) + _WorldSpaceCameraPos;
				float2 uv = float2(0,0);
				float4 toCam = mul(unity_WorldToCamera, float4(pos.xyz,1));
				float camPosZ = toCam.z;
				float height = 2 * camPosZ / unity_CameraProjection._m11;
				float width = _ScreenParams.x / _ScreenParams.y * height;
				uv.x = (toCam.x + width / 2) / width;
				uv.y = (toCam.y + height / 2) / height;
				return uv;
			}

			float2 raySphereIntersect(float3 r0, float3 rd, float3 s0, float sr) {

				float a = dot(rd, rd);
				float3 s0_r0 = r0 - s0;
				float b = 2.0 * dot(rd, s0_r0);
				float c = dot(s0_r0, s0_r0) - (sr * sr);
				float disc = b * b - 4.0 * a * c;
				if (disc < 0.0) {
					return float2(-1.0, -1.0);
				}
				else {
					return float2(-b - sqrt(disc), -b + sqrt(disc)) / (2.0 * a);
				}
			}

			float3x3 rotationMatrix(float3 axis, float angle)
			{
				axis = normalize(axis);
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;

				return float3x3 (oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
					oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s,
					oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c);
			}
			float4x4 rotationMatrix4(float3 axis, float angle)
			{
				axis = normalize(axis);
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;

				return float4x4 (oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
				oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
				oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
				0.0, 0.0, 0.0, 1.0);

			}

			/*
			///// IMPOSTOR LIGHTS
			//shadows = half4(VolumeFog(result.rgb, normalizeMag, (posToCameraA * noiseer * VolumeLightNoisePower) / normalizeMag, 
			//_WorldSpaceCameraPos, wpos, input.texcoord, depth), result.a);
			float3 VolumeFog(float3 sourceImg, float3 WorldPosition, float2 texcoord, float depth)
			{

				float _RaySamples = 9;
				float4 _stepsControl = float4(1, 1, 1, 1);
				float4 lightNoiseControl = float4(1, 1, 1, 1);

				//float steps = _RaySamples * 5; //v1.5
				//float stepLength = dist / steps;
				//float3 step = ray * stepLength;

				//float3 stepD = step;
				//if (_stepsControl.x != 0) {
				//	stepLength = (_stepsControl.x) * 50 * 0.005 * 1.4;
				//}

				//Light directionalLight = GetMainLight();

				//half shadowdirectionalLight = GetMainLightShadowStrength();
				//ShadowSamplingData shadowSampleData = GetMainLightShadowSamplingData();

				//float3 pos = rayStart + step;
				//float ColorE = 0;
				//float ColorA = 0;

				//float lightINT = max(dot(normalize(directionalLight.direction), -ray), 0);
				//float3 ColorFOG = lerp(_ScatterColor, directionalLight.color, _LightSpread * pow(lightINT, 7));
				//float2 uv = texcoord + _Time.x;
				//float attenuate = 0;

				//v1.9.8
				float w = 0.02;
				float o = 0.75;
				float3 coordAlongRay = WorldPosition // pos - _WorldSpaceCameraPos //v2.0
					+ float3(_Time.y * _NoiseSpeed.x * 0.1 * lightNoiseControl.w,
						_Time.y * _NoiseSpeed.y * 0.1 * lightNoiseControl.w,
						_Time.y * _NoiseSpeed.z * 0.1 * lightNoiseControl.w);
						o += 1.5 * cnoise(coordAlongRay * 0.17 * lightNoiseControl.z) * w * _stepsControl.w;
				if (_NoiseThickness == 0) {
					o = 1;
				}

				//v1.9.9.1 - Light Array
				float3 forward = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
				for (int i = 0; i < lightsArrayLength; i++)
				{
					float3 lightPos = _LightsArrayPos[i].xyz;
					float3 lightDir = _LightsArrayDir[i].xyz;
					float lightPower = _LightsArrayPos[i].w;
					float lightFalloff = _LightsArrayDir[i].w;
					float3 lightColor = _LightsArrayColor[i].xyz;

					float3 dir = WorldPosition - lightPos;
					float dist = length(dir);
					float dist2 = length(lightPos - _WorldSpaceCameraPos);

					float2 pos2d = WorldToScreenPos(lightPos);
					//float uvsDist = 150*length(pos2d - texcoord)/dist;
					float uvsDist = 1 * length(pos2d - texcoord) / 1;

					float outOfViewCheck = 0;
					if (dot(normalize(forward), normalize(lightPos - _WorldSpaceCameraPos)) >= 0) {
						outOfViewCheck = 1;
					}
					//depth test
					float lightPower2 = lightPower;
					if (length(lightPos - _WorldSpaceCameraPos) > length(WorldPosition - _WorldSpaceCameraPos)) { //if behind obstacle, zero intensity
						lightPower2 = 0;
					}
					if (lightDir.x != 0 || lightDir.y != 0 || lightDir.z != 0) {
						float2 pos2dEND = WorldToScreenPos(lightPos + lightDir * 2);
						float diff1 = dot(normalize(texcoord - pos2d), normalize(pos2dEND - pos2d));
						float diff2 = pow(lightFalloff, 2.5) * 0.5 * 1 * o;

						if (diff1 < diff2) {
							if (lightPower2 != 0) {
								lightPower2 = abs(diff1 + 0.1) * 1811 / pow(diff2 - diff1 + 1, 2);
							}
						}
						else {
						}
						lightFalloff = 2 + lightFalloff * o * 0.5;
					}
					sourceImg = sourceImg * 1 + (
						lightColor * sourceImg * (1 / pow(uvsDist, (1 / lightFalloff * 3.2))) * lightPower2 * 0.0009 / (dist2 * 0.1 + 0.15)) * outOfViewCheck * o;
				}
				return sourceImg;
			}//END IMPOSTORS
			*/

			float4 scaling;
			TEXTURE2D(_CameraDepthNormalsTextureA);//_CameraDepthNormalsTextureA
			SAMPLER(sampler_CameraDepthNormalsTextureA);
			float4 _CameraDepthNormalsTextureA_TexelSize;
			//TEXTURE2D(_CameraDepthTexture);
			//SAMPLER(sampler_CameraDepthTexture);
			float3 DecodeNormal(float4 enc)
			{
				float kScale = 1.7777;
				float3 nn = enc.xyz * float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
				float g = 2.0 / dot(nn.xyz, nn.xyz);
				float3 n;
				n.xy = g * nn.xy;
				n.z = g - 1;
				return n;
			}
			float DecodeFloatRG(float2 enc)
			{
				float2 kDecodeDot = float2(1.0, 1 / 255.0);
				return dot(enc, kDecodeDot);
			}
			void DecodeDepthNormal(float4 enc, out float depth, out float3 normal)
			{
				depth = DecodeFloatRG(enc.zw);
				normal = DecodeNormal(enc);
			}

			half4 Fragment(VaryingsA input) : SV_Target
			{
						float3 forward = mul((float3x3)(unity_WorldToCamera), float3(0, 0, 1));
						float zsample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord);
						float3 vpos = ComputeViewSpacePosition(input, zsample);
						float3 wpos = mul(_InverseView, float4(vpos, 1)).xyz;

						//float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy), _ZBufferParams);
						float depth = Linear01Depth(zsample * (zsample < 1.0), _ZBufferParams);


						float4 wsDir = depth * float4(input.ray, 1); // input.interpolatedRay;				
						float4 wsPos = _CameraWS + wsDir;

						///// SCATTER
						float3 lightDirection = float3(-v3LightDir.x - 0 * _cameraDiff.w * forward.x, -v3LightDir.y - 0 * _cameraDiff.w * forward.y, v3LightDir.z);


						//return float4(wsPos) * 1.5;// float4(1, 0, 0, 0.1);

						/*
						//int noise3D = 0;
						half4 noise;
						half4 noise1;
						half4 noise2;
						if (noise3D == 0) {
							float fixFactor1 = 0;
							float fixFactor2 = 0;
							float dividerScale = 1; //1
							float scaler1 = 1.00; //0.05
							float scaler2 = 0.8; //0.01
							float scaler3 = 0.3; //0.01
							float signer1 = 0.004 / (dividerScale * 1.0);//0.4 !!!! (0.005 for 1) (0.4 for 0.05) //0.004
							float signer2 = 0.004 / (dividerScale * 1.0);//0.001

							if (_cameraDiff.w < 0) {
								fixFactor1 = -signer1 * 90 * 2 * 2210 / 1 * (dividerScale / 1);//2210
								fixFactor2 = -signer2 * 90 * 2 * 2210 / 1 * (dividerScale / 1);
							}
							float hor1 = -_cameraDiff.w * signer1 * _cameraDiff.y * 2210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 10 + fixFactor1;
							float hor2 = -_cameraDiff.w * signer2 * _cameraDiff.y * 2210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 10 + fixFactor2;
							float hor3 = -_cameraDiff.w * signer2 * _cameraDiff.y * 1210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 2 + fixFactor2;

							float vert1 = _cameraTiltSign * _cameraDiff.x * 0.77 * 1.05 * 160 + 0.0157 * _cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
								- 2 * 0.33 * _WorldSpaceCameraPos.z * 2.13 + 50 * abs(cos(_WorldSpaceCameraPos.z * 0.01)) + 35 * abs(sin(_WorldSpaceCameraPos.z * 0.005));

								float vert2 = _cameraTiltSign * _cameraDiff.x * 0.20 * 1.05 * 160 + 0.0157 * _cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
								- 1 * 0.33 * _WorldSpaceCameraPos.z * 3.24 + 75 * abs(sin(_WorldSpaceCameraPos.z * 0.02)) + 85 * abs(cos(_WorldSpaceCameraPos.z * 0.01));

								float vert3 = _cameraTiltSign * _cameraDiff.x * 0.10 * 1.05 * 70 + 0.0117 * _cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
								- 1 * 1.03 * _WorldSpaceCameraPos.z * 3.24 + 75 * abs(sin(_WorldSpaceCameraPos.z * 0.02)) + 85 * abs(cos(_WorldSpaceCameraPos.z * 0.01));

							 noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (float2(input.texcoord.x * scaler1 * 1, input.texcoord.y * scaler1))
								+ (-0.001 * float2((0.94) * hor1, vert1)) + 3 * abs(cos(_Time.y * 1.22 * 0.012)))) * 2 * 9;
							 noise1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (input.texcoord.xy * scaler2)
								+ (-0.001 * float2((0.94) * hor2, vert2) + 3 * abs(cos(_Time.y * 1.22 * 0.010))))) * 3 * 9;
							 noise2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (input.texcoord.xy * scaler3)
								+ (-0.001 * float2((0.94) * hor3, vert3) + 1 * abs(cos(_Time.y * 1.22 * 0.006))))) * 3 * 9;
						}
						else {

							/////////// NOISE 3D //////////////
							const float epsilon = 0.0001;

							float2 uv = input.texcoord * 4.0 + float2(0.2, 1) * _Time.y * 0.01;

							//#if defined(SNOISE_AGRAD) || defined(SNOISE_NGRAD)
							//	#if defined(THREED)
							//						float3 o = 0.5;
							//	#else
							//						float2 o = 0.5;
							//	#endif
							//#else
							float o = 0.5 * 1.5;
							//#endif

							float s = 0.011;

							//#if defined(SNOISE)
							//					float w = 0.25;
							//#else
							float w = 0.02;
							//#endif


							//v0.2
							//v0.2
							if (zsample < 1 - 0.9999995) {
								_NoiseScale = _NoiseScale * 0.001;
							}


							//#ifdef FRACTAL
							for (int i = 0; i < 5; i++)
								//#endif
								{
									float3 coord = (wpos + 0) + float3(_Time.y * 3 * _NoiseSpeed.x,
										_Time.y * _NoiseSpeed.y,
										_Time.y * _NoiseSpeed.z);
									float3 period = float3(s, s, 1.0) * 1111;

									//#if defined(CNOISE)
															o += cnoise(coord * 0.17 * _NoiseScale) * w;

															float3 pointToCamera = (wpos + 0) * 0.47;
															int steps = 2;
															float stepCount = 1;
															float step = length(pointToCamera) / steps;
															for (int j = 0; j < steps; j++) {
																//ray trace noise												
																//float3 coordAlongRay = _WorldSpaceCameraPos + normalize(pointToCamera) * step
																float3 coordAlongRay = 0 + normalize(pointToCamera) * step
																	+ float3(_Time.y * 6 * _NoiseSpeed.x,
																		_Time.y * _NoiseSpeed.y,
																		_Time.y * _NoiseSpeed.z);
																o += 1.5 * cnoise(coordAlongRay * 0.17 * _NoiseScale) * w * 1;
																//stepCount++;
																if (depth < 0.99999) {
																	o += depth * 45 * _NoiseThickness;
																}
																step = step + step;
															}

										s *= 2.0;
										w *= 0.5;
									}
									noise = float4(o, o, o, 1);
									noise1 = float4(o, o, o, 1);
									noise2 = float4(o, o, o, 1);
								}
								*/
								float cosTheta = dot(normalize(wsDir.xyz), lightDirection);
								cosTheta = dot(normalize(wsDir.xyz), -lightDirection);

								float lumChange = clamp(luminance * pow(abs(((1 - depth) / (_OcclusionDrop * 0.1 * 2))), _OcclusionExp), luminance, luminance * 2);
								if (depth <= _OcclusionDrop * 0.1 * 1) {
									luminance = lerp(4 * luminance, 1 * luminance, (0.001 * 1) / (_OcclusionDrop * 0.1 - depth + 0.001));
								}

								//	return float4(noise) * 1.5;
								//	return float4(noise.r, noise1.r, noise2.r,1) * 1.5;

									float3 up = float3(0.0, 1.0, 0.0); //float3(0.0, 0.0, 1.0);			
									float3 lambda = float3(680E-8, 550E-8, 450E-8);
									float3 K = float3(0.686, 0.678, 0.666);
									float  rayleighZenithLength = 8.4E3;
									float  mieZenithLength = 1.25E3;
									float  pi = 3.141592653589793238462643383279502884197169;
									float3 betaR = totalRayleigh(lambda) * reileigh * 1000;
									float3 lambda1 = float3(_TintColor.r, _TintColor.g, _TintColor.b) * 0.0000001;//  680E-8, 1550E-8, 3450E-8);
									lambda = lambda1;
									float3 betaM = totalMie(lambda1, K, turbidity * Multiplier2) * mieCoefficient;
									float zenithAngle = acos(max(0.0, dot(up, normalize(lightDirection))));
									float sR = rayleighZenithLength / (cos(zenithAngle) + 0.15 * pow(abs(93.885 - ((zenithAngle * 180.0) / pi)), -1.253));
									float sM = mieZenithLength / (cos(zenithAngle) + 0.15 * pow(abs(93.885 - ((zenithAngle * 180.0) / pi)), -1.253));
									float  rPhase = rayleighPhase(cosTheta * 0.5 + 0.5);
									float3 betaRTheta = betaR * rPhase;
									float  mPhase = hgPhase(cosTheta, mieDirectionalG) * Multiplier1;
									float3 betaMTheta = betaM * mPhase;
									float3 Fex = exp(-(betaR * sR + betaM * sM));
									float  sunE = sunIntensity(dot(lightDirection, up));
									float3 Lin = ((betaRTheta + betaMTheta) / (betaR + betaM)) * (1 - Fex) + sunE * Multiplier3 * 0.0001;
									float  sunsize = 0.0001;
									float3 L0 = 1.5 * Fex + (sunE * 1.0 * Fex) * sunsize;
									float3 FragColor = tonemap(Lin + L0);
									///// END SCATTER

									//return float4(lambda.rgb, 1) * 1111111112.5;
									//return float4(depth.rrr, 1) * 1.5;

									//occlusion !!!!
									half4 sceneColor = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, input.texcoord.xy);

									//return sceneColor * 1.5;

									float3 subtractor = saturate(pow(abs(dot(normalize(input.ray), normalize(lightDirection))),36)) - (float3(1, 1, 1) * depth * 1);
									if (depth < _OcclusionDrop * 0.1) {
										FragColor = saturate(FragColor * pow(abs((depth / (_OcclusionDrop * 0.1))), _OcclusionExp));
									}
									else {
										if (depth < 0.9999) {
											FragColor = saturate(FragColor * pow(abs((depth / (_OcclusionDrop * 0.1))), 0.001));
										}
									}

									//SCATTER
									int doHeightA = 1;
									int doDistanceA = 1;
									float g = ComputeDistance(input.ray, depth) - _DistanceOffset;
									if (doDistanceA == 1) {
										g += ComputeDistance(input.ray, depth) - _DistanceOffset;
									}
									if (doHeightA == 1) {
										g += ComputeHalfSpace(wpos);
									}


									//v0.2
									//if (zsample < 1 - 0.9999995) {
									//	noise.r = noise.r / 2;
									//	noise1.r = noise1.r / 2;
									//	noise2.r = noise2.r / 2;
									//}

									//g = g * pow(abs((noise.r + 1 * noise1.r + _NoiseDensity * noise2.r * 1)), 1.2) * 0.3;// *(1 - input.uvFOG.y * (0.15 * _NoiseScale * depth));//v0.2



									//return float4(g.rrr, 1)*1;


									half fogFac = ComputeFogFactorA(max(0.0, g));

									//return float4(fogFac.rrr, 1) * 1;

									if (zsample >= 0.999995) {
										if (FogSky <= 0) {
											fogFac = 1.0;
										}
										else {
											if (doDistanceA == 1) {
												fogFac = fogFac * ClearSkyFac;
											}
										}
									}

									if (zsample < 1 - 0.999995) {
										fogFac = fogFac * ClearSkyFac;
										if (fogFac > 1.4) {
											fogFac = 1.4;
										}
										if (fogFac < 0) {
											fogFac = 0;
										}
									}

									float4 Final_fog_color = lerp(unity_FogColor + float4(FragColor, 1), sceneColor, fogFac);
									float fogHeight = _Height;
									half fog = ComputeFogFactorA(max(0.0, g));

									//local light
									float3 visual = 0;// VisualizePosition(input, wpos);
									if (1 == 1) {

										float3 light1 = localLightPos.xyz;
										float dist1 = length(light1 - wpos);

										float2 screenPos = WorldToScreenPos(light1);
										float lightRadius = localLightColor.w;

										float dist2 = length(screenPos - float2(input.texcoord.x, input.texcoord.y * 0.62 + 0.23));
										if (
											length(_WorldSpaceCameraPos - wpos) < length(_WorldSpaceCameraPos - light1) - lightRadius
											&&
											dot(normalize(_WorldSpaceCameraPos - wpos), normalize(_WorldSpaceCameraPos - light1)) > 0.95// 0.999
											) { //occlusion
										}
										else {
											float factorOcclusionDist = length(_WorldSpaceCameraPos - wpos) - (length(_WorldSpaceCameraPos - light1) - lightRadius);
											float factorOcclusionDot = dot(normalize(_WorldSpaceCameraPos - wpos), normalize(_WorldSpaceCameraPos - light1));

											Final_fog_color = lerp(Final_fog_color,
												Final_fog_color * (1 - ((11 - dist2) / 11))
												+ Final_fog_color * float4(2 * localLightColor.x, 2 * localLightColor.y, 2 * localLightColor.z, 1) * (11 - dist2) / 11,
												(localLightPos.w * saturate(1 * 0.1458 / pow(dist2, 0.95))
													+ 0.04 * saturate(pow(1 - input.uvFOG.y * (1 - fogHeight), 1.0)) - 0.04)
											);
										}
									}

									//if (lightsArrayLength > 0) {
									//	float4 shadows = float4(VolumeFog(Final_fog_color.rgb, wpos, input.uvFOG, depth), Final_fog_color.a);
									//	Final_fog_color = saturate(shadows);
									//}

					//				#if USE_SKYBOX//
					//				// Look up the skybox color.
					//				half3 skyColor = DecodeHDR(texCUBE(_SkyCubemap, input.ray), _SkyCubemap_HDR);
					//				skyColor *= _SkyTint.rgb * _SkyExposure * unity_ColorSpaceDouble;
					//				// Lerp between source color to skybox color with fog amount.
					//				return (lerp(half4(skyColor, 1), sceneColor, fog)) * _Blend + sceneColor * (1 - _Blend);
					//#else
									// Lerp between source color to fog color with the fog amount.
									half4 skyColor = lerp(_FogColor, sceneColor, saturate(fog));

									float distToWhite = (Final_fog_color.r - 0.99) + (Final_fog_color.g - 0.99) + (Final_fog_color.b - 0.99);

									Final_fog_color = Final_fog_color + 0.0 * Final_fog_color * float4(8,2,0,1);




									//RAIN WORLD POS RECONSTRUCTION plus NORMALS
									float3 normal = 0;
									//float depth = 0; 
									float depth2 = 0;
									DecodeDepthNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTextureA, sampler_CameraDepthNormalsTextureA, input.uvFOG), depth2, normal);
									float3 normalWS = mul((float3x3)_InverseView, normal);
									depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, input.uvFOG).r;
									depth = Linear01Depth(depth, _ZBufferParams);
									_InverseView[0][3] = 0;
									_InverseView[1][3] = 0;
									_InverseView[2][3] = 0;
									_InverseView[3][3] = 0;
									// find out snow color
									float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
									float3 vposA = float3((input.uvFOG * 2.0 - 1.0) / p11_22, -1.0) * depth;
									float4 wposA = mul(_InverseView, float4(vposA, 1));
									wposA += float4(_WorldSpaceCameraPos, 0) / _ProjectionParams.z;
									wposA *= scaling.x * _ProjectionParams.z;



									float4 color = float4(0.0, 0.0, 0.0, 0.0);
									float _Scale = scaling.x;
									////https://github.com/alelievr/HDRP-Custom-Passes/blob/master/Assets/CustomPasses/ScrollingFormulas/Resources/ScrollingFormulas.shader
									float3 offsetedPosition = wpos.xyz - float3(0, 0, 0);// _SphereSize);
									float2 uvX = offsetedPosition.yz * _Scale;
									float2 uvY = offsetedPosition.xz * _Scale;
									float2 uvZ = offsetedPosition.xy * _Scale;
									float4 colorX = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uvX + float2(_Time.y * 0, 0)).rgba;
									float4 colorY = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uvY + float2(_Time.y * 0, 0)).rgba;
									float4 colorZ = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uvZ + float2(_Time.y * 0, 0)).rgba;
									float3 s = sign(normalWS);
									normalWS = normalize(pow(abs(normalWS), scaling.y)) * s;//       _Power))* s;
									color = colorX * abs(normalWS.x) + colorY * abs(normalWS.y) + colorZ * abs(normalWS.z);
									//color = (colorX * 1 + colorY * 1+ colorZ * 1)/5;
									float d = saturate((wposA.z / 5.0));// -_SphereSize) / 5.0);
									color.a *= d;
									// Put alpha to 0 for sky
									color.a *= depth > 0;
									// Fade value allow you to increase the strength of the effect while the camera gets closer to the custom pass volume
									float f = 1 - abs(scaling.z * 2 - 1);//  _FadeValue * 2 - 1);
									
							//		float mask = length(abs(depth2.rrrr) - abs(depth.rrrr)) < 0.11;
									//return mask;
//
							//		return depth.rrrr;
							//		return normalWS.xyzz;



									/*
									//float4 outpu = VisualizePosition(input, wpos);
									const float grid = 5;
									const float width = 3;
									wpos *= grid;
									// Detect borders with using derivatives.
									float3 fw = fwidth(wpos);
									half3 bc = saturate(width - abs(1 - 2 * frac(wpos)) / fw);
									// Frequency filter
									half3 f1 = smoothstep(1 / grid, 2 / grid, fw);
									half3 f2 = smoothstep(2 / grid, 4 / grid, fw);
									bc = lerp(lerp(bc, 0.5, f1), 0, f2);
									// Blend with the source color.
									half4 c = sceneColor;// SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, input.texcoord);
									c.rgb = SRGBToLinear(lerp(LinearToSRGB(c.rgb), bc, 0.5));
									*/

									//return normalWS.xyzz;

									//float4 colorXA = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, wpos.xz * (normalWS.y) ).rgba;
									//return colorXA;
									if (depth > 0.9999) {
										color.a = 0;
									}
									float4 outME = float4((color.rgb + f)* color.a + (1- color.a)* 1, color.a) * float4(sceneColor.rgb, 1) * 2;// +float4(color.rgb + f, color.a) * 0.5;

									return (pow(outME, _contrast)*_brightness) *_Blend* _TintColor + (1-_Blend)*sceneColor;


							//		return wposA * depth.rrrr;//* normal.xyzz;// float4(normal.xyz * (0.1 - depth * depth * depth * depth), 1);////depth.rrrr;// float4(normal.xyz, 1) + float4(1, 1, 1, 1) * depth;

							//		return sceneColor * _Blend;// *scaling.xyxy;
									//return saturate(float4(1, 1, 1, 0.4) * Final_fog_color.rgba) + float4(1, 0, 0, 0.4)*saturate(sceneColor * (1- saturate(Final_fog_color.r)));
									//return (Final_fog_color * _FogColor + float4(visual, 0)) * _Blend + sceneColor * (1 - _Blend);
					//#endif					                
			}
		ENDHLSL
	}
	Pass //PASS 7
	{
		HLSLPROGRAM

		#pragma vertex Vertex
		#pragma fragment Fragment

		float4x4 _InverseView;

		half4 Fragment(VaryingsA input) : SV_Target
		{
			half4 sceneColor = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, input.texcoord.xy);


			float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord);
			float3 vpos = ComputeViewSpacePosition(input,z);
			float3 wpos = mul(_InverseView, float4(vpos, 1)).xyz;

			float4 outpu = VisualizePosition(input, wpos) * 1 + sceneColor;
			return float4(outpu.rgb,0.1);
		}

		ENDHLSL
	}

		//PASS CROP SKY MASK


		///// END VOLUME FOG


		}
}