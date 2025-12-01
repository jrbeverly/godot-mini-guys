namespace MiniGuys.Tests;

using System.Collections.Generic;
using FluentAssertions;
using Godot;
using Xunit;

public class TowerTargetingTests
{
    [Fact]
    public void FindNearestIndex_with_empty_list_returns_minus_one()
    {
        var targets = new List<Vector2>();
        var result = TowerTargeting.FindNearestIndex(Vector2.Zero, targets);
        result.Should().Be(-1);
    }

    [Fact]
    public void FindNearestIndex_with_single_target_returns_zero()
    {
        var targets = new List<Vector2> { new(100, 0) };
        var result = TowerTargeting.FindNearestIndex(Vector2.Zero, targets);
        result.Should().Be(0);
    }

    [Fact]
    public void FindNearestIndex_returns_closest()
    {
        var targets = new List<Vector2>
        {
            new(100, 0),
            new(20, 0),
            new(80, 0),
        };
        var result = TowerTargeting.FindNearestIndex(Vector2.Zero, targets);
        result.Should().Be(1);
    }

    [Fact]
    public void FindNearestIndex_with_multiple_at_same_distance_returns_first()
    {
        var targets = new List<Vector2>
        {
            new(50, 0),
            new(0, 50),
            new(0, -50),
        };
        var result = TowerTargeting.FindNearestIndex(Vector2.Zero, targets);
        result.Should().Be(0);
    }

    [Fact]
    public void FindNearestIndex_with_offset_origin()
    {
        var targets = new List<Vector2>
        {
            new(80, 100),
            new(85, 100),
            new(200, 100),
        };
        var origin = new Vector2(100, 100);
        var result = TowerTargeting.FindNearestIndex(origin, targets);
        result.Should().Be(1);
    }
}
