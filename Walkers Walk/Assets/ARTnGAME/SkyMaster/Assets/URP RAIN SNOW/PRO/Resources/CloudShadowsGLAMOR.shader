Shader "Hidden/VolFx/CloudShadowsGLAMOR"
{
    Properties
    {
        /* Outline_Depth_Sensitivity("Depth Sensitivity", Float) = 0
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
        _Lux_RainBrightness("_Lux_RainBrightness", Float) = 1*/

        _cloudTexture("_cloudTexture", 2D) = "white" {}
        _sunTransform("_sunTransform", Float) = (0, 0, 0, 0)
		_Weight("Weight", Float) = 1
        _noiseCloudSpeed("_noiseCloudSpeed", Float) = (0, 0, 0, 0)

        _cloudShadowScale("_cloudShadowScale", Float) = (1, 1, 0, 0)
        _cloudShadowColor("_cloudShadowColor", Color) = (0, 0, 0, 0)

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
       /* float Outline_Depth_Sensitivity;
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
        float _ObjectRainPower;*/
        float4 cameraDepthNormalsTextureA_TexelSize;
       // UNITY_TEXTURE_STREAMING_DEBUG_VARS;
       // CBUFFER_END


        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);
        TEXTURE2D(_CameraDepthNormalsTextureA);
        SAMPLER(sampler_CameraDepthNormalsTextureA);
            TEXTURE2D(_cloudTexture);
            SAMPLER(sampler_cloudTexture);
            float3 _sunTransform;
            float2 _noiseCloudSpeed;
            float4x4 _CamToWorld;
            float4 _cloudShadowScale;
            float4 _cloudShadowColor;

            // Object and Global properties
         /*   SAMPLER(SamplerState_Trilinear_Repeat);
            TEXTURE2D(_snowTexture);
            SAMPLER(sampler_snowTexture);
            TEXTURE2D(_snowBumpMap);
            SAMPLER(sampler_snowBumpMap);
            TEXTURE2D(_Lux_RainRipples);
            SAMPLER(sampler_Lux_RainRipples);*/
            TEXTURE2D(cameraDepthNormalsTextureA);
            SAMPLER(samplercameraDepthNormalsTextureA);

            //#include "UnityCG.cginc"
            //#include "../../RainLITEWEATHERNONSG.hlsl"


           // sampler2D _MainTex;   
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
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




            ///////////////////// UnityCG
            // Encoding/decoding [0..1) floats into 8 bit/channel RGBA. Note that 1.0 will not be encoded properly.
            inline float4 EncodeFloatRGBA(float v)
            {
                float4 kEncodeMul = float4(1.0, 255.0, 65025.0, 16581375.0);
                float kEncodeBit = 1.0 / 255.0;
                float4 enc = kEncodeMul * v;
                enc = frac(enc);
                enc -= enc.yzww * kEncodeBit;
                return enc;
            }
            inline float DecodeFloatRGBA(float4 enc)
            {
                float4 kDecodeDot = float4(1.0, 1 / 255.0, 1 / 65025.0, 1 / 16581375.0);
                return dot(enc, kDecodeDot);
            }
            // Encoding/decoding [0..1) floats into 8 bit/channel RG. Note that 1.0 will not be encoded properly.
            inline float2 EncodeFloatRG(float v)
            {
                float2 kEncodeMul = float2(1.0, 255.0);
                float kEncodeBit = 1.0 / 255.0;
                float2 enc = kEncodeMul * v;
                enc = frac(enc);
                enc.x -= enc.y * kEncodeBit;
                return enc;
            }
            inline float DecodeFloatRG(float2 enc)
            {
                float2 kDecodeDot = float2(1.0, 1 / 255.0);
                return dot(enc, kDecodeDot);
            }
            // Encoding/decoding view space normals into 2D 0..1 vector
            inline float2 EncodeViewNormalStereo(float3 n)
            {
                float kScale = 1.7777;
                float2 enc;
                enc = n.xy / (n.z + 1);
                enc /= kScale;
                enc = enc * 0.5 + 0.5;
                return enc;
            }
            inline float3 DecodeViewNormalStereo(float4 enc4)
            {
                float kScale = 1.7777;
                float3 nn = enc4.xyz * float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
                float g = 2.0 / dot(nn.xyz, nn.xyz);
                float3 n;
                n.xy = g * nn.xy;
                n.z = g - 1;
                return n;
            }
            inline float4 EncodeDepthNormal(float depth, float3 normal)
            {
                float4 enc;
                enc.xy = EncodeViewNormalStereo(normal);
                enc.zw = EncodeFloatRG(depth);
                return enc;
            }
            inline void DecodeDepthNormal(float4 enc, out float depth, out float3 normal)
            {
                depth = DecodeFloatRG(enc.zw);
                normal = DecodeViewNormalStereo(enc);
            }
            ///////////// END UNITY CG

            ///////////// CAUSTIC
            //https://twitter.com/ilgiz_den
            float2x2 rotate2D(float a)
            {
                return float2x2(cos(a), sin(a), -sin(a), cos(a));
            }
            float mod(float x, float y)
            {
                return x - y * floor(x / y);
            }
            //https://github.com/doxas/twigl
            float3 hsv(float h, float s, float v) {
                float4 t = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(float3(1, 1, 1) * (h)+t.xyz) * 6.0 - float3(1, 1, 1) * (t.w));
                return v * lerp(float3(1, 1, 1) * (t.x), clamp(p - float3(1, 1, 1) * (t.x), 0.0, 1.0), s);
            }
            float3x3 rotate3D(float angle, float3 axis) {
                float3 a = normalize(axis);
                float s = sin(angle);
                float c = cos(angle);
                float r = 1.0 - c;
                return float3x3(
                    a.x * a.x * r + c,
                    a.y * a.x * r + a.z * s,
                    a.z * a.x * r - a.y * s,
                    a.x * a.y * r - a.z * s,
                    a.y * a.y * r + c,
                    a.z * a.y * r + a.x * s,
                    a.x * a.z * r + a.y * s,
                    a.y * a.z * r - a.x * s,
                    a.z * a.z * r + c
                    );
            }
            //float PI = 3.141592653589793;
            //////////// END CAUSTIC




            // Generate a random float between 0 and 1 using the pixel position and seed
            float RandomFloat(float2 seed)
            {
                return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453) + 0.00001;
            }


            // Generate random numbers in a normal distribution using Box-Muller transform
            void GenerateNormalRandom_float(float2 seed, float min, float max, out float3 Out)
            {
                // Pseudo-random numbers between 0 and 1
                float u1 = RandomFloat(seed);
                float u2 = RandomFloat(seed + float2(132.54, 465.32));

                // Box-Muller transform to convert uniform distribution to normal distribution
                float radius = sqrt(-2.0 * log(u1)) * 4.0;
                float theta = 2.0 * 3.1415926535897932384626433832795 * u2;

                // Convert polar coordinates to Cartesian coordinates
                float x = radius * cos(theta);
                float y = radius * sin(theta);

                // Scale and shift the results to the desired range [min, max]
                float2 result;
                result.x = x * (max - min) + (min);
                result.y = y * (max - min) + (min);

                Out = float3(result.xy, 1);
            }











            half4 frag(const frag_in i) : SV_Target
            {
                float4 colorBuffer = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
                half4 main = (colorBuffer + 5 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, ditherB(i.uv))) / 6;

                main.rgb = pow(main.rgb, 15) * 1;

                //main = pow(main, 4.5)*1810;
                //half4 main = tex2D(_MainTex, i.uv);
                half4 col  = luma(main.rgb);

                //return main + float4(ditherB(i.uv).rgb,0);
                
                //return saturate(half4((main.rgb), main.a)) * 2;

                float4 out1 = float4(0,0,0,0);
                if (_rainMode == 2) {
                    out1 = saturate(half4(lerp(main.rgb + dither(i.uv).rgb, luma(col.rgb) + 2 * dither(i.uv).rgb, _Weight), main.a)) * 64
                        + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, dither(i.uv)) * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (i.uv)) * 0.5 + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (i.uv)) * 0.5;
                    out1 = out1 * 0.65 + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * 0.65;
                }
                if (_rainMode == 3) {
                    out1 = saturate(half4(lerp(main.rgb + ditherA(i.uv).rgb, luma(col.rgb) + 1 * dither(i.uv).rgb, _Weight), main.a)) * 64
                        + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, dither(i.uv)) * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (i.uv)) * 0.5 + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (i.uv)) * 0.5;
                    out1 = out1 * 0.65 + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * 0.65;
                } 
                if (_rainMode == 4) {
                    out1 = saturate(half4(lerp(main.rgb + ditherB(i.uv).rgb, luma(col.rgb) + 1 * ditherB(i.uv).rgb, _Weight), main.a)) * 64
                        + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, dither(i.uv)) * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (i.uv)) * 0.5 + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (i.uv)) * 0.5;
                    out1 = out1 * 0.65 + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * 0.65;
                }



                float3 normal = 0;
                //float depth = 0;
                float depth2 = 0;
                DecodeDepthNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTextureA, sampler_CameraDepthNormalsTextureA, i.uv), depth2, normal);
                normal = mul((float3x3)_CamToWorld, normal);

                float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).r;
                depth = Linear01Depth(depth, _ZBufferParams);
                _CamToWorld[0][3] = 0;
                _CamToWorld[1][3] = 0;
                _CamToWorld[2][3] = 0;
                _CamToWorld[3][3] = 0;
                float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
                float3 vpos = float3((i.uv * 2.0 - 1.0) / p11_22, -1.0) * depth;
                float4 wpos = mul(_CamToWorld, float4(vpos, 1));
                wpos += float4(_WorldSpaceCameraPos, 0) / _ProjectionParams.z;
                wpos *= _cloudShadowScale.x * _ProjectionParams.z * 0.1;


                if (_rainMode == 0) {
                   
                    float4 cloudColor = 
                        SAMPLE_TEXTURE2D(_cloudTexture, sampler_cloudTexture, wpos.xz * _cloudShadowScale.x * 0.1 + _noiseCloudSpeed.xy*_Time.y) * _cloudShadowColor;

                    if (depth > 0.99999 ){//|| (normal.y < 0.5 && normal.y>0)) {
                        out1 =  colorBuffer;
                    }
                    else {
                        out1 = pow(cloudColor, _cloudShadowScale.y) * colorBuffer * 8 * cloudColor.a;// *abs(normal.y);
                    }
                   // out1 = colorBuffer;
                    //CAUSTICS
                    //vec2 p = FC.xy / r.y * 2e1 + t; for (float i; i++ < 8.;)
                    //p += sin(p + t / .2 + i) * .4, p *= mat2(6, -8, 8, 6) / 9.; 
                    //o = vec4(tanh(length(fwidth(sin(p * .3) / .1))), texture(b, FC.xy / r));
                }

                // CAUSTIC
                float t = _Time.y;
                if (_rainMode == 1) {
                    float4 o = 0;
                    //float2 n, q, p = (i.uv - .6) / 1;
                    float2 n, q, p = (wpos.xz * _cloudShadowScale.x * 0.1 + float2(_cloudShadowScale.y, _cloudShadowScale.z) - .6) / 1;
                    float d = dot(p, p), S = 6., i, a, j;
                    for (float2x2 m = rotate2D(2.9); j++ < 9.;) {
                        p = mul(p, m * 1.3);
                        n = mul(n, m * 1.2);
                        q = p * S + t * 1.75 + sin(t * 0.4 - d * 2.) * 0.03 - j - n;
                        a += dot(cos(q) / S, float2(0.2, 0.1));
                        n -= sin(q);
                        S *= 1.229;
                    }
                    o += (a + .2) * float4(1, 1, 1, 0) + a + a - d + d * 0.5;
                    //wpos.xz * _cloudShadowScale.x * 0.1
                    out1 = saturate(float4(pow(o.rgb,1),4))* colorBuffer*10 + colorBuffer * 1;
                    // void GenerateNormalRandom_float(float2 seed, float min, float max, out float3 Out)
                }

                if (_rainMode == 5) {
                  /*  float2 n, q, u = float2(i.uv - .5);
                    float d = dot(u, u), s = 8, t = _Time.y, j;
                    float4 o = 0;
                    for (float2x2 m = rotate2D(5); j++ < 16;) {
                        u = mul(m, u); n = mul(m, n);
                        q = u * s + t * 4 + sin(t * 4 - d * 6) * 0.8 + j + n;
                        o += dot(cos(q) / s, float2(2, 2));
                        n -= sin(q);
                        s *= 1.3;
                    }
                    return o * float4(4, 2, 1, 0);*/

                    float2 p = float2((wpos.xz * _cloudShadowScale.x * 0.1 + float2(_cloudShadowScale.y, _cloudShadowScale.z) - .5) * 2e1 + t);
                   // float2 p = float2((i.uv - .5) * 2e1 + t);
                    //float d = dot(u, u), s = 8, t = _Time.y, j;
                    float4 o = 0;
                    for (float j = 0; j<8; j++) {
                        p += sin(p + t / .2 + j) * .4;
                        //p *= float2x2(6, -8, 8, 6) / 9.;
                        p = mul(p,float2x2(6, -8, 8, 6) / 9.);

                        //u = mul(m, u); n = mul(m, n);
                        //q = u * s + t * 4 + sin(t * 4 - d * 6) * 0.8 + j + n;
                        //o += dot(cos(q) / s, float2(2, 2));
                       // n -= sin(q);
                       // s *= 1.3;
                    }
                    float4 outtt = pow(saturate(2*1 * float(tanh(length(fwidth(sin(p * .3) / .1))))*0.5*pow(abs(normal.y),1)),2.5)*float4(0.5, 0.5,0,1);
                    float4 outtt1 = pow(saturate(2 * 1 * float(tanh(length(fwidth(sin(p * .1) / .1)))) * 0.5 * pow(abs(normal.y), 1)), 2.5) * float4(5, 0.4, 0.8, 1);
                    float4 outtt2 = pow(saturate(2 * 1 * float(tanh(length(fwidth(sin(p * .2) / .1)))) * 0.5 * pow(abs(normal.y), 1)), 2.5) * float4(0, 0.5, 0.8, 1);
                    return (outtt + outtt1 + outtt2) + colorBuffer * 1;

                    //vec2 p = FC.xy / r.y * 2e1 + t; 
                    //for (float i; i++ < 8.;)
                    //    p += sin(p + t / .2 + i) * .4, 
                    // p *= mat2(6, -8, 8, 6) / 9.; 
                    //o = vec4(tanh(length(fwidth(sin(p * .3) / .1))), texture(b, FC.xy / r));
                }

                if (_rainMode == 6) {
                    float4 o = 0;
                    float2 n, q, p = (wpos.xz * _cloudShadowScale.x * 0.1  +float2(_cloudShadowScale.y, _cloudShadowScale.z) - .5 * 1) / 1;
                    float d = dot(p, p), S = 9., a, j;
                    for (float2x2 m = rotate2D(5.); j++ < 20.;) {
                        p = mul(p, m);
                        n = mul(n, m);
                        q = p * S + _Time.y * 4. + sin(_Time.y * 4. - d * 6.) * .8 + j + n;
                        a += dot(cos(q) / S, float2(0.2, 0.2));
                        n -= sin(q);
                        S *= 1.2;
                    }
                    o += (a + .2) * float4(4, 2, 1, 0) + a + a - d;
                    float4 colorBufferA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, saturate(i.uv-o.xz*0.0525) + n*0.2);
                    if (length(i.uv - o.xz * 0.0125) > 0.5) {
                        //colorBufferA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                    }
                    out1 = saturate(float4(pow(o.rgb, 1), 5)) * 1 * 2 + colorBufferA * 0.2*pow(normal.y,5) * normal.y * normal.y + colorBuffer * 1.1;
                   // return o;
                }

                if (_rainMode == 7) {
                    float4 o = 0;
                    float2 n, q, p = (wpos.xz * _cloudShadowScale.x * 0.1 + float2(_cloudShadowScale.y, _cloudShadowScale.z) - .5 * 1) / 1;//  (i.uv - .5 * 1) / 1;
                    float d = dot(p, p), S = 20., a, j;
                    for (float2x2 m = rotate2D(10.); j++ < 30.;) {
                        p = mul(p, m);
                        n = mul(n, m);
                        q = p * S + _Time.y * 4. + sin(_Time.y * 4. - d * 6.) * .8 + j + n;
                        a += dot(cos(q) / S, float2(.3, .3));
                        n -= sin(q);
                        S *= 1.2;
                    }
                    o += (12 * (a + .1) * float4(1, 2, 1, 0) * 0.5 + a + a - d) + float4(0.5, 0, 0, 1);

                    //wpos.xz * _cloudShadowScale.x * 0.1
                    //out1 = saturate(float4(pow(o.rgb, 1), 6)) * colorBuffer * 4 + colorBuffer * 1;

                    float4 colorBufferA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, saturate(i.uv - o.xz * 0.0525) + n * 1.6);                   
                    out1 = saturate(float4(pow(o.rgb, 1), 5)) * 1 * 1 + colorBufferA * 0.2 * pow(normal.y, 2) * normal.y * normal.y + colorBuffer * 1.1;
                    //return o;
                }

                if (_rainMode == 8) {
                    float4 o = 0;
                    float2 n, q, p = (i.uv - .5 * 1) / 1;
                    float d = dot(p, p), S = 6., i, a, j;
                    for (float2x2 m = rotate2D(2.); j++ < 70.;) {
                        p = mul(p, m * 1.3);
                        n = mul(n, m * 1.2);
                        q = p * S + t * 4. + sin(t * 10. - d * 12.) * 0.7 + j - n;
                        a += dot(cos(q) / S, float2(0.2, 0.2));
                        n -= sin(q);
                        S *= 1.2;
                    }
                    o += (a + .2) * float4(4, 2, 1, 0) + a + a + a + a + d - d;
                    return o;
                }
                if (_rainMode == 9) {
                    float4 o = 0;
                    float2 n, q, p = (i.uv - .5 * 1) / 1;
                    float d = dot(p, p), S = 6., i, a, j;
                    for (float2x2 m = rotate2D(2.); j++ < 70.;) {
                        p = mul(p, m * 1.24);
                        n = mul(n, m * 1.2);
                        q = p * S + t * 4. + sin(t * 10. - d * 12.) * 0.3 - j - n;
                        a += dot(cos(q) / S, float2(0.2, 0.2));
                        n -= sin(q);
                        S *= 1.2;
                    }
                    o += (a + .2) * float4(4, 2, 1, 0) + a + a + a + d - d;
                    return o;
                }

                // CASUTIC
                if (_rainMode == 10) {
                    float4 o = 0;
                    float2 n, q, p = (i.uv - .6) / 1;
                    float d = dot(p, p), S = 6., i, a, j;
                    for (float2x2 m = rotate2D(2.9); j++ < 9.;) {
                        p = mul(p, m * 1.3);
                        n = mul(n, m * 1.2);
                        q = p * S + t * 1.75 + sin(t * 0.4 - d * 2.) * 0.03 - j - n;
                        a += dot(cos(q) / S, float2(0.2, 0.1));
                        n -= sin(q);
                        S *= 1.229;
                    }
                    o += (a + .2) * float4(4, 2, 1, 0) + a + a - d + d * 0.5;
                    return o;
                }
                if (_rainMode == 11) {
                    float4 o = 0;
                    float2 n, q, p = (wpos.xz * _cloudShadowScale.x * 1 - .6*_cloudShadowScale.y) / 1; //(i.uv - .6) / 1;
                    float d = dot(p, p), S = 6., i, a, j;
                    for (float2x2 m = rotate2D(1.5); j++ < 25.;) {
                        p = mul(p, m * 1.24) * _cloudShadowScale.z;
                        n = mul(n, m * 1.2);
                        q = p * S + t * 3. + sin(t * 0.1 - d * 12.) * 0.13 - j - n;
                        a += dot(cos(q) / S, float2(0.2, 0.1));
                        n -= sin(q);
                        S *= 1.2;
                    }
                    o += (a + .2) * float4(7.5, 5, 4, 0) + a - d * 2.;

                    //wpos.xz * _cloudShadowScale.x * 0.1
                    float3 randOut;
                    GenerateNormalRandom_float(1, 0, 10, randOut);
                    out1 = saturate(float4(pow(o.rgb, 1), 3)) * colorBuffer * 2 * 1 + colorBuffer;
                    // void GenerateNormalRandom_float(float2 seed, float min, float max, out float3 Out)
                    //return o;
                }

                //NEBULA 1
                if (_rainMode == 12) {
                    float4 o = 0;
                    float i1, e, R, s;
                    float3 p, d = float3(i.uv, 2);
                    float3 q = float3(i.uv, 1.7);//2
                    for (q.zy--; i1++ < 39.;)
                    {
                        e += i1 / 9e4;
                        o.rgb += hsv(q.z, R * d.y, e * i1 / 15.);
                        s = 5.;
                        p = q += d * e * R * .2;
                        p = float3(log(R = length(p)) - t * .2, exp2(mod(-p.z, p.y / p.z * 5.) / R), p.x);
                        for (e = --p.y; s < 1e3; s += s) {
                            e += -abs(dot(sin(p * s), max(cos(p), exp2(cos(e * e)))) / s * .18);
                        }
                    }
                    float3 outA = 1 - pow(o.rgb, 3);
                    return float4(pow(outA, 1.4), 1) * 0.22 * float4(0.65, 1, 1, 0) + 0.1 * float4(1.5, 1, 1, 0);
                }

                if (_rainMode == 13) {
                    float4 o = 0;
                    float i1, e, R, s;
                    float3 p, d = float3(i.uv, 2);
                    float3 q = float3(i.uv, 1.7);//2
                    for (q.zy--; i1++ < 42.;)//low is smooother
                    {
                        e += i1 / 9e4;
                        o.rgb += hsv(q.z, R * d.y, e * i1 / 15.);
                        s = 5.;
                        p = q += d * e * R * .2;
                        p = float3(log(R = length(p)) - t * .4, exp2(mod(-p.z, p.y / p.z * 5.) / R), p.x);
                        for (e = --p.y; s < 1e3; s += s) {
                            e += -abs(dot(sin(p * s), max(cos(p), exp2(cos(e * e)))) / s * .35); //higher is more defined edges
                        }
                    }
                    float3 outA = pow(o.rgb, 2);
                    return float4(pow(outA, 1), 1) * 0.22 * float4(0.65, 0.65, 0.65, 0) + 0.1 * float4(1, 1, 1, 0);
                }


                ///FLOWER
                if (_rainMode == 14) {
                    float iA = 0;
                    float e = 0;
                    float R = 0;
                    float s = 0;
                    float4 o = 0;
                    float3 p, d = float3(i.uv / 1 * .2 + float2(-.1, .85), 1);
                    float3 q = float3(i.uv / 1 * .2 + float2(-.1, .85), 1);
                    for (q.zy--; iA++ < 90.;)
                    {
                        o.rgb += .03 - hsv(R + R, -d.y, min(e * e * s / .1, R) / 4.);
                        s = 2.5, p = q += d * e * R * .1 * d.z;
                        p = float3(log2(R = length(p)), exp2(R - p.z / R), atan2(p.y, p.x) * 2. - t);
                        for (e = --p.y; s < 1e2; s += s) {
                            e += dot(cos(p.xz * s), sin(p.xz * s - 5.)) / s;
                        }
                    }
                    //return o*0.5;
                    float3 outA = pow(o.rgb, 1);
                    return float4(pow(outA, 1), 1) * 0.62 * float4(0.65, 0.65, 0.65, 0) + 0.1 * float4(1, 1, 1, 0);
                }

                /*
                if (_rainMode == 3) {
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
                }
                */

                return  (out1*1) * _Weight + (1- _Weight)* float4(colorBuffer.rgb,1);//
            }
            ENDHLSL
        }
    }
}
