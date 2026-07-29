using System.Collections.Generic;

public class BoolLock
{
    readonly List<object> lockInstigators = new();
    public bool IsLocked => GetInsitgatorCount() > 0;
    public bool IsUnlocked => GetInsitgatorCount() == 0;
    public int LockCount => GetInsitgatorCount();

    public void AddInstigator(object obj)
    {
        RefreshLockInstigators();
        
        if (lockInstigators.Contains(obj)) return;

        lockInstigators.Add(obj);
    }

    public void RemoveInstigator(object obj)
    {
        RefreshLockInstigators();
        
        if (!lockInstigators.Contains(obj)) return;

        lockInstigators.Remove(obj);
    }

    int GetInsitgatorCount()
    {
        RefreshLockInstigators();
        return lockInstigators.Count;
    }
    
    void RefreshLockInstigators()
    {
        if(lockInstigators.Count == 0) return;

        lockInstigators.RemoveAll(IsMissingOrNull);
    }
    
    bool IsMissingOrNull(object instigator)
    {
        if (instigator == null) return true;
        return instigator is UnityEngine.Object unityObj && unityObj == null; //Checks for Missing Reference Exceptions
    }
}