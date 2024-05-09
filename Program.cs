if (OperatingSystem.IsWindows()) {
	if (args is ["install"]) {
		Utils.ExecuteScCommand($"create \"{Utils.ServiceName}\" binpath=\"{Process.GetCurrentProcess().MainModule?.FileName}\" start=\"delayed-auto\" DisplayName=\"LC6464 腾讯云私有网络安全组参数模板 IP 地址更新器\"");
		Utils.ExecuteScAction("start");
		await Utils.Delay1sAsync().ConfigureAwait(false);
		Utils.ExecuteScQuery();

		return;
	}

	if (args is ["uninstall"]) {
		Utils.ExecuteScAction("stop");
		await Utils.Delay1sAsync().ConfigureAwait(false);
		Utils.ExecuteScAction("delete");

		return;
	}

	if (args is ["start"] or ["stop"]) {
		Utils.ExecuteScAction(args[0]);
		await Utils.Delay1sAsync().ConfigureAwait(false);
		Utils.ExecuteScQuery();

		return;
	}

	if (args is ["restart"]) {
		Utils.ExecuteScAction("stop");
		await Utils.Delay1sAsync().ConfigureAwait(false);
		Utils.ExecuteScAction("start");
		await Utils.Delay1sAsync().ConfigureAwait(false);
		Utils.ExecuteScQuery();

		return;
	}

	if (args is ["status"]) {
		Utils.ExecuteScQuery();

		return;
	}
}

var builder = Host.CreateApplicationBuilder(args);
builder.Services
	.AddWindowsService(options => options.ServiceName = Utils.ServiceName)
	.AddHostedService<WindowsBackgroundService>();

builder.Build().Run();

return;