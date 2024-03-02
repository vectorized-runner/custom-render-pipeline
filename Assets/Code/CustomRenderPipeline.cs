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
				var commandBuffer = new CommandBuffer
				{
					name = "Render Camera"
				};

				commandBuffer.ClearRenderTarget(true, true, Color.clear);
				commandBuffer.BeginSample("Custom Render");
				{
					context.ExecuteCommandBuffer(commandBuffer);
					commandBuffer.Clear();
					
					context.SetupCameraProperties(camera);
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