using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomSRP
{
	public class PerObjectMaterialProperty : MonoBehaviour
	{
		[SerializeField]
		private Color _color = Color.white;

		[SerializeField]
		private float _metallic = 0.0f;

		[SerializeField]
		private float _smoothness = 0.5f;
		
		private static MaterialPropertyBlock _mpb;
		private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
		private static readonly int _metallicId = Shader.PropertyToID("_Metallic");
		private static readonly int _smoothnessId = Shader.PropertyToID("_Smoothness");

		private void OnValidate()
		{
			if (_mpb == null)
			{
				_mpb = new MaterialPropertyBlock();
			}

			if (_color == Color.white)
			{
				_color = new Color32(
					(byte)Random.Range(0, 255),
					(byte)Random.Range(0, 255),
					(byte)Random.Range(0, 255),
					255);
			}
			
			_mpb.SetColor(_baseColorId, _color);
			_mpb.SetFloat(_metallicId, _metallic);
			_mpb.SetFloat(_smoothnessId, _smoothness);
			
			GetComponent<Renderer>().SetPropertyBlock(_mpb);
		}
	}
}