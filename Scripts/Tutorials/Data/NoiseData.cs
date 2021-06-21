using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class NoiseData : UpdatableData
{

	

	public Noise.NormalizeMode normalizeMode;




	public float noiseScale;


	[Range(1, 20)]
	public int octaves;
	[Range(0, 1)]
	public float persistance;
	[Range(1, 5)]
	public float lacunarity;

	public int seed;
	public Vector2 offset;
	public float yOffset = 0.0f;
	public float reduction = 1.0f;

	public int worldScale = 500;

	protected override void OnValidate()
	{
		if (lacunarity < 1)
		{
			lacunarity = 1;
		}
		if (octaves < 0)
		{
			octaves = 0;
		}

		base.OnValidate();
	}

}