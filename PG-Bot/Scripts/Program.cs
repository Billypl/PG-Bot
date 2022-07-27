namespace PG_Bot.Scripts
{
    class Program
    {
        static void Main() 
        {
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
