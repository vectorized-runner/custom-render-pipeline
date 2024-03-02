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
					name = "Render Camera"
				};

				context.SetupCameraProperties(camera);
				commandBuffer.ClearRenderTarget(true, true, Color.clear);
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
				
				commandBuffer.BeginSample("Custom Render");
				{
					var drawingSettings = new DrawingSettings();
					var filteringSettings = new FilteringSettings();
					var cullingResults = context.Cull(ref cullingParameters);
					context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
					
					context.DrawSkybox(camera);
				}
				commandBuffer.EndSample("Custom Render");
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
				
				context.Submit();
			}
		}
	}
}