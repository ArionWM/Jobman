# How to Use Jobman

## Requirements

### Without UI

.NET Framework or .NET Core 3.1 or later.

Microsoft Dependency Injection 

### With UI

.NET 7 or later.

## Installation

1. Install the Jobman NuGet package:
   ```
   dotnet add package Jobman
   ```

2. Install Storage Package:
   ```
   dotnet add package Jobman.Storage.SQLServer
   ```

   or
   ```
   dotnet add package Jobman.Storage.PostgreSQL
   ```

3. Install the Jobman UI package (optional):
   ```
   dotnet add package Jobman.UI
   ```

## Sample Configuration

Add jobman service and configure for Work Pools and Storages in your application startup.

   ```csharp
   using Jobman;

   var builder = WebApplication.CreateBuilder(args);
   //or 
   var builder = Host.CreateDefaultBuilder(args)

   builder.Services.AddJobMan(opt =>
            {
                opt.DelayedStart = true;
                opt.DelayedStartMilliseconds = 5000; // 5 seconds

                int defaultThreadCount = 8;

                opt.AddPool("Default",
                    popt =>
                    {
                        popt.ThreadCount = defaultThreadCount ;
                        popt.Priority = ThreadPriority.Normal;
                    });

                opt.AddPool("Low",
                    popt =>
                    {
                        popt.ThreadCount = defaultThreadCount / 2;
                        popt.Priority = ThreadPriority.BelowNormal;
                    });

                opt.AddPool("Mailing",
                    popt =>
                    {
                        popt.ThreadCount = 2;
                        popt.Priority = ThreadPriority.BelowNormal;
                    });


                opt.UsePostgreSqlStorage("Connection string");
            });

   
   ```

### Default Pool

Jobman automatically creates a *default* pool named *Default* if you don't add "Default" named pool explicitly.

## Add UI

To add the Jobman UI, you need to install the Jobman.UI package and configure it in your application:

Jobman UI is added under *Jobman* area ..

```csharp

var app = builder.Build();

...

app.UseJobManUi(); // Add JobMan UI for application ('/jobman')'

...

app.Run();

```

## Startup

Jobman automatically registers the WorkServer (as HostedService) and start (With WorkServerOptions.DelayedStart property).

The work server can stop and start at runtime.

### Manual Stop and Start

*With DI*
```csharp

class MyClass
{
    private readonly IWorkServer jobMan;

    MyClass(IWorkServer workServer)
    {
        this.jobMan = workServer;
    }

     void MyMethod1()
    {
        this.jobMan.StartAsync(); // Can use await or GetAwaiter().GetResult()
    }

    void MyMethod2()
    {
        this.jobMan.StopAsync(); // Can use await or GetAwaiter().GetResult()
    }
    
}
```

*Without DI*
```csharp

JobManGlobals.Server.StartAsync(); 

....

JobManGlobals.Server.StopAsync(); //Can use await or GetAwaiter().GetResult() 
   
```

## Add Jobs

Jobman uses static methods for job definitions. 

Static methods can have parameters, but they must be serializable.
ByRef or out parameters are not supported.

*Without DI*

```csharp

class MyClass
{
    public static void MyJob1(string myParam1, int myParam2)
    {
        Console.WriteLine($"Job1 executed with parameters: {myParam1}, {myParam2}");
    }
}

JobManGlobals.Server.Enqueue("Default", () => MyClass.MyJob1("Hello", 1234));

```

*Default Pool*

If you don't specify a pool, the job will be added to the "Default" pool.

```csharp

//For "Default" pool
JobManGlobals.Server.Enqueue(() => MyClass.MyJob1("Hello", 1234));

```


## Add Jobs With Delay

You can add jobs with a delay using the `Enqueue` method with a `TimeSpan` parameter.

```csharp

class MyClass
{
    public static void MyJob1(string myParam1, int myParam2)
    {
        Console.WriteLine($"Job1 executed with parameters: {myParam1}, {myParam2}");
    }
}

JobManGlobals.Server.Enqueue("Default", () => MyClass.MyJob1("Hello", 1234), TimeSpan.FromMinutes(5));

```

## Scheduling Jobs

You can schedule jobs to run at specific intervals using cron expressions.

See: [https://crontab.guru/](https://crontab.guru/) for cron expression syntax.

```csharp

class MyClass
{
    public static void MyJob1(string myParam1, int myParam2)
    {
        Console.WriteLine($"Job1 executed with parameters: {myParam1}, {myParam2}");
    }
}

// Schedule a job to run daily at 1 AM (every day)
JobManGlobals.Server.Schedule("Default", () => MyClass.MyJob1("Hello", 1234), "0 1 * * *");

```


## Access to UI

To access the Jobman UI, navigate to the following URL in your web browser:

\<YourApplicationRoot>**/jobman**


## Persistence and Direct Invoke

Enqueue jobs can be persisted in the storage, allowing them to be executed even after application restarts.

If workpool preprocess buffer is have empty slot, the job will be executed immediately. If not, it will be persisted and executed later when resources (Workpool buffer) are available.


## Tips & Suggestions

### Use Simple Types for Parameters

When defining jobs, use simple types for parameters to performance and compatibility reasons. Complex types may lead to serialization issues or performance overhead.

### Use Different Work Pools With Priorities

Jobs begin processing when there are free workers in the pool. It is recommended that low-priority jobs be run in another WorkPool to avoid blocking important ones.

### Place long-running jobs in separate pools

When you have jobs that are expected to run for a long time, it's a good idea to place them in another work pool. This prevents them from blocking other jobs and allows for better resource management.

### Use *Default* Pool For Priority Jobs

The *Default* pool is designed for jobs that require immediate attention and should be processed as soon as possible. Use it for high-priority tasks that need to be executed quickly.

