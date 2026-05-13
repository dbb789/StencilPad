namespace StencilPad.Models;

public static class HandleFactory
{
    private static ulong _counter = 1;

    public static HandleSetId NewId() => new(_counter++);
}
