Shader "Hidden/VolFx/RainLite"
{
    Properties
    {
         Outline_Depth_Sensitivity("Depth Sensitivity", Float) = 0
        Outline_Normals_Sensitivity("Normals Sensitivity", Float) = 0
        Outline_Color_Sensitivity("Color Sensitivity", Float) = 0
        Outline_Color("Outline Color", Color) = (1, 1, 1, 0)
        OutlineThickness("Outline Thickness", Range(0, 10)) = 1
        EdgesControlA("EdgesControlA", Float) = 1
        EdgesControlB("EdgesControlB", Float) = 1
        EdgesControlC("EdgesControlC", Float) = 1
        EdgesControlD("EdgesControlD", Float) = 1
        _CameraRainPower("CameraRainPower", Float) = 0.5
        _ObjectRainPower("ObjectRainPower", Float) = 0.5
        [NoScaleOffset]cameraDepthNormalsTextureA("cameraDepthNormalsTextureA", 2D) = "white" {}
        _interactPointRadius("interactPointRadius", Vector) = (0, 0, 0, 0)
        _radialControls("radialControls", Vector) = (0, 0, 0, 0)
        _directionControls("directionControls", Vector) = (0, 0, 0, 0)
        _wipeControls("wipeControls", Vector) = (0, 0, 0, 0)
        _mainTexTilingOffset("mainTexTilingOffset", Vector) = (0, 0, 0, 0)
        _maskPower("maskPower", Float) = 0
        _Size("_Size", Float) = 0
        _Distortion("_Distortion", Float) = 0
        _Blur("_Blur", Float) = 0
        _TimeOffset("_TimeOffset", Vector) = (0, 0, 0, 0)
        _EraseCenterRadius("_EraseCenterRadius", Vector) = (0, 0, 0, 0)
        _erasePower("erasePower", Float) = 0
        _TileNumCausticRotMin("_TileNumCausticRotMin", Float) = 0
        _RainSmallDirection("_RainSmallDirection", Vector) = (0, 0, 0, 0)
        _rainContrast("rainContrast", Float) = 0
        _rainPower("rainPower", Float) = 1
        [NoScaleOffset]_snowTexture("snowTexture", 2D) = "white" {}
        _SnowTexScale("_SnowTexScale", Float) = 0
        _SnowColor("_SnowColor", Color) = (0, 0, 0, 0)
        _TopThreshold("_TopThreshold", Float) = 0
        _BottomThreshold("_BottomThreshold", Float) = 0
        _snowBrightness("snowBrightness", Float) = 1
        [NoScaleOffset]_snowBumpMap("snowBumpMap", 2D) = "white" {}
        _snowBumpPower("snowBumpPower", Float) = 1
        _snowBumpScale("snowBumpScale", Float) = 1
        _ShininessSnow("ShininessSnow", Float) = 0
        [NoScaleOffset]_Lux_RainRipples("_Lux_RainRipples", 2D) = "white" {}
        _Lux_RainIntensity("_Lux_RainIntensity", Float) = 0
        _Lux_RippleAnimSpeed("_Lux_RippleAnimSpeed", Float) = 0
        _Lux_RippleTiling("_Lux_RippleTiling", Float) = 0
        _Lux_WaterBumpDistance("_Lux_WaterBumpDistance", Float) = 0
        _Lux_RainBrightness("_Lux_RainBrightness", Float) = 1


		_Weight("Weight", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 0
        
        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off

        Pass
        {
            name "Draw Color Sample"
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #define UnityBuildTexture2DStructNoScaleA(n) UnityBuildTexture2DStructInternalA(TEXTURE2D_ARGS(n, sampler##n), n##_TexelSize, float4(1, 1, 0, 0))
            Texture2D UnityBuildTexture2DStructInternalA(TEXTURE2D_PARAM(tex, samplerstate), float4 texelSize, float4 scaleTranslate)
            {
                Texture2D result;
                result = tex;
                //result.samplerstate = samplerstate;
                //result.texelSize = texelSize;
                //result.scaleTranslate = scaleTranslate;
                return result;
            }


         // Graph Properties
       // CBUFFER_START(UnityPerMaterial)
        float Outline_Depth_Sensitivity;
        float Outline_Normals_Sensitivity;
        float Outline_Color_Sensitivity;
        float4 Outline_Color;
        float OutlineThickness;
        float EdgesControlA;
        float EdgesControlB;
        float EdgesControlC;
        float EdgesControlD;
        float4 _interactPointRadius;
        float4 _radialControls;
        float4 _directionControls;
        float4 _wipeControls;
        float4 _mainTexTilingOffset;
        float _maskPower;
        float _Size;
        float _Distortion;
        float _Blur;
        float4 _TimeOffset;
        float4 _EraseCenterRadius;
        float _erasePower;
        float _TileNumCausticRotMin;
        float4 _RainSmallDirection;
        float4x4 _CamToWorld;
        float4 _snowTexture_TexelSize;
        float _SnowTexScale;
        float4 _SnowColor;
        float _BottomThreshold;
        float _TopThreshold;
        float _snowBrightness;
        float _rainContrast;
        float _rainPower;
        float4 _snowBumpMap_TexelSize;
        float _snowBumpPower;
        float _snowBumpScale;
        float _ShininessSnow;
        float4 _Lux_RainRipples_TexelSize;
        float _Lux_RainIntensity;
        float _Lux_RippleAnimSpeed;
        float _Lux_RippleTiling;
        float _Lux_WaterBumpDistance;
        float _Lux_RainBrightness;
        float _CameraRainPower;
        float _ObjectRainPower;
        float4 cameraDepthNormalsTextureA_TexelSize;
       // UNITY_TEXTURE_STREAMING_DEBUG_VARS;
       // CBUFFER_END


            // Object and Global properties
            SAMPLER(SamplerState_Trilinear_Repeat);
            TEXTURE2D(_snowTexture);
            SAMPLER(sampler_snowTexture);
            TEXTURE2D(_snowBumpMap);
            SAMPLER(sampler_snowBumpMap);
            TEXTURE2D(_Lux_RainRipples);
            SAMPLER(sampler_Lux_RainRipples);
            TEXTURE2D(cameraDepthNormalsTextureA);
            SAMPLER(samplercameraDepthNormalsTextureA);

            //#include "UnityCG.cginc"
#include "../../RainLITEWEATHERNONSG.hlsl"


            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float  _Weight;
            float  _rainMode;

            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 wpos : TEXCOORD1;
                float2 screenPos : TEXCOORD2;
            };
            
            half luma(half3 rgb)
            {
                return dot(rgb, half3(0.299, 0.587, 0.114));
            }

            frag_in vert(const vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.wpos = worldPos;

                o.screenPos = ComputeScreenPos(o.vertex);

                return o;
            }

            //DITHER
            //https://github.com/cubedparadox/Cubeds-Unity-Shaders
            float3 dither(float2 p) {
                float3 p3 = frac(float3(p.xyx) * (float3(443.8975, 397.2973, 491.1871) + _Time.y));
                p3 += dot(p3, p3.yxz + 19.19);
                return (frac(float3((p3.x + p3.y) * p3.z, (p3.x + p3.z) * p3.y, (p3.y + p3.z) * p3.x)) - 0.5) * 2 * 0.00075*1;
            }
            float nrand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }
            //https://github.com/keijiro/ColorSuite/blob/master/Assets/ColorSuite/Shader/ColorSuite.shader
            float3 ditherA(float2 uv)
            {
                float r = nrand(uv) + nrand(uv + (float2)1.1) - 0.5;
                return (float3)(r / 255);
            }

            float interleaved_gradient(float2 uv)
            {
                float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
                return frac(magic.z * frac(dot(uv, magic.xy)));
            }

            float3 ditherB(float2 uv)
            {
                return (float3)(interleaved_gradient(uv / _MainTex_TexelSize) / 255);
            }


            void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
            {
                RGBA = float4(R, G, B, A);
                RGB = float3(R, G, B);
                RG = float2(R, G);
            }
            void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }
            void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }
            void Unity_Multiply_float4_float4x4(float4 A, float4x4 B, out float4 Out)
            {
                Out = mul(A, B);
            }
            void Unity_Multiply_float_float(float A, float B, out float Out)
            {
                Out = mul(A, B);
            }
            void Unity_Multiply_float4x4_float4x4(float4x4 A, float4x4 B, out float4x4 Out)
            {
                Out = mul(A, B);
            }
            void Unity_Add_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A + B;
            }
            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }
            void Unity_Power_float4(float4 A, float4 B, out float4 Out)
            {
                Out = pow(A, B);
            }
            void Unity_Power_float3(float3 A, float3 B, out float3 Out)
            {
                Out = pow(A, B);
            }
            void Unity_Saturate_float3(float3 In, out float3 Out)
            {
                Out = saturate(In);
            }


            half4 frag(const frag_in i) : SV_Target
            {
                half4 main = (tex2D(_MainTex, i.uv) + 5 * tex2D(_MainTex, ditherB(i.uv))) / 6;

                main.rgb = pow(main.rgb, 15) * 1;

                //main = pow(main, 4.5)*1810;
                //half4 main = tex2D(_MainTex, i.uv);
                half4 col  = luma(main.rgb);

                //return main + float4(ditherB(i.uv).rgb,0);
                
                //return saturate(half4((main.rgb), main.a)) * 2;

                float4 out1 = float4(0,0,0,0);
             /*   if (_rainMode == 0) {
                    out1 = saturate(half4(lerp(main.rgb + dither(i.uv).rgb, luma(col.rgb) + 2 * dither(i.uv).rgb, _Weight), main.a)) * 14
                        + tex2D(_MainTex, dither(i.uv)) * tex2D(_MainTex, (i.uv)) * 0.5 + tex2D(_MainTex, (i.uv)) * 0.5;
                    out1 = out1 * 0.65 + tex2D(_MainTex, i.uv) * 0.65;
                }
                if (_rainMode == 1) {
                    out1 = saturate(half4(lerp(main.rgb + ditherA(i.uv).rgb, luma(col.rgb) + 1 * dither(i.uv).rgb, _Weight), main.a)) * 14
                        + tex2D(_MainTex, dither(i.uv)) * tex2D(_MainTex, (i.uv)) * 0.5 + tex2D(_MainTex, (i.uv)) * 0.5;
                    out1 = out1 * 0.65 + tex2D(_MainTex, i.uv) * 0.65;
                } 
                if (_rainMode == 2) {
                    out1 = saturate(half4(lerp(main.rgb + ditherB(i.uv).rgb, luma(col.rgb) + 1 * ditherB(i.uv).rgb, _Weight), main.a)) * 14
                        + tex2D(_MainTex, dither(i.uv)) * tex2D(_MainTex, (i.uv)) * 0.5 + tex2D(_MainTex, (i.uv)) * 0.5;
                    out1 = out1 * 0.65 + tex2D(_MainTex, i.uv) * 0.65;
                }

                if (_rainMode == 3) {*/
                    //out1 *= 22;
                    float4 _UV_42f187a9c9bfec89b114f1eb5e162d18_Out_0_Vector4 = i.uv.xyxx; //IN.uv0;
                    float _Property_5b1553fcff376a8ca74dd3eed2a5d184_Out_0_Float = OutlineThickness;
                    float _Property_03c50ad283391283aa8542fdac3037c9_Out_0_Float = Outline_Depth_Sensitivity;
                    float _Property_9f2db7005b7f1d88ad44ab621f47edca_Out_0_Float = Outline_Normals_Sensitivity;
                    float _Property_572647935a49d881b1a163fbce2d2818_Out_0_Float = Outline_Color_Sensitivity;
                    float4 _Property_c94db4cdd5812a819fb65e72136963ea_Out_0_Vector4 = Outline_Color;
                    float _Property_cce8972ce8591486b7e601440bd201f0_Out_0_Float = EdgesControlA;
                    float _Property_612ac8996b55278ea5e075f98af71740_Out_0_Float = EdgesControlB;
                    float _Property_ab11905175a1778289cf9c3144111922_Out_0_Float = EdgesControlC;
                    float _Property_4c5b957d3133b682af443321fe90777d_Out_0_Float = EdgesControlD;
                    float4 _Combine_e051dfc0ced05880b27440caa778e7db_RGBA_4_Vector4;
                    float3 _Combine_e051dfc0ced05880b27440caa778e7db_RGB_5_Vector3;
                    float2 _Combine_e051dfc0ced05880b27440caa778e7db_RG_6_Vector2;
                    Unity_Combine_float(_Property_cce8972ce8591486b7e601440bd201f0_Out_0_Float, _Property_612ac8996b55278ea5e075f98af71740_Out_0_Float, _Property_ab11905175a1778289cf9c3144111922_Out_0_Float, _Property_4c5b957d3133b682af443321fe90777d_Out_0_Float, _Combine_e051dfc0ced05880b27440caa778e7db_RGBA_4_Vector4, _Combine_e051dfc0ced05880b27440caa778e7db_RGB_5_Vector3, _Combine_e051dfc0ced05880b27440caa778e7db_RG_6_Vector2);
                    float4 _OutlineGCustomFunction_be7d133399258989840a9e05942a3725_Out_0_Vector4;
                    OutlineG_float((_UV_42f187a9c9bfec89b114f1eb5e162d18_Out_0_Vector4.xy), _Property_5b1553fcff376a8ca74dd3eed2a5d184_Out_0_Float, _Property_03c50ad283391283aa8542fdac3037c9_Out_0_Float, _Property_9f2db7005b7f1d88ad44ab621f47edca_Out_0_Float, _Property_572647935a49d881b1a163fbce2d2818_Out_0_Float, _Property_c94db4cdd5812a819fb65e72136963ea_Out_0_Vector4, _Combine_e051dfc0ced05880b27440caa778e7db_RGBA_4_Vector4, _OutlineGCustomFunction_be7d133399258989840a9e05942a3725_Out_0_Vector4);
                    float _Property_cfe328f23066421cac1fd16191331eed_Out_0_Float = _rainContrast;
                    float4x4 _Property_acd68bd49475400b8ea9cf1c66aacff6_Out_0_Matrix4 = _CamToWorld;
                    float4 _ScreenPosition_7ae6f1418a014e0d8f671681bf269b14_Out_0_Vector4 = float4(i.screenPos.xy, 0, 0);
                    float _Split_0d99586972e743b5b4821073abd4db84_R_1_Float = _ScreenPosition_7ae6f1418a014e0d8f671681bf269b14_Out_0_Vector4[0];
                    float _Split_0d99586972e743b5b4821073abd4db84_G_2_Float = _ScreenPosition_7ae6f1418a014e0d8f671681bf269b14_Out_0_Vector4[1];
                    float _Split_0d99586972e743b5b4821073abd4db84_B_3_Float = _ScreenPosition_7ae6f1418a014e0d8f671681bf269b14_Out_0_Vector4[2];
                    float _Split_0d99586972e743b5b4821073abd4db84_A_4_Float = _ScreenPosition_7ae6f1418a014e0d8f671681bf269b14_Out_0_Vector4[3];
                    float2 _Vector2_58dcfc7766f749fe80b3c0dab107ab25_Out_0_Vector2 = float2(_Split_0d99586972e743b5b4821073abd4db84_R_1_Float, _Split_0d99586972e743b5b4821073abd4db84_G_2_Float);
                    float4 _Property_d39a592a532b46febbcaa9f3b99b4263_Out_0_Vector4 = _interactPointRadius;
                    float4 _Property_4c4b454bb6a34546aa8a9c891366989d_Out_0_Vector4 = _radialControls;
                    float4 _Property_1ae94a49f46944da9ecbde706ee4f622_Out_0_Vector4 = _directionControls;
                    float4 _Property_9975bec75d8f460ca6c00c9a375335a2_Out_0_Vector4 = _wipeControls;
                    float4 _Property_bffd8c9b0f9d4e93a580c06d736951f2_Out_0_Vector4 = _mainTexTilingOffset;
                    float _Property_d19406e38a3e4a4ebbb5774ff96ca4c6_Out_0_Float = _maskPower;
                    float _Property_c947100fca8040f785fa6173bb52040a_Out_0_Float = _Size;
                    float _Property_155425a620f54407932f079b022a487f_Out_0_Float = _Distortion;
                    float _Property_244e723a86004c44b8e296b5ba794814_Out_0_Float = _Blur;
                    float4 _Property_00cd008de37944dc8131bc4bd3a47421_Out_0_Vector4 = _TimeOffset;
                    float4 _Property_5116bb32fc87407a84caea4c48d32584_Out_0_Vector4 = _EraseCenterRadius;
                    float _Property_732686e83b774c1596e04c06a1d47de7_Out_0_Float = _erasePower;
                    float _Property_09c7f725fa1946c3ac249b27296fc4fa_Out_0_Float = _TileNumCausticRotMin;
                    float4 _Property_828df1b8c9d1455b9884dfc1f2c4b90f_Out_0_Vector4 = _RainSmallDirection;
                    float4 _Rain3DGCustomFunction_bc2c1f8dc1a348a1826facf2bc356766_Out_0_Vector4;
                    Rain3DG_float(_Property_acd68bd49475400b8ea9cf1c66aacff6_Out_0_Matrix4, i.wpos, _Vector2_58dcfc7766f749fe80b3c0dab107ab25_Out_0_Vector2, _Vector2_58dcfc7766f749fe80b3c0dab107ab25_Out_0_Vector2, (float4(_Vector2_58dcfc7766f749fe80b3c0dab107ab25_Out_0_Vector2, 0.0, 1.0)), _Property_d39a592a532b46febbcaa9f3b99b4263_Out_0_Vector4, _Property_4c4b454bb6a34546aa8a9c891366989d_Out_0_Vector4, _Property_1ae94a49f46944da9ecbde706ee4f622_Out_0_Vector4, _Property_9975bec75d8f460ca6c00c9a375335a2_Out_0_Vector4, _Property_bffd8c9b0f9d4e93a580c06d736951f2_Out_0_Vector4, _Property_d19406e38a3e4a4ebbb5774ff96ca4c6_Out_0_Float, _ScreenPosition_7ae6f1418a014e0d8f671681bf269b14_Out_0_Vector4, _Property_c947100fca8040f785fa6173bb52040a_Out_0_Float, _Property_155425a620f54407932f079b022a487f_Out_0_Float, _Property_244e723a86004c44b8e296b5ba794814_Out_0_Float, _Property_00cd008de37944dc8131bc4bd3a47421_Out_0_Vector4, _Property_5116bb32fc87407a84caea4c48d32584_Out_0_Vector4, _Property_732686e83b774c1596e04c06a1d47de7_Out_0_Float, _Property_09c7f725fa1946c3ac249b27296fc4fa_Out_0_Float, _Property_828df1b8c9d1455b9884dfc1f2c4b90f_Out_0_Vector4, _Rain3DGCustomFunction_bc2c1f8dc1a348a1826facf2bc356766_Out_0_Vector4);
                    float _Property_2343525e6b7c4a7682fbb0db2b83fcf9_Out_0_Float = _ObjectRainPower;
                    float4 _Multiply_d4de52a4d4684d2ba5804f85549f60a2_Out_2_Vector4;
                    Unity_Multiply_float4_float4(_Rain3DGCustomFunction_bc2c1f8dc1a348a1826facf2bc356766_Out_0_Vector4, (_Property_2343525e6b7c4a7682fbb0db2b83fcf9_Out_0_Float.xxxx), _Multiply_d4de52a4d4684d2ba5804f85549f60a2_Out_2_Vector4);
                    float _Property_ca502e88bef04ab9834402991644cdec_Out_0_Float = _CameraRainPower;
                    float4 _RainGCustomFunction_1e23d5ac2dbb48a69ffdfcc0f189fea3_Out_0_Vector4;
                    RainG_float(i.wpos, _Vector2_58dcfc7766f749fe80b3c0dab107ab25_Out_0_Vector2, _Vector2_58dcfc7766f749fe80b3c0dab107ab25_Out_0_Vector2, (float4(_Vector2_58dcfc7766f749fe80b3c0dab107ab25_Out_0_Vector2, 0.0, 1.0)), _Property_d39a592a532b46febbcaa9f3b99b4263_Out_0_Vector4, _Property_4c4b454bb6a34546aa8a9c891366989d_Out_0_Vector4, _Property_1ae94a49f46944da9ecbde706ee4f622_Out_0_Vector4, _Property_9975bec75d8f460ca6c00c9a375335a2_Out_0_Vector4, _Property_bffd8c9b0f9d4e93a580c06d736951f2_Out_0_Vector4, _Property_d19406e38a3e4a4ebbb5774ff96ca4c6_Out_0_Float, _ScreenPosition_7ae6f1418a014e0d8f671681bf269b14_Out_0_Vector4, _Property_c947100fca8040f785fa6173bb52040a_Out_0_Float, _Property_155425a620f54407932f079b022a487f_Out_0_Float, _Property_244e723a86004c44b8e296b5ba794814_Out_0_Float, _Property_00cd008de37944dc8131bc4bd3a47421_Out_0_Vector4, _Property_5116bb32fc87407a84caea4c48d32584_Out_0_Vector4, _Property_732686e83b774c1596e04c06a1d47de7_Out_0_Float, _Property_09c7f725fa1946c3ac249b27296fc4fa_Out_0_Float, _Property_828df1b8c9d1455b9884dfc1f2c4b90f_Out_0_Vector4, _RainGCustomFunction_1e23d5ac2dbb48a69ffdfcc0f189fea3_Out_0_Vector4);
                    float4 _Multiply_59be09a325e245afbef8616daa6c5ea6_Out_2_Vector4;
                    Unity_Multiply_float4_float4((_Property_ca502e88bef04ab9834402991644cdec_Out_0_Float.xxxx), _RainGCustomFunction_1e23d5ac2dbb48a69ffdfcc0f189fea3_Out_0_Vector4, _Multiply_59be09a325e245afbef8616daa6c5ea6_Out_2_Vector4);
                    float4 _Add_1242b844e5414b1a92d2c2df40e72298_Out_2_Vector4;
                    Unity_Add_float4(_Multiply_d4de52a4d4684d2ba5804f85549f60a2_Out_2_Vector4, _Multiply_59be09a325e245afbef8616daa6c5ea6_Out_2_Vector4, _Add_1242b844e5414b1a92d2c2df40e72298_Out_2_Vector4);
                    float _Split_67b7dad1e5124124bb48e2593ff3f70c_R_1_Float = _Add_1242b844e5414b1a92d2c2df40e72298_Out_2_Vector4[0];
                    float _Split_67b7dad1e5124124bb48e2593ff3f70c_G_2_Float = _Add_1242b844e5414b1a92d2c2df40e72298_Out_2_Vector4[1];
                    float _Split_67b7dad1e5124124bb48e2593ff3f70c_B_3_Float = _Add_1242b844e5414b1a92d2c2df40e72298_Out_2_Vector4[2];
                    float _Split_67b7dad1e5124124bb48e2593ff3f70c_A_4_Float = _Add_1242b844e5414b1a92d2c2df40e72298_Out_2_Vector4[3];
                    float3 _Vector3_6f1c7465a2a94d4ea4455bc662130bad_Out_0_Vector3 = float3(_Split_67b7dad1e5124124bb48e2593ff3f70c_R_1_Float, _Split_67b7dad1e5124124bb48e2593ff3f70c_R_1_Float, _Split_67b7dad1e5124124bb48e2593ff3f70c_R_1_Float);
                    float _Property_d877fe2ee63c41cc873da570a1f642d0_Out_0_Float = _rainPower;
                    float3 _Power_5815338e4322493c932c961db32a0967_Out_2_Vector3;
                    Unity_Power_float3(_Vector3_6f1c7465a2a94d4ea4455bc662130bad_Out_0_Vector3, (_Property_d877fe2ee63c41cc873da570a1f642d0_Out_0_Float.xxx), _Power_5815338e4322493c932c961db32a0967_Out_2_Vector3);
                    float3 _Saturate_e71282933dba43b4b9dca0d50605de07_Out_1_Vector3;
                    Unity_Saturate_float3(_Power_5815338e4322493c932c961db32a0967_Out_2_Vector3, _Saturate_e71282933dba43b4b9dca0d50605de07_Out_1_Vector3);
                    float3 _Multiply_75a232e9fcac47cfab4c6a92b3cc57cb_Out_2_Vector3;
                    Unity_Multiply_float3_float3((_Property_cfe328f23066421cac1fd16191331eed_Out_0_Float.xxx), _Saturate_e71282933dba43b4b9dca0d50605de07_Out_1_Vector3, _Multiply_75a232e9fcac47cfab4c6a92b3cc57cb_Out_2_Vector3);
                    float3 _Add_97551952cafe442892ec172a13b6c556_Out_2_Vector3;
                    Unity_Add_float3((_OutlineGCustomFunction_be7d133399258989840a9e05942a3725_Out_0_Vector4.xyz), _Multiply_75a232e9fcac47cfab4c6a92b3cc57cb_Out_2_Vector3, _Add_97551952cafe442892ec172a13b6c556_Out_2_Vector3);
                    float _Property_0000421d0a6e49cd9d5805c2884be9fe_Out_0_Float = _snowBrightness;
                    float _Multiply_13f11ae5f1b247668d3516a92b514cd5_Out_2_Float;
                    Unity_Multiply_float_float(0, _Property_0000421d0a6e49cd9d5805c2884be9fe_Out_0_Float, _Multiply_13f11ae5f1b247668d3516a92b514cd5_Out_2_Float);
                    float3 _Add_a1d12e4ab52445a4a7cc6bb79feda8bb_Out_2_Vector3;
                    Unity_Add_float3(_Add_97551952cafe442892ec172a13b6c556_Out_2_Vector3, (_Multiply_13f11ae5f1b247668d3516a92b514cd5_Out_2_Float.xxx), _Add_a1d12e4ab52445a4a7cc6bb79feda8bb_Out_2_Vector3);
                    
                    //
                    Texture2D _Property_f8d46e3480414dc1be2614747813d736_Out_0_Texture2D = UnityBuildTexture2DStructNoScaleA(_Lux_RainRipples);
                    
                    float _Property_93e79906d73145ed83f10a94c3b70424_Out_0_Float = _Lux_RainIntensity;
                    float _Property_93b764a213f24640a498bdd048fe4e8b_Out_0_Float = _Lux_RippleAnimSpeed;
                    float _Property_f47250fde915416e961f0e32e5eeb3a4_Out_0_Float = _Lux_RippleTiling;
                    float _Property_e9cf9a5a1a7d486f87ef9b0b7f93572b_Out_0_Float = _Lux_WaterBumpDistance;
                    float3 _RainDropsCustomFunction_d656f49d24dc48d085ceeabfe3a7d9b4_Out_0_Vector3;
                   // RainDrops_float(_Vector2_58dcfc7766f749fe80b3c0dab107ab25_Out_0_Vector2, _Property_acd68bd49475400b8ea9cf1c66aacff6_Out_0_Matrix4, _Property_f8d46e3480414dc1be2614747813d736_Out_0_Texture2D, UnityBuildSamplerStateStruct(SamplerState_Trilinear_Repeat), _Property_93e79906d73145ed83f10a94c3b70424_Out_0_Float, _Property_93b764a213f24640a498bdd048fe4e8b_Out_0_Float, _Property_f47250fde915416e961f0e32e5eeb3a4_Out_0_Float, _Property_e9cf9a5a1a7d486f87ef9b0b7f93572b_Out_0_Float, _RainDropsCustomFunction_d656f49d24dc48d085ceeabfe3a7d9b4_Out_0_Vector3);
                    RainDrops_float(_Vector2_58dcfc7766f749fe80b3c0dab107ab25_Out_0_Vector2, 
                        _Property_acd68bd49475400b8ea9cf1c66aacff6_Out_0_Matrix4, 
                        _Property_f8d46e3480414dc1be2614747813d736_Out_0_Texture2D, 
                        (SamplerState)UnityBuildSamplerStateStruct(SamplerState_Trilinear_Repeat), _Property_93e79906d73145ed83f10a94c3b70424_Out_0_Float, _Property_93b764a213f24640a498bdd048fe4e8b_Out_0_Float, _Property_f47250fde915416e961f0e32e5eeb3a4_Out_0_Float, _Property_e9cf9a5a1a7d486f87ef9b0b7f93572b_Out_0_Float, _RainDropsCustomFunction_d656f49d24dc48d085ceeabfe3a7d9b4_Out_0_Vector3);

                    
                    float _Split_c663a0af4c3943dc9693b9d3b2122ad1_R_1_Float = _RainDropsCustomFunction_d656f49d24dc48d085ceeabfe3a7d9b4_Out_0_Vector3[0];
                    float _Split_c663a0af4c3943dc9693b9d3b2122ad1_G_2_Float = _RainDropsCustomFunction_d656f49d24dc48d085ceeabfe3a7d9b4_Out_0_Vector3[1];
                    float _Split_c663a0af4c3943dc9693b9d3b2122ad1_B_3_Float = _RainDropsCustomFunction_d656f49d24dc48d085ceeabfe3a7d9b4_Out_0_Vector3[2];
                    float _Split_c663a0af4c3943dc9693b9d3b2122ad1_A_4_Float = 0;
                    float _Property_72dcc6d6a47449f580d9c1363b4d98be_Out_0_Float = _Lux_RainBrightness;
                    float _Multiply_0107fe7f75dd464ba40fcfb3133f840a_Out_2_Float;
                    Unity_Multiply_float_float(_Split_c663a0af4c3943dc9693b9d3b2122ad1_R_1_Float, _Property_72dcc6d6a47449f580d9c1363b4d98be_Out_0_Float, _Multiply_0107fe7f75dd464ba40fcfb3133f840a_Out_2_Float);
                    float3 _Add_726bc2747bcf4c9d84f53dea28ce4703_Out_2_Vector3;
                    Unity_Add_float3(_Add_a1d12e4ab52445a4a7cc6bb79feda8bb_Out_2_Vector3, (_Multiply_0107fe7f75dd464ba40fcfb3133f840a_Out_2_Float.xxx), _Add_726bc2747bcf4c9d84f53dea28ce4703_Out_2_Vector3);
                    out1.rgb = _Add_726bc2747bcf4c9d84f53dea28ce4703_Out_2_Vector3;
                //}

                return  (out1) * _Weight + (1- _Weight)* float4(tex2D(_MainTex, i.uv).rgb,1);//
            }
            ENDHLSL
        }
    }
}
