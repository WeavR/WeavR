using System;
using System.Linq;

namespace WeavR
{
    public class ColourConsoleLogger : ConsoleLogger
    {
        private static void SetColor(ConsoleColor c)
        {
            var colour = c;
            if (c == Console.BackgroundColor)
            {
                if (Console.BackgroundColor != ConsoleColor.Black)
                {
                    colour = ConsoleColor.Black;
                }
                else
                {
                    colour = ConsoleColor.Gray;
                }
            }
            Console.ForegroundColor = colour;
        }

        protected override void WriteInfo(string message)
        {
            SetColor(ConsoleColor.Cyan);
            base.WriteInfo(message);
        }

        protected override void WriteWarning(string message)
        {
            SetColor(ConsoleColor.Yellow);
            base.WriteWarning(message);
        }

        protected override void WriteError(string message)
        {
            SetColor(ConsoleColor.Red);
            base.WriteError(message);
        }
    }
}