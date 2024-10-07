namespace AxleLoadSystem.Store;

public interface ILiveNotificationState
{
    event Action<int> OnChangeFAQ;
    Task SetFAQCount(int count);
    int GetFAQCount();
}

public class LiveNotificationState : ILiveNotificationState
{
    public event Action<int> OnChangeFAQ;
    private int faqCount {  get; set; }
    public async Task SetFAQCount(int count)
    {
        faqCount = count;
        this.OnChangeFAQ.Invoke(count);
    }
    public int GetFAQCount()
    {
        return faqCount;
    }
}
