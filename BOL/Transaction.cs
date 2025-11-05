namespace BOL;
public class Transaction
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string TransactionId { get; set; }
    public DateTime TransactionTime { get; set; }
    public int Type { get; set; }
    public string Narration { get; set; }
    public bool IsSystemGenerated { get; set; }
    public int Status { get; set; }
    public string MeterNumber { get; set; }
    public decimal Amount { get; set; }
}
