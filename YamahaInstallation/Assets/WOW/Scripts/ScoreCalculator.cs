using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreCalculator : MonoBehaviour
{
	public static void AddCreateScore(Pad pad)
	{
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;
		
		context.playerDatas[(int)pad.player].make++;
		context.playerDatas[(int)pad.player].makeTotal++;
		context.balance = CalcBalance(context.playerDatas[(int)Player.One].makeBuffer, context.playerDatas[(int)Player.Two].makeBuffer);
		context.level = CalcLevel(context.balance);

        if (context.level == Level.Level1)
		{
			var deduction = config.levelDeduction;
			context.score += deduction;
			context.score = Math.Max(context.score, 0);
			context.musicChange = context.score / config.scoreMusicChange;
		}
	}

	public static int AddDeathScore()
	{
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;

		var deduction = config.deathDeduction;
		context.score += deduction;
		context.score = Math.Max(context.score, 0);
		context.musicChange = context.score / config.scoreMusicChange;

		return deduction;
	}

	public static Hit AddHitScore(Note note) //mm: adds the score to scorecalculator
	{
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;

		Hit hit = ScoreCalculator.CalcHit(note.GetGap(0.5)); // calculate if it is "good/nice/perfect"
		var score = ScoreCalculator.CalcScore(hit);
		if (hit == Hit.Perfect) context.playerDatas[(int)note.pad.player].combo++; // if perfect hit, add to player's combo
		else context.playerDatas[(int)note.pad.player].combo = 0; // set combo to 0 if its not a perfect hit

		if (context.playerDatas[(int)note.pad.player].combo >= config.comboThreshold) // if curr combo is >= 6
			score += config.bonus[(int)context.level];  // add bonus depending on the level

        context.playerDatas[(int)note.pad.player].hitCount[(int)hit]++; //mm: hit quality is tallyed in playerdata
        context.score += score;
		context.score = Math.Max(context.score, 0);
		context.musicChange = context.score / config.scoreMusicChange;

		return hit; // returns the hit calculated in line 3 of function
	}

	public static Hit AddHitScore(Hit hit, Note note) // could be for sustained hit
	{
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;

		//Hit hit = ScoreCalculator.CalcHit(note.GetGap(0.5));
		var score = ScoreCalculator.CalcScore(hit);
		if (hit == Hit.Perfect) context.playerDatas[(int)note.pad.player].combo++;
		else context.playerDatas[(int)note.pad.player].combo = 0;

		if (context.playerDatas[(int)note.pad.player].combo >= config.comboThreshold)
			score += config.bonus[(int)context.level];

        context.playerDatas[(int)note.pad.player].hitCount[(int)hit]++;
        context.score += score;
		context.score = Math.Max(context.score, 0);
		context.musicChange = context.score / config.scoreMusicChange;

		return hit;
	}


	public static int CalcScore(Hit hit)
	{
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;

		int score = 0;
		switch (hit)
		{
			case Hit.Perfect:
				score = config.score[(int)context.level, (int)Hit.Perfect];
				break;
			case Hit.Nice:
				score = config.score[(int)context.level, (int)Hit.Nice];
				break;
			case Hit.Good:
				score = config.score[(int)context.level, (int)Hit.Good];
				break;
		}
		return score;
	}

	public static Hit CalcHit(double gap)
	{
		var config = SharedConfig.Instance.config;

		if (gap < config.hitArea[(int)Hit.Perfect])
			return Hit.Perfect;
		else if (gap < config.hitArea[(int)Hit.Nice])
			return Hit.Nice;
		else if (gap < config.hitArea[(int)Hit.Good])
			return Hit.Good;
		else
			return Hit.None;
	}

	public static float CalcBalance(List<int> countP1, List<int> countP2)
	{
		var config = SharedConfig.Instance.config;
		int totalP1, totalP2;
		totalP1 = totalP2 = 0;
		foreach(var item in countP1) totalP1 += item;
		foreach(var item in countP2) totalP2 += item;
		int total = totalP1 + totalP2;
		if (total < 30) return 1f;
		return Mathf.Abs(((float)totalP1 / (float)total - 0.5f) * 2f);
	}

	public static float GetBalanceBufferAverage(int count)
    {
		var context = SharedContext.Instance.context;
        var c = Math.Min(count, context.balanceBuffer.Count);
        float balance = 0;
        for (var i = 0; i < c; ++i)
        {
            balance += context.balanceBuffer[context.balanceBuffer.Count - 1 - i];
        }
        return balance / (float)c;
    }

	public static Level CalcLevel(float balance)
	{
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;

		if (balance < config.levelThreshold[(int)Level.Level4] && context.score > 5000)
			return Level.Level4;
		else if (balance < config.levelThreshold[(int)Level.Level3])
			return Level.Level3;
		else if (balance < config.levelThreshold[(int)Level.Level2])
			return Level.Level2;
		else
			return Level.Level1;
	}
}
