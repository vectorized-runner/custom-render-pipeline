using UnityEngine;
using UnityEngine.Rendering;

namespace CustomSRP
{
	[CreateAssetMenu(menuName = "CustomSRPAsset", fileName = "CustomSRPAsset")]
	public class CustomRenderPipelineAsset : RenderPipelineAsset
	{
		protected override RenderPipeline CreatePipeline()
		{
			return null;
		}
	}
}