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

		public bool Clipping
		{
			set => SetProperty("_Clipping", "_CLIPPING", value);
		}

		private bool PremultiplyAlpha
		{
			set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
		}

		public BlendMode SrcBlend
		{
			set => SetProperty("_SrcBlend", (float)value);
		}

		public BlendMode DstBlend
		{
			set => SetProperty("_DstBlend", (float)value);
		}

		public bool ZWrite
		{
			set => SetProperty("_ZWrite", value ? 1f : 0f);
		}

		public RenderQueue RenderQueue
		{
			set
			{
				foreach (var material in _materials)
				{
					material.renderQueue = (int)value;
				}
			}
		}

		private bool HasPremultiplyAlpha => TryGetProperty("_PremulAlpha", out _);

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			base.OnGUI(materialEditor, properties);
			_properties = properties;
			_editor = materialEditor;
			_materials = materialEditor.targets.Select(o => o as Material).ToArray();

			OpaquePreset();
			ClipPreset();
			FadePreset();
			TransparentPreset();
		}

		private bool TryGetProperty(string name, out MaterialProperty property)
		{
			property = FindProperty(name, _properties, false);
			return property != null;
		}

		private bool SetProperty(string name, float value)
		{
			if (TryGetProperty(name, out var prop))
			{
				prop.floatValue = value;
				return true;
			}

			Debug.LogError($"The property '{name}' doesn't exist.");
			return false;
		}

		private void SetProperty(string name, string keyword, bool value)
		{
			if (SetProperty(name, value ? 1f : 0f))
			{
				SetKeyword(keyword, value);
			}
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

		private bool PresetButton(string name)
		{
			if (GUILayout.Button(name))
			{
				_editor.RegisterPropertyChangeUndo(name);
				return true;
			}

			return false;
		}

		private void OpaquePreset()
		{
			if (PresetButton("Opaque"))
			{
				Clipping = false;
				PremultiplyAlpha = false;
				SrcBlend = BlendMode.One;
				DstBlend = BlendMode.Zero;
				ZWrite = true;
				RenderQueue = RenderQueue.Geometry;
			}
		}

		private void ClipPreset()
		{
			if (PresetButton("Clip"))
			{
				Clipping = true;
				PremultiplyAlpha = false;
				SrcBlend = BlendMode.One;
				DstBlend = BlendMode.Zero;
				ZWrite = true;
				RenderQueue = RenderQueue.AlphaTest;
			}
		}

		private void FadePreset()
		{
			if (PresetButton("Fade"))
			{
				Clipping = false;
				PremultiplyAlpha = false;
				SrcBlend = BlendMode.SrcAlpha;
				DstBlend = BlendMode.OneMinusSrcAlpha;
				ZWrite = false;
				RenderQueue = RenderQueue.Transparent;
			}
		}

		private void TransparentPreset()
		{
			if (HasPremultiplyAlpha && PresetButton("Transparent"))
			{
				Clipping = false;
				PremultiplyAlpha = true;
				SrcBlend = BlendMode.One;
				DstBlend = BlendMode.OneMinusSrcAlpha;
				ZWrite = false;
				RenderQueue = RenderQueue.Transparent;
			}
		}
	}
}