using System;

public interface IFreight
{
    bool WasDelivered { get; set; }

    event Action<Freight, ResourceAction> OnFreightStatusChanged;

    void Consumed();

    public Resource Kind { get; set; }
    public float Amount { get; set; }

}