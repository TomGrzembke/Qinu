using System;

[Serializable]
public class State
{
    #region Inspector

    public string id;
    public int amount;

    #endregion

    public State(string id, int amount)
    {
        this.id = id;
        this.amount = amount;
    }
}
