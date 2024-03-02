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
				// Render the Camera
				{
					context.DrawSkybox(camera);
					context.Submit();
				}
			}
		}
	}
}