using System;
using DI;

public class TestFoo : IFactoryItem
{
    public event Action OnItemFree;
    
    void IFactoryItem.FreeForce()
    {
        OnItemFree?.Invoke();
    }
}
