using System.Diagnostics.Contracts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dump : SingletonMonoBehaviour<Dump>
{
	[SerializeField] TextMeshProUGUI text;

	[SerializeField] bool isDump = true;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;
		var st = string.Empty;
		st += "Score : " + context.score + "\n";
		st += "Player1 Make : " + context.playerDatas[(int)Player.One].make + " Total : " + context.playerDatas[(int)Player.One].makeTotal + "\n";
		st += "Player2 Make : " + context.playerDatas[(int)Player.Two].make + " Total : " + context.playerDatas[(int)Player.Two].makeTotal + "\n";
		st += "Balance :" + context.balance + "\n";
		st += "Level : " + context.level + "\n";
		st += "MusicChange : " + context.musicChange + "\n";
		text.text = st;
	}
}
