using JackToVmCompiler.Utils;

namespace JackToVmCompiler
{
    internal class App
    {
        static async Task Main(string[] args)
        {
            if (args.IsNullOrEmpty())
                args = new string[] { @"D:\Software\nand2tetris_without_changes\nand2tetris\projects\10\ExpressionLessSquare" };
            if (args.IsNullOrEmpty())
            {
                Console.WriteLine("Please, write .jack file path or directory with .jack files as first argument");
                Wait();
                return;
            }
                
            var sourcePath = args[0];
            var compiler = new JackSyntaxCompiler(sourcePath);
            if (!compiler.IsValidSource)
            {
                Wait();
                return;
            }

            var result = await compiler.Compile();
            Console.WriteLine($"Compile succesfull: {result}");

            Wait();
        }

        static void Wait() =>
            Console.ReadLine();
    }
}