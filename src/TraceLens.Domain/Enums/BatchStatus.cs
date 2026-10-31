namespace TraceLens.Domain.Enums;

public enum BatchStatus
{
    Created = 0,
    InProduction = 1,
    QCPending = 2,
    QCPassed = 3,
    QCFailed = 4,
    Quarantined = 5,
    Shipped = 6,
    Cancelled = 7
}
