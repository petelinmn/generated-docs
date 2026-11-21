using System.Runtime;
using Examples;

// GC.Collect, no-GC regions and latency modes affect the whole process: run tests one at a time.
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace Tests;

public class Ex01_ReachabilityTests
{
    [Fact]
    public void UnreachableObject_IsCollected()
    {
        Assert.Equal((true, false), Reachability.Demo());
    }

    [Fact]
    public void Cycle_IsCollected()
    {
        Assert.True(Reachability.CycleIsCollected());
    }
}

public class Ex02_GenerationsTests
{
    [Fact]
    public void Survivor_IsPromotedUpToGen2()
    {
        Assert.Equal([0, 1, 2, 2], Generations.Promotion());
    }

    [Fact]
    public void LargeObjects_StartInGen2()
    {
        Assert.True(Environment.Is64BitProcess, "array header size assumes a 64-bit runtime");
        Assert.Equal((0, 0, 2), Generations.LargeObjects());
    }

    [Fact]
    public void PinnedArray_ReportsGen2()
    {
        Assert.Equal(2, Generations.PinnedArrayGeneration());
    }

    [Fact]
    public void FullCollection_CountsForAllGenerations()
    {
        Assert.Equal((1, 1, 1), Generations.CountersAfterFullCollection());
    }
}

public class Ex03_UsingTests
{
    [Fact]
    public void Statement_DisposesAtEndOfBlock()
    {
        Assert.Equal(["body", "dispose file", "after"], UsingExamples.Statement());
    }

    [Fact]
    public void Declarations_DisposeInReverseOrder()
    {
        Assert.Equal(["body end", "dispose b", "dispose a", "after block"], UsingExamples.Declarations());
    }

    [Fact]
    public void Exception_StillDisposes()
    {
        Assert.Equal(["dispose resource", "caught"], UsingExamples.WithException());
    }

    [Fact]
    public void Null_IsAllowed()
    {
        Assert.True(UsingExamples.NullIsAllowed());
    }
}

public class Ex04_DisposePatternTests
{
    [Fact]
    public void Dispose_IsIdempotent_AndBlocksFurtherUse()
    {
        var writer = new ReportWriter();
        writer.WriteLine("ok");
        writer.Dispose();
        writer.Dispose();

        Assert.Equal(2, writer.DisposeCalls);
        Assert.Throws<ObjectDisposedException>(() => writer.WriteLine("late"));
    }

    [Fact]
    public void Derived_ReleasesItsResourcesBeforeBase()
    {
        var resource = new DerivedResource();
        resource.Dispose();
        resource.Dispose();

        Assert.Equal(["derived: managed", "base: managed"], resource.Log);
    }
}

public class Ex05_AsyncDisposeTests
{
    [Fact]
    public async Task AwaitUsing_CallsDisposeAsync()
    {
        Assert.Equal(["body", "flushed 2 async"], await AsyncDisposeExamples.AwaitUsing());
    }

    [Fact]
    public void PlainUsing_CallsDispose()
    {
        Assert.Equal(["flushed 1 sync"], AsyncDisposeExamples.PlainUsing());
    }
}

public class Ex06_FinalizersTests
{
    [Fact]
    public void Finalizer_RunsWhenNotDisposed()
    {
        Assert.True(Finalizers.RunsWhenNotDisposed());
    }

    [Fact]
    public void Dispose_SuppressesFinalizer()
    {
        Assert.False(Finalizers.SuppressedByDispose());
    }

    [Fact]
    public void FinalizableObject_SurvivesFirstCollection()
    {
        Assert.Equal((true, false), Finalizers.SurvivesOneMoreCollection());
    }
}

public class Ex07_SafeHandlesTests
{
    [Fact]
    public void Handle_IsReleasedExactlyOnce()
    {
        var (closed, isClosed) = SafeHandles.DisposeTwice();

        Assert.Equal([(nint)100], closed);
        Assert.True(isClosed);
    }

    [Fact]
    public void InvalidHandle_IsNotReleased()
    {
        Assert.Equal(0, SafeHandles.InvalidHandleIsNotReleased());
    }
}

public class Ex08_EventLeaksTests
{
    [Fact]
    public void ForgottenSubscriber_StaysAlive()
    {
        Assert.Equal((true, 1), EventLeaks.Forgotten());
    }

    [Fact]
    public void UnsubscribedSubscriber_IsCollected()
    {
        Assert.Equal((false, 0), EventLeaks.Unsubscribed());
    }
}

public class Ex09_WeakReferencesTests
{
    [Fact]
    public void WeakReference_LosesTargetAfterCollection()
    {
        Assert.Equal((true, false), WeakReferences.TryGetTarget());
    }

    [Fact]
    public void ConditionalWeakTable_EntryDiesWithKey()
    {
        Assert.Equal((2, 1, "kept"), WeakReferences.AttachedData());
    }
}

public class Ex10_AllocationsTests
{
    [Fact]
    public void Boxing_AllocatesAnObjectPerValue()
    {
        Assert.Equal(1_000 * 24, Allocations.BoxingIntoArrayList(1_000));
    }

    [Fact]
    public void GenericList_DoesNotAllocatePerValue()
    {
        Assert.Equal(0, Allocations.GenericList(1_000));
    }

    [Fact]
    public void SmallestObjects_Take24Bytes()
    {
        Assert.True(Environment.Is64BitProcess);
        Assert.Equal((24, 24, 48), Allocations.ObjectSizes());
    }
}

public class Ex11_GcControlTests
{
    [Fact]
    public void Defaults_WorkstationInteractive()
    {
        Assert.Equal((false, GCLatencyMode.Interactive, 2), GcControl.Settings());
    }

    [Fact]
    public void ForcedCompaction_Compacts()
    {
        Assert.True(GcControl.ForcedCompaction());
    }

    [Fact]
    public void CompactOnce_ResetsAfterCollection()
    {
        Assert.Equal((true, GCLargeObjectHeapCompactionMode.Default), GcControl.CompactLargeObjectHeapOnce());
    }

    [Fact]
    public void NoGcRegion_SuppressesCollections()
    {
        Assert.Equal((true, GCLatencyMode.NoGCRegion, 0), GcControl.NoGcRegion());
    }
}

public class Ex12_PinningTests
{
    [Fact]
    public void Fixed_GivesAPointerToArrayData()
    {
        Assert.Equal(6, Pinning.SumWithPointer([1, 2, 3]));
    }

    [Fact]
    public void PinnedObject_IsNotMoved()
    {
        Assert.True(Pinning.PinnedHandleAddressIsStable());
    }
}
