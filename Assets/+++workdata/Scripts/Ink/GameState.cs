using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    #region Inspector

    [SerializeField] List<State> states;

    #endregion
    public State Get(string id)
    {
        foreach (State state in states)
        {
            if (state.id == id)
            {
                return state;
            }
        }
        return null;
    }

    public void Add(string id, int amount)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }
        if (amount == 0)
        {
            return;
        }

        State state = Get(id);
        if (state == null)
        {
            State newState = new(id, amount);
            states.Add(newState);
        }
        else
        {
            state.amount += amount;
        }
    }

    public void Add(State state)
    {
        Add(state.id, state.amount);
    }

    public void Add(List<State> states)
    {
        foreach (State state in states)
        {
            Add(state);
        }
    }

    public void Subtract(State state, int amount)
    {
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i].id == state.id && states[i].amount > 0)
            {
                states[i].amount -= amount;
            }
            if (states[i].amount < 1)
            {
                states.RemoveAt(i);
            }
        }
    }
}
