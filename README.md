# Jobman

Jobman is *high performance*, *stable*, persistent, embedded producer-consumer (background processing) job server for .NET. 

Compatible with PostgreSQL, MySQL, MS SQL Server, and various other databases.

## Use Cases

Primarily for web projects, and generally for any service or application project requiring background job management, Jobman can be used for:

- Sending notifications (mail, messages)
- Batch operations
- Background API client operations
- Report automation
- Periodic database maintenance tasks
- File cache and temporary management
- Any computation not directly dependent on user interaction
- Asynchronous log/record generation

## Why a Background Job Management System?

It's crucial to organize operations in web applications (especially) in a way that doesn't impact user interaction (e.g., delays in responding to users). Structures like Jobman and Hangfire allow jobs to be queued and executed within a single process, making maximum use of system resources while minimizing impact on foreground operations.
If you don't require complex queue/addressing management (like MQTT or RabbitMQ), Jobman provides the most practical and effective solution.

## Why Jobman?

Hangfire is a successful project. 
However, we identified with a simpler architecture; more efficient and capable of task prioritization and stability.

Core objective is to provide a codebase that prioritizes performance and simplicity, allowing developers to easily intervene in the core code.

## Key Differences

- Simple codebase
- In-memory jobs that run directly without waiting for persistence (if an opportunity exists)
- For databases: Low transaction cost / minimal blocking operations

## Features

- Direct start/stop
- Job pool management with non-terminating tasks, job pool prioritization
- Scheduling / Recurring with Cron Expressions
- Work pool prioritization
- Direct invoke (immediate execution) when resources are available
- ...
- ...
- ...

## Architecture

Jobman is designed around the concepts of ThreadPool (named WorkPool), allowing for efficient job management and execution.

Each WorkPool have multiple threads and can be configured with different priorities and storages.

![Architecture](https://github.com/ArionWM/Jobman/blob/main/_documentation/img/Architecture.png)

## Links

- [How to Use](https://github.com/ArionWM/Jobman/blob/main/_documentation/HowToUse.md)
- [Contribution](https://github.com/ArionWM/Jobman/blob/main/_documentation/Contribution.md)
- [License](https://github.com/ArionWM/Jobman/blob/main/_documentation/license.md)

## Packages (nuget)

| Package | Version |
|---|---|
|[JobMan](https://www.nuget.org/packages/JobMan/)|![JobMan](https://img.shields.io/nuget/v/JobMan)|
|[Jobman.UI.AspNetCore](https://www.nuget.org/packages/Jobman.UI.AspNetCore)|![Jobman.UI.AspNetCore](https://img.shields.io/nuget/v/Jobman.UI.AspNetCore)|
|[JobMan.Storage.PostgreSql](https://www.nuget.org/packages/JobMan.Storage.PostgreSql)|![JobMan.Storage.PostgreSql](https://img.shields.io/nuget/v/JobMan.Storage.PostgreSql)|
|[JobMan.Storage.SqlServer](https://www.nuget.org/packages/JobMan.Storage.SqlServer)|![JobMan.Storage.SqlServer](https://img.shields.io/nuget/v/JobMan.Storage.SqlServer)|


## Other Alternatives / Related Infrastructures

- [Hangfire](https://www.hangfire.io/)
- [Quartz .Net](https://www.quartz-scheduler.net/)
- [RabbitMQ](https://www.rabbitmq.com/)
- [Delayed Job](https://github.com/collectiveidea/delayed_job)
- [Celery](https://docs.celeryproject.org/en/stable/)
- [Sidekiq](https://sidekiq.org/)
- [Resque](https://github.com/resque/resque)
