using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Level
{
	Level1 = 0,
	Level2, Level3, Level4
}

public enum Player
{
	One = 0,
	Two
}

public enum Hit
{
	None = -1,
	Good = 0,
	Nice, Perfect
}

public enum Part
{
	Beat = 0,
	Keys1, Keys2, Guitter, Voice, Synth
}

[System.Serializable]
public class Config
{
	[SerializeField] public int oscPort = 7400;
	[SerializeField] public float gameDuration = 60; // shouldnt this be 180?
	[SerializeField] public int comboThreshold = 6;
	[SerializeField] public int maxScore = 15000;
	[SerializeField] public int scoreMusicChange = 300;
	[SerializeField] public int deathDeduction = -30;
	[SerializeField] public int levelDeduction = -20;
	[SerializeField] public double noteLife = 0.75d;
	[SerializeField] public double[] hitArea = new double[3] { 0.01, 0.005, 0.0025 }; //good perfect nice, with the "gap" values that define each type of accuracy (good has largest gap, perfect has smallest gap)
	[SerializeField] public int[] hitVelocity = new int[3] { 31, 63, 127 };
	[SerializeField] public float[] levelThreshold = new float[4] { 1f, 0.7f, 0.4f, 0.1f };
	[SerializeField] public int[,] score = new int[4, 3] { { 0, 0, 0 }, { 0, 10, 35 }, { 10, 35, 45 }, { 10, 45, 55 } };
	[SerializeField] public int[] bonus = new int[4] { 0, 5, 10, 10 };
	[SerializeField] public int[] numSpark = new int[4] { 3, 5, 10, 10 };
	[SerializeField] public int maxCenterBall = 128;
	[SerializeField] public int minCenterBall = 32;
	[SerializeField] public int maxBall = 30;
}

public class SharedConfig : SingletonMonoBehaviour<SharedConfig>
{
	[SerializeField] public Config config = new Config();

	public void Save()
	{
		var filePath = Application.streamingAssetsPath + "/Config.json";
		JsonSerializeIO.Save<Config>(config, filePath);
	}

	public void Load()
	{
		var filePath = Application.streamingAssetsPath + "/Config.json";
		config = JsonSerializeIO.Load<Config>(filePath);
	}
}