namespace Test;

public partial class Program
{
    public void DoStuff()
    {
        HelloFrom("from the consumer");
    }

    static partial void HelloFrom(string name);
}