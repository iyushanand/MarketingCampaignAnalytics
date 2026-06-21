using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Backend.Services
{
    /// <summary>
    /// Executes Python analytical scripts asynchronously in a subprocess.
    /// </summary>
    public class PythonRunner
    {
        private readonly string _pythonPath;

        public PythonRunner(IConfiguration configuration)
        {
            _pythonPath = configuration["PythonSettings:PythonPath"] ?? "python";
        }

        /// <summary>
        /// Runs a Python script with the specified arguments and returns the console output.
        /// </summary>
        public async Task<string> RunScriptAsync(string scriptName, params string[] args)
        {
            // Resolve script path relative to the Backend/Analytics directory
            string baseDir = AppContext.BaseDirectory;
            string scriptPath = Path.Combine(baseDir, "Analytics", scriptName);
            
            if (!File.Exists(scriptPath))
            {
                scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Analytics", scriptName);
                if (!File.Exists(scriptPath))
                {
                    scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Backend", "Analytics", scriptName);
                }
            }

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Python script not found: {scriptName} at {scriptPath}");
            }

            // Build arguments escaping quotes properly
            var escapedArgs = string.Join(" ", args.Select(a => $"\"{a.Replace("\"", "\\\"")}\""));
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _pythonPath,
                Arguments = $"\"{scriptPath}\" {escapedArgs}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Python runner error: {error}\nOutput: {output}");
            }

            return output;
        }
    }
}
