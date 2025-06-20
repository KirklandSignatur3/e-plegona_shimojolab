using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;
using System;
using System.Linq;

public class AbletonManager : MonoBehaviour
{
	OscClient client;

	[SerializeField] public List<int> instruments = new List<int>();

	[SerializeField] public string[] part = new string[6] { "/beat", "/keys1", "/keys2", "/guiter", "/voice", "/synth" };
	[SerializeField]
	public List<List<Part>> levelPart = new List<List<Part>>
	{
		new List<Part>{Part.Beat},
		new List<Part>{Part.Beat, Part.Keys1},
		new List<Part>{Part.Beat, Part.Keys1, Part.Keys2},
		new List<Part>{Part.Beat, Part.Keys1, Part.Keys2, Part.Guitter},
		new List<Part>{Part.Beat, Part.Keys1, Part.Keys2, Part.Guitter,Part.Synth},
		new List<Part>{Part.Beat, Part.Keys1, Part.Keys2, Part.Guitter,Part.Synth, Part.Voice}
	};
	[SerializeField] public int longNoteOffset = 108;
	[SerializeField] public int numLongNote = 16;

	void Start()
	{
		Connect();
	}

	private void OnDestroy()
	{
		client?.Dispose();
	}

	public void Connect()
	{
		var config = SharedConfig.Instance.config;
		client = new OscClient("127.0.0.1", config.oscPort);
	}

	public void Play()
	{
		client.Send("/start");
	}

	public void Pause()
	{
		client.Send("/stop");
	}

	public void VolumeUp()
	{
		client.Send("/volumeUp");
	}

	public void VolumeDown()
	{
		client.Send("/volumeDown");
	}

	public void VolumeZero()
	{
		client.Send("/volumeZero");
	}

	public void NoteOn(int note, int velocity)
	{
		client.Send("/note", note, velocity);
	}

	public void NoteOff(int note)
	{
		client.Send("/note", note, 0);
	}

	public void RingSonic(Player player)
	{
		client.Send(player == Player.One ? "/sonic1" : "/sonic2");
	}

	public void RingBeep()
	{
		client.Send("/beep");
	}

	[ContextMenu("ChangeMusic")]
	public void ChangeMusic(int i)
	{	
		StopMusic();
		List<Part> list = levelPart[i];
		foreach (var item in list) client.Send(part[(int)item], UnityEngine.Random.Range(1, 10));

		client.Send(part[0], 10);
	}

	public void StopMusic()
	{
		foreach (var item in part) client.Send(item, 0);
	}

	public void PlayClick()
	{
		StopMusic();
		foreach (var item in part) client.Send(item, 0);
		client.Send("/click", 1);
	}

	public void StopClick()
	{
		client.Send("/click", 0);
	}

	public void SpecialUp()
	{
		client.Send("/SpecialUp");
	}

	public void SpecialDown()
	{
		client.Send("/SpecialDown");
	}

	public void SpecialZero()
	{
		client.Send("/specialZero");
	}

	public void EffectControll(int index, bool b)
	{
		client.Send("/effect" + index.ToString(), b ? 1 : 0);
	}

	/// <summary>
	/// 
	/// </summary>
	public void SetUpInstruments()
	{
		var list = Enumerable.Range(0, 11)
		.Select(i => i)
		.OrderBy(i => Guid.NewGuid());
		instruments = list.ToList();
	}

	private void OnApplicationQuit()
    {
        Pause();
    }
}
