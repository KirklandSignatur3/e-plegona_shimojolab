using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class IView : MonoBehaviour
{
    protected bool isShow = false;
    public virtual void Init()
    { 

    }
    
    public async virtual Task Show(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var ts = new TaskCompletionSource<bool>();
        //ts.SetResult(true);//on complete.
        token.Register(() => { ts.TrySetCanceled();
            //on cancel.
        });
        await ts.Task;
    }

    public async virtual Task Hide(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var ts = new TaskCompletionSource<bool>();
        //tweener = textGroup_1.DOFade(0, 0.5f).OnComplete(() => { ts.SetResult(true); });
        token.Register(() => { ts.TrySetCanceled(); });
        await ts.Task;
    }
}
