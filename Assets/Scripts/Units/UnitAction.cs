using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAction {

    public bool hasFinished;
    public bool canCancel;

    protected Unit owner;
    protected List<Coroutine> coroutines = new List<Coroutine>();

    public UnitAction(Unit owner) {
        this.owner = owner;
        canCancel = true;
    }

    public void StartAction() {
        addCoroutine(owner.StartCoroutine(execute()));
    }

    public void StopAction() {
        if (!canCancel) return;
        foreach(Coroutine coroutine in coroutines) if(coroutine != null) owner.StopCoroutine(coroutine);
        onCancel();
        hasFinished = true;
    }

    private IEnumerator execute() {
        hasFinished = false;
        yield return addCoroutine(owner.StartCoroutine(actionIEnum()));
        hasFinished = true;
    }

    protected virtual IEnumerator actionIEnum() {
        yield return null;
    }

    protected virtual void onCancel() {
    }

    // Adds a coroutine to the list and returns it if needed to yield to
    protected Coroutine addCoroutine(Coroutine coroutine) {
        coroutines.Add(coroutine);
        return coroutine;
    }
}
