using System.Collections;
using UnityEngine;

public class UnitState
{
    protected Unit unit;
    private Coroutine stateCoroutine;

    public UnitState(Unit unit)
    {
        this.unit = unit;
    }

    public virtual void OnEnterState()
    {
        if (stateCoroutine != null) unit.StopCoroutine(stateCoroutine);
        stateCoroutine = unit.StartCoroutine(stateIEnum());
    }

    public virtual void OnUpdateState()
    {
    }

    public virtual void OnExitState()
    {
        if (stateCoroutine != null) unit.StopCoroutine(stateCoroutine);
    }

    protected virtual IEnumerator stateIEnum()
    {
        yield return null;
    }
}
