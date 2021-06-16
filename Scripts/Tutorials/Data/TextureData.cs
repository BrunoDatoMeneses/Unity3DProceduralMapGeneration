using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class TextureData : UpdatableData
{
	public Layer[] layers;
	

	float savedMinHeight;
	float savedMaxHeight;

	public void ApplyToMaterial(Material material)
	{

		material.SetInt("layerCount", layers.Length);
		material.SetColorArray("baseColours", baseColours); //TODO
		material.SetFloatArray("baseStartHeights", baseStartHeights);
		material.SetFloatArray("baseBlends", baseBlends);

		UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
	}

	public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
	{
		savedMinHeight = minHeight;
		savedMaxHeight = maxHeight;


		material.SetFloat("minHeight", minHeight);
		material.SetFloat("maxHeight", maxHeight);
	}


	[System.Serializable]
	public class Layer
	{
		public Texture2D texture;
		public Color tint;
		[Range(0, 1)]
		public float tintStrength;
		[Range(0, 1)]
		public float startHeight;
		[Range(0, 1)]
		public float blendStrength;
		public float textureScale;
	}
}