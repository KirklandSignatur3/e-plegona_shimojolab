using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum Scene
{
	None = -1,
	Standby = 0,
	Game,
	Finish
}

public class SceneManager : MonoBehaviour
{
	[SerializeField] StanbyView standbyView;
	[SerializeField] GameView gameView;
	[SerializeField] FinishView finishView;

    private CancellationTokenSource cts = null;

	IView current = null;
    Scene scene = Scene.None;
    LabJackLog LJScript; // mm


    private async void Start()
	{
        standbyView.Init();
        gameView.Init();
        finishView.Init();
		try
		{
            LJScript = GameObject.FindGameObjectWithTag("Log").GetComponent<LabJackLog>();
        }
        catch (Exception e)
        {
            Debug.Log("lj not found");
        }

        await Task.Delay(1000);
		Goto(Scene.Standby);
	}

	private void Update()
	{
		if (Input.GetKey((KeyCode)49))
		{
			Goto(Scene.Standby);
		}
		if (Input.GetKey((KeyCode)50))
		{

			try
			{
                LJScript.SendLabJackSignal(LJScript.signalDelay, 7);
            }
            catch (Exception e)
            {
                Debug.Log("lj signal 8 not send");
            }

            Goto(Scene.Game);

		}
		if (Input.GetKey((KeyCode)51))
		{
			Goto(Scene.Finish);
		}
	}

	public async void Goto(Scene scene)
	{
		if(this.scene == scene) return;
		this.scene = scene;
        var prev = current;
		switch (this.scene)
		{
			case Scene.Standby:
				current = standbyView;
				break;
			case Scene.Game:
				Debug.Log("game start"); // MM
				current = gameView;
				break;
			case Scene.Finish:
				current = finishView;
				break;
		}
		cts?.Cancel();
        cts = new CancellationTokenSource();
        CancellationToken token = this.cts.Token;
        try
        {
            if (prev != null) await prev.Hide(token);
			current.Init();
            await current.Show(token);
        }
		catch (OperationCanceledException)
        {
            Debug.Log("cancel");
        }
        finally
        {
            cts?.Dispose();
            cts = null;
        }
	}
}
