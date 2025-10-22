namespace UtilClasses;

public class ProcessData
{
    public string ProcessID { get; set; }
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int Priority { get; set; }
}

public class SchedulingResult
{
    public string ProcessID { get; set; }
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int StartTime { get; set; }
    public int FinishTime { get; set; }
    public int WaitingTime { get; set; }
    public int TurnaroundTime { get; set; }
}