using System;
using UnityEngine.Serialization;

namespace CustomSRP
{
	[Serializable]
	public struct SRPSettings
	{
		public bool UseGPUInstancing;
		public bool UseSRPBatcher;
		public bool UseDynamicBatching;

		public ShadowSettings ShadowSettings;
	}

	[Serializable]
	public struct ShadowSettings
	{
		public float MaxDistance;
	}

	[Serializable]
	public struct Directional
	{
		public TextureSize AtlasSize;
	}

	public enum TextureSize
	{
		_256 = 256,
		_512 = 512,
		_1024 = 1024,
		_2048 = 2048,
		_4096 = 4096,
		_8192 = 8192
	}
}