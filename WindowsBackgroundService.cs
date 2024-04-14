using System.Net;
using System.Net.Http.Json;
using TencentCloud.Common;
using TencentCloud.Vpc.V20170312;
using TencentCloud.Vpc.V20170312.Models;

namespace TencentCloudVPCTemplateUpdater;

public sealed class WindowsBackgroundService(ILogger<WindowsBackgroundService> logger, IConfiguration configuration) : BackgroundService {
	private string? lastAddress = null;
	private readonly HttpClient httpClient = new() {
		BaseAddress = new("https://api.lcwebsite.cn/GetIP"),
		Timeout = TimeSpan.FromSeconds(5),
		DefaultRequestVersion = HttpVersion.Version30,
		DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
	};

	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		logger.LogDebug("服务已启动。");

		try {
			Credential credential = new() {
				SecretId = configuration.GetValue<string>("Secret:Id"),
				SecretKey = configuration.GetValue<string>("Secret:Key")
			};

			VpcClient client = new(credential, configuration.GetValue<string>("Region"));

			ModifyAddressTemplateAttributeRequest request = new() {
				AddressTemplateId = configuration.GetValue<string>("TemplateId")
			};

			while (!stoppingToken.IsCancellationRequested) {
				try {
					string? address = null;

					try {
						var addressResult = await httpClient.GetFromJsonAsync<IP>("", stoppingToken).ConfigureAwait(false);

						if (addressResult.Family != "IPv4") {
							logger.LogInformation("检测到不是使用 IPv4 连接，可能是使用了 IPv6，无需更新 IPv4 地址，已跳过，三十分钟后继续。");

							await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);

							continue;
						}

						address = addressResult.Address;
					} catch (Exception e) {
						logger.LogError(e, "获取 IP 地址时出现异常，五分钟后重试。");

						await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

						continue;
					}

					if (address is null) {
						logger.LogError("获取 IP 地址失败，地址为空，五分钟后重试。");

						await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

						continue;
					}

					if (address == lastAddress) {
						logger.LogInformation("IP 地址未发生变化，已跳过，十分钟后继续。");

						await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

						continue;
					}

					request.Addresses = [address];

					var response = await client.ModifyAddressTemplateAttribute(request).ConfigureAwait(false);

					lastAddress = address;

					logger.LogInformation("IP 地址已更新为 {Address}，请求 ID 为 {RequestId}，十分钟后继续。", address, response.RequestId);
				} catch (TencentCloudSDKException e) {
					logger.LogError(e, "更新 IP 地址时出现腾讯云 SDK 异常，请求 ID 为 {RequestId}，五分钟后重试。", e.RequestId);

					await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

					continue;

				} catch (Exception e) {
					logger.LogError(e, "更新 IP 地址时出现异常，五分钟后重试。");

					await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

					continue;
				}

				await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
			}
		} catch (OperationCanceledException) {
			// When the stopping token is canceled, for example, a call made from services.msc,
			// we shouldn't exit with a non-zero exit code. In other words, this is expected...

			logger.LogDebug("服务已停止。");
		} catch (Exception ex) {
			logger.LogError(ex, "服务发生异常：{Message}", ex.Message);

			// Terminates this process and returns an exit code to the operating system.
			// This is required to avoid the 'BackgroundServiceExceptionBehavior', which
			// performs one of two scenarios:
			// 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
			// 2. When set to "StopHost": will cleanly stop the host, and log errors.
			//
			// In order for the Windows Service Management system to leverage configured
			// recovery options, we need to terminate the process with a non-zero exit code.
			Environment.Exit(1);
		}
	}
}

public readonly struct IP {
	public required string Address { get; init; }

	public required string Family { get; init; }
}