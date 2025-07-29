using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace JobMan;

public class WorkServerMetrics 
{
    ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();


    internal ProcessDataSample WorkDataGlobalLive { get; set; }

    public string Name { get; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WorkServerStatus Status { get; }

    public int PoolCount { get; set; }
    public int WorkerCount { get; set; }

    public ProcessDataSample WorkDataGlobal { get; set; }

    public Dictionary<string, ProcessDataSample> WorkDataPools { get; set; }


    public Dictionary<string, ProcessDataSample> WorkDataPoolsUi { get; set; }


    public WorkServerMetrics(string name)
    {
        this.Name = name;
        this.WorkDataGlobal = new ProcessDataSample(JobManGlobals.Time.Now);
        this.WorkDataGlobalLive = new ProcessDataSample(JobManGlobals.Time.Now);
        this.WorkDataPools = new Dictionary<string, ProcessDataSample>();
        this.WorkDataPoolsUi = new Dictionary<string, ProcessDataSample>();
    }

    protected void CheckClear()
    {

        if (this.WorkDataGlobal.Time != JobManGlobals.Time.Now.WithSecond())
        {
            this.WorkDataGlobal = (ProcessDataSample)this.WorkDataGlobalLive.Clone();
            //this.WorkDataGlobal.Time= Globals.Time.Now;
            //try
            //{
            //    this.WorkDataGlobalLive.Processed = 0;
            //    this.WorkDataGlobalLive.DoFail = 0;
            //    this.WorkDataGlobalLive.Time = Globals.Time.Now.WithSecond();
            //}
            //finally
            //{
            //}
        }
    }

    public async Task Add(IWorkPool pool, ProcessDataSample sample)
    {
        _lock.EnterWriteLock();
        try
        {
            this.WorkDataPools.Set(pool.Name, sample);

            if (pool.Options.ShowInUi)
                this.WorkDataPoolsUi.Set(pool.Name, sample);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        await Task.CompletedTask;
    }

    public void UpdateGlobalLive(int waiting, int inQueue)
    {
        this.CheckClear();

        this.WorkDataGlobalLive.Waiting = waiting;
        //this.WorkDataGlobalLive.Processed = 0;
        //this.WorkDataGlobalLive.DoFail = 0;

        this.WorkDataGlobalLive.InQueue = inQueue;

        foreach (ProcessDataSample smp in this.WorkDataPools.Values)
        {
            this.WorkDataGlobalLive.Processed += smp.Processed;
            this.WorkDataGlobalLive.Fail += smp.Fail;
            this.WorkDataGlobalLive.InQueue = this.WorkDataGlobalLive.InQueue + smp.InQueue;
            this.WorkDataGlobalLive.Time = JobManGlobals.Time.Now.WithSecond();
        }
    }
}
