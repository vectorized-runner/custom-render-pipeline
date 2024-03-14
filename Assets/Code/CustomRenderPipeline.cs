using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomSRP
{
	public class CustomRenderPipeline : RenderPipeline
	{
		private readonly SRPSettings _settings;

		// Defined in our Shaders
		private static readonly int _dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
		private static readonly int _dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
		private static readonly int _dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
		private static int _dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

		private const int _maxDirLightCount = 4;
		private static Vector4[] _dirLightColors = new Vector4[_maxDirLightCount];
		private static Vector4[] _dirLightDirections = new Vector4[_maxDirLightCount];

		public CustomRenderPipeline(SRPSettings settings)
		{
			_settings = settings;
			GraphicsSettings.useScriptableRenderPipelineBatching = settings.UseSRPBatcher;

			// Unity doesn't convert light colors to Linear Space by default.
			GraphicsSettings.lightsUseLinearIntensity = true;
		}

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
				var clearFlags = camera.clearFlags;
				var clearDepth = clearFlags <= CameraClearFlags.Depth;
				var clearColor = clearFlags <= CameraClearFlags.Color;
				var color = clearFlags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear;
				commandBuffer.ClearRenderTarget(clearDepth, clearColor, color);

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

					// It doesn't make sense to render the shadows further away than camera can see
					var shadowDistance = math.min(_settings.ShadowSettings.MaxDistance, camera.farClipPlane);
					cullingParameters.shadowDistance = shadowDistance;
					
					var cullingResults = context.Cull(ref cullingParameters);

					// Setup Shadows
					{
						var shadowBufferName = "Custom Render - Shadows";
						var shadowBuffer = new CommandBuffer { name = shadowBufferName };
						shadowBuffer.BeginSample(shadowBufferName);
						
						const int maxShadowedDirectionalLightCount = 1;

						var shadowedVisibleLightIndices = new List<int>();
						var visibleLights = cullingResults.visibleLights;
						var visibleLightCount = visibleLights.Length;
						
						for (int lightIndex = 0; lightIndex < visibleLightCount; lightIndex++)
						{
							if (shadowedVisibleLightIndices.Count < maxShadowedDirectionalLightCount)
							{
								var light = visibleLights[lightIndex];
								if (
									light.light.shadows != LightShadows.None && 
									light.light.shadowStrength > 0.0f &&
									cullingResults.GetShadowCasterBounds(lightIndex, out var shadowCasterBounds))
								{
									shadowedVisibleLightIndices.Add(lightIndex);
								}
							}
						}

						// Render Shadows
						{
							if (shadowedVisibleLightIndices.Count > 0)
							{
								var atlasSize = (int)_settings.ShadowSettings.DirectionalAtlasSize;
								commandBuffer.GetTemporaryRT(_dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
								commandBuffer.SetRenderTarget(_dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

								commandBuffer.ClearRenderTarget(true, false, Color.clear);
								// Will this cause a problem (?)
								context.ExecuteCommandBuffer(commandBuffer);
								commandBuffer.Clear();
								
								commandBuffer.ReleaseTemporaryRT(_dirShadowAtlasId);
							}
							else
							{
								// TODO: Could get 1x1 Texture, for the reasons mentioned in the Tutorial.
							}
						}
						
						shadowBuffer.EndSample(shadowBufferName);
						
						context.ExecuteCommandBuffer(shadowBuffer);
					}
					
					// Setup Lighting
					{
						var lightBufferName = "Custom Render - Lighting";
						var lightBuffer = new CommandBuffer { name = lightBufferName };
						lightBuffer.BeginSample(lightBufferName);

						var visibleLights = cullingResults.visibleLights;
						var usedLights = 0;

						for (int i = 0; i < visibleLights.Length; i++)
						{
							var visibleLight = visibleLights[i];
							if (visibleLight.lightType != LightType.Directional)
							{
								continue;
							}
							if (usedLights >= _maxDirLightCount)
							{
								Debug.LogError("Maximum amount of Directional Lights exceeded.");
								break;
							}
							
							_dirLightColors[i] = visibleLight.finalColor;
							// Extract (-Forward) from Matrix 
							_dirLightDirections[i] = -visibleLight.localToWorldMatrix.GetColumn(2);
							usedLights++;
						}

						lightBuffer.SetGlobalInt(_dirLightCountId, visibleLights.Length);
						lightBuffer.SetGlobalVectorArray(_dirLightColorsId, _dirLightColors);
						lightBuffer.SetGlobalVectorArray(_dirLightDirectionsId, _dirLightDirections);

						lightBuffer.EndSample(lightBufferName);

						context.ExecuteCommandBuffer(lightBuffer);
					}

					// This is Unity Default
					var unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
					// We have defined this on "CustomLit.shader"
					var litShaderTagId = new ShaderTagId("CustomLit");

					// Draw Opaque Objects
					{
						var sortingSettings = new SortingSettings(camera)
						{
							criteria = SortingCriteria.CommonOpaque
						};
						var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
						{
							enableInstancing = _settings.UseGPUInstancing,
							enableDynamicBatching = _settings.UseDynamicBatching,
						};
						drawingSettings.SetShaderPassName(1, litShaderTagId);

						var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
						context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
					}

					// Draw Skybox
					context.DrawSkybox(camera);

					// Draw Transparent objects after Skybox, as Transparent objects don't write to the Depth Buffer, Skybox overwrites them
					{
						var sortingSettings = new SortingSettings(camera)
						{
							criteria = SortingCriteria.CommonTransparent
						};
						var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
						{
							enableInstancing = _settings.UseGPUInstancing,
							enableDynamicBatching = _settings.UseDynamicBatching
						};
						drawingSettings.SetShaderPassName(1, litShaderTagId);

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