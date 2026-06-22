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
        private string _pythonPath;
        private static bool _dependenciesChecked = false;

        public PythonRunner(IConfiguration configuration)
        {
            var configPath = configuration["PythonSettings:PythonPath"] ?? "python";
            _pythonPath = ResolvePythonPath(configPath);
            EnsureDependenciesAsync();
        }

        private string ResolvePythonPath(string configPath)
        {
            if (CanExecute(configPath))
            {
                return configPath;
            }

            if (!OperatingSystem.IsWindows())
            {
                if (CanExecute("python3")) return "python3";
                if (CanExecute("python")) return "python";
            }
            
            return configPath;
        }

        private bool CanExecute(string command)
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
                return process != null && process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private void EnsureDependenciesAsync()
        {
            if (_dependenciesChecked) return;
            _dependenciesChecked = true;

            Task.Run(async () =>
            {
                try
                {
                    // 1. Bootstrap pip if not present
                    using (var bootstrapProc = Process.Start(new ProcessStartInfo
                    {
                        FileName = _pythonPath,
                        Arguments = "-m ensurepip --default-pip",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }))
                    {
                        if (bootstrapProc != null)
                        {
                            await bootstrapProc.WaitForExitAsync();
                        }
                    }

                    // 2. Install required packages
                    var packages = new[] { "pandas", "numpy", "scikit-learn", "scipy", "statsmodels", "openpyxl", "reportlab", "joblib" };
                    var args = $"-m pip install --user {string.Join(" ", packages)}";
                    
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = _pythonPath,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(processStartInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        Console.WriteLine($"Python Dependency Installer Output: {output}");
                        if (process.ExitCode != 0)
                        {
                            Console.WriteLine($"Python Dependency Installer Error: {error}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to auto-verify or install Python dependencies: {ex.Message}");
                }
            });
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
