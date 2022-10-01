using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public static class GodotExtensions
{
    private const int WAIT_COUNT_FOR_SIGNAL = 4;
    public static async Task<T> ToSignal<T>(this Godot.Object target, Godot.Object source, string signal, Predicate<T> condition, string debugMessage = null)
    {
        var awaiter = target.ToSignal(source, signal);
        var i = 0;
        var startedAt = Time.GetTicksMsec();
        while (i < WAIT_COUNT_FOR_SIGNAL && (Time.GetTicksMsec() - startedAt) < 60 * 1000)
        {
            i++;
            var res = await awaiter;
            var resT = (T)res[0];
            if (condition == null)
                return resT;
            if (condition.Invoke(resT))
                return resT;
            awaiter = target.ToSignal(source, signal);
        }
        GD.PrintErr($"SIGNAL WAIT COUNT EXCEEDED FOR {signal} on {source}|{(source as Node)?.Name}\n{debugMessage}");
        return default;
    }

    public static bool IsDebugMainScene(this Node n) => (OS.IsDebugBuild() && n.GetParent() == n.GetTree().Root);

    public static async Task<bool> WaitTween(this Godot.Object o, Tween t)
    {
        var b = t.Start();
        await o.ToSignal(t, "tween_completed");
        return b;
    }

    public static async Task<bool> WaitTweenAll(this Godot.Object o, Tween t)
    {
        var b = t.Start();
        await o.ToSignal(t, "tween_all_completed");
        return b;
    }

    public static async Task WaitSeconds(this Godot.Node o, float timeSec)
    {
        var t = new Timer();
        o.AddChild(t);
        var awaiter = o.ToSignal(t, "timeout");
        t.Start(timeSec);
        await awaiter;
        t.QueueFree();
    }
    //  Waits a specified number of frames and returns if the frame counter was successfull or cancelled
    public static async Task<bool> WaitFrames(this Godot.Node o, int frames, Game.FrameWaiter.PROCESS p = Game.FrameWaiter.PROCESS.IDLE, Node.PauseModeEnum pauseModeEnum = Node.PauseModeEnum.Inherit, System.Threading.CancellationToken token = default)
    {
        var fw = new Game.FrameWaiter(frames, p, pauseModeEnum, token);
        var awaiter = o.ToSignal(fw, "completed");
        o.AddChild(fw);

        var ok = await awaiter;
        return (bool)ok[0];
    }

    public static Vector3 RandVec3() => new Vector3(
        (float)GD.RandRange(-1, 1),
         (float)GD.RandRange(-1, 1),
         (float)GD.RandRange(-1, 1)
    ).Normalized();

    public static Vector2 UnitCircleRand() => new Vector2(
        Mathf.Cos(GD.Randf() * 2 * Mathf.Pi),
        Mathf.Sin(GD.Randf() * 2 * Mathf.Pi)
    ).Normalized();
    public static Vector3 UnitCircleRand3D() => new Vector3(
        Mathf.Cos(GD.Randf() * 2 * Mathf.Pi),
        0,
        Mathf.Sin(GD.Randf() * 2 * Mathf.Pi)
    ).Normalized();

    public static System.Collections.Generic.List<T> GetNodesInGroup<T>(this SceneTree tree, string group, Node owner)
    {
        if (owner == null)
            return new System.Collections.Generic.List<T>();
        var ns = tree.GetNodesInGroup(group);
        var res = new System.Collections.Generic.List<T>(ns.Count);
        foreach (var n in ns)
            if (owner.IsAParentOf(n as Node))
                res.Add((T)n);
        return res;
    }

    public static System.Collections.Generic.List<T> GetNodesInGroup<T>(this SceneTree tree, string group)
    {
        var ns = tree.GetNodesInGroup(group);
        var res = new System.Collections.Generic.List<T>(ns.Count);
        foreach (var n in ns)
            res.Add((T)n);
        return res;
    }

    public static void Disconnect(this Godot.Object o, Predicate<(Godot.Object source, String signal, String method)> p = null)
    {
        var conns = new Array<Dictionary>(o.GetIncomingConnections());
        foreach (var cd in conns)
        {
            var c = (source: cd["source"] as Godot.Object, signal: cd["signal_name"] as String, method: cd["method_name"] as String);
            if (p == null || p.Invoke(c))
                c.source.Disconnect(c.signal, o, c.method);
        }
    }

    public static void Disconnect(this Godot.Object o, Godot.Object source) => Disconnect(o, c => c.source == source);
    public static void Disconnect(this Godot.Object o, string method) => Disconnect(o, c => c.method == method);

    public static T Get<T>(this Godot.Object o, string property) => (T)o.Get(property);
    public static Array<T> Cast<T>(this Godot.Collections.Array a) => new Array<T>(a);
}