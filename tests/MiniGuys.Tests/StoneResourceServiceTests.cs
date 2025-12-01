namespace MiniGuys.Tests;

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

public class StoneResourceServiceTests
{
    [Fact]
    public void Initial_amount_is_zero()
    {
        var service = new StoneResourceService();
        service.Amount.Should().Be(0);
    }

    [Fact]
    public void Add_increments_amount_and_fires_changed()
    {
        var service = new StoneResourceService();
        var calls = new List<int>();
        service.Changed += amount => calls.Add(amount);

        service.Add(30);

        service.Amount.Should().Be(30);
        calls.Should().HaveCount(1);
        calls[0].Should().Be(30);
    }

    [Fact]
    public void Multiple_adds_accumulate()
    {
        var service = new StoneResourceService();
        service.Add(10);
        service.Add(20);
        service.Add(5);

        service.Amount.Should().Be(35);
    }

    [Fact]
    public void TrySpend_succeeds_when_amount_is_sufficient()
    {
        var service = new StoneResourceService();
        service.Add(50);

        var result = service.TrySpend(30);

        result.Should().BeTrue();
        service.Amount.Should().Be(20);
    }

    [Fact]
    public void TrySpend_fires_changed_on_success()
    {
        var service = new StoneResourceService();
        service.Add(50);

        var calls = new List<int>();
        service.Changed += amount => calls.Add(amount);

        service.TrySpend(30);

        calls.Should().HaveCount(1);
        calls[0].Should().Be(20);
    }

    [Fact]
    public void TrySpend_returns_false_when_insufficient()
    {
        var service = new StoneResourceService();
        service.Add(10);

        var result = service.TrySpend(30);

        result.Should().BeFalse();
    }

    [Fact]
    public void TrySpend_does_not_mutate_amount_when_insufficient()
    {
        var service = new StoneResourceService();
        service.Add(10);

        service.TrySpend(30);

        service.Amount.Should().Be(10);
    }

    [Fact]
    public void TrySpend_does_not_fire_changed_when_insufficient()
    {
        var service = new StoneResourceService();
        service.Add(10);

        var fireCount = 0;
        service.Changed += _ => fireCount++;

        service.TrySpend(30);

        fireCount.Should().Be(0);
    }

    [Fact]
    public void TrySpend_entire_balance_succeeds()
    {
        var service = new StoneResourceService();
        service.Add(25);

        var result = service.TrySpend(25);

        result.Should().BeTrue();
        service.Amount.Should().Be(0);
    }

    [Fact]
    public void Reset_sets_amount_to_zero()
    {
        var service = new StoneResourceService();
        service.Add(50);

        service.Reset();

        service.Amount.Should().Be(0);
    }

    [Fact]
    public void Reset_does_not_fire_changed()
    {
        var service = new StoneResourceService();
        service.Add(50);

        var fireCount = 0;
        service.Changed += _ => fireCount++;

        service.Reset();

        fireCount.Should().Be(0);
    }
}
