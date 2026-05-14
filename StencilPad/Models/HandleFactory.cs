namespace StencilPad.Models;

public static class HandleFactory
{
    private static ulong _counter = 1;

    public static HandleSourceId NewId() => new(_counter++);
}
