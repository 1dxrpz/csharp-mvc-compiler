

using DoplomaCompilator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DoplomaCompilator.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		void Build()
		{
			var process = new Process();
			var startInfo = new ProcessStartInfo
			{
				FileName = "dotnet.exe",
				Arguments = @"build ..\CompilationConsole\CompilationConsole.csproj"
			};
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();
		}

		string Run()
		{
			var psi = new ProcessStartInfo()
			{
				FileName = @"..\CompilationConsole\out\CompilationConsole.exe",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var proc = Process.Start(psi);
			string output = proc.StandardOutput.ReadToEnd();

			using (StreamReader s = proc.StandardError)
			{
				string error = s.ReadToEnd();
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
		public IActionResult CompileCode(string code)
		{
			System.IO.File.WriteAllText(@"..\CompilationConsole\Program.cs", code);

			Build();

			return Ok(Run());
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
