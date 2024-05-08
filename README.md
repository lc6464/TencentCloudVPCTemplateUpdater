# TencentCloudVPCTemplateUpdater
腾讯云私有网络参数模板 IP 地址自动更新器，支持服务，支持自动安装为 Windows 服务。

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