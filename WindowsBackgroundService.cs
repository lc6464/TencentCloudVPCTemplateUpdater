﻿using System.Net;
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
			var mode = configuration.GetValue<int>("TemplateIds:Mode");
			if (!new int[] { -1, 0, 1, 2, 3, 4, 5, 6 }.Contains(mode)) {
				logger.LogError("服务配置的运行模式有误，请修改配置文件后再次运行！");
				Environment.Exit(-1); // skipcq: CS-W1005 模式有误，直接退出
			}

			if (mode == -1) {
				logger.LogError("服务在配置文件中被禁用，请修改配置文件后再次运行！");
				Environment.Exit(-1); // skipcq: CS-W1005 服务被禁用，直接退出
			}

			Credential credential = new() {
				SecretId = configuration.GetValue<string>("Secret:Id"),
				SecretKey = configuration.GetValue<string>("Secret:Key")
			};

			VpcClient client = new(credential, configuration.GetValue<string>("Region"));

			ModifyAddressTemplateAttributeRequest requestV4 = new() {
				AddressTemplateId = configuration.GetValue<string>("TemplateIds:V4")
			}, requestV6 = new() {
				AddressTemplateId = configuration.GetValue<string>("TemplateIds:V6")
			}, requestSingle = new() {
				AddressTemplateId = configuration.GetValue<string>("TemplateIds:Single")
			};

			while (!stoppingToken.IsCancellationRequested) {
				try {
					string? address = null,
						family = null;

					try {
						var addressResult = await httpClient.GetFromJsonAsync<IP>("", stoppingToken).ConfigureAwait(false);
						family = addressResult.Family;

						if (family == "IPv4") {
							requestV4.Addresses = [addressResult.Address];
							requestV6.Addresses = ["::1"];
							requestSingle.Addresses = requestV4.Addresses;
						} else if (family == "IPv6") {
							requestV4.Addresses = ["127.0.0.1"];
							requestV6.Addresses = [addressResult.Address];
							requestSingle.Addresses = requestV6.Addresses;
						} else {
							logger.LogError("获取到的 IP 地址 {Address} 协议为 {Family}，目前暂不支持不支持，五分钟后重试。", addressResult.Address, family);

							await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

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

					ModifyAddressTemplateAttributeResponse? responseV4 = null,
						responseV6 = null,
						responseSingle = null;

					switch (mode) {
						case 0: // 单模板模式
							responseSingle = await client.ModifyAddressTemplateAttribute(requestSingle).ConfigureAwait(false);
							break;
						case 1: // 双模板模式，只更新获取到的地址
							if (family == "IPv4") {
								responseV4 = await client.ModifyAddressTemplateAttribute(requestV4).ConfigureAwait(false);
							} else {
								responseV6 = await client.ModifyAddressTemplateAttribute(requestV6).ConfigureAwait(false);
							}
							break;
						case 2: // 双模板模式，将未获取到的地址设置为环回地址
							responseV4 = await client.ModifyAddressTemplateAttribute(requestV4).ConfigureAwait(false);
							responseV6 = await client.ModifyAddressTemplateAttribute(requestV6).ConfigureAwait(false);
							break;
						case 3: // IPv6 only 模式，不修改 IPv4 地址；若接收到 IPv4 地址则跳过，十分钟后重试
							if (family == "IPv4") {
								logger.LogWarning("获取到的 IP 地址 {Address} 协议为 IPv4，目前设置了 IPv6 only 模式，十分钟后重试。", address);

								await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

								continue;
							}
							responseV6 = await client.ModifyAddressTemplateAttribute(requestV6).ConfigureAwait(false);
							break;
						case 4: // IPv6 only 模式，将 IPv4 地址设为环回地址；若接收到 IPv4 地址则跳过，十分钟后重试
							if (family == "IPv4") {
								logger.LogWarning("获取到的 IP 地址 {Address} 协议为 IPv4，目前设置了 IPv6 only 模式，十分钟后重试。", address);

								await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

								continue;
							}
							responseV4 = await client.ModifyAddressTemplateAttribute(requestV4).ConfigureAwait(false);
							responseV6 = await client.ModifyAddressTemplateAttribute(requestV6).ConfigureAwait(false);
							break;
						case 5: // IPv4 only 模式，不修改 IPv6 地址；若接收到 IPv6 地址则跳过，十分钟后重试
							if (family == "IPv6") {
								logger.LogWarning("获取到的 IP 地址 {Address} 协议为 IPv6，目前设置了 IPv4 only 模式，十分钟后重试。", address);

								await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

								continue;
							}
							responseV4 = await client.ModifyAddressTemplateAttribute(requestV4).ConfigureAwait(false);
							break;
						case 6: // IPv4 only 模式，将 IPv6 地址设为环回地址；若接收到 IPv6 地址则跳过，十分钟后重试
							if (family == "IPv6") {
								logger.LogWarning("获取到的 IP 地址 {Address} 协议为 IPv6，目前设置了 IPv4 only 模式，十分钟后重试。", address);

								await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

								continue;
							}
							responseV4 = await client.ModifyAddressTemplateAttribute(requestV4).ConfigureAwait(false);
							responseV6 = await client.ModifyAddressTemplateAttribute(requestV6).ConfigureAwait(false);
							break;
					}

					lastAddress = address;

					logger.LogInformation("Single 地址已更新为 {AddressSingle}，V6 地址已更新为 {AddressV6}，V4 地址已更新为 {AddressV4}，" +
						"Single 请求 ID 为 {RequestIdSingle}，V6 请求 ID 为 {RequestIdV6}，V4 请求 ID 为 {RequestIdV4}，十分钟后继续。",
						responseSingle is null ? "未更改" : requestSingle.Addresses[0], responseV6 is null ? "未更改" : requestV6.Addresses[0],
						responseV4 is null ? "未更改" : requestV4.Addresses[0], responseSingle?.RequestId ?? "未请求",
						responseV6?.RequestId ?? "未请求", responseV4?.RequestId ?? "未请求");
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
			Environment.Exit(1); // skipcq: CS-W1005 具体原因见上文注释
		}
	}
}

public readonly struct IP {
	public required string Address { get; init; }

	public required string Family { get; init; }
}