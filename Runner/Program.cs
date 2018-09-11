using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace vena.testrunner
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-clean")
            {
                CleanTests();
                Console.WriteLine("Tests Cleaned: Tests\\Tmp\\");
                return 0;
            }
            var workingDirectory = args.Length > 0 ? args[0] : @"D:\github\vena\";
            var testsPath = Path.Combine(workingDirectory, @"Tests\");
            var buildtests = Path.Combine(workingDirectory, @"Tests\Tmp\");
            var venaPath = @".\bin\Debug\netcoreapp2.1\vena.dll";

            // Create buildtests location if not exist
            if (!Directory.Exists(buildtests))
            {
                Directory.CreateDirectory(buildtests);
            }

            // Compile all test files
            foreach (var file in Directory.EnumerateFiles(testsPath, "*.vena"))
            {
                (bool pass, string error) = BuildTest(workingDirectory, venaPath, file);
                if (!pass)
                {
                    Console.WriteLine($"Test {Path.GetFileNameWithoutExtension(file)}.vena: Failed");
                    Console.WriteLine(error);
                    return 1;
                }
            }
            // Test build tests and check outputs
            int tests = 0;
            foreach (var file in Directory.EnumerateFiles(buildtests, "*.dll"))
            {
                (bool pass, string error) = RunTest(workingDirectory, file);
                if (!pass)
                {
                    Console.WriteLine($"Test {Path.GetFileNameWithoutExtension(file)}.vena: Failed");
                    Console.WriteLine(error);
                    return 1;
                }
                tests += 1;
            }
            Console.WriteLine($"All {tests} Tests: Passed");
            return 0;
        }

        private static void CleanTests(string directory = @"Tests\Tmp\")
        {
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                File.Delete(file);
            }
        }

        private static (bool, string) BuildTest(string working, string venadll, string inputFile)
        {
            var output = $".\\Tests\\Tmp\\{Path.GetFileNameWithoutExtension(inputFile)}";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(@"C:\Program Files\dotnet\dotnet.exe")
                {
                    WorkingDirectory = working,
                    Arguments = $"exec \"{venadll}\" {inputFile} {output}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            // Check for errors
            if (result.Length != 0 || error.Length != 0)
            {
                return (false, result + "\r\nError: " + error);
            }
            return (true, null);
        }

        private static (bool, string) RunTest(string working, string testdll)
        {
            var expected = Path.Combine(working, $".\\Tests\\{Path.GetFileNameWithoutExtension(testdll)}.vena.out");
            var expectedOutput = File.ReadAllText(expected);

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(@"C:\Program Files\dotnet\dotnet.exe")
                {
                    WorkingDirectory = working,
                    Arguments = $"exec \"{testdll}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            // Check for errors
            if (error.Length != 0)
            {
                return (false, error);
            }
            // Check for equality to output
            result = result?.TrimEnd();
            if (result != expectedOutput)
            {
                return (false, $"Result: {result}\r\nExpected: {expectedOutput}");
            }
            return (true, null);
        }
    }
}
