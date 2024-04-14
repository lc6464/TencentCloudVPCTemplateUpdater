using System.Diagnostics;
using TencentCloudVPCTemplateUpdater;

if (OperatingSystem.IsWindows()) {
	if (args is ["install"]) {
		{
			using var process = Process.Start(new ProcessStartInfo {
				FileName = "sc.exe",
				Arguments = $"create \"LC6464 TencentCloud VPC Template Updater\" binpath=\"{Process.GetCurrentProcess().MainModule?.FileName}\" start=\"delayed-auto\" DisplayName=\"LC6464 腾讯云私有网络安全组参数模板 IPv4 地址更新器\"",
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			});

			process?.WaitForExit();
			Console.WriteLine(process?.StandardOutput.ReadToEnd());
		}

		{
			using var process = Process.Start(new ProcessStartInfo {
				FileName = "sc.exe",
				Arguments = "start \"LC6464 TencentCloud VPC Template Updater\"",
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			});

			process?.WaitForExit();
			Console.WriteLine(process?.StandardOutput.ReadToEnd());

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		{
			using var process = Process.Start(new ProcessStartInfo {
				FileName = "sc.exe",
				Arguments = "query \"LC6464 TencentCloud VPC Template Updater\"",
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			});

			process?.WaitForExit();
			Console.WriteLine(process?.StandardOutput.ReadToEnd());
		}

		return;
	}

	if (args is ["uninstall"]) {
		{
			using var process = Process.Start(new ProcessStartInfo {
				FileName = "sc.exe",
				Arguments = "stop \"LC6464 TencentCloud VPC Template Updater\"",
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			});

			process?.WaitForExit();
			Console.WriteLine(process?.StandardOutput.ReadToEnd());

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		{
			using var process = Process.Start(new ProcessStartInfo {
				FileName = "sc.exe",
				Arguments = "delete \"LC6464 TencentCloud VPC Template Updater\"",
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			});

			process?.WaitForExit();
			Console.WriteLine(process?.StandardOutput.ReadToEnd());
		}

		return;
	}

	if (args is ["start"] or ["stop"]) {
		{
			using var process = Process.Start(new ProcessStartInfo {
				FileName = "sc.exe",
				Arguments = $"{args[0]} \"LC6464 TencentCloud VPC Template Updater\"",
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			});

			process?.WaitForExit();
			Console.WriteLine(process?.StandardOutput.ReadToEnd());

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		{
			using var process = Process.Start(new ProcessStartInfo {
				FileName = "sc.exe",
				Arguments = "query \"LC6464 TencentCloud VPC Template Updater\"",
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			});

			process?.WaitForExit();
			Console.WriteLine(process?.StandardOutput.ReadToEnd());
		}

		return;
	}
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options => options.ServiceName = "LC6464 TencentCloud VPC Template Updater");

builder.Services.AddHostedService<WindowsBackgroundService>();

var host = builder.Build();
host.Run();

return;