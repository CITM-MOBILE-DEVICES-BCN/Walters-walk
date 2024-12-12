using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_2023_3_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
using System.Reflection;
using System.Collections.Generic;

namespace Artngame.GLAMOR.VolFx
{
    public class DepthNormalsByLayerSMURPRainLIteFeature : ScriptableRendererFeature
    {
        class DepthNormalsByLayerSMURPRainLItePass : ScriptableRenderPass
        {

#if UNITY_2023_3_OR_NEWER
        /// <summary>
        /// ///////////////////////// RENDER GRAPH
        /// </summary>
    
        private static List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        public class PassData
        {
            public RenderingData renderingData;
            public UniversalCameraData cameraData;
            public TextureHandle colorTargetHandle;
            public CullingResults cullResults;
            public Shader _blitShader;
            public Material _blit;
            public TextureHandle cameraDepthTargetHandle;
            public ContextContainer frameDataA;
            //internal TextureHandle copySourceTexture;
            public RendererListHandle rendererListHandle;
            public void Init(RenderGraph renderGraph, ContextContainer frameData, IUnsafeRenderGraphBuilder builder = null)
            {
                // Access the relevant frame data from the Universal Render Pipeline
                UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();

                var sortFlags = cameraData.defaultOpaqueSortFlags;
                RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
                FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, m_LayerMask);

                ShaderTagId[] forwardOnlyShaderTagIds = new ShaderTagId[]
                {
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("SRPDefaultUnlit"), // Legacy shaders (do not have a gbuffer pass) are considered forward-only for backward compatibility
                new ShaderTagId("LightweightForward") // Legacy shaders (do not have a gbuffer pass) are considered forward-only for backward compatibility
                };

                m_ShaderTagIdList.Clear();

                foreach (ShaderTagId sid in forwardOnlyShaderTagIds)
                    m_ShaderTagIdList.Add(sid);

                DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, universalRenderingData, cameraData, lightData, sortFlags);

                var param = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);
                rendererListHandle = renderGraph.CreateRendererList(param);







                cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                frameDataA = frameData;

                if (builder == null)
                {
                    // colorTargetHandle = cameraData.renderer.cameraColorTargetHandle;
                }
                else
                {
                    cullResults = frameData.Get<UniversalRenderingData>().cullResults;// renderingData.cullResults;
                    //public ref CullingResults cullResults => ref frameData.Get<UniversalRenderingData>().cullResults
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
                data.Init(renderGraph, frameData, builder);

                builder.UseRendererList(data.rendererListHandle);
                //builder.UseTextureFragment(passData.dst, 0);

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
            //RenderTextureDescriptor descriptor = dataA.cameraData.cameraTargetDescriptor;
            //descriptor.depthBufferBits = 32;
            //descriptor.colorFormat = RenderTextureFormat.ARGBFloat;
            //cmd.GetTemporaryRT(Shader.PropertyToID(destination.name), descriptor, FilterMode.Trilinear);
            //ConfigureTarget(destination);//.Identifier()); //v0.1
            //ConfigureClear(ClearFlag.All, Color.black);
        }
        public DrawingSettings CreateDrawingSettingsA(ShaderTagId shaderTagIdList,
            ref RenderingData renderingData, SortingCriteria sortingCriteria, PassData dataA)
        {
            ContextContainer frameData = dataA.frameDataA; // renderingData.frameData;
            UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            return RenderingUtils.CreateDrawingSettings(shaderTagIdList, universalRenderingData, cameraData, lightData, sortingCriteria);
        }
        public void ExecutePass(ScriptableRenderContext context, CommandBuffer command, PassData dataA)//(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            CommandBuffer cmd = CommandBufferPool.Get("DepthNormals Prepass");

            //using (new ProfilingScope(cmd, "DepthNormals Prepass")) //using (new ProfilingSample(cmd, "DepthNormals Prepass"))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                //var sortFlags = dataA.cameraData.defaultOpaqueSortFlags;
                //var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
               // var drawSettings = CreateDrawingSettingsA(m_ShaderTagId, ref dataA.renderingData, sortFlags,dataA);

                //drawSettings.perObjectData = PerObjectData.None;

                //CameraData cameraData = dataA.cameraData;
                //Camera camera = dataA.cameraData.camera;

                //drawSettings.overrideMaterial = depthNormalsMaterial;

                //CommandBuffer.SetRenderTarget
                //cmd.SetRenderTarget(dataA.colorTargetHandle);
                //context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.green, 1, 0);
                cmd.DrawRendererList(dataA.rendererListHandle);
           //     context.DrawRenderers(dataA.cullResults, ref drawSettings,ref m_FilteringSettings);

                //cmd.SetGlobalTexture("_CameraDepthNormalsTexture", dataA.colorTargetHandle);
               // Debug.Log(dataA.colorTargetHandle);
                //cmd.SetGlobalTexture("_CameraDepthNormalsTexture", Shader.PropertyToID(destination.name));// destination.id); //v0.1
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

        }
#endif



            private static LayerMask m_LayerMask;


            private RTHandle destination { get; set; }// RenderTargetHandle destination { get; set; }//v0.1

            private Material depthNormalsMaterial = null;
            private FilteringSettings m_FilteringSettings;
            ShaderTagId m_ShaderTagId = new ShaderTagId("DepthOnly");

            public DepthNormalsByLayerSMURPRainLItePass(RenderQueueRange renderQueueRange, LayerMask layerMask, Material material)
            {
                m_LayerMask = layerMask;
                m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
                this.depthNormalsMaterial = material;
            }

            public void Setup(RTHandle destination)// RenderTargetHandle destination) //v0.1
            {
                this.destination = destination;
            }

            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in an performance manner.
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                RenderTextureDescriptor descriptor = cameraTextureDescriptor;
                descriptor.depthBufferBits = 32;
                descriptor.colorFormat = RenderTextureFormat.ARGBFloat;// ARGB32;

                //cmd.GetTemporaryRT(destination.id, descriptor, FilterMode.Point);
                cmd.GetTemporaryRT(Shader.PropertyToID(destination.name), descriptor, FilterMode.Trilinear); //destination.id, descriptor, FilterMode.Trilinear); //v0.1
                ConfigureTarget(destination);//.Identifier()); //v0.1
                ConfigureClear(ClearFlag.All, Color.black);
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("DepthNormals Prepass");

                //using (new ProfilingScope(cmd, "DepthNormals Prepass")) //using (new ProfilingSample(cmd, "DepthNormals Prepass"))
                {
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                    var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
                    drawSettings.perObjectData = PerObjectData.None;

                    ref CameraData cameraData = ref renderingData.cameraData;
                    Camera camera = cameraData.camera;
                    //if (cameraData.isStereoEnabled)
                    //    context.StartMultiEye(camera);


                    drawSettings.overrideMaterial = depthNormalsMaterial;


                    context.DrawRenderers(renderingData.cullResults, ref drawSettings,
                        ref m_FilteringSettings);

                    cmd.SetGlobalTexture("_CameraDepthNormalsTextureA", Shader.PropertyToID(destination.name));// destination.id); //v0.1
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd)
            {
                //if (destination != RenderTargetHandle.CameraTarget) //v0.1
                //{
                //    cmd.ReleaseTemporaryRT(destination.id);
                //    destination = RenderTargetHandle.CameraTarget;
                //}
            }
        }

        [System.Serializable]
        public class DepthNormalsSettings
        {
            // public Material outlineMaterial = null;
            public LayerMask layerMask;
        }

        public DepthNormalsSettings settings = new DepthNormalsSettings();
        DepthNormalsByLayerSMURPRainLItePass depthNormalsPass;
        RTHandle depthNormalsTexture;//RenderTargetHandle depthNormalsTexture;//v0.1
        Material depthNormalsMaterial;

        public override void Create()
        {
            depthNormalsMaterial = CoreUtils.CreateEngineMaterial("Hidden/Internal-DepthNormalsTexture");
            depthNormalsPass = new DepthNormalsByLayerSMURPRainLItePass(RenderQueueRange.opaque, settings.layerMask, depthNormalsMaterial);
            depthNormalsPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;

            //v0.1
            //depthNormalsTexture = RTHandles.Alloc("_CameraDepthNormalsTexture", name: "_CameraDepthNormalsTexture");
            depthNormalsTexture = RTHandles.Alloc("_CameraDepthNormalsTexture", name: "_CameraDepthNormalsTexture");
            //depthNormalsTexture.Init("_CameraDepthNormalsTexture");
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            depthNormalsPass.Setup(depthNormalsTexture);
            renderer.EnqueuePass(depthNormalsPass);
        }
    }
}

