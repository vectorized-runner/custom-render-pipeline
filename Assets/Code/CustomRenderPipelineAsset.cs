using UnityEngine;
using UnityEngine.Rendering;

namespace CustomSRP
{
	[CreateAssetMenu(menuName = "CustomSRPAsset", fileName = "CustomSRPAsset")]
	public class CustomRenderPipelineAsset : RenderPipelineAsset
	{
		[SerializeField]
		private SRPSettings _settings;
		
		protected override RenderPipeline CreatePipeline()
		{
			return new CustomRenderPipeline(_settings);
		}
	}
}