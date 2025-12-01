namespace MiniGuys;

using Godot;
using System.Collections.Generic;

public static class TowerTargeting
{
    public static int FindNearestIndex(Vector2 origin, IReadOnlyList<Vector2> targets)
    {
        if (targets.Count == 0)
            return -1;

        var nearestIndex = 0;
        var nearestDistSq = origin.DistanceSquaredTo(targets[0]);

        for (var i = 1; i < targets.Count; i++)
        {
            var distSq = origin.DistanceSquaredTo(targets[i]);
            if (distSq < nearestDistSq)
            {
                nearestDistSq = distSq;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }
}
