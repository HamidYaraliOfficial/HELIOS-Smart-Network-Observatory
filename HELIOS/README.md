# HELIOS — Smart Network Observatory

**A Visual Network Intelligence, Topology Modeling, Traffic Analysis & Infrastructure Observability Platform for Windows.**

Built with **C# + .NET 10**, **WinUI 3 + Windows App SDK**, **SQLite + EF Core**, and a fully modular, production-grade solution architecture.

---

<div align="center">

🇬🇧 [English](#-english) &nbsp;|&nbsp; 🇮🇷 [فارسی](#-فارسی) &nbsp;|&nbsp; 🇨🇳 [中文](#-中文)

</div>

---

## 🇬🇧 English

### Overview

HELIOS is not just another ping-monitor. It is a full **Network Observatory**: a living, visual system that turns your authorized, controlled network into an interactive graph — devices, services, dependencies, traffic, events, health and history — all in one Fluent Design, Mica/Acrylic-powered desktop application.

HELIOS is designed strictly for **networks you own or are explicitly authorized to monitor**. It performs no exploitation, credential harvesting, or offensive scanning of any kind — discovery is limited to standard, low-risk techniques (ICMP sweeps, ARP cache reads, official Windows networking APIs) within a scope you explicitly select.

### ✨ Key Features

- **Interactive Topology Canvas** — zoom, pan, multi-select, group, focus, and auto-layout (Force-Directed, Hierarchical, Radial, Grid) across Physical / Logical / Service / Application / Dependency graph views.
- **Real Network Discovery Engine** — interface enumeration, scoped ICMP subnet sweeps, ARP resolution, and optional hostname resolution, all via official .NET/Windows APIs.
- **Traffic Observation Engine** — flow/metadata-level traffic visibility (source, destination, protocol, port, bytes, rate) without deep packet inspection by default.
- **Health, Rules & Alerting** — a real Health Engine with explainable states, a visual Rule Builder, deduplicated Alert Center, and an Incident Workspace.
- **Event Correlation & Anomaly Detection** — groups related events into explainable correlation clusters and reports *Observed Anomalies* / *Potential Issues* — never definitive attack claims — with full evidence.
- **Path Analysis & Dependency Impact** — shortest observed path between any two nodes, plus an Impact Estimate for what else might be affected if a component degrades.
- **Snapshots, Diff & Historical Playback** — version your topology, compare snapshots, and replay history on the canvas.
- **Baseline Engine** — statistical baselining with honest, sample-size-aware confidence levels (never a false sense of certainty).
- **Scenario Simulator** — a clearly labeled `SIMULATION` sandbox for "what-if" scenarios that never touches your live network.
- **AI Network Analyst (optional)** — a read-only assistant grounded in your live graph and metrics, with pluggable local (ONNX) or cloud providers you configure yourself.
- **Configurable Monitoring Schedule ("Open Hours")** — you define exactly which days and hours HELIOS should actively monitor each scope. The app then tells you, live, whether monitoring is **Active** or **Inactive** right now, and shows a running countdown to the next transition (e.g. *"Active — closes in 2h 14m"* or *"Inactive — opens in 6h 40m"*). Nothing is pre-filled — every window is entered by you.
- **Reports & Export Studio** — Markdown, JSON, CSV and PDF reports, always stamped with generation time, scope, and data source.
- **5 Built-in Themes** — Windows Default, Light, Dark, AMOLED Black, Red, and Blue — plus full **English / Persian (فارسی) / Chinese (中文)** localization with correct RTL/LTR mirroring for Persian.
- **Plugin System & Connector SDK** — isolated, permissioned plugin loading with automatic Safe Mode fallback.
- **Security-first design** — DPAPI-encrypted credential vault, full audit log, and a dedicated Security & Privacy Center.

### 🧱 Solution Structure

The solution is split into 23 focused projects (`Helios.Core`, `Helios.Networking`, `Helios.Discovery`, `Helios.Topology`, `Helios.Graph`, `Helios.Traffic`, `Helios.Telemetry`, `Helios.Events`, `Helios.Alerts`, `Helios.Rules`, `Helios.Analytics`, `Helios.Storage`, `Helios.Search`, `Helios.Reports`, `Helios.AI`, `Helios.Security`, `Helios.Agents`, `Helios.Simulation`, `Helios.Integrations`, `Helios.Infrastructure`, `Helios.Server`, `Helios.UI`, `Helios.Tests`). See `docs/ARCHITECTURE.md` for the full map.

### 🖥️ Requirements

- Windows 10 (19041+) or Windows 11
- Visual Studio 2022 (17.11+) with the **".NET Desktop Development"** and **"Windows App SDK C# Templates"** workloads
- **.NET 10 SDK**
- **Windows App SDK 1.7+**

### 🚀 Installation & First Run

1. Install the **.NET 10 SDK** from the official .NET website.
2. Open **Visual Studio Installer** and add the **Windows App SDK C# Templates** and **.NET Desktop Development** workloads.
3. Open `Helios.sln` in Visual Studio 2022.
4. Let NuGet restore all package references (`Microsoft.WindowsAppSDK`, `Microsoft.EntityFrameworkCore.Sqlite`, `QuestPDF`, `Microsoft.ML.OnnxRuntime`, `CommunityToolkit.Mvvm`, and others listed in each `.csproj`).
5. Set **Helios.UI** as the Startup Project.
6. Choose your target platform (`x64`, `x86`, or `ARM64`) from the platform selector.
7. Press **F5** to build and run.
8. On first launch, HELIOS creates its local SQLite database under `%LocalAppData%\Helios\helios.db`.
9. Open **Settings → Appearance** to pick your theme, **Settings → Language** to pick English / Persian / Chinese, and **Settings → Monitoring Schedule** to enter your own open-hours windows if you want scoped, scheduled monitoring instead of always-on.

### 📦 Command-line restore/build (optional)

```bash
dotnet restore Helios.sln
dotnet build Helios.sln -c Release
dotnet test tests/Helios.Tests/Helios.Tests.csproj
```

### ⚖️ Responsible Use

HELIOS is a defensive observability and documentation tool. Only point it at networks and hosts you own or have explicit written authorization to monitor. It contains no exploitation, credential-harvesting, or offensive-scanning functionality, and every anomaly it reports is phrased as an observation or estimate, never a confirmed verdict.

---

## 🇮🇷 فارسی

### معرفی

هلیوس صرفاً یک ابزار پینگ‌گیری ساده نیست؛ یک **رصدخانه کامل شبکه** است: یک سیستم بصری و زنده که شبکه مجاز و تحت کنترل شما را به یک گراف تعاملی تبدیل می‌کند — دستگاه‌ها، سرویس‌ها، وابستگی‌ها، ترافیک، رویدادها، سلامت و تاریخچه، همه در یک برنامه دسکتاپ با طراحی Fluent و افکت‌های Mica/Acrylic.

هلیوس منحصراً برای **شبکه‌هایی که مالک آن هستید یا اجازه صریح برای مانیتورینگ آن‌ها را دارید** طراحی شده است. این نرم‌افزار هیچ‌گونه Exploitation، سرقت Credential یا اسکن تهاجمی انجام نمی‌دهد؛ کشف شبکه محدود به روش‌های استاندارد و کم‌خطر (ICMP، خواندن جدول ARP، APIهای رسمی ویندوز) در محدوده‌ای است که شما صراحتاً انتخاب می‌کنید.

### ✨ ویژگی‌های کلیدی

- **بوم توپولوژی تعاملی** — Zoom، Pan، انتخاب چندگانه، گروه‌بندی، Focus Mode و چیدمان خودکار (Force-Directed، Hierarchical، Radial، Grid) در Viewهای Physical، Logical، Service، Application و Dependency.
- **موتور کشف واقعی شبکه** — شمارش Interfaceها، Scan محدودشده ICMP، خواندن ARP و تشخیص Hostname، همگی از طریق APIهای رسمی .NET و ویندوز.
- **موتور مشاهده ترافیک** — دید سطح Metadata روی ترافیک (Source، Destination، Protocol، Port، حجم، نرخ) بدون Packet Capture عمیق به‌صورت پیش‌فرض.
- **سلامت، Ruleها و هشدارها** — موتور سلامت واقعی با دلیل قابل توضیح، Rule Builder بصری، مرکز هشدار با Deduplication، و فضای کاری Incident.
- **همبستگی رویداد و تشخیص Anomaly** — رویدادهای مرتبط را در خوشه‌های قابل‌توضیح گروه‌بندی می‌کند و آن‌ها را با عنوان *Observed Anomaly* یا *Potential Issue* نمایش می‌دهد — هرگز ادعای قطعی حمله نمی‌کند.
- **تحلیل مسیر و اثر وابستگی** — کوتاه‌ترین مسیر مشاهده‌شده بین دو گره، به‌همراه تخمین اینکه در صورت افت یک Component چه اجزای دیگری ممکن است تحت تأثیر قرار گیرند.
- **Snapshot، Diff و پخش تاریخی** — نسخه‌بندی توپولوژی، مقایسه Snapshotها و Replay تغییرات روی بوم.
- **موتور Baseline** — خط پایه آماری با سطح اطمینان صادقانه و وابسته به حجم داده (هرگز حس اطمینان کاذب ایجاد نمی‌کند).
- **شبیه‌ساز سناریو** — محیطی کاملاً مجزا و برچسب‌خورده با `SIMULATION` برای سناریوهای فرضی، بدون هیچ تأثیری روی شبکه واقعی.
- **تحلیل‌گر هوش مصنوعی شبکه (اختیاری)** — دستیاری فقط‌خواندنی که بر پایه گراف و متریک‌های زنده شما کار می‌کند، با Providerهای محلی (ONNX) یا Cloud قابل تنظیم توسط خودتان.
- **برنامه زمانی مانیتورینگ قابل تنظیم («ساعات باز»)** — شما دقیقاً مشخص می‌کنید هلیوس در چه روزها و ساعاتی باید هر محدوده را به‌صورت فعال مانیتور کند. برنامه سپس به‌صورت زنده نشان می‌دهد که مانیتورینگ در همین لحظه **فعال** است یا **غیرفعال**، و یک شمارش معکوس تا انتقال بعدی نمایش می‌دهد (مثلاً «فعال — تا ۲ ساعت و ۱۴ دقیقه دیگر بسته می‌شود» یا «غیرفعال — تا ۶ ساعت و ۴۰ دقیقه دیگر باز می‌شود»). هیچ مقداری از پیش پر نشده؛ هر بازه زمانی را خودتان وارد می‌کنید.
- **گزارش‌گیری و Export Studio** — گزارش‌های Markdown، JSON، CSV و PDF، همیشه با زمان تولید، محدوده و منبع داده مشخص.
- **۵ تم آماده** — پیش‌فرض ویندوز، روشن، تاریک، مشکی AMOLED، قرمز و آبی — به‌همراه بومی‌سازی کامل **انگلیسی / فارسی / چینی** با رعایت صحیح جهت راست‌چین برای فارسی.
- **سیستم Plugin و Connector SDK** — بارگذاری Plugin با محدودیت دسترسی و Isolation، همراه با Safe Mode خودکار در صورت خطا.
- **طراحی امنیت‌محور** — Vault رمزنگاری‌شده با DPAPI، Audit Log کامل، و مرکز اختصاصی امنیت و حریم خصوصی.

### 🧱 ساختار پروژه

راه‌حل شامل ۲۳ پروژه مجزا و تخصصی است (`Helios.Core`، `Helios.Networking`، `Helios.Discovery`، `Helios.Topology`، `Helios.Graph`، `Helios.Traffic`، `Helios.Telemetry`، `Helios.Events`، `Helios.Alerts`، `Helios.Rules`، `Helios.Analytics`، `Helios.Storage`، `Helios.Search`، `Helios.Reports`، `Helios.AI`، `Helios.Security`، `Helios.Agents`، `Helios.Simulation`، `Helios.Integrations`، `Helios.Infrastructure`، `Helios.Server`، `Helios.UI`، `Helios.Tests`). نقشه کامل در `docs/ARCHITECTURE.md` موجود است.

### 🖥️ پیش‌نیازها

- ویندوز ۱۰ (نسخه 19041 به بعد) یا ویندوز ۱۱
- ویژوال استودیو ۲۰۲۲ (نسخه 17.11 یا بالاتر) همراه با Workloadهای **".NET Desktop Development"** و **"Windows App SDK C# Templates"**
- **.NET 10 SDK**
- **Windows App SDK 1.7 یا بالاتر**

### 🚀 نصب و اجرای اولیه

۱. **.NET 10 SDK** را از سایت رسمی .NET نصب کنید.
۲. **Visual Studio Installer** را باز کرده و Workloadهای **Windows App SDK C# Templates** و **.NET Desktop Development** را اضافه کنید.
۳. فایل `Helios.sln` را در Visual Studio 2022 باز کنید.
۴. اجازه دهید NuGet تمام Packageهای مورد نیاز (`Microsoft.WindowsAppSDK`، `Microsoft.EntityFrameworkCore.Sqlite`، `QuestPDF`، `Microsoft.ML.OnnxRuntime`، `CommunityToolkit.Mvvm` و موارد دیگر ذکرشده در هر `.csproj`) را Restore کند.
۵. پروژه **Helios.UI** را به‌عنوان Startup Project تنظیم کنید.
۶. پلتفرم هدف (`x64`، `x86` یا `ARM64`) را از منوی انتخاب پلتفرم مشخص کنید.
۷. کلید **F5** را برای Build و اجرا بزنید.
۸. در اولین اجرا، هلیوس دیتابیس محلی SQLite خود را در مسیر `%LocalAppData%\Helios\helios.db` می‌سازد.
۹. از مسیر **Settings → Appearance** تم مورد نظر، از **Settings → Language** زبان انگلیسی/فارسی/چینی و از **Settings → Monitoring Schedule** بازه‌های زمانی مانیتورینگ دلخواه خود را وارد کنید (در صورت نیاز به مانیتورینگ زمان‌بندی‌شده به‌جای حالت همیشه‌فعال).

### 📦 Build و Restore از خط فرمان (اختیاری)

```bash
dotnet restore Helios.sln
dotnet build Helios.sln -c Release
dotnet test tests/Helios.Tests/Helios.Tests.csproj
```

### ⚖️ استفاده مسئولانه

هلیوس یک ابزار مستندسازی و مشاهده دفاعی است. آن را فقط روی شبکه‌ها و Hostهایی اجرا کنید که مالک آن‌ها هستید یا مجوز کتبی صریح برای مانیتورینگ آن‌ها دارید. این نرم‌افزار هیچ قابلیت Exploitation، سرقت Credential یا اسکن تهاجمی ندارد و هر Anomaly که گزارش می‌کند به‌صورت مشاهده یا تخمین بیان می‌شود، نه یک حکم قطعی.

---

## 🇨🇳 中文

### 概述

HELIOS 不只是一个简单的 Ping 监控工具，而是一套完整的**网络天文台（Network Observatory）**：一个鲜活的可视化系统，将您拥有或明确获得授权监控的网络转化为可交互的图谱——设备、服务、依赖关系、流量、事件、健康状态与历史记录，全部集中在一个采用 Fluent Design、Mica/Acrylic 效果的桌面应用中。

HELIOS 严格设计用于**您拥有或明确获得监控授权的网络**。它不包含任何漏洞利用、凭据窃取或攻击性扫描功能——网络发现仅限于标准、低风险的技术（ICMP 探测、读取 ARP 缓存、官方 Windows 网络 API），且范围完全由您明确选定。

### ✨ 核心功能

- **交互式拓扑画布** — 支持缩放、平移、多选、分组、聚焦以及自动布局（力导向、层次、放射状、网格），并可在物理 / 逻辑 / 服务 / 应用 / 依赖等多种图视图之间切换。
- **真实网络发现引擎** — 通过官方 .NET / Windows API 进行接口枚举、限定范围的 ICMP 子网扫描、ARP 解析以及可选的主机名解析。
- **流量观测引擎** — 提供元数据级别的流量可见性（源、目的地、协议、端口、字节数、速率），默认不进行深度包检测。
- **健康状态、规则与告警** — 具备可解释状态的真实健康引擎、可视化规则构建器、支持去重的告警中心，以及事件工作区。
- **事件关联与异常检测** — 将相关事件归并为可解释的关联组，并以「观察到的异常（Observed Anomaly）」或「潜在问题（Potential Issue）」的措辞呈现——绝不做出确定性的攻击结论。
- **路径分析与依赖影响分析** — 计算任意两个节点之间观测到的最短路径，并对某组件状态恶化时可能受影响的其他组件给出影响估算。
- **快照、差异对比与历史回放** — 对拓扑进行版本化管理，比较不同快照，并在画布上回放历史变化。
- **基线引擎** — 基于统计的基线建模，并根据样本量诚实地给出置信度，绝不制造虚假的确定感。
- **场景模拟器** — 一个明确标注为 `SIMULATION` 的完全隔离沙盒，用于「假设」场景推演，绝不影响真实网络。
- **AI 网络分析师（可选）** — 基于您实时图谱与指标数据的只读助手，支持您自行配置的本地（ONNX）或云端提供方。
- **可自定义的监控计划（"开放时间"）** — 您可以精确设定 HELIOS 应在哪些天、哪些时段主动监控每个范围。应用会实时告知当前监控状态是**活动中**还是**未激活**，并显示距离下一次状态切换的倒计时（例如「活动中 — 将在 2 小时 14 分钟后关闭」或「未激活 — 将在 6 小时 40 分钟后开放」）。所有内容均无预设值，每个时间段均由您自行输入。
- **报告与导出工作室** — 生成 Markdown、JSON、CSV 和 PDF 格式的报告，并始终标注生成时间、范围与数据来源。
- **5 种内置主题** — Windows 默认、浅色、深色、AMOLED 纯黑、红色与蓝色，并完整支持**英文 / 波斯语 / 中文**本地化，波斯语界面会正确呈现从右到左（RTL）的排版方向。
- **插件系统与连接器 SDK** — 支持隔离、带权限控制的插件加载，并在异常时自动回退至安全模式。
- **安全优先设计** — 基于 DPAPI 加密的凭据保险库、完整的审计日志，以及专属的安全与隐私中心。

### 🧱 解决方案结构

整个解决方案划分为 23 个职责清晰的项目（`Helios.Core`、`Helios.Networking`、`Helios.Discovery`、`Helios.Topology`、`Helios.Graph`、`Helios.Traffic`、`Helios.Telemetry`、`Helios.Events`、`Helios.Alerts`、`Helios.Rules`、`Helios.Analytics`、`Helios.Storage`、`Helios.Search`、`Helios.Reports`、`Helios.AI`、`Helios.Security`、`Helios.Agents`、`Helios.Simulation`、`Helios.Integrations`、`Helios.Infrastructure`、`Helios.Server`、`Helios.UI`、`Helios.Tests`）。完整架构图请参见 `docs/ARCHITECTURE.md`。

### 🖥️ 环境要求

- Windows 10（19041 及以上版本）或 Windows 11
- Visual Studio 2022（17.11 及以上版本），并安装 **".NET 桌面开发"** 与 **"Windows App SDK C# 模板"** 工作负载
- **.NET 10 SDK**
- **Windows App SDK 1.7 及以上版本**

### 🚀 安装与首次运行

1. 从 .NET 官方网站安装 **.NET 10 SDK**。
2. 打开 **Visual Studio Installer**，添加 **Windows App SDK C# 模板** 与 **.NET 桌面开发** 工作负载。
3. 在 Visual Studio 2022 中打开 `Helios.sln`。
4. 让 NuGet 还原所有包引用（`Microsoft.WindowsAppSDK`、`Microsoft.EntityFrameworkCore.Sqlite`、`QuestPDF`、`Microsoft.ML.OnnxRuntime`、`CommunityToolkit.Mvvm` 及各 `.csproj` 中列出的其他依赖）。
5. 将 **Helios.UI** 设置为启动项目。
6. 在平台选择器中选择目标平台（`x64`、`x86` 或 `ARM64`）。
7. 按 **F5** 进行生成并运行。
8. 首次启动时，HELIOS 会在 `%LocalAppData%\Helios\helios.db` 路径下创建本地 SQLite 数据库。
9. 在 **Settings → Appearance** 中选择您喜欢的主题，在 **Settings → Language** 中选择英文 / 波斯语 / 中文，并在 **Settings → Monitoring Schedule** 中输入您自己的开放时间段（如果您希望采用按计划监控而非全天候监控）。

### 📦 命令行还原与生成（可选）

```bash
dotnet restore Helios.sln
dotnet build Helios.sln -c Release
dotnet test tests/Helios.Tests/Helios.Tests.csproj
```

### ⚖️ 负责任使用声明

HELIOS 是一款用于防御性可观测性与网络文档化的工具。请仅将其用于您拥有或已获得明确书面授权进行监控的网络与主机。本软件不包含任何漏洞利用、凭据窃取或攻击性扫描功能，其报告的每一项异常均以观察或估算的方式表述，而非确定性结论。

