namespace DoplomaCompilator.Models
{
	public class ErrorModel
	{
		public int Line { get; set; }
		public int Column { get; set; }
		public string ErrorMessage { get; set; }
	}

	public class CompilationResultModel
	{
		public List<ErrorModel> Errors { get; set; } = [];
		public bool BuildSucceed { get; set; }
		public string ElapsedTime { get; set; } = string.Empty;
		public string Result { get; set; } = string.Empty;
	}
}
