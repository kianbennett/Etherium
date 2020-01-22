using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionHandler : MonoBehaviour {

    public readonly List<UnitAction> actions = new List<UnitAction>();

    void Update() {
        evaluateActions();
    }

    // If the first action has finished, remove it and start the next in the list
    private void evaluateActions() {
        if (actions.Count > 0) {
            bool complete = actions[0].hasFinished;
            if (complete) {
                actions.RemoveAt(0);
                if (actions.Count > 0) {
                    actions[0].StartAction();
                }
            }
        }
    }

    // Add an action to the list and start it if is the only action
    public void AddAction(UnitAction action) {
        actions.Add(action);
        if (actions.Count == 1) {
            action.StartAction();
        }
    }

    // Cancel and remove all actions that can be cancelled (returns success)
    public bool ClearActions() {
        if (actions.Count == 0) return true;
        if (IsInUnskibbableAction()) return false;
        List<UnitAction> actionsToRemove = new List<UnitAction>();
        foreach (UnitAction action in actions) {
            if (action.canCancel) {
                action.StopAction();
                actionsToRemove.Add(action);
            }
        }
        actions.RemoveAll(o => o.canCancel);
        return true;
    }

    // Returns true if the current action cannot be cancelled
    public bool IsInUnskibbableAction() {
        return actions.Count > 0 && !actions[0].canCancel;
    }
}
