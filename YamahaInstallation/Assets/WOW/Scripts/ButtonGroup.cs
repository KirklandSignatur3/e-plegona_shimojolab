using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonGroup : MonoBehaviour
{
	[SerializeField] public UnityEngine.Events.UnityEvent Pattern1Down = new UnityEngine.Events.UnityEvent();
	[SerializeField] public UnityEngine.Events.UnityEvent Pattern1Up = new UnityEngine.Events.UnityEvent();
	[SerializeField] public UnityEngine.Events.UnityEvent Pattern2Down = new UnityEngine.Events.UnityEvent();
	[SerializeField] public UnityEngine.Events.UnityEvent Pattern2Up = new UnityEngine.Events.UnityEvent();
	[SerializeField] public UnityEngine.Events.UnityEvent Pattern3Down = new UnityEngine.Events.UnityEvent();
	[SerializeField] public UnityEngine.Events.UnityEvent Pattern3Up = new UnityEngine.Events.UnityEvent(); 
	bool[] downs = new bool[3]{false, false, false};
	
    public void OnTapDownPad(int index)
	{
		downs[index] = true;
		Check();
	}

	public void OnTapUpPad(int index)
	{
		downs[index] = false;
		Check();
	}

	void Check()
	{
		if(downs[0] && downs[1] && !downs[2]) Pattern1Down.Invoke();
		else if(downs[0] && !downs[1] && downs[2]) Pattern2Down.Invoke();
		else if(!downs[0] && downs[1] && downs[2]) Pattern3Down.Invoke();
		else{
			Pattern1Up.Invoke();
			Pattern2Up.Invoke();
			Pattern3Up.Invoke();
		}
	}
}