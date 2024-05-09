using System.Net;

namespace TencentCloudVPCTemplateUpdater;

public static class Utils {
	public const string ServiceName = "LC6464 TencentCloud VPC Template Updater";

	public static readonly TimeSpan TimeSpan1s = TimeSpan.FromSeconds(1);

	public static readonly HttpClient httpClient = new() {
		BaseAddress = new("https://api.lcwebsite.cn/GetIP"),
		Timeout = TimeSpan.FromSeconds(5),
		DefaultRequestVersion = HttpVersion.Version30,
		DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
	};


	public static async Task Delay1s() => await Task.Delay(TimeSpan1s).ConfigureAwait(false);


	public static void ExecuteScQuery() => ExecuteScAction("query");

	public static void ExecuteScAction(string action) => ExecuteScCommand($"{action} \"{ServiceName}\"");

	public static void ExecuteScCommand(string arguments) {
		using var process = Process.Start(new ProcessStartInfo {
			FileName = "sc.exe",
			Arguments = arguments,
			CreateNoWindow = true,
			RedirectStandardError = true,
			RedirectStandardOutput = true,
		});

		process?.WaitForExit();
		Console.WriteLine(process?.StandardOutput.ReadToEnd());
	}
}