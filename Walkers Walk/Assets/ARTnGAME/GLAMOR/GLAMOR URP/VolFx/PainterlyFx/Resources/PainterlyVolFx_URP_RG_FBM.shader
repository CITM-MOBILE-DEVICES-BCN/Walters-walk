Shader "Unlit/PainterlyVolFx"
{

    Properties
    {
        [HideInInspector] _MainTex("Base (RGB)", 2D) = "white" {}
    //[HideInInspector]_ColorBuffer("Base (RGB)", 2D) = "white" {}

        //[HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
        //_Delta("Line Thickness", Range(0.0005, 0.0025)) = 0.001
        //[Toggle(RAW_OUTLINE)]_Raw("Outline Only", Float) = 0
        //[Toggle(POSTERIZE)]_Poseterize("Posterize", Float) = 0
        //_PosterizationCount("Count", int) = 8

        _SunThreshold("sun thres", Color) = (0.87, 0.74, 0.65,1)
        _SunColor("sun color", Color) = (1.87, 1.74, 1.65,1)
        _BlurRadius4("blur", Color) = (0.00325, 0.00325, 0,0)
        _SunPosition("sun pos", Vector) = (111, 11,339, 11)
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

            TEXTURE2D(_MainTexA);
        SAMPLER(sampler_MainTexA);

        TEXTURE2D(_MainTex);
        TEXTURE2D(_ColorBuffer);
        TEXTURE2D(_Skybox);

        SAMPLER(sampler_MainTex);
        SAMPLER(sampler_ColorBuffer);
        SAMPLER(sampler_Skybox);
        float _Blend;

        //sampler2D _MainTex;
        //sampler2D _ColorBuffer;
        //sampler2D _Skybox;
        //sampler2D_float _CameraDepthTexture;
        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);
        half4 _CameraDepthTexture_ST;

        half4 _SunThreshold = half4(0.87, 0.74, 0.65, 1);

        half4 _SunColor = half4(0.87, 0.74, 0.65, 1);
        uniform half4 _BlurRadius4 = half4(2.5 / 768, 2.5 / 768, 0.0, 0.0);
        uniform half4 _SunPosition = half4(1, 1, 1, 1);
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
        return  (1.0f - (1.0f - colorA) * (1.0f - depthMask)) * _Blend + colorA * (1 - _Blend);//colorA * 5.6;// 1.0f - (1.0f - colorA) * (1.0f - depthMask);
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
        return (1 * colorA + depthMask) * _Blend + colorA * (1 - _Blend);
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

            ENDHLSL





















            ////////////////////////////////////////////////////// PASSES ///////////////////////////////////////////////////////
            ////////////////////////////////////////////////////// PASSES ///////////////////////////////////////////////////////
            ////////////////////////////////////////////////////// PASSES ///////////////////////////////////////////////////////
            ////////////////////////////////////////////////////// PASSES ///////////////////////////////////////////////////////


			Subshader {              
                ZTest Always Cull Off ZWrite Off
                //PASS0
                Pass{
                        ZTest Always Cull Off ZWrite Off

                        HLSLPROGRAM

                        //TEXTURE2D(_MainTexB);
                        //SAMPLER(sampler_MainTexB);
                        //uniform half4 _SunPosition = half4(1, 1, 1, 1);

                        struct VertexData {
                            float4 vertex : POSITION;
                            float2 uv : TEXCOORD0;
                        };

                       /* struct v2f {
                            float2 uv : TEXCOORD0;
                            float4 vertex : SV_POSITION;
                            float4 pos: TEXCOORD1;
                            float2 screenPos:  TEXCOORD2;
                        };*/
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
                            //o.sunScreenPosition = circle_screenPos;//  worldToScreenPosition(_SunPosition.xyz);
                            //o.screenPos.xy = o.sunScreenPosition;
                            o.uv = UnityStereoTransformScreenSpaceTex(v.uv);
                            o.uv.y = 1 - o.uv.y;
                            return o;
                        }

                        float4 FragGreyA(v2f i) : SV_Target
                        {   
                        
                            //return float4(1,0,0,1);
                             float4 colorA = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);//
                            float4 color = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv.xy);//
                            // color = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, float2(i.uv.x,1-i.uv.y));
                            //half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
                            //float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.0721750));
                            //color.rgb = lerp(color.rgb, luminance.xxx, _Blend.xxx);
                            //return color/2 + colorB/2;
                            return color * _Blend + (1-_Blend)*colorA;
                        }

                        #pragma vertex vert
                        #pragma fragment FragGreyA

                        ENDHLSL
                }


                //DRAWING
                Pass//PASS 1
                {
                    HLSLPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag

                   // #include "UnityCG.cginc"

                    struct appdata
                    {
                        float4 vertex : POSITION;
                        float2 uv : TEXCOORD0;
                    };

                   /* struct v2f
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
                    sampler2D _DrawingTex;
                    //sampler2D _CameraDepthTexture;

                    float _OverlayOffset;
                    float _Strength;
                    float _Tiling;
                    float _Smudge;
                    float _DepthThreshold;

                    float4 frag(v2f i) : SV_Target
                    {
                        float2 drawingUV = i.uv * _Tiling + _OverlayOffset;
                        drawingUV.y *= _ScreenParams.y / _ScreenParams.x;
                        float4 drawingCol = (tex2D(_DrawingTex, drawingUV) +
                            tex2D(_DrawingTex, drawingUV / 3.0f)) / 2.0f;

                        float2 texUV = i.uv + drawingCol * _Smudge;
                        float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texUV);

                        float lum = dot(col, float3(0.3f, 0.59f, 0.11f));
                        float4 drawing = lerp(col, drawingCol * col, (1.0f - lum) * _Strength);

                        float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).r;
                        depth = Linear01Depth(depth, _ZBufferParams);

                        return depth < _DepthThreshold ? drawing : col;
                    }
                        ENDHLSL
                }

                    //PAINTING
                        Pass//PASS 2
                    {
                        HLSLPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag

                       // #include "UnityCG.cginc"

                        struct appdata
                        {
                            float4 vertex : POSITION;
                            float2 uv : TEXCOORD0;
                        };

                      /*  struct v2f
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
                       // float2 _MainTex_TexelSize;

                        int _KernelSize;

                        struct region
                        {
                            float3 mean;
                            float variance;
                        };

                        region calcRegion(int2 lower, int2 upper, int samples, float2 uv)
                        {
                            region r;
                            float3 sum = 0.0;
                            float3 squareSum = 0.0;

                            for (int x = lower.x; x <= upper.x; ++x)
                            {
                                for (int y = lower.y; y <= upper.y; ++y)
                                {
                                    float2 offset = float2(_MainTex_TexelSize.x * x, _MainTex_TexelSize.y * y);
                                    float3 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset);
                                    sum += tex;
                                    squareSum += tex * tex;
                                }
                            }

                            r.mean = sum / samples;
                            float3 variance = abs((squareSum / samples) - (r.mean * r.mean));
                            r.variance = length(variance);

                            return r;
                        }

                        float4 frag(v2f i) : SV_Target
                        {
                            int upper = (_KernelSize - 1) / 2;
                            int lower = -upper;

                            int samples = (upper + 1) * (upper + 1);

                            region regionA = calcRegion(int2(lower, lower), int2(0, 0), samples, i.uv);
                            region regionB = calcRegion(int2(0, lower), int2(upper, 0), samples, i.uv);
                            region regionC = calcRegion(int2(lower, 0), int2(0, upper), samples, i.uv);
                            region regionD = calcRegion(int2(0, 0), int2(upper, upper), samples, i.uv);

                            float3 col = regionA.mean;
                            float minVar = regionA.variance;

                            float testVal;

                            testVal = step(regionB.variance, minVar);
                            col = lerp(col, regionB.mean, testVal);
                            minVar = lerp(minVar, regionB.variance, testVal);

                            testVal = step(regionC.variance, minVar);
                            col = lerp(col, regionC.mean, testVal);
                            minVar = lerp(minVar, regionC.variance, testVal);

                            testVal = step(regionD.variance, minVar);
                            col = lerp(col, regionD.mean, testVal);

                            return float4(col, 1.0);
                        }
                            ENDHLSL
                    }

                        //KAWAHARA
                            Pass{ //PASS3
                                HLSLPROGRAM
                                #pragma vertex vert
                                #pragma fragment fp

                                float4 FragGreyA(v2f i) : SV_Target
                                                    {

                                                        return float4(1,0,0,1);

                                                        //float4 color = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.uv.xy);//
                                                        // color = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, float2(i.uv.x,1-i.uv.y));
                                                        //half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
                                                        //float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.0721750));
                                                        //color.rgb = lerp(color.rgb, luminance.xxx, _Blend.xxx);
                                                        //return color/2 + colorB/2;
                                                        //return color * 1.5;
                                                    }

                                //#include "UnityCG.cginc"
                                //uniform half4 _SunPosition = half4(1,1,1,1);

                                struct VertexData {
                                    float4 vertex : POSITION;
                                    float2 uv : TEXCOORD0;
                                };

                               /* struct v2f {
                                    float2 uv : TEXCOORD0;
                                    float4 vertex : SV_POSITION;
                                    float4 pos: TEXCOORD1;
                                    float2 screenPos:  TEXCOORD2;
                                };*/

                               /* v2f vp(VertexData v) {
                                    v2f o;
                                    o.vertex = UnityObjectToClipPos(v.vertex);
                                    o.uv = v.uv;
                                    return o;
                                }*/

                                //HLSL
                                //struct Attributes
                                //{
                                //    float4 vertex       : POSITION;
                                //    float2 uv           : TEXCOORD0;
                                //};
//                                v2f vp(VertexData v) {
//                                    v2f o = (v2f)0;
//                                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
//                                    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
//                                    o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
//                                    float2 uv = v.uv;                                   
//
//#if !UNITY_UV_STARTS_AT_TOP
//                                    uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);                                   
//#endif
//
//                                    o.uv = uv;
//#if !UNITY_UV_STARTS_AT_TOP
//                                    o.uv = uv.xy;
//                                    if (_MainTexA_TexelSize.y < 0)
//                                        o.uv.y = 1 - o.uv.y;
//#endif	
//                                    o.pos = TransformObjectToHClip(v.vertex.xyz);
//                                    float3 circle_worldPos = _SunPosition.xyz;
//                                    float4 circle_cameraPos = mul(unity_WorldToCamera, float4(circle_worldPos, 1.0));
//                                    circle_cameraPos.z = -circle_cameraPos.z;
//                                    float4 circle_clipPos = mul(unity_CameraProjection, circle_cameraPos);
//                                    float2 circle_screenPos = (circle_clipPos.xy / circle_clipPos.w) * 0.5 + 0.5;
//                                    o.screenPos.xy = circle_screenPos;
//                                    //o.sunScreenPosition = circle_screenPos;//  worldToScreenPosition(_SunPosition.xyz);
//                                    //o.screenPos.xy = o.sunScreenPosition;
//                                    o.uv = UnityStereoTransformScreenSpaceTex(v.uv);
//                                    o.uv.y = 1 - o.uv.y;
//                                    return o;
//                                }

                                //sampler2D _MainTex;
                                //float4 _MainTex_TexelSize;
                                int _kernelSizeKAWAHARA, _MinKernelSize, _AnimateSize, _AnimateOrigin;
                                float _SizeAnimationSpeed, _NoiseFrequency;

                                float luminance(float3 color) {
                                    return dot(color, float3(0.299f, 0.587f, 0.114f));
                                }

                                float hash(uint n) {
                                    // integer hash copied from Hugo Elias
                                    n = (n << 13U) ^ n;
                                    n = n * (n * n * 15731U + 0x789221U) + 0x1376312589U;
                                    return float(n & uint(0x7fffffffU)) / float(0x7fffffff);
                                }

                                // Returns avg color in .rgb, std in .a
                                float4 SampleQuadrant(float2 uv, int x1, int x2, int y1, int y2, float n) {
                                    float luminance_sum = 0.0f;
                                    float luminance_sum2 = 0.0f;
                                    float3 col_sum = 0.0f;

                                    [loop]
                                    for (int x = x1; x <= x2; ++x) {
                                        [loop]
                                        for (int y = y1; y <= y2; ++y) {
                                            float3 sample = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, uv + float2(x, y) * _MainTexA_TexelSize.xy).rgb;
                                            float l = luminance(sample);
                                            luminance_sum += l;
                                            luminance_sum2 += l * l;
                                            col_sum += saturate(sample);
                                        }
                                    }

                                    float mean = luminance_sum / n;
                                    float std = abs(luminance_sum2 / n - mean * mean);

                                    return float4(col_sum / n, std);
                                }

                                float4 fp(v2f i) : SV_Target{

                                    //return float4(0.1,0.1,0,1);
                                    //return SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv)/ _kernelSizeKAWAHARA;

                                    if (_AnimateSize) {
                                        uint seed = i.uv.x + _MainTexA_TexelSize.z * i.uv.y + _MainTexA_TexelSize.z * _MainTexA_TexelSize.w;
                                        seed = i.uv.y * _MainTexA_TexelSize.z * _MainTexA_TexelSize.w;
                                        float kernelRange = (sin(_Time.y * _SizeAnimationSpeed + hash(seed) * _NoiseFrequency) * 0.5f + 0.5f)
                                            * _kernelSizeKAWAHARA + _MinKernelSize;
                                        int minKernelSize = floor(kernelRange);
                                        int maxKernelSize = ceil(kernelRange);
                                        float t = frac(kernelRange);

                                        float windowSize = 2.0f * minKernelSize + 1;
                                        int quadrantSize = int(ceil(windowSize / 2.0f));
                                        int numSamples = quadrantSize * quadrantSize;

                                        float4 q1 = SampleQuadrant(i.uv, -minKernelSize, 0, -minKernelSize, 0, numSamples);
                                        float4 q2 = SampleQuadrant(i.uv, 0, minKernelSize, -minKernelSize, 0, numSamples);
                                        float4 q3 = SampleQuadrant(i.uv, 0, minKernelSize, 0, minKernelSize, numSamples);
                                        float4 q4 = SampleQuadrant(i.uv, -minKernelSize, 0, 0, minKernelSize, numSamples);

                                        float minstd = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                                        int4 q = float4(q1.a, q2.a, q3.a, q4.a) == minstd;

                                        float4 result1 = 0;
                                        if (dot(q, 1) > 1)
                                            result1 = saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));
                                        else
                                            result1 = saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));

                                        windowSize = 2.0f * maxKernelSize + 1;
                                        quadrantSize = int(ceil(windowSize / 2.0f));
                                        numSamples = quadrantSize * quadrantSize;

                                        q1 = SampleQuadrant(i.uv, -maxKernelSize, 0, -maxKernelSize, 0, numSamples);
                                        q2 = SampleQuadrant(i.uv, 0, maxKernelSize, -maxKernelSize, 0, numSamples);
                                        q3 = SampleQuadrant(i.uv, 0, maxKernelSize, 0, maxKernelSize, numSamples);
                                        q4 = SampleQuadrant(i.uv, -maxKernelSize, 0, 0, maxKernelSize, numSamples);

                                        minstd = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                                        q = float4(q1.a, q2.a, q3.a, q4.a) == minstd;

                                        float4 result2 = 0;
                                        if (dot(q, 1) > 1)
                                            result2 = saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));
                                        else
                                            result2 = saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));

                                        return lerp(result1, result2, t);
                                    }
                                     else {
                                          float windowSize = 2.0f * _kernelSizeKAWAHARA + 1;
                                          int quadrantSize = int(ceil(windowSize / 2.0f));
                                          int numSamples = quadrantSize * quadrantSize;


                                          float4 q1 = SampleQuadrant(i.uv, -_kernelSizeKAWAHARA, 0, -_kernelSizeKAWAHARA, 0, numSamples);
                                          float4 q2 = SampleQuadrant(i.uv, 0, _kernelSizeKAWAHARA, -_kernelSizeKAWAHARA, 0, numSamples);
                                          float4 q3 = SampleQuadrant(i.uv, 0, _kernelSizeKAWAHARA, 0, _kernelSizeKAWAHARA, numSamples);
                                          float4 q4 = SampleQuadrant(i.uv, -_kernelSizeKAWAHARA, 0, 0, _kernelSizeKAWAHARA, numSamples);

                                          float minstd = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                                          int4 q = float4(q1.a, q2.a, q3.a, q4.a) == minstd;

                                          


                                          if (dot(q, 1) > 1)
                                              return saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));// * 0.2;
                                          else
                                              return saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));// *0.2;
                                  }
              }
              ENDHLSL
                        }//END PASS 2

                        //GeneralizedKuwahara
                        //PASS4
                  Pass{
                      HLSLPROGRAM

                      // #include "UnityCG.cginc"

                      struct VertexData {
                          float4 vertex : POSITION;
                          float2 uv : TEXCOORD0;
                      };

                      /*struct v2f {
                          float2 uv : TEXCOORD0;
                          float4 vertex : SV_POSITION;
                      };

                      v2f vp(VertexData v) {
                          v2f o;
                          o.vertex = UnityObjectToClipPos(v.vertex);
                          o.uv = v.uv;
                          return o;
                      }*/

                      //#define PI 3.14159265358979323846f

                      //sampler2D _MainTex;
                      //sampler2D _K0;
                      //float4 _MainTex_TexelSize;
                      int _KernelSize, _N, _Size;
                      float _Hardness, _Q, _ZeroCrossing, _Zeta;

                      #pragma vertex vert
                      #pragma fragment fp

                      float4 fp(v2f i) : SV_Target{


                          //return SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv)*2;

                          int k;
                          float4 m[8];
                          float3 s[8];

                          int kernelRadius = _KernelSize / 2;

                          //float zeta = 2.0f / (kernelRadius);
                          float zeta = _Zeta;

                          float zeroCross = _ZeroCrossing;
                          float sinZeroCross = sin(zeroCross);
                          float eta = (zeta + cos(zeroCross)) / (sinZeroCross * sinZeroCross);

                          for (k = 0; k < _N; ++k) {
                              m[k] = 0.0f;
                              s[k] = 0.0f;
                          }

                          [loop]
                          for (int y = -kernelRadius; y <= kernelRadius; ++y) {
                              [loop]
                              for (int x = -kernelRadius; x <= kernelRadius; ++x) {
                                  float2 v = float2(x, y) / kernelRadius;
                                  float3 c = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(x, y) * _MainTexA_TexelSize.xy).rgb;

                                  

                                  c = saturate(c);
                                  float sum = 0;
                                  float w[8];
                                  float z, vxx, vyy;

                                  /* Calculate Polynomial Weights */
                                  vxx = zeta - eta * v.x * v.x;
                                  vyy = zeta - eta * v.y * v.y;
                                  z = max(0, v.y + vxx);
                                  w[0] = z * z;
                                  sum += w[0];
                                  z = max(0, -v.x + vyy);
                                  w[2] = z * z;
                                  sum += w[2];
                                  z = max(0, -v.y + vxx);
                                  w[4] = z * z;
                                  sum += w[4];
                                  z = max(0, v.x + vyy);
                                  w[6] = z * z;
                                  sum += w[6];
                                  v = sqrt(2.0f) / 2.0f * float2(v.x - v.y, v.x + v.y);
                                  vxx = zeta - eta * v.x * v.x;
                                  vyy = zeta - eta * v.y * v.y;
                                  z = max(0, v.y + vxx);
                                  w[1] = z * z;
                                  sum += w[1];
                                  z = max(0, -v.x + vyy);
                                  w[3] = z * z;
                                  sum += w[3];
                                  z = max(0, -v.y + vxx);
                                  w[5] = z * z;
                                  sum += w[5];
                                  z = max(0, v.x + vyy);
                                  w[7] = z * z;
                                  sum += w[7];

                                  float g = exp(-3.125f * dot(v,v)) / sum;

                                  for (int k = 0; k < 8; ++k) {
                                      float wk = w[k] * g;
                                      m[k] += float4(c * wk, wk);
                                      s[k] += c * c * wk;
                                  }

                                  
                              }
                          }

                          //return float4(saturate(m[0].rgb), 1) * 2;//

                          float4 output = 0;
                          for (k = 0; k < _N; ++k) {
                              m[k].rgb /= m[k].w;
                              s[k] = abs(s[k] / m[k].w - m[k].rgb * m[k].rgb);

                              float sigma2 = s[k].r + s[k].g + s[k].b;
                              float w = 1.0f / (1.0f + pow(_Hardness * 1000.0f * sigma2, 0.5f * _Q));

                              output += float4(m[k].rgb * w, w);
                          }

                          return saturate(output / output.w);
                      }
                          ENDHLSL
              }
                  //END PASS3

                  //PASS 4 ANISOTROPIC KAWAHARA
                  // Calculate Eigenvectors
                          Pass{ //PASS 5 //////
                              HLSLPROGRAM

                              // #include "UnityCG.cginc"

                              struct VertexData {
                                  float4 vertex : POSITION;
                                  float2 uv : TEXCOORD0;
                              };

                           /*   struct v2f {
                                  float2 uv : TEXCOORD0;
                                  float4 vertex : SV_POSITION;
                              };

                              v2f vp(VertexData v) {
                                  v2f o;
                                  o.vertex = UnityObjectToClipPos(v.vertex);
                                  o.uv = v.uv;
                                  return o;
                              }*/

                             // #define PI 3.14159265358979323846f

                              //sampler2D _MainTex;


                              //sampler2D _TFM;
                             // TEXTURE2D(_TFM);
                              //SAMPLER(sampler_TFM);

                              //float4 _MainTex_TexelSize;
                              //int _KernelSize, _N, _Size;
                              //float _Hardness, _Q, _Alpha, _ZeroCrossing, _Zeta;

                              float gaussian(float sigma, float pos) {
                                  return (1.0f / sqrt(2.0f * PI * sigma * sigma)) * exp(-(pos * pos) / (2.0f * sigma * sigma));
                              }

                              #pragma vertex vert
                              #pragma fragment fp

                              float4 fp(v2f i) : SV_Target {
                                  float2 d = _MainTexA_TexelSize.xy;

                                  float3 Sx = (
                                      1.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(-d.x, -d.y)).rgb +
                                      2.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(-d.x,  0.0)).rgb +
                                      1.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(-d.x,  d.y)).rgb +
                                      -1.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(d.x, -d.y)).rgb +
                                      -2.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(d.x,  0.0)).rgb +
                                      -1.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(d.x,  d.y)).rgb
                                  ) / 4.0f;

                                  float3 Sy = (
                                      1.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(-d.x, -d.y)).rgb +
                                      2.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(0.0, -d.y)).rgb +
                                      1.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(d.x, -d.y)).rgb +
                                      -1.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(-d.x, d.y)).rgb +
                                      -2.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(0.0, d.y)).rgb +
                                      -1.0f * SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(d.x, d.y)).rgb
                                  ) / 4.0f;


                                  return float4(dot(Sx, Sx), dot(Sy, Sy), dot(Sx, Sy), 1.0f);
                              }
                                  ENDHLSL
                      }

                          // Blur Pass 1 //PASS 6
                                  Pass{
                                      HLSLPROGRAM

                                        //  #include "UnityCG.cginc"

                                      struct VertexData {
                                          float4 vertex : POSITION;
                                          float2 uv : TEXCOORD0;
                                      };

                                    /*  struct v2f {
                                          float2 uv : TEXCOORD0;
                                          float4 vertex : SV_POSITION;
                                      };

                                      v2f vp(VertexData v) {
                                          v2f o;
                                          o.vertex = UnityObjectToClipPos(v.vertex);
                                          o.uv = v.uv;
                                          return o;
                                      }*/

                                     // #define PI 3.14159265358979323846f

                                     // sampler2D _MainTex;
                                       //sampler2D _TFM;
                                      //TEXTURE2D(_TFM);
                                     // SAMPLER(sampler_TFM);
                                      //float4 _MainTex_TexelSize;
                                      //int _KernelSize, _N, _Size;
                                     // float _Hardness, _Q, _Alpha, _ZeroCrossing, _Zeta;

                                      float gaussian(float sigma, float pos) {
                                          return (1.0f / sqrt(2.0f * PI * sigma * sigma)) * exp(-(pos * pos) / (2.0f * sigma * sigma));
                                      }

                                      #pragma vertex vert
                                      #pragma fragment fp

                                      float4 fp(v2f i) : SV_Target {
                                          int kernelRadius = 5;

                                          float4 col = 0;
                                          float kernelSum = 0.0f;

                                          for (int x = -kernelRadius; x <= kernelRadius; ++x) {
                                              float4 c = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(x, 0) * _MainTexA_TexelSize.xy);
                                              float gauss = gaussian(2.0f, x);

                                              col += c * gauss;
                                              kernelSum += gauss;
                                          }

                                          return col / kernelSum;
                                      }
                                          ENDHLSL
                              }

                                  // Blur Pass 2 //PASS 7
                                          Pass{
                                              HLSLPROGRAM

                                                //  #include "UnityCG.cginc"

                                              struct VertexData {
                                                  float4 vertex : POSITION;
                                                  float2 uv : TEXCOORD0;
                                              };

                                          /*    struct v2f {
                                                  float2 uv : TEXCOORD0;
                                                  float4 vertex : SV_POSITION;
                                              };

                                              v2f vp(VertexData v) {
                                                  v2f o;
                                                  o.vertex = UnityObjectToClipPos(v.vertex);
                                                  o.uv = v.uv;
                                                  return o;
                                              }*/

                                              #define PI 3.14159265358979323846f

                                              //sampler2D _MainTex;
                                                  sampler2D   _TFM;
                                             // float4 _MainTex_TexelSize;
                                              int _KernelSize, _N, _Size;
                                              float _Hardness, _Q, _Alpha, _ZeroCrossing, _Zeta;

                                              float gaussian(float sigma, float pos) {
                                                  return (1.0f / sqrt(2.0f * PI * sigma * sigma)) * exp(-(pos * pos) / (2.0f * sigma * sigma));
                                              }

                                              #pragma vertex vert
                                              #pragma fragment fp

                                              float4 fp(v2f i) : SV_Target {
                                                  int kernelRadius = 5;

                                                  float4 col = 0;
                                                  float kernelSum = 0.0f;

                                                  for (int y = -kernelRadius; y <= kernelRadius; ++y) {
                                                      float4 c = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(0, y) * _MainTexA_TexelSize.xy);
                                                      float gauss = gaussian(2.0f, y);

                                                      col += c * gauss;
                                                      kernelSum += gauss;
                                                  }

                                                  float3 g = col.rgb / kernelSum;

                                                  float lambda1 = 0.5f * (g.y + g.x + sqrt(g.y * g.y - 2.0f * g.x * g.y + g.x * g.x + 4.0f * g.z * g.z));
                                                  float lambda2 = 0.5f * (g.y + g.x - sqrt(g.y * g.y - 2.0f * g.x * g.y + g.x * g.x + 4.0f * g.z * g.z));

                                                  float2 v = float2(lambda1 - g.x, -g.z);
                                                  float2 t = length(v) > 0.0 ? normalize(v) : float2(0.0f, 1.0f);
                                                  float phi = -atan2(t.y, t.x);

                                                  float A = (lambda1 + lambda2 > 0.0f) ? (lambda1 - lambda2) / (lambda1 + lambda2) : 0.0f;

                                                  return float4(t, phi, A);
                                              }
                                                  ENDHLSL
                                      }

                                          // Apply Kuwahara Filter //PASS 8
                                                  Pass{
                                                      HLSLPROGRAM

                                                         // #include "UnityCG.cginc"

                                                      struct VertexData {
                                                          float4 vertex : POSITION;
                                                          float2 uv : TEXCOORD0;
                                                      };

                                                    /*  struct v2f {
                                                          float2 uv : TEXCOORD0;
                                                          float4 vertex : SV_POSITION;
                                                      };

                                                      v2f vp(VertexData v) {
                                                          v2f o;
                                                          o.vertex = UnityObjectToClipPos(v.vertex);
                                                          o.uv = v.uv;
                                                          return o;
                                                      }*/

                                                      #define PI 3.14159265358979323846f

                                                     // sampler2D _MainTex, 
                                                      //    sampler2D   _TFM;
                                                      // 
                                              TEXTURE2D(_TFM);
                                              SAMPLER(sampler_TFM);
                                                      //float4 _MainTex_TexelSize;
                                                      int _KernelSize, _N, _Size;
                                                      float _Hardness, _Q, _Alpha, _ZeroCrossing, _Zeta;

                                                      float gaussian(float sigma, float pos) {
                                                          return (1.0f / sqrt(2.0f * PI * sigma * sigma)) * exp(-(pos * pos) / (2.0f * sigma * sigma));
                                                      }
                                                     
                                                      #pragma vertex vert
                                                      #pragma fragment fp

                                                      //float4 FragGreyA(v2f i) : SV_Target
                                                      //{

                                                      //    return float4(1,0,0,1);

                                                      //    float4 color = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.uv.xy);//
                                                      //    // color = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, float2(i.uv.x,1-i.uv.y));
                                                      //    //half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
                                                      //    //float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.0721750));
                                                      //    //color.rgb = lerp(color.rgb, luminance.xxx, _Blend.xxx);
                                                      //    //return color/2 + colorB/2;
                                                      //    return color * 1.5;
                                                      //}

                                                      float4 fp(v2f i) : SV_Target {
                                                          float alpha = _Alpha;
                                                         // float4 t = tex2D(_TFM, i.uv);
                                                          float4 t = SAMPLE_TEXTURE2D(_TFM, sampler_TFM, i.uv);

                                                          //return t;

                                                          int kernelRadius = _KernelSize / 2;
                                                          float a = float((kernelRadius)) * clamp((alpha + t.w) / alpha, 0.1f, 2.0f);
                                                          float b = float((kernelRadius)) * clamp(alpha / (alpha + t.w), 0.1f, 2.0f);

                                                          float cos_phi = cos(t.z);
                                                          float sin_phi = sin(t.z);

                                                          float2x2 R = {cos_phi, -sin_phi,
                                                                          sin_phi, cos_phi};

                                                          float2x2 S = {0.5f / a, 0.0f,
                                                                          0.0f, 0.5f / b};

                                                          float2x2 SR = mul(S, R);

                                                          int max_x = int(sqrt(a * a * cos_phi * cos_phi + b * b * sin_phi * sin_phi));
                                                          int max_y = int(sqrt(a * a * sin_phi * sin_phi + b * b * cos_phi * cos_phi));

                                                          //float zeta = 2.0f / (kernelRadius);
                                                          float zeta = _Zeta;

                                                          float zeroCross = _ZeroCrossing;
                                                          float sinZeroCross = sin(zeroCross);
                                                          float eta = (zeta + cos(zeroCross)) / (sinZeroCross * sinZeroCross);
                                                          int k;
                                                          float4 m[8];
                                                          float3 s[8];

                                                          for (k = 0; k < _N; ++k) {
                                                              m[k] = 0.0f;
                                                              s[k] = 0.0f;
                                                          }

                                                          [loop]
                                                          for (int y = -max_y; y <= max_y; ++y) {
                                                              [loop]
                                                              for (int x = -max_x; x <= max_x; ++x) {
                                                                  float2 v = mul(SR, float2(x, y));
                                                                  if (dot(v, v) <= 0.25f) {
                                                                      float3 c = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv + float2(x, y) * _MainTexA_TexelSize.xy).rgb;
                                                                      c = saturate(c);
                                                                      float sum = 0;
                                                                      float w[8];
                                                                      float z, vxx, vyy;

                                                                      /* Calculate Polynomial Weights */
                                                                      vxx = zeta - eta * v.x * v.x;
                                                                      vyy = zeta - eta * v.y * v.y;
                                                                      z = max(0, v.y + vxx);
                                                                      w[0] = z * z;
                                                                      sum += w[0];
                                                                      z = max(0, -v.x + vyy);
                                                                      w[2] = z * z;
                                                                      sum += w[2];
                                                                      z = max(0, -v.y + vxx);
                                                                      w[4] = z * z;
                                                                      sum += w[4];
                                                                      z = max(0, v.x + vyy);
                                                                      w[6] = z * z;
                                                                      sum += w[6];
                                                                      v = sqrt(2.0f) / 2.0f * float2(v.x - v.y, v.x + v.y);
                                                                      vxx = zeta - eta * v.x * v.x;
                                                                      vyy = zeta - eta * v.y * v.y;
                                                                      z = max(0, v.y + vxx);
                                                                      w[1] = z * z;
                                                                      sum += w[1];
                                                                      z = max(0, -v.x + vyy);
                                                                      w[3] = z * z;
                                                                      sum += w[3];
                                                                      z = max(0, -v.y + vxx);
                                                                      w[5] = z * z;
                                                                      sum += w[5];
                                                                      z = max(0, v.x + vyy);
                                                                      w[7] = z * z;
                                                                      sum += w[7];

                                                                      float g = exp(-3.125f * dot(v,v)) / sum;

                                                                      for (int k = 0; k < 8; ++k) {
                                                                          float wk = w[k] * g;
                                                                          m[k] += float4(c * wk, wk);
                                                                          s[k] += c * c * wk;
                                                                      }
                                                                  }
                                                              }
                                                          }

                                                          float4 output = 0;
                                                          for (k = 0; k < _N; ++k) {
                                                              m[k].rgb /= m[k].w;
                                                              s[k] = abs(s[k] / m[k].w - m[k].rgb * m[k].rgb);

                                                              float sigma2 = s[k].r + s[k].g + s[k].b;
                                                              float w = 1.0f / (1.0f + pow(_Hardness * 1000.0f * sigma2, 0.5f * _Q));

                                                              output += float4(m[k].rgb * w, w);
                                                          }

                                                          return saturate(output / output.w);
                                                      }
                                                      ENDHLSL
                                              }
                                                  //END PASS


                                              //PASS9 TEST STAGE
                                              Pass{
                                                      ZTest Always Cull Off ZWrite Off

                                                      HLSLPROGRAM
                                                  
                                                      float4 FragGreyA(v2f i) : SV_Target
                                                      {	//return float4(1,0,0,1);
                                                          float4 color = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.uv.xy);
                                                          return color * 1;
                                                      }
                                                      #pragma vertex vert
                                                      #pragma fragment FragGreyA
                                                      ENDHLSL
                                              }


			//Tags{ "RenderType" = "Opaque" }
			


		}
}