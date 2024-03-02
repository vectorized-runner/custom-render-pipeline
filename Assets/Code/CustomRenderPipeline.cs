using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomSRP
{
	public class CustomRenderPipeline : RenderPipeline
	{
		// This is the old API. Allocates Memory every frame for Camera[], so we won't use it.
		protected override void Render(ScriptableRenderContext context, Camera[] cameras)
		{
			throw new System.NotImplementedException();
		}

		protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
		{
			foreach (var camera in cameras)
			{
				if (!camera.TryGetCullingParameters(out var cullingParameters))
					continue;

				var commandBuffer = new CommandBuffer
				{
					name = "Custom Render Buffer"
				};

				context.SetupCameraProperties(camera);
				// Clear what's drawn on the RenderTarget from the previous frame
				commandBuffer.ClearRenderTarget(true, true, Color.clear);
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();

				commandBuffer.BeginSample("Custom Render");
				{
					var cullingResults = context.Cull(ref cullingParameters);
					// Currently we only Support unlit material
					var shaderTagId = new ShaderTagId("SRPDefaultUnlit");

					// Draw Opaque Objects
					{
						var sortingSettings = new SortingSettings(camera)
						{
							criteria = SortingCriteria.CommonOpaque
						};
						var drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
						var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
						context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
					}

					context.DrawSkybox(camera);

					// Draw Transparent objects after Skybox, as Transparent objects don't write to the Depth Buffer, Skybox overwrites them
					{
						var sortingSettings = new SortingSettings(camera)
						{
							criteria = SortingCriteria.CommonTransparent
						};
						var drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
						var filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
						context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
					}

					// Draw Unsupported Shaders (Unsupported by our pipeline)
					{
						var legacyShaderTagIds = new[]
						{
							new ShaderTagId("Always"),
							new ShaderTagId("ForwardBase"),
							new ShaderTagId("PrepassBase"),
							new ShaderTagId("Vertex"),
							new ShaderTagId("VertexLMRGBM"),
							new ShaderTagId("VertexLM")
						};

						var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera));

						for (int i = 1; i < legacyShaderTagIds.Length; i++)
						{
							drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
						}

						var filteringSettings = FilteringSettings.defaultValue;
						context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
					}
				}
				commandBuffer.EndSample("Custom Render");
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();

				context.Submit();
			}
		}
	}
}