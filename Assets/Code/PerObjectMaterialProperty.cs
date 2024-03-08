using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomSRP
{
	public class PerObjectMaterialProperty : MonoBehaviour
	{
		[SerializeField]
		private Color _color = Color.white;

		private static MaterialPropertyBlock _mpb;
		private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");

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
			GetComponent<Renderer>().SetPropertyBlock(_mpb);
		}
	}
}