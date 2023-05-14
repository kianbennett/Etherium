using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public enum BTStatus
{
    RUNNING,
    SUCCESS,
    FAILURE
}

public abstract class BehaviourTree
{
    protected BTNode rootNode;

    public void Start(MonoBehaviour monoBehaviour)
    {
        rootNode = Build();
        monoBehaviour.StartCoroutine(executeBT());
    }

    //Execute our BT every 0.1 seconds
    private IEnumerator executeBT()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            rootNode.Execute();
        }
    }

    protected abstract BTNode Build();
}

public abstract class BTNode
{
    public Blackboard bb;

    public BTNode(Blackboard bb)
    {
        this.bb = bb;
    }

    public abstract BTStatus Execute();

    public virtual void Update() {}

    // Called when a node is abruptly aborted before it can finish with a success or failure (i.e. still running)
    public virtual void Reset() {}
}

// Base class for node that can take child nodes
public abstract class CompositeNode : BTNode
{
    protected int currentChildIndex;
    protected List<BTNode> children;

    public CompositeNode(Blackboard bb) : base(bb)
    {
        currentChildIndex = 0;
        children = new();
    }

    public void AddChild(BTNode child)
    {
        children.Add(child);
    }

    public override void Reset()
    {
        currentChildIndex = 0;

        // Propagate the reset down to each child
        foreach(BTNode child in children)
        {
            child.Reset();
        }
    }
}

// Selector executes children in order until a child succeeds
public class Selector : CompositeNode
{
    public Selector(Blackboard bb) : base(bb)
    {
    }

    public override BTStatus Execute()
    {
        while(true)
        {
            BTStatus currentChildStatus = children[currentChildIndex].Execute();

            if (currentChildStatus != BTStatus.FAILURE)
            {
                currentChildIndex = 0;
                return currentChildStatus;
            }

            currentChildIndex++;
            if (currentChildIndex >= children.Count)
            {
                currentChildIndex = 0;
                return BTStatus.FAILURE;
            }
        }
    }
}

// Execute children in order until a child fails, at which point it stops execution
public class Sequence : CompositeNode
{
    public Sequence(Blackboard bb) : base(bb)
    {
    }

    public override BTStatus Execute()
    {
        while (true)
        {
            BTStatus currentChildStatus = children[currentChildIndex].Execute();

            if(currentChildStatus == BTStatus.FAILURE)
            {
                return BTStatus.FAILURE;
            }

            currentChildIndex++;
            if (currentChildIndex >= children.Count)
            {
                currentChildIndex = 0;
                return BTStatus.SUCCESS;
            }
        }
    }
}

// Decorator nodes customise functionality of other nodes by wrapping around them
public abstract class DecoratorNode : BTNode
{
    protected BTNode wrappedNode;

    public DecoratorNode(BTNode wrappedNode, Blackboard bb) : base(bb)
    {
        this.wrappedNode = wrappedNode;
    }

    public BTNode GetWrappedNode()
    {
        return wrappedNode;
    }

    public override void Reset()
    {
        wrappedNode.Reset();
    }
}

// Inverter decorator simply inverts the result of success/failure of the wrapped node
public class InverterDecorator : DecoratorNode
{
    public InverterDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {
    }

    public override BTStatus Execute()
    {
        BTStatus status = wrappedNode.Execute();

        if (status == BTStatus.FAILURE)
        {
            status = BTStatus.SUCCESS;
        }
        else if (status == BTStatus.SUCCESS)
        {
            status = BTStatus.FAILURE;
        }

        return status;
    }
}

// Calls CheckStatus() before executing
public abstract class ConditionalDecorator : DecoratorNode
{
    public ConditionalDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {
    }

    public abstract bool CheckStatus();

    public override BTStatus Execute()
    {
        if (CheckStatus())
        {
            return wrappedNode.Execute();
        }

        return BTStatus.FAILURE;
    }

   
}

/// This node simply returns success after the allotted delay time has passed
public class DelayNode : BTNode
{
    protected float delay = 0.0f;

    private Timer regulator;
    private bool started;
    private bool delayFinished;

    public DelayNode(float delayTime, Blackboard bb) : base(bb)
    {
        this.delay = delayTime;
        regulator = new Timer(delay * 1000.0f); // in milliseconds, so multiply by 1000
        regulator.Elapsed += onTimedEvent;
        regulator.Enabled = true;
        regulator.Stop();
    }

    public override BTStatus Execute()
    {
        BTStatus status = BTStatus.RUNNING;
        
        if (!started && !delayFinished)
        {
            started = true;
            regulator.Start();
        }
        else if (delayFinished)
        {
            delayFinished = false;
            started = false;
            status = BTStatus.SUCCESS;
        }

        return status;
    }

    private void onTimedEvent(object sender, ElapsedEventArgs e)
    {
        started = false;
        delayFinished = true;
        regulator.Stop();
    }

    //Timers count down independently of the Behaviour Tree, so we need to stop them when the behaviour is aborted/reset
    public override void Reset()
    {
        regulator.Stop();
        delayFinished = false;
        started = false;
    }
}
