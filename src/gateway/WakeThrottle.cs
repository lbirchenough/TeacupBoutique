namespace gateway;

// Single-replica gateway (min=max=1), so process-local state is safe.
// Container Apps cooldown after KEDA / activity is 5 min; we throttle re-wake
// calls to 4 min to leave a margin before backends scale to zero.
public static class WakeThrottle
{
    private static readonly TimeSpan WarmWindow = TimeSpan.FromMinutes(4);
    private static long _lastWokenTicks;

    public static bool IsWarm()
    {
        var last = Interlocked.Read(ref _lastWokenTicks);
        if (last == 0) return false;
        return DateTime.UtcNow - new DateTime(last, DateTimeKind.Utc) < WarmWindow;
    }

    public static void MarkWoken()
    {
        Interlocked.Exchange(ref _lastWokenTicks, DateTime.UtcNow.Ticks);
    }
}
