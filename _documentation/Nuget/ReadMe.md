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

## Why Jobman?

Hangfire is a successful project. 
However, we identified with a simpler architecture; more efficient and capable of task prioritization and stability.

Core objective is to provide a codebase that prioritizes performance and simplicity, allowing developers to easily intervene in the core code.

## Why a Background Job Management System?

It's crucial to organize operations in web applications (especially) in a way that doesn't impact user interaction (e.g., delays in responding to users). Structures like Jobman and Hangfire allow jobs to be queued and executed within a single process, making maximum use of system resources while minimizing impact on foreground operations.
If you don't require complex queue/addressing management (like MQTT or RabbitMQ), Jobman provides the most practical and effective solution.

## Key Differences

- Simple codebase
- Runtime scalability
- In-memory jobs that run directly without waiting for persistence (if an opportunity exists)
- For databases: Low transaction cost / minimal blocking operations

## Features (within information)

- Direct start/stop
- Job pool management with non-terminating tasks, job pool prioritization
- Scheduling / Recurring with Cron Expressions
- Work pool prioritization
- Direct invoke (immediate execution) when resources are available
- ...
- ...
- ...
