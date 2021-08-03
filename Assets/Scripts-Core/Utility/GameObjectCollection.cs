using System.Collections.Generic;

public class GameObjectCollection<T>
{
    public List<T> Members { get; private set; } = new List<T>();

    public void RememberObject(T go)
    {
        if (!Members.Contains(go))
            Members.Add(go);
    }
    public void ForgetObject(T go)
    {
        Members.Remove(go);
    }
}

