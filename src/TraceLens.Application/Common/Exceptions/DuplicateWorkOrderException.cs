namespace TraceLens.Application.Common.Exceptions;

public sealed class DuplicateWorkOrderException : Exception
{
    public DuplicateWorkOrderException(string orderNumber)
        : base($"Work order '{orderNumber}' already exists.")
    {
        OrderNumber = orderNumber;
    }

    public string OrderNumber { get; }
}
