using System.Collections.Generic;
using System.Linq;

namespace Utilities;

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
    public int ResponseTime => StartTime - ArrivalTime;
    public int NumTimesSwitched { get; set; }
}

public class SchedulingOverview : SchedulingResult
{
    public double AverageProcessesServedPerUnitTime { get; set; }
}

public static class PerformanceMetrics
{
    public static double CalculateCPUUtilization(List<SchedulingResult> results, int totalTime)
    {
        int totalBurstTime = results.Sum(r => r.BurstTime);
        return (double)totalBurstTime / (totalTime + results.Sum(r => r.NumTimesSwitched)) * 100;
    }

    public static double CalculateThroughput(List<SchedulingResult> results, int totalTime)
    {
        return (double)results.Count / totalTime;
    }

    public static double CalculateAverageResponseTime(List<SchedulingResult> results)
    {
        return results.Average(r => r.ResponseTime);
    }
}