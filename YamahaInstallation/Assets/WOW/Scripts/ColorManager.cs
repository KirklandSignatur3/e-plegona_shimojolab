using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorType
{
	Base = 0,
	Base30, Base60, Base90
}

[System.Serializable]
public class ColorSet
{
	public Texture2D baseColor;
	//public Texture2D base30;
	public Texture2D base60;
	public Texture2D base90;
}

public class ColorManager : MonoBehaviour
{
	[SerializeField] List<ColorSet> textures = new List<ColorSet>();

	public ColorSet GetTexture(int index)
	{
		return textures[index];
	}

    public Color[] GetColors(Player player)
	{
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;
		var offset = player == Player.One ? 0f : 0.5f;
		float score = (float)context.score / (float)config.maxScore * 0.5f + offset;
		Color[] colors = new Color[3]
		{
			GetTextureColorRandomX(context.ballColor, offset, score, ColorType.Base),
			GetTextureColorRandomX(context.ballColor, offset, score, ColorType.Base90),
			GetTextureColorRandomX(context.ballColor, offset, score, ColorType.Base60)
		};
		return colors;
	}

	public Color GetTextureColorRandomX(int index, float xMin, float xMax, ColorType type)
	{
		var rx = Random.Range(xMin, xMax);
		return GetTextureColorX(index, rx, type);
	}

	public Color GetTextureColorX(int index, float x, ColorType type)
	{
		Texture2D texture;

		switch(type)
		{
			default:
			case ColorType.Base:
				texture = textures[index].baseColor;
			break;
			//case ColorType.Base30:
			//	texture = textures[index].base30;
			//break;
			case ColorType.Base60:
				texture = textures[index].base60;
			break;
			case ColorType.Base90:
				texture = textures[index].base90;
			break;
		}
		
		var w = texture.width;
		var h = texture.height;
		var rx = (int)((float)w * x) + 5;
		var ry = h - 1;

		return texture.GetPixel(rx, ry);
	}

	public int GetNumTexture()
	{
		return textures.Count;
	}
}
