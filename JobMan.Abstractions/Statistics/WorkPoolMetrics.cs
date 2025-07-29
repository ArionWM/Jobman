using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;

namespace JobMan
{


    public class WorkPoolMetrics
    {

        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public string Name { get; set; }
        public int WorkerCount { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WorkPoolStatus Status { get; set; }
        public ProcessDataSample Current { get; set; }
        public SortedDictionary<DateTime, ProcessDataSample> Metrics { get; set; } = new SortedDictionary<DateTime, ProcessDataSample>();

        public event Action DataShift;

        public WorkPoolMetrics(string name)
        {
            this.Current = new ProcessDataSample(JobManGlobals.Time.Now);
            Name = name;
        }

        protected void Clear()
        {
            if (this.Metrics.Count < 100)
                return;
            
            try
            {
                DateTime[] keys = this.Metrics.Keys.ToArray();
                for (int i = 0; i < keys.Length - 100; i++)
                {
                    this.Metrics.Remove(keys[i]);
                }
            }
            finally
            {
                
            }
        }

        protected async Task DoShiftEvent()
        {
            this.DataShift?.Invoke();
            await Task.CompletedTask;
        }

        protected void ShiftCurrent()
        {
            try
            {
                ProcessDataSample current = this.Current;
                this.Current = new ProcessDataSample(JobManGlobals.Time.Now);
                this.Metrics.Set(current.Time, current);
            }
            finally
            {
            }
        }

        protected bool CheckShift()
        {
            if (this.Current.Time != JobManGlobals.Time.Now.WithSecond())
            {
                this.ShiftCurrent();
                this.Clear();
                return true;
            }
            return false;
        }

        public async Task Add(int processed, int fail, int inQueue)
        {
            bool isShifted = false;
            _lock.EnterWriteLock();
            try
            {
                isShifted = this.CheckShift();
                this.Current.Processed = this.Current.Processed + processed;
                this.Current.Fail = this.Current.Fail + fail;
                this.Current.InQueue = inQueue;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            if (isShifted)
                _ = this.DoShiftEvent();

            await Task.CompletedTask;
        }

        public void SetWaiting(int waiting)
        {
            _lock.EnterWriteLock();
            try
            {
                this.Current.Waiting = waiting;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

        }

        //public void Set(int waiting, int processed, int fail)
        //{
        //    bool isShifted = false;
        //    _lock.EnterWriteLock();
        //    try
        //    {
        //        isShifted = this.CheckShift();
        //        this.Current.Waiting = waiting;
        //        this.Current.Processed = processed;
        //        this.Current.DoFail = fail;
        //    }
        //    finally
        //    {
        //        _lock.ExitWriteLock();
        //    }

        //    if (isShifted)
        //        _ = this.DoShiftEvent();
        //}

        public ProcessDataSample GetLast()
        {
            _lock.EnterReadLock();
            try
            {
                ProcessDataSample sample = this.Metrics.Values.LastOrDefault();
                if (sample == null)
                    return this.Current;

                return sample;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
