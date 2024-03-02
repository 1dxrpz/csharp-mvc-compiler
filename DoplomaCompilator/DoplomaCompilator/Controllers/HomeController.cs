

using DoplomaCompilator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DoplomaCompilator.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		async Task<string> Build()
		{
			return await Compile("dotnet.exe", @"build ..\CompilationConsole\CompilationConsole.csproj");
		}

		async Task<string> Run()
		{
			return await Compile(@"..\CompilationConsole\out\CompilationConsole.exe");
		}

		async Task<string> Compile(string filename, string args = "")
		{
			var psi = new ProcessStartInfo()
			{
				FileName = filename,
				Arguments = args,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var proc = Process.Start(psi);
			string output = await proc.StandardOutput.ReadToEndAsync();

			using (StreamReader s = proc.StandardError)
			{
				string error = await s.ReadToEndAsync();
				proc.WaitForExit(20000);
				if (error.Length != 0) return error;
			}
			return output;
		}

		public IActionResult Index()
		{
			return View();
		}

        [HttpPost]
        public async Task<JsonResult> RunCode()
        {
			Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
			var runResult = await Run();
			sw.Stop();

            var result = new CompilationResultModel();

			if (runResult.Contains("Unhandled exception"))
			{
                result.BuildSucceed = false;
				var error = new ErrorModel();
                runResult!
                    .Split('\n').ToList()
                    .ForEach(line =>
                    {
                        if (line.Contains(@"Unhandled exception"))
                        {
                            var message = line.Trim();

                            error.ErrorMessage = message;
                        }
                        if (line.Contains("Program.cs:line"))
                        {
                            error.Line = int.Parse(line.Split("Program.cs:line")[1].Trim());
                        }
                        result.Result = "";
                    });
				result.Errors.Add(error);
                result.BuildSucceed = false;
            }
            else
            {
                result.BuildSucceed = true;
				result.Result = runResult;
            }

			result.ElapsedTime = sw.Elapsed.ToString();

            return new JsonResult(result);
        }

        [HttpPost]
		public async Task<JsonResult> CompileCode(string code)
		{
			System.IO.File.WriteAllText(@"..\CompilationConsole\Program.cs", code);

			var buildResult = await Build();
			var result = new CompilationResultModel();

			if (buildResult.Contains("Build FAILED."))
			{
				result.BuildSucceed = false;
				buildResult
					.Split("Build FAILED.")[1]
					.Trim()
					.Split('\n').ToList()
					.ForEach(line =>
					{
						if (line.Contains(@"CompilationConsole\Program.cs"))
						{
							var message = line.Split(@"CompilationConsole\Program.cs")[1].Split(@"[")[0].Trim();
							var regexMessage = Regex.Match(message, @"\((?<line>\d+),(?<column>\d+)\):(?<message>.+)");
							result.Errors.Add(new ErrorModel()
							{
								ErrorMessage = regexMessage.Groups["message"].Value.Trim(),
								Line = int.Parse(regexMessage.Groups["line"].Value),
								Column = int.Parse(regexMessage.Groups["column"].Value),
							});
						}
						if (line.Contains("Time Elapsed"))
						{
							result.ElapsedTime = line.Split("Time Elapsed")[1].Trim();
						}
						result.Result = "";
					});
			} else
			{
                result.ElapsedTime = buildResult.Split("Time Elapsed")[1].Trim();
                result.BuildSucceed = true;
			}

			Console.WriteLine(buildResult);

			return new JsonResult(result);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
