
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using System.Windows.Forms;
using System.Security.Policy;

namespace CpuScheduler;

public static class CPUAlgorithms
{
    #region FCFS
    /// <summary>
    /// STUDENTS: Example FCFS algorithm implementation using DataGrid data
    /// This replaces the old prompt-based system with direct data access
    /// </summary>
    public static List<SchedulingResult> RunFCFSAlgorithm(List<ProcessData> processes)
    {
        string prevProcess = null;
        var results = new List<SchedulingResult>();
        var overview = new SchedulingOverview();
        results.Add(overview);
        overview.StartTime = -1;
        var currentTime = 0;
        int prevTime = 0;

        HashSet<string> processesSeenInInterval = new HashSet<string>();
        int intervalSize = (int)Math.Sqrt(processes.Sum(p => p.BurstTime));
        overview.NumTimesSwitched = intervalSize;
        int unitsPassedSinceInterval = 0;
        int numIntervalsSeen = 0;

        // Sort by arrival time for FCFS
        var sortedProcesses = processes.OrderBy(p => p.ArrivalTime).ToList();

        foreach (var process in sortedProcesses)
        {
            var startTime = Math.Max(currentTime, process.ArrivalTime);
            var finishTime = startTime + process.BurstTime;
            var waitingTime = startTime - process.ArrivalTime;
            var turnaroundTime = finishTime - process.ArrivalTime;

            if (prevProcess != null && prevProcess != process.ProcessID)
            {
                results.Last().NumTimesSwitched++;
                processesSeenInInterval.Add(prevProcess);
                unitsPassedSinceInterval += currentTime - prevTime;
                if (unitsPassedSinceInterval >= intervalSize)
                {
                    numIntervalsSeen++;
                    overview.AverageProcessesServedPerUnitTime =
                        ((overview.AverageProcessesServedPerUnitTime * (numIntervalsSeen - 1)) +
                        ((double)processesSeenInInterval.Count / intervalSize)) / numIntervalsSeen;
                    processesSeenInInterval.Clear();
                    unitsPassedSinceInterval = unitsPassedSinceInterval - intervalSize;
                }
            }
            prevTime = currentTime;
            prevProcess = process.ProcessID;

            results.Add(new SchedulingResult
            {
                ProcessID = process.ProcessID,
                ArrivalTime = process.ArrivalTime,
                BurstTime = process.BurstTime,
                StartTime = startTime,
                FinishTime = finishTime,
                WaitingTime = waitingTime,
                TurnaroundTime = turnaroundTime
            });

            currentTime = finishTime;
        }

        return results;
    }
    #endregion

    #region SJF
    /// <summary>
    /// STUDENTS: SJF algorithm implementation using DataGrid data
    /// Shortest Job First - selects process with minimum burst time
    /// </summary>
    public static List<SchedulingResult> RunSJFAlgorithm(List<ProcessData> processes)
    {
        string prevProcess = null;

        var results = new List<SchedulingResult>();
        var overview = new SchedulingOverview();
        results.Add(overview);
        overview.StartTime = -1;
        var currentTime = 0;
        int prevTime = 0;
        var remainingProcesses = processes.ToList();

        HashSet<string> processesSeenInInterval = new HashSet<string>();
        int intervalSize = (int)Math.Sqrt(processes.Sum(p => p.BurstTime));
        overview.NumTimesSwitched = intervalSize;
        int unitsPassedSinceInterval = 0;
        int numIntervalsSeen = 0;

        while (remainingProcesses.Count > 0)
        {
            // Get processes that have arrived by current time
            var availableProcesses = remainingProcesses.Where(p => p.ArrivalTime <= currentTime).ToList();

            if (availableProcesses.Count == 0)
            {
                // No process has arrived yet, jump to next arrival time
                currentTime = remainingProcesses.Min(p => p.ArrivalTime);
                continue;
            }

            // Select process with shortest burst time
            var nextProcess = availableProcesses.OrderBy(p => p.BurstTime).ThenBy(p => p.ArrivalTime).First();

            var startTime = Math.Max(currentTime, nextProcess.ArrivalTime);
            var finishTime = startTime + nextProcess.BurstTime;
            var waitingTime = startTime - nextProcess.ArrivalTime;
            var turnaroundTime = finishTime - nextProcess.ArrivalTime;

            if (prevProcess != null && prevProcess != nextProcess.ProcessID)
            {
                results.Last().NumTimesSwitched++;
                processesSeenInInterval.Add(prevProcess);
                unitsPassedSinceInterval += currentTime - prevTime;
                if (unitsPassedSinceInterval >= intervalSize)
                {
                    numIntervalsSeen++;
                    overview.AverageProcessesServedPerUnitTime =
                        ((overview.AverageProcessesServedPerUnitTime * (numIntervalsSeen - 1)) +
                        ((double)processesSeenInInterval.Count / intervalSize)) / numIntervalsSeen;
                    processesSeenInInterval.Clear();
                    unitsPassedSinceInterval = unitsPassedSinceInterval - intervalSize;
                }
            }
            prevProcess = nextProcess.ProcessID;

            results.Add(new SchedulingResult
            {
                ProcessID = nextProcess.ProcessID,
                ArrivalTime = nextProcess.ArrivalTime,
                BurstTime = nextProcess.BurstTime,
                StartTime = startTime,
                FinishTime = finishTime,
                WaitingTime = waitingTime,
                TurnaroundTime = turnaroundTime
            });

            currentTime = finishTime;
            remainingProcesses.Remove(nextProcess);
        }

        return results.OrderBy(r => r.StartTime).ToList();
    }
    #endregion

    #region Priority
    /// <summary>
    /// STUDENTS: Priority algorithm implementation using DataGrid data
    /// Higher priority number = higher priority (1 is lowest, higher numbers are higher priority)
    /// </summary>
    public static List<SchedulingResult> RunPriorityAlgorithm(List<ProcessData> processes)
    {
        string prevProcess = null;

        var results = new List<SchedulingResult>();
        var overview = new SchedulingOverview();
        results.Add(overview);
        overview.StartTime = -1;
        var currentTime = 0;
        int prevTime = 0;
        var remainingProcesses = processes.ToList();

        HashSet<string> processesSeenInInterval = new HashSet<string>();
        int intervalSize = (int)Math.Sqrt(processes.Sum(p => p.BurstTime));
        overview.NumTimesSwitched = intervalSize;
        int unitsPassedSinceInterval = 0;
        int numIntervalsSeen = 0;

        while (remainingProcesses.Count > 0)
        {
            // Get processes that have arrived by current time
            var availableProcesses = remainingProcesses.Where(p => p.ArrivalTime <= currentTime).ToList();

            if (availableProcesses.Count == 0)
            {
                // No process has arrived yet, jump to next arrival time
                currentTime = remainingProcesses.Min(p => p.ArrivalTime);
                continue;
            }

            // Select process with highest priority (highest number)
            var nextProcess = availableProcesses.OrderByDescending(p => p.Priority).ThenBy(p => p.ArrivalTime).First();

            var startTime = Math.Max(currentTime, nextProcess.ArrivalTime);
            var finishTime = startTime + nextProcess.BurstTime;
            var waitingTime = startTime - nextProcess.ArrivalTime;
            var turnaroundTime = finishTime - nextProcess.ArrivalTime;

            if (prevProcess != null && prevProcess != nextProcess.ProcessID)
            {
                results.Last().NumTimesSwitched++;
                processesSeenInInterval.Add(prevProcess);
                unitsPassedSinceInterval += currentTime - prevTime;
                if (unitsPassedSinceInterval >= intervalSize)
                {
                    numIntervalsSeen++;
                    overview.AverageProcessesServedPerUnitTime =
                        ((overview.AverageProcessesServedPerUnitTime * (numIntervalsSeen - 1)) +
                        ((double)processesSeenInInterval.Count / intervalSize)) / numIntervalsSeen;
                    processesSeenInInterval.Clear();
                    unitsPassedSinceInterval = unitsPassedSinceInterval - intervalSize;
                }
            }
            prevTime = currentTime;
            prevProcess = nextProcess.ProcessID;

            results.Add(new SchedulingResult
            {
                ProcessID = nextProcess.ProcessID,
                ArrivalTime = nextProcess.ArrivalTime,
                BurstTime = nextProcess.BurstTime,
                StartTime = startTime,
                FinishTime = finishTime,
                WaitingTime = waitingTime,
                TurnaroundTime = turnaroundTime
            });

            currentTime = finishTime;
            remainingProcesses.Remove(nextProcess);
        }

        return results.OrderBy(r => r.StartTime).ToList();
    }
    #endregion

    #region RR
    /// <summary>
    /// STUDENTS: Round Robin algorithm implementation using DataGrid data
    /// Each process gets a time quantum, then cycles to next process
    /// </summary>
    public static List<SchedulingResult> RunRoundRobinAlgorithm(List<ProcessData> processes, int quantumTime = 4)
    {
        string prevProcess = null;
        var currentTime = 0;
        int prevTime = 0;
        var processQueue = new Queue<ProcessData>();
        var processResults = new Dictionary<string, SchedulingResult>();
        var remainingBurstTimes = new Dictionary<string, int>();

        var overview = new SchedulingOverview();
        processResults.Add("-1", overview); // dummy entry for overview
        overview.StartTime = -1;

        HashSet<string> processesSeenInInterval = new HashSet<string>();
        int intervalSize = (int)Math.Sqrt(processes.Sum(p => p.BurstTime));
        overview.NumTimesSwitched = intervalSize;
        int unitsPassedSinceInterval = 0;
        int numIntervalsSeen = 0;

        // Initialize remaining burst times and results
        foreach (var process in processes)
        {
            remainingBurstTimes[process.ProcessID] = process.BurstTime;
            processResults[process.ProcessID] = new SchedulingResult
            {
                ProcessID = process.ProcessID,
                ArrivalTime = process.ArrivalTime,
                BurstTime = process.BurstTime,
                StartTime = -1, // Will be set on first execution
                FinishTime = 0,
                WaitingTime = 0,
                TurnaroundTime = 0
            };
        }

        // Add processes that arrive at time 0
        foreach (var process in processes.Where(p => p.ArrivalTime <= currentTime).OrderBy(p => p.ArrivalTime))
        {
            processQueue.Enqueue(process);
        }

        var processesNotInQueue = processes.Where(p => p.ArrivalTime > currentTime).OrderBy(p => p.ArrivalTime).ToList();

        while (processQueue.Count > 0 || processesNotInQueue.Count > 0)
        {
            // Add any processes that have now arrived
            while (processesNotInQueue.Count > 0 && processesNotInQueue[0].ArrivalTime <= currentTime)
            {
                processQueue.Enqueue(processesNotInQueue[0]);
                processesNotInQueue.RemoveAt(0);
            }

            if (processQueue.Count == 0)
            {
                // No processes in queue, jump to next arrival
                currentTime = processesNotInQueue[0].ArrivalTime;
                continue;
            }

            var currentProcess = processQueue.Dequeue();
            var result = processResults[currentProcess.ProcessID];

            if (prevProcess != null && prevProcess != currentProcess.ProcessID)
            {
                processResults[prevProcess].NumTimesSwitched++;
                processesSeenInInterval.Add(prevProcess);
                unitsPassedSinceInterval += currentTime - prevTime;
                if (unitsPassedSinceInterval >= intervalSize)
                {
                    numIntervalsSeen++;
                    overview.AverageProcessesServedPerUnitTime =
                        ((overview.AverageProcessesServedPerUnitTime * (numIntervalsSeen - 1)) +
                        ((double)processesSeenInInterval.Count / intervalSize)) / numIntervalsSeen;
                    processesSeenInInterval.Clear();
                    unitsPassedSinceInterval = unitsPassedSinceInterval - intervalSize;
                }
            }
            prevTime = currentTime;
            prevProcess = currentProcess.ProcessID;

            // Set start time if this is the first execution
            if (result.StartTime == -1)
            {
                result.StartTime = currentTime;
            }

            // Execute for quantum time or remaining burst time, whichever is smaller
            var executionTime = Math.Min(quantumTime, remainingBurstTimes[currentProcess.ProcessID]);
            currentTime += executionTime;
            remainingBurstTimes[currentProcess.ProcessID] -= executionTime;

            // Add any processes that arrived during this execution
            while (processesNotInQueue.Count > 0 && processesNotInQueue[0].ArrivalTime <= currentTime)
            {
                processQueue.Enqueue(processesNotInQueue[0]);
                processesNotInQueue.RemoveAt(0);
            }

            // Check if process is completed
            if (remainingBurstTimes[currentProcess.ProcessID] == 0)
            {
                result.FinishTime = currentTime;
                result.TurnaroundTime = result.FinishTime - result.ArrivalTime;
                result.WaitingTime = result.TurnaroundTime - result.BurstTime;
            }
            else
            {
                // Process not completed, add back to queue
                processQueue.Enqueue(currentProcess);
            }
        }

        return processResults.Values.OrderBy(r => r.StartTime).ToList();
    }
    #endregion

    #region MLFQ
    // >*** Multilevel Feedback Queue Algorithm ***<
    public static List<SchedulingResult> RunMLFQAlgorithm(List<ProcessData> processes)
    {
        string prevProcess = null;
        var results = new Dictionary<string, SchedulingResult>();
        SchedulingOverview overview = new SchedulingOverview();
        results.Add("-1", overview); // dummy entry for overview
        overview.StartTime = -1;
        var remainingBurst = new Dictionary<string, int>();

        // Define 3 queues with different quantum times
        var queues = new List<Queue<ProcessData>>
    {
        new Queue<ProcessData>(), // High priority
        new Queue<ProcessData>(), // Medium
        new Queue<ProcessData>()  // Low
    };
        var quantums = new[] { 4, 16, 24 };

        int currentTime = 0;
        int prevTime = 0;
        foreach (var process in processes)
        {
            remainingBurst[process.ProcessID] = process.BurstTime;
            results[process.ProcessID] = new SchedulingResult
            {
                ProcessID = process.ProcessID,
                ArrivalTime = process.ArrivalTime,
                BurstTime = process.BurstTime,
                StartTime = -1, // Will be set on first execution
                FinishTime = 0,
                WaitingTime = 0,
                TurnaroundTime = 0
            };
        }

        HashSet<string> processesSeenInInterval = new HashSet<string>();
        int intervalSize = (int)Math.Sqrt(results.Values.Sum(r => r.BurstTime));
        overview.NumTimesSwitched = intervalSize; // Storing here for later use
        int unitsPassedSinceInterval = 0;
        int numIntervalsSeen = 0;

        // Sort by arrival time initially
        var waitingList = processes.OrderBy(p => p.ArrivalTime).ToList();

        while (queues.Any(q => q.Count > 0) || waitingList.Count > 0)
        {
            // Add newly arrived processes to the top queue
            while (waitingList.Count > 0 && waitingList[0].ArrivalTime <= currentTime)
            {
                queues[0].Enqueue(waitingList[0]);
                waitingList.RemoveAt(0);
            }

            // Find the highest non-empty queue
            int level = queues.FindIndex(q => q.Count > 0);
            if (level == -1)
            {
                // Jump to next process arrival
                prevTime = currentTime;
                currentTime = waitingList[0].ArrivalTime;
                continue;
            }

            var process = queues[level].Dequeue();
            var result = results[process.ProcessID];

            if (prevProcess != null && prevProcess != process.ProcessID)
            {
                results[prevProcess].NumTimesSwitched++;
                processesSeenInInterval.Add(prevProcess);
                unitsPassedSinceInterval += currentTime - prevTime;
                if (unitsPassedSinceInterval >= intervalSize)
                {
                    numIntervalsSeen++;
                    overview.AverageProcessesServedPerUnitTime =
                        ((overview.AverageProcessesServedPerUnitTime * (numIntervalsSeen - 1)) +
                        ((double)processesSeenInInterval.Count / intervalSize)) / numIntervalsSeen;
                    // reset for next interval
                    processesSeenInInterval.Clear();
                    unitsPassedSinceInterval = unitsPassedSinceInterval - intervalSize;
                }
            }
            prevProcess = process.ProcessID;

            // Mark first execution
            if (result.StartTime == -1)
                result.StartTime = currentTime;

            int execTime = Math.Min(quantums[level], remainingBurst[process.ProcessID]);
            currentTime += execTime;
            remainingBurst[process.ProcessID] -= execTime;

            // Add newly arrived processes during this time
            while (waitingList.Count > 0 && waitingList[0].ArrivalTime <= currentTime)
            {
                queues[0].Enqueue(waitingList[0]);
                waitingList.RemoveAt(0);
            }

            if (remainingBurst[process.ProcessID] == 0)
            {
                result.FinishTime = currentTime;
                result.TurnaroundTime = result.FinishTime - result.ArrivalTime;
                result.WaitingTime = result.TurnaroundTime - result.BurstTime;
            }
            else
            {
                // Process not finished — move to lower queue (if not lowest)
                int nextLevel = Math.Min(level + 1, queues.Count - 1);
                queues[nextLevel].Enqueue(process);
            }
        }

        return results.Values.OrderBy(r => r.StartTime).ToList();
    }
    #endregion

    #region CFS
    // >*** Completely Fair Scheduler Algorithm ***<
    public static List<SchedulingResult> RunCFSAlgorithm(List<ProcessData> processes)
    {
        string prevProcess = null;
        const double NICE_0_LOAD = 1024.0; // CFS constant used to scale vruntime increments
        const int targetLatency = 20;      // target latency for the run queue (ms)
        var results = new Dictionary<string, SchedulingResult>();
        SchedulingOverview overview = new SchedulingOverview();
        results.Add("-1", overview); // dummy entry for overview
        overview.StartTime = -1;
        var remaining = new Dictionary<string, int>();

        // Initialize
        foreach (var p in processes)
        {
            remaining[p.ProcessID] = p.BurstTime;
            results[p.ProcessID] = new SchedulingResult
            {
                ProcessID = p.ProcessID,
                ArrivalTime = p.ArrivalTime,
                BurstTime = p.BurstTime,
                StartTime = -1, // Will be set on first execution
                FinishTime = 0,
                WaitingTime = 0,
                TurnaroundTime = 0
            };
        }

        var waiting = processes.OrderBy(p => p.ArrivalTime).ToList();
        var runqueue = new RBTree();
        var vruntimeMap = new Dictionary<string, double>(); // store current vruntime per process
        double prevTime = 0;
        double currentTime = 0;

        HashSet<string> processesSeenInInterval = new HashSet<string>();
        int intervalSize = (int)Math.Sqrt(results.Values.Sum(r => r.BurstTime));
        overview.NumTimesSwitched = intervalSize; // Storing here for later use
        int unitsPassedSinceInterval = 0;
        int numIntervalsSeen = 0;

        // helper: map priority->weight
        Func<int, double> priorityToWeight = (prio) =>
        {
            // if priority is small=high priority: use (21-prio)
            var p = Math.Max(1, Math.Min(20, prio));
            return (double)(21 - p);
        };

        while (!runqueue.IsEmpty || waiting.Count > 0)
        {
            // enqueue newly arrived processes (vruntime 0 when inserted)
            while (waiting.Count > 0 && waiting[0].ArrivalTime <= (int)currentTime)
            {
                var proc = waiting[0];
                waiting.RemoveAt(0);
                vruntimeMap[proc.ProcessID] = 0.0;
                runqueue.Insert(vruntimeMap[proc.ProcessID], proc.ProcessID, proc);
            }

            if (runqueue.IsEmpty)
            {
                // jump to next arrival
                if (waiting.Count > 0)
                {
                    prevTime = currentTime;
                    currentTime = waiting[0].ArrivalTime;
                    continue;
                }
                else break;
            }

            // compute total weight of all runnable processes
            double totalWeight = 0;
            // naive walk: we can sum weights by iterating waiting & tree - instead, compute by scanning results/remaining
            foreach (var kv in remaining)
            {
                if (kv.Value > 0)
                {
                    // lookup priority from processes list
                    var p = processes.First(pr => pr.ProcessID == kv.Key);
                    totalWeight += priorityToWeight(p.Priority);
                }
            }

            // pop the process with minimum vruntime
            var popped = runqueue.PopMin();
            if (popped == null) break;
            var (procV, pid, procData) = popped.Value;

            if (prevProcess != null && prevProcess != pid)
            {
                results[prevProcess].NumTimesSwitched++;
                processesSeenInInterval.Add(prevProcess);
                unitsPassedSinceInterval += (int)(currentTime - prevTime);
                if (unitsPassedSinceInterval >= intervalSize)
                {
                    numIntervalsSeen++;
                    overview.AverageProcessesServedPerUnitTime =
                        ((overview.AverageProcessesServedPerUnitTime * (numIntervalsSeen - 1)) +
                        ((double)processesSeenInInterval.Count / intervalSize)) / numIntervalsSeen;
                    // reset for next interval
                    processesSeenInInterval.Clear();
                    unitsPassedSinceInterval = unitsPassedSinceInterval - intervalSize;
                }
            }
            prevProcess = pid;

            var result = results[pid];
            if (result.StartTime == -1)
                result.StartTime = (int)currentTime;

            var weight = priorityToWeight(procData.Priority);

            // compute timeslice: proportional to weight
            int timeslice;
            if (totalWeight <= 0)
                timeslice = 4;
            else
                timeslice = Math.Max(1, (int)Math.Floor(targetLatency * (weight / totalWeight)));

            // ensure we at least allow a small quantum
            timeslice = Math.Max(1, timeslice);

            var exec = Math.Min(timeslice, remaining[pid]);

            currentTime += exec;
            remaining[pid] -= exec;

            // increment vruntime for this process
            // delta_vruntime = exec * (NICE_0_LOAD / weight)
            // this means heavier (larger weight) processes accumulate vruntime slower, getting more CPU share.
            var deltaV = exec * (NICE_0_LOAD / weight);
            vruntimeMap[pid] = procV + deltaV;

            // insert newly arrived processes during execution
            while (waiting.Count > 0 && waiting[0].ArrivalTime <= (int)currentTime)
            {
                var proc = waiting[0];
                waiting.RemoveAt(0);
                vruntimeMap[proc.ProcessID] = 0.0;
                runqueue.Insert(vruntimeMap[proc.ProcessID], proc.ProcessID, proc);
            }

            if (remaining[pid] == 0)
            {
                result.FinishTime = (int)currentTime;
                result.TurnaroundTime = result.FinishTime - result.ArrivalTime;
                result.WaitingTime = result.TurnaroundTime - result.BurstTime;
                // done — do not reinsert
            }
            else
            {
                // reinsert with updated vruntime so tree ordering changes
                runqueue.Insert(vruntimeMap[pid], pid, procData);
            }
        }
        // return results ordered by StartTime (similar to your previous style)
        return results.Values.OrderBy(r => r.StartTime).ToList();
    }
    #endregion
}