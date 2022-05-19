namespace neoml;
static class Macro
{
    public static void print<T>(this T val) => Console.WriteLine(val);
    public static void write(this byte[] val) => Console.OpenStandardOutput().Write(val);
    public static S pipe<S, T>(this T val, Func<T, S> f) => f(val);
    public static T with<T>(this T val, Action<T> f) { f(val); return val; }
    public static T withif<T>(this T val, bool cond, Action<T> f) { if (cond) f(val); return val; }
    public static T withassert<T>(this T val, bool cond, string? msg = null) { if (!cond) throw new Exception(msg); return val; }
}
