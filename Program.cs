namespace ElevatorSystem;

static class Program
{
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        //https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }    
}