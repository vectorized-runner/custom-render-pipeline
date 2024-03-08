using System;

namespace CustomSRP
{
	[Serializable]
	public struct SRPSettings
	{
		public bool UseGPUInstancing;
		public bool UseSRPBatcher;
		public bool UseDynamicBatching;
	}
}