# TencentCloudVPCTemplateUpdater
腾讯云私有网络参数模板 IP 地址自动更新器，支持服务，支持自动安装为 Windows 服务。

## How to use (for Windows)
1. Download the [latest release](https://github.com/lc6464/TencentCloudVPCTemplateUpdater/releases/latest) or clone and build it.
1. Edit the `appsettings.json` file, see [app settings](#app-settings) below.
1. Move the executable file to a directory which you want and will not be moved or deleted, for example `C:\Program Files\LC\TencentCloudVPCTemplateUpdater\`.
1. Put the `appsettings.json` file in the same directory as the executable file.
1. Open PowerShell or CMD as administrator in the directory, and run `.\TencentCloudVPCTemplateUpdater.exe install` to install the Windows service.

## How to use (for Linux)
1. Clone and build it.
1. Edit the `appsettings.json` file, see [app settings](#app-settings) below.
1. Move the executable file to a directory which you want and will not be moved or deleted, for example `/usr/local/bin/TencentCloudVPCTemplateUpdater`.
1. Put the `appsettings.json` file in the same directory as the executable file.
1. Create a service by yourself, and run the executable file in the service.

## Quick commands (only for Windows)
- `.\TencentCloudVPCTemplateUpdater.exe install` - Install the Windows service (Administrator permission **required**).
- `.\TencentCloudVPCTemplateUpdater.exe uninstall` - Uninstall the Windows service (Administrator permission **required**).
- `.\TencentCloudVPCTemplateUpdater.exe start` - Start the Windows service (Administrator permission **required**).
- `.\TencentCloudVPCTemplateUpdater.exe stop` - Stop the Windows service (Administrator permission **required**).
- `.\TencentCloudVPCTemplateUpdater.exe restart` - Restart the Windows service (Administrator permission **required**).
- `.\TencentCloudVPCTemplateUpdater.exe status` - Show the Windows service status (Administrator permission is **not** needed).

## app settings
| Key                | Value                      |
| ------------------ | -------------------------- |
| Secret:Id          | TencentCloud SecretId      |
| Secret:Key         | TencentCloud SecretKey     |
| Region             | TencentCloud Region        |
| TemplateIds:Mode   | Templates updating mode    |
| TemplateIds:Single | Template Id, single mode   |
| TemplateIds:V4     | TemplateV4 Id, multi mode  |
| TemplateIds:V6     | TemplateV6 Id, multi mode  |

## TemplateIds:Mode
| Mode | Description                                                 |
| ---- | ----------------------------------------------------------- |
| 0    | Single template, auto version (**Recommended**).            |
| 1    | Multiple templates, auto version, keep the other.           |
| 2    | Multiple templates, auto version, set the other loopback.   |
| 3    | Multiple templates, IPv6 only, keep IPv4.                   |
| 4    | Multiple templates, IPv6 only, set IPv4 loopback.           |
| 5    | Multiple templates, IPv4 only, keep IPv6.                   |
| 6    | Multiple templates, IPv4 only, set IPv6 loopback.           |
| -1   | Disable updater.                                            |