using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomSRP.Editor
{
	public class CustomShaderGUI : ShaderGUI
	{
		private MaterialEditor _editor;
		private Material[] _materials;
		private MaterialProperty[] _properties;

		bool Clipping
		{
			set => SetProperty("_Clipping", "_CLIPPING", value);
		}

		bool PremultiplyAlpha
		{
			set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
		}

		BlendMode SrcBlend
		{
			set => SetProperty("_SrcBlend", (float)value);
		}

		BlendMode DstBlend
		{
			set => SetProperty("_DstBlend", (float)value);
		}

		bool ZWrite
		{
			set => SetProperty("_ZWrite", value ? 1f : 0f);
		}

		RenderQueue RenderQueue
		{
			set
			{
				foreach (var material in _materials)
				{
					material.renderQueue = (int)value;
				}
			}
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			base.OnGUI(materialEditor, properties);
			_properties = properties;
			_editor = materialEditor;
			_materials = materialEditor.targets.Select(o => o as Material).ToArray();
		}

		private void SetProperty(string name, float value)
		{
			FindProperty(name, _properties).floatValue = value;
		}

		private void SetProperty(string name, string keyword, bool value)
		{
			SetProperty(name, value ? 1f : 0f);
			SetKeyword(keyword, value);
		}

		private void SetKeyword(string keyword, bool enabled)
		{
			if (enabled)
			{
				foreach (var material in _materials)
				{
					material.EnableKeyword(keyword);
				}
			}
			else
			{
				foreach (var material in _materials)
				{
					material.DisableKeyword(keyword);
				}
			}
		}
	}
}