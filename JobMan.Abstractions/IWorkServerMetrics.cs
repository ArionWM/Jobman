
//namespace JobMan;

//public interface IWorkServerMetrics
//{
//    string Name { get; }
//    int PoolCount { get; set; }
//    WorkServerStatus Status { get; }
//    ProcessDataSample WorkDataGlobal { get; set; }
//    Dictionary<string, ProcessDataSample> WorkDataPools { get; set; }
//    Dictionary<string, ProcessDataSample> WorkDataPoolsUi { get; set; }
//    int WorkerCount { get; set; }

//    Task Add(IWorkPool pool, ProcessDataSample sample);
//    void UpdateGlobalLive(int waiting, int inQueue);
//}