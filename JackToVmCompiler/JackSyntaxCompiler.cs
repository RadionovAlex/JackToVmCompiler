﻿using JackToVmCompiler.Tokenizer;
using JackToVmCompiler.Utils;

namespace JackToVmCompiler
{
    internal class JackSyntaxCompiler
    {
        private const string CompiledDirectoryName = "CompiledHackToVm";

        private readonly HashSet<string> _jackFilePathes;

        public JackSyntaxCompiler(string sourcePath)
        {
            if (Path.HasExtension(sourcePath) && Path.GetExtension(sourcePath) == "jack")
            {
                Console.WriteLine("Source path is for jack files");
                _jackFilePathes = new HashSet<string>() { sourcePath };
            }
            else if (Directory.Exists(sourcePath))
            {
                Console.WriteLine("Directory exists");
                _jackFilePathes = Directory.EnumerateFiles(sourcePath, "*.jack").ToHashSet();
            }
            else
            {
                Console.WriteLine("Directory does not exist");
            }
              

#pragma warning disable CS8604 // Possible null reference argument.
            if (_jackFilePathes.IsNullOrEmpty())
            {
                Console.WriteLine($"{sourcePath} is not a directory with .jack files or it is not a .jack file");
                return;
            }
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public async Task<bool> Compile()
        {
            var tasks = new List<Task<bool>>();
            foreach (var filePath in _jackFilePathes)
            {
                var task = CompileFile(filePath);
                tasks.Add(task);
            }


            await Task.WhenAll(tasks.ToArray());
            return tasks.All(x => x.Result);
            /*foreach(var jackFilePath in _jackFilePathes)
            {
                var text = await File.ReadAllTextAsync(jackFilePath);
            }

            return true;*/
        }

        private async Task<bool> CompileFile(string path)
        {
            try
            {
                var text = await File.ReadAllTextAsync(path);
                var cleanLines = TextUtils.ClearTextFromSpacesAndComments(text);
                var tokenizer = new JackTokenizer(cleanLines);


                var pathDirectory = Path.GetDirectoryName(path);
                
                var fileName = Path.GetFileNameWithoutExtension(path);
                var newPath = $"{Path.Combine(pathDirectory, fileName)}.xml";
                var newTestPath = $"{Path.Combine(pathDirectory, fileName)}.tokens";

                await File.WriteAllLinesAsync(newPath, cleanLines);
                await File.WriteAllLinesAsync(newTestPath, tokenizer.Tokens);

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool IsValidSource => !_jackFilePathes.IsNullOrEmpty();

    }
}