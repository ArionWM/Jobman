using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Tests;

#pragma warning disable xUnit1013 // Public method should be marked as test
public abstract class StorageTestBase<T> : IClassFixture<DbFixture> where T : DbFixture
{
    protected DbFixture fixture;

    public StorageTestBase(DbFixture fixture)
    {
        this.fixture = fixture;
    }

    protected abstract void ClearData(IWorkItemStorage storage);

    public static void DbTestSampleAction1()
    {

    }

    public static void DbTestSampleAction2(string myArg0, int myArg1, Guid myArg2)
    {

    }

    //[Fact]
    //public void SimpleSaveAndLoad()
    //{

    //    this.ClearData(this.fixture.Storage);

    //    Guid id = Guid.NewGuid();
    //    Expression<Action> exDbTestSampleAction2 = () => DbTestSampleAction2("myValue0", 2, id);
    //    IWorkItemDefinition wid1 = Globals.WorkServerOptions.WorkItemDefinitionFactory.Create(exDbTestSampleAction2);

    //    this.fixture.Storage.Set(wid1);

    //    Assert.Equal(1, this.fixture.Storage.GetMetrics().TotalItemCount);

    //    var cancelSource = new CancellationTokenSource(1000);

    //    IWorkItemDefinition[] wids = this.fixture.Storage.PeekOrWait(1, WorkPoolOptions.POOL_DEFAULT, 100, cancelSource.Token);

    //    IWorkItemDefinition wid = wids[0];
    //    Assert.Equal("myValue0", wid.Data.ArgumentValues[0]);
    //    Assert.Equal((int)2, wid.Data.ArgumentValues[1]);
    //    Assert.Equal(id, wid.Data.ArgumentValues[2]);

    //    StorageMetrics metrics = this.fixture.Storage.GetMetrics();

    //    Assert.Equal(1, metrics.TotalItemCount);
    //    Assert.Equal(0, metrics.WaitingItemCountOnStorate);
    //    //Assert.Equal(1, metrics.StatusCounts.Get(WorkItemStatus.Enqueuing)); remove for performance problem


    //}
}
#pragma warning restore xUnit1013 // Public method should be marked as test

