/*
This content locate and use from resource file
*/

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='jm_jobs' and xtype='U')
	create table jm_jobs
	(
		[Id] bigint identity(1,1) primary key,
		[Schedule] uniqueidentifier, 
		[Type] int not null,
		[Pool] nvarchar(50) not null,
		[Cron] nvarchar(40),
		[Tag] nvarchar(40),

		LastExecuteTime datetime,
		NextExecuteTime datetime not null,

		[Status] int not null,
		[ProcessTimeMs] bigint,
		[RetryCount] int,

		Data nvarchar(max),
		[Description] nvarchar(200)
)

GO;

IF not EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('jm_jobs') AND NAME ='ix_jobs_NextExecuteTime')
	create index ix_jobs_NextExecuteTime on jm_jobs ([Status], [Pool], NextExecuteTime)
	where ( Status = 10 )

GO;

IF not EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('jm_jobs') AND NAME ='ix_jobs_Status')
	create index ix_jobs_Status on jm_jobs ([Status])

GO;

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='jm_enums_WorkItemStatuses' and xtype='U')
	create table jm_enums_WorkItemStatuses
	(
	 [Value] int primary key,
	 [Name] nvarchar(50)
	)

GO;

IF NOT EXISTS (SELECT * FROM jm_enums_WorkItemStatuses WHERE [Value] = 10)
insert into jm_enums_WorkItemStatuses ([Value], [Name]) 
values 
(10, 'WaitingProcess'),
(14, 'Enqueuing'),
(16, 'Enqueued'),
(20, 'Processing'),
(50, 'Completed'),
(99, 'Canceled'),
(100, 'Fail')
