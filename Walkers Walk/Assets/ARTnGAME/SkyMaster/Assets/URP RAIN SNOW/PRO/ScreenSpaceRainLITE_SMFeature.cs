using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
#if UNITY_2023_3_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
using System.Reflection;

namespace Artngame.SKYMASTER
{
    public class ScreenSpaceRainLITE_SM_URPFeature : ScriptableRendererFeature
    {
        class WeatherEffectsSkyMasterPass : ScriptableRenderPass
        {

/*
#if UNITY_2023_3_OR_NEWER
            /// <summary>
            /// ///////// GRAPH
            /// </summary>
            // This class stores the data needed by the pass, passed as parameter to the delegate function that executes the pass
            private class PassData
            {    //v0.1               
                internal TextureHandle src;
                //internal TextureHandle tmpBuffer1;
                // internal TextureHandle copySourceTexture;
                public Material BlitMaterial { get; set; }
                public Material outlineMaterial { get; set; }
                // public TextureHandle SourceTexture { get; set; }
            }
            private Material m_BlitMaterial;
            TextureHandle tmpBuffer1A;
            TextureHandle tmpBuffer2A;
            TextureHandle tmpBuffer3A;
            TextureHandle previousFrameTextureA;
            TextureHandle previousDepthTextureA;
            TextureHandle currentDepth;

            RTHandle _handleTAART;
            TextureHandle _handleTAA;

            RTHandle _handleA; RTHandle _handleB; RTHandle _handleC;

            Camera currentCamera;
            float prevDownscaleFactor;//v0.1
            public Material blitMaterial = null;

            //TextureHandle currentDepth;
            //TextureHandle currentNormal;
            // Each ScriptableRenderPass can use the RenderGraph handle to add multiple render passes to the render graph
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (Camera.main != null && outlineMaterialNEW != null)
                {
                    m_BlitMaterial = blitMaterial;// blitMaterial;
                    Material CloudMaterial = outlineMaterialNEW;// blitMaterial;

                    Camera.main.depthTextureMode = DepthTextureMode.Depth;

                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                    RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                    UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                    if (Camera.main != null && cameraData.camera != Camera.main)
                    {
                        return;
                    }
                    desc.msaaSamples = 1;
                    desc.depthBufferBits = 0;
                    int rtW = desc.width;
                    int rtH = desc.height;
                    int xres = (int)(rtW / ((float)1));
                    int yres = (int)(rtH / ((float)1));
                    if (_handleA == null || _handleA.rt.width != xres || _handleA.rt.height != yres)
                    {
                        _handleA = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D);
                    }
                    if (_handleB == null || _handleB.rt.width != xres || _handleB.rt.height != yres)
                    {
                        _handleB = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D);
                    }
                    if (_handleC == null || _handleC.rt.width != xres || _handleC.rt.height != yres)
                    {
                        _handleC = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D);
                    }
                    tmpBuffer2A = renderGraph.ImportTexture(_handleA);
                    previousFrameTextureA = renderGraph.ImportTexture(_handleB);
                    previousDepthTextureA = renderGraph.ImportTexture(_handleC);
                    tmpBuffer1A = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "tmpBuffer1A", true);
                    tmpBuffer3A = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "tmpBuffer3A", true);
                    currentDepth = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "currentDepth", true);
                    if (_handleTAART == null || _handleTAART.rt.width != xres || _handleTAART.rt.height != yres)
                    {
                        _handleTAART = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D);
                        _handleTAART.rt.wrapMode = TextureWrapMode.Clamp;
                        _handleTAART.rt.filterMode = FilterMode.Bilinear;
                    }
                    _handleTAA = renderGraph.ImportTexture(_handleTAART);

                    //currentDepth = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "currentDepth", true);
                    //currentNormal = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "currentNormal", true);              
                    TextureHandle sourceTexture = resourceData.activeColorTexture;
                    //Debug.Log("IN2");
                    //grab settings if script on scene camera


                    outlineMaterialNEW.SetMatrix("_CamToWorld", Camera.main.cameraToWorldMatrix);

                    //        cmd.Blit(source, destination, outlineMaterial, 0); //v0.1

                    string passName = "BLIT1 Keep Source";
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                    {
                        passData.src = resourceData.activeColorTexture;
                        desc.msaaSamples = 1; desc.depthBufferBits = 0;
                        builder.UseTexture(passData.src, AccessFlags.Read);
                        builder.SetRenderAttachment(tmpBuffer1A, 0, AccessFlags.Write);
                        builder.AllowPassCulling(false);
                        passData.BlitMaterial = m_BlitMaterial;
                        passData.outlineMaterial = outlineMaterialNEW;
                        builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                            ExecuteBlitPass(data, context, 1, passData.src));
                    }

                    passName = "DO SNOW";
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                    {
                        passData.src = resourceData.activeColorTexture;
                        desc.msaaSamples = 1; desc.depthBufferBits = 0;
                        //builder.UseTexture(tmpBuffer1A, IBaseRenderGraphBuilder.AccessFlags.Read);
                        builder.SetRenderAttachment(tmpBuffer2A, 0, AccessFlags.Write);
                        builder.AllowPassCulling(false);
                        passData.BlitMaterial = m_BlitMaterial;
                        passData.outlineMaterial = outlineMaterialNEW; // PASS SNOW MATERIAL!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1
                        builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                           // ExecuteBlitPass(data, context, LUMINA.Pass.GetCameraDepthTexture, tmpBuffer1A));
                           ExecuteBlitPassNOTEX(data, context, 0));
                    }
                    //Debug.Log(outlineMaterial.name);
                    //Debug.Log(m_BlitMaterial.name);
                    //BLIT FINAL
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve", out var passData, m_ProfilingSampler))
                    {
                        passData.BlitMaterial = m_BlitMaterial;
                        // Similar to the previous pass, however now we set destination texture as input and source as output.
                        builder.UseTexture(tmpBuffer2A, AccessFlags.Read);
                        passData.src = tmpBuffer2A;
                        builder.SetRenderAttachment(sourceTexture, 0, AccessFlags.Write);
                        // We use the same BlitTexture API to perform the Blit operation.
                        builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                    }


                }
            }
            static void ExecuteBlitPassTEX9NAME(PassData data, RasterGraphContext context, int pass,
         string texname1, TextureHandle tmpBuffer1,
         string texname2, TextureHandle tmpBuffer2,
         string texname3, TextureHandle tmpBuffer3,
         string texname4, TextureHandle tmpBuffer4,
         string texname5, TextureHandle tmpBuffer5,
         string texname6, TextureHandle tmpBuffer6,
         string texname7, TextureHandle tmpBuffer7,
         string texname8, TextureHandle tmpBuffer8,
         string texname9, TextureHandle tmpBuffer9,
         string texname10, TextureHandle tmpBuffer10
         )
            {
                data.BlitMaterial.SetTexture(texname1, tmpBuffer1);
                data.BlitMaterial.SetTexture(texname2, tmpBuffer2);
                data.BlitMaterial.SetTexture(texname3, tmpBuffer3);
                data.BlitMaterial.SetTexture(texname4, tmpBuffer4);
                data.BlitMaterial.SetTexture(texname5, tmpBuffer5);
                data.BlitMaterial.SetTexture(texname6, tmpBuffer6);
                data.BlitMaterial.SetTexture(texname7, tmpBuffer7);
                data.BlitMaterial.SetTexture(texname8, tmpBuffer8);
                data.BlitMaterial.SetTexture(texname9, tmpBuffer9);
                data.BlitMaterial.SetTexture(texname10, tmpBuffer10);
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
            }
            //temporal
            static void ExecuteBlitPassTEN(PassData data, RasterGraphContext context, int pass,
                TextureHandle tmpBuffer1, TextureHandle tmpBuffer2, TextureHandle tmpBuffer3,
                string varname1, float var1,
                string varname2, float var2,
                string varname3, Matrix4x4 var3,
                string varname4, Matrix4x4 var4,
                string varname5, Matrix4x4 var5,
                string varname6, Matrix4x4 var6,
                string varname7, Matrix4x4 var7
                )
            {
                data.BlitMaterial.SetTexture("_CloudTex", tmpBuffer1);
                data.BlitMaterial.SetTexture("_PreviousColor", tmpBuffer2);
                data.BlitMaterial.SetTexture("_PreviousDepth", tmpBuffer3);
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
                //lastFrameViewProjectionMatrix = viewProjectionMatrix;
                //lastFrameInverseViewProjectionMatrix = viewProjectionMatrix.inverse;
            }

            static void ExecuteBlitPassTHREE(PassData data, RasterGraphContext context, int pass,
                TextureHandle tmpBuffer1, TextureHandle tmpBuffer2, TextureHandle tmpBuffer3)
            {
                data.BlitMaterial.SetTexture("_ColorBuffer", tmpBuffer1);
                data.BlitMaterial.SetTexture("_PreviousColor", tmpBuffer2);
                data.BlitMaterial.SetTexture("_PreviousDepth", tmpBuffer3);
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
            }
            static void ExecuteBlitPass(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1aa)
            {
                data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1aa);
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
            }
            static void ExecuteBlitPassNOTEX(PassData data, RasterGraphContext context, int pass)
            {
                // data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1aa);
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.outlineMaterial, pass);
            }
            static void ExecuteBlitPassTWO(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1, TextureHandle tmpBuffer2)
            {
                data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1);// _CloudTexP", tmpBuffer1);
                data.BlitMaterial.SetTexture("_CameraDepthCustom", tmpBuffer2);
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
            }
            static void ExecuteBlitPassTWO_MATRIX(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1, TextureHandle tmpBuffer2, Matrix4x4 matrix)
            {
                data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1);// _CloudTexP", tmpBuffer1);
                data.BlitMaterial.SetTexture("_CameraDepthCustom", tmpBuffer2);
                data.BlitMaterial.SetMatrix("frustumCorners", matrix);
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
            }
            static void ExecuteBlitPassTEXNAME(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1aa, string texname)
            {
                data.BlitMaterial.SetTexture(texname, tmpBuffer1aa);
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
            }
            static void ExecuteBlitPassTEX5NAME(PassData data, RasterGraphContext context, int pass,
                string texname1, TextureHandle tmpBuffer1,
                string texname2, TextureHandle tmpBuffer2,
                string texname3, TextureHandle tmpBuffer3,
                string texname4, TextureHandle tmpBuffer4,
                string texname5, TextureHandle tmpBuffer5
                )
            {
                data.BlitMaterial.SetTexture(texname1, tmpBuffer1);
                data.BlitMaterial.SetTexture(texname2, tmpBuffer2);
                data.BlitMaterial.SetTexture(texname3, tmpBuffer3);
                data.BlitMaterial.SetTexture(texname4, tmpBuffer4);
                data.BlitMaterial.SetTexture(texname5, tmpBuffer5);
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
            }
            // It is static to avoid using member variables which could cause unintended behaviour.
            static void ExecutePass(PassData data, RasterGraphContext rgContext)
            {
                Blitter.BlitTexture(rgContext.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, 0);
            }
            //private Material m_BlitMaterial;
            private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("After Opaques");
            ////// END GRAPH
#endif
*/


            /*
#if UNITY_2023_3_OR_NEWER
            // This class stores the data needed by the pass, passed as parameter to the delegate function that executes the pass
            private class PassData
            {
                internal TextureHandle src;
                internal TextureHandle dst;
                internal Material blitMaterial;
            }

            // This static method is used to execute the pass and passed as the RenderFunc delegate to the RenderGraph render pass
            static void ExecutePass(PassData data, RasterGraphContext context)
            {
                data.blitMaterial.SetMatrix("_CamToWorld", Camera.main.cameraToWorldMatrix);
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.blitMaterial, 0);
            }

            private void InitPassData(RenderGraph renderGraph, ContextContainer frameData, ref PassData passData)
            {
                // Fill up the passData with the data needed by the passes

                // UniversalResourceData contains all the texture handles used by the renderer, including the active color and depth textures
                // The active color and depth textures are the main color and depth buffers that the camera renders into
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                // The destination texture is created here,
                // the texture is created with the same dimensions as the active color texture, but with no depth buffer, being a copy of the color texture
                // we also disable MSAA as we don't need multisampled textures for this sample

                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;

                TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "BlitMaterialTexture", false);

                passData.src = resourceData.activeColorTexture;
                passData.dst = destination;
                passData.blitMaterial = outlineMaterial;
            }

            // This is where the renderGraph handle can be accessed.
            // Each ScriptableRenderPass can use the RenderGraph handle to add multiple render passes to the render graph
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                string passName = "Blit With Material";

                // This simple pass copies the active color texture to a new texture using a custom material. This sample is for API demonstrative purposes,
                // so the new texture is not used anywhere else in the frame, you can use the frame debugger to verify its contents.

                // add a raster render pass to the render graph, specifying the name and the data type that will be passed to the ExecutePass function
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                {
                    // Initialize the pass data
                    InitPassData(renderGraph, frameData, ref passData);

                    // We declare the src texture as an input dependency to this pass, via UseTexture()
                    builder.UseTexture(passData.src);

                    // Setup as a render target via UseTextureFragment, which is the equivalent of using the old cmd.SetRenderTarget
                    builder.SetRenderAttachment(passData.dst, 0);

                    // We disable culling for this pass for the demonstrative purpose of this sampe, as normally this pass would be culled,
                    // since the destination texture is not used anywhere else
                    builder.AllowPassCulling(false);
                    builder.AllowGlobalStateModification(true);

                    // Assign the ExecutePass function to the render pass delegate, which will be called by the render graph when executing the pass
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                }
            }
#endif
            */

            
#if UNITY_2023_3_OR_NEWER
            /// <summary>
            /// ///////////////////////// RENDER GRAPH
            /// </summary>
            public class PassData
            {
                public RenderingData renderingData;
                public UniversalCameraData cameraData;
                public TextureHandle colorTargetHandle;



                public TextureHandle destinationA;

                public Shader _blitShader;
                public Material _blit;
                public TextureHandle cameraDepthTargetHandle;
                public ContextContainer frameDataA;
                //internal TextureHandle copySourceTexture;
                public void Init(ContextContainer frameData, IUnsafeRenderGraphBuilder builder = null)               
                {
                    //_blit = outlineMaterial;

                    cameraData = frameData.Get<UniversalCameraData>();
                    UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                    frameDataA = frameData;

                    if (builder == null)
                    {
                        // colorTargetHandle = cameraData.renderer.cameraColorTargetHandle;
                    }
                    else
                    {
                        colorTargetHandle = resources.activeColorTexture;
                        builder.UseTexture(colorTargetHandle, AccessFlags.ReadWrite);
                        cameraDepthTargetHandle = resources.activeDepthTexture;
                        builder.UseTexture(cameraDepthTargetHandle, AccessFlags.ReadWrite);
                    }
                }
            }            
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                string passName = "Screen Rain Pass";
                using (var builder = renderGraph.AddUnsafePass<PassData>(passName,
                    out var data))
                {
                    data.Init(frameData, builder);

                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                   // data.destination = data.colorTargetHandle;
                    //builder.UseTextureFragment(passData.dst, 0);
                    //builder.UseTexture(data.destination, AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    builder.AllowGlobalStateModification(true);

                   // data.destinationA = resourceData.activeColorTexture;
                    //builder.UseTexture(data.destinationA, AccessFlags.Write);

                    data._blit = outlineMaterial;

                    builder.SetRenderFunc<PassData>((data, ctx) =>
                    {
                        var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                        var renderContext = GetRenderContextB(ctx);
                        OnCameraSetupA(cmd, data);
                        ExecutePass(renderContext, cmd, data);
                    });
                }
            }
            static FieldInfo AR_renderContext = typeof(InternalRenderGraphContext).GetField("renderContext", BindingFlags.NonPublic | BindingFlags.Instance);
            static FieldInfo AR_InternalRenderGraphContext = typeof(UnsafeGraphContext).GetField("wrappedContext", BindingFlags.NonPublic | BindingFlags.Instance);
            static InternalRenderGraphContext GetInternalRenderGraphContextB(UnsafeGraphContext unsafeContext)
            {
                return (InternalRenderGraphContext)AR_InternalRenderGraphContext.GetValue(unsafeContext);
            }
            public static ScriptableRenderContext GetRenderContextB(UnsafeGraphContext unsafeContext)
            {
                return (ScriptableRenderContext)AR_renderContext.GetValue(GetInternalRenderGraphContextB(unsafeContext));
            }
            public void OnCameraSetupA(CommandBuffer cmd, PassData dataA)
            {
                //var renderer = dataA.renderingData.cameraData.renderer;
                //destination = rend dataA.renderingDataeringData.cameraData.renderer.cameraColorTargetHandle; //UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget //v0.1                          
               // destination = dataA.colorTargetHandle;
               // source = dataA.colorTargetHandle;//.renderingData.cameraData.renderer.cameraColorTargetHandle;
            }
            public void ExecutePass(ScriptableRenderContext context, CommandBuffer command, PassData dataA)//(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = command;// CommandBufferPool.Get("Screen Rain Pass");

                RenderTextureDescriptor opaqueDescriptor = dataA.cameraData.cameraTargetDescriptor;
                opaqueDescriptor.depthBufferBits = 0;

                if (Camera.main != null && dataA.cameraData.camera == Camera.main
                    && Camera.main.GetComponent<ScreenSpaceRainLITE_SM_URP>() != null)
                {
                    dataA._blit.SetMatrix("_CamToWorld", Camera.main.cameraToWorldMatrix);

                    CommandBuffer unsafeCommandBuffer = command;// CommandBufferHelpers.GetNativeCommandBuffer(command);

                    // Add a command to set the render target to the active color buffer so URP draws to it
                            command.SetRenderTarget(dataA.colorTargetHandle);

                     cmd.Blit(dataA.colorTargetHandle, dataA.colorTargetHandle, dataA._blit, 0);///
                    //context.ExecuteCommandBuffer(cmd);
                    //CommandBufferPool.Release(cmd);

                    //command.SetRenderTarget(dataA.destinationA);

                    // Add a command to copy the camera normals texture to the render target
                    //Blitter.BlitTexture(unsafeCommandBuffer, dataA.colorTargetHandle, new Vector4(1, 1, 0, 0), 0, false);
                }
            }
#endif
            








            private RenderTargetIdentifier source { get; set; }

#if UNITY_2022_1_OR_NEWER
            private RTHandle destination { get; set; } //v0.1
#else
            private RenderTargetHandle destination { get; set; } //v0.1
#endif

            public Material outlineMaterial = null;
            public Material outlineMaterialNEW = null;

#if UNITY_2022_1_OR_NEWER
            public void Setup(RenderTargetIdentifier source, RTHandle destination)//v0.1
            {
                this.source = source;
                this.destination = destination;
                //temporaryColorTexture = RTHandles.Alloc("temporaryColorTexture", name: "temporaryColorTexture"); //v0.1
            }
#else
            RenderTargetHandle temporaryColorTexture; //v0.1
            public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination)//v0.1
            {
                this.source = source;
                this.destination = destination;
            }
#endif

            public WeatherEffectsSkyMasterPass(Material outlineMaterial, Material outlineMaterialNEW)
            {
                this.outlineMaterial = outlineMaterial;
                this.outlineMaterialNEW = outlineMaterialNEW;
                //this.blitMaterial = blitMaterial;
            }

            //v1.5
#if UNITY_2020_2_OR_NEWER
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                // get a copy of the current camera’s RenderTextureDescriptor
                // this descriptor contains all the information you need to create a new texture
                //RenderTextureDescriptor cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;

                // _handle = RTHandles.Alloc(settings.textureId, name: settings.textureId); //v0.1

                var renderer = renderingData.cameraData.renderer;
#if UNITY_2022_1_OR_NEWER
                destination = renderingData.cameraData.renderer.cameraColorTargetHandle; //UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget //v0.1                          
                source = renderingData.cameraData.renderer.cameraColorTargetHandle; 
#else
                destination = UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget; //v0.1                          
                source = renderingData.cameraData.renderer.cameraColorTarget;
#endif

            }
#endif

            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in an performance manner.
            //public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            //{

            // }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Outline Pass");

                RenderTextureDescriptor opaqueDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDescriptor.depthBufferBits = 0;

                //v1.6
                if (Camera.main != null && renderingData.cameraData.camera == Camera.main
                    && Camera.main.GetComponent<ScreenSpaceRainLITE_SM_URP>() != null)
                {
                    //if (destination == renderingData.cameraData.renderer.cameraColorTargetHandle)//RenderTargetHandle.CameraTarget) //v0.1
                    //{

                    //temporaryColorTexture = RTHandles.Alloc("temporaryColorTexture", name: "temporaryColorTexture"); //v0.1

                    //cmd.GetTemporaryRT(Shader.PropertyToID(temporaryColorTexture.name), opaqueDescriptor, FilterMode.Point); //v0.1
                    //cmd.Blit( source, temporaryColorTexture, outlineMaterial, 0); //v0.1
                    //cmd.Blit( temporaryColorTexture, destination); //v0.1
                   
                    outlineMaterial.SetMatrix("_CamToWorld", Camera.main.cameraToWorldMatrix);

#if UNITY_2022_1_OR_NEWER
                    cmd.Blit(source, destination, outlineMaterial, 0); //v0.1
#else
                    cmd.GetTemporaryRT(temporaryColorTexture.id, opaqueDescriptor, FilterMode.Bilinear);// FilterMode.Point);
                    ////UnityEngine.RenderTexture.active = source;
                    ////GL.ClearWithSkybox(true, Camera.main);
                    Blit(cmd, source, temporaryColorTexture.Identifier(), outlineMaterial, 0);
                    Blit(cmd, temporaryColorTexture.Identifier(), source);

                    //Blit(cmd, source, source, outlineMaterial, 0);
#endif

                    //}
                    //else cmd.Blit( source, destination, outlineMaterial, 0); //v0.1

                    //cmd.ReleaseTemporaryRT(Shader.PropertyToID(temporaryColorTexture.name));//v0.1
                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }


            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd)
            {
#if UNITY_2022_1_OR_NEWER
                
#else
                if (destination == RenderTargetHandle.CameraTarget)
                { //v0.1
                    cmd.ReleaseTemporaryRT(temporaryColorTexture.id);
                }
#endif
            }
        }

        [System.Serializable]
        public class OutlineSettings
        {
            public Material outlineMaterial = null;
            public Material outlineMaterialNEW = null;
            public Material blitMaterial = null;
        }

        public OutlineSettings settings = new OutlineSettings();
        WeatherEffectsSkyMasterPass weatherEffectsSkyMaster;

#if UNITY_2022_1_OR_NEWER
        RTHandle outlineTexture; //v0.1
#else
        RenderTargetHandle outlineTexture; //v0.1
#endif

        [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;//v1.6a RenderPassEvent.BeforeRenderingPostProcessing;// RenderPassEvent.AfterRenderingOpaques;//v1.6a
        public override void Create()
        {
            weatherEffectsSkyMaster = new WeatherEffectsSkyMasterPass(settings.outlineMaterial, settings.outlineMaterialNEW);
            weatherEffectsSkyMaster.renderPassEvent = renderPassEvent; //RenderPassEvent.AfterRenderingTransparents;//v1.6a

            //
#if UNITY_2022_1_OR_NEWER
            outlineTexture = RTHandles.Alloc("_OutlineTexture", name: "_OutlineTexture"); //v0.1
#else
            outlineTexture.Init("_OutlineTexture"); //v0.1
#endif

        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings.outlineMaterial == null)
            {
                Debug.LogWarningFormat("Missing Outline Material");
                return;
            }
            //outlinePass.Setup(renderer.cameraColorTarget, RenderTargetHandle.CameraTarget);//v1.5
            renderer.EnqueuePass(weatherEffectsSkyMaster);
        }
    }


}
