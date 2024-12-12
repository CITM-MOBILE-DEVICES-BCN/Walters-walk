using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
#if UNITY_2023_3_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
using System.Reflection;

namespace Artngame.GLAMOR.VolFx
{
    public class PrePassGLAMORVolFX : ScriptableRendererFeature
    {
        public RenderPassEvent passeEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        private GenerateMaxZPassAVolFX m_GenerateMaxZPass;
        private FPVolumetricLightingPassAVolFX m_VolumetricLightingPass;
        public override void Create()
        {
            m_GenerateMaxZPass = new GenerateMaxZPassAVolFX();
            m_VolumetricLightingPass = new FPVolumetricLightingPassAVolFX(passeEvent);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Reflection)
                return;
#if UNITY_2023_3_OR_NEWER
#if UNITY_EDITOR
            if (renderingData.cameraData.cameraType == CameraType.SceneView)
            {
                if (Application.isPlaying)
                    return;
            }
            else if (renderingData.cameraData.cameraType == CameraType.Game)
            {
            }
            else
            {
                return;
            }
#endif
            m_GenerateMaxZPass.Setup();//
            renderer.EnqueuePass(m_GenerateMaxZPass);
            m_VolumetricLightingPass.Setup();
            renderer.EnqueuePass(m_VolumetricLightingPass);
#endif
        }
#if UNITY_2023_3_OR_NEWER
        protected override void Dispose(bool disposing)
        {
            m_GenerateMaxZPass.Dispose();
            m_VolumetricLightingPass.Dispose();
        }
#endif
    }
    public class FPVolumetricLightingPassAVolFX : ScriptableRenderPass
    {
        private ProfilingSampler m_ProfilingSampler;
        public FPVolumetricLightingPassAVolFX(RenderPassEvent passEvent)
        {
            renderPassEvent = passEvent;
        }
        public void Setup()
        {
            m_ProfilingSampler = new ProfilingSampler("Enable Vol FX");
        }
        public void Dispose()
        {
        }
        public class PassData
        {
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData data)
        {
        }

#if UNITY_2023_3_OR_NEWER
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            string passName = "Enable VolFX";
            using (var builder = renderGraph.AddUnsafePass<PassData>(passName,
                out var data))
            {
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderFunc<PassData>((data, ctx) =>
                {
                    var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                    var renderContext = GetRenderContextB(ctx);
                    ExecutePass(renderContext, cmd, data, ctx);
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
        void ExecutePass(ScriptableRenderContext context, CommandBuffer command, PassData data, UnsafeGraphContext ctx)//, RasterGraphContext context)
        {
            using (new ProfilingScope(command, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(command);
                command.Clear();
            }
        }
#endif
    }
    public class GenerateMaxZPassAVolFX : ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData data)
        {
        }
#if UNITY_2023_3_OR_NEWER
        public class PassData
        {
            public TextureHandle colorTargetHandle;
            public void Init(ContextContainer frameData, IUnsafeRenderGraphBuilder builder = null)
            {
                UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                colorTargetHandle = resources.activeColorTexture;
                builder.UseTexture(colorTargetHandle, AccessFlags.ReadWrite);
            }
        }
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            string passName = "MaxZPass";
            using (var builder = renderGraph.AddUnsafePass<PassData>(passName,
                out var data))
            {
                data.Init(frameData, builder);
                builder.SetRenderFunc<PassData>((data, ctx) =>
                {
                    var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                    var renderContext = GetRenderContextB(ctx);
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
        void ExecutePass(ScriptableRenderContext context, CommandBuffer command, PassData dataA)
        {
            var cmd = command;
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }
        }
        public void OnCameraSetupA(CommandBuffer cmd, PassData renderingData)
        {
        }
        private ProfilingSampler m_ProfilingSampler;
        public GenerateMaxZPassAVolFX()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }
        public void Setup()
        {
            m_ProfilingSampler = new ProfilingSampler("Generate MaxZ");
        }
        public void Dispose()
        {
        }
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
#endif
    }
}