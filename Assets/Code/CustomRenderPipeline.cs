using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
				// Reason why I've set it up like this: https://forum.unity.com/threads/profilingsample-usage-in-custom-srp.638941/
				// It needs to show up nicely on the Frame Debugger
				var sampleName = $"Custom Render Loop - {camera.name}";
				var commandBuffer = new CommandBuffer
				{
					name = sampleName,
				};

				context.SetupCameraProperties(camera);
				// Clear what's drawn on the RenderTarget from the previous frame
				commandBuffer.ClearRenderTarget(true, true, Color.clear);

				commandBuffer.BeginSample(sampleName);
				{
					context.ExecuteCommandBuffer(commandBuffer);
					commandBuffer.Clear();

#if UNITY_EDITOR
					// This may add geometry to the scene, it needs to be done before culling
					if (camera.cameraType == CameraType.SceneView)
					{
						ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
					}
#endif

					if (!camera.TryGetCullingParameters(out var cullingParameters))
					{
						// TODO: I probably need to EndSample here, but currently this branch is never taken
						continue;
					}

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

						var errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
						var sortingSettings = new SortingSettings(camera);
						var drawingSettings = new DrawingSettings(default, sortingSettings)
						{
							overrideMaterial = errorMaterial
						};

						for (int i = 0; i < legacyShaderTagIds.Length; i++)
						{
							drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
						}

						var filteringSettings = FilteringSettings.defaultValue;
						context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
					}

#if UNITY_EDITOR
					// Draw Gizmos
					{
						if (Handles.ShouldRenderGizmos())
						{
							context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
							context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
						}
					}
#endif
				}
				commandBuffer.EndSample(sampleName);
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();

				context.Submit();
			}
		}
	}
}