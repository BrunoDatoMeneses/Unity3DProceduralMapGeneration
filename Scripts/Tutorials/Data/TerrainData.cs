using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class TerrainData : UpdatableData
{

	public enum NoiseMode { Simple, Diff, Land, Mountains, LandAndMountains, Hills, All };
	public NoiseMode noiseMode;

	public float uniformScale = 1.0f;


	public bool useFalloff;
	public float paramFalloffa = 15f;
	public float paramFalloffb = 2.2f;
	public float paramFalloffNoiseReduction = 0.1f;

	public bool useErosion;
	[Range(0, 500)]
	public int erosionIterations;

	public bool useFlatShading;

	

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public float minHeight
	{
		get
		{
			return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
		}
	}

	public float maxHeight
	{
		get
		{
			return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
		}
	}
}