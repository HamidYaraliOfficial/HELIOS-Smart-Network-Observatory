# HELIOS Solution Map

| Project | Responsibility |
|---|---|
| Helios.Core | Domain models, enums, and cross-cutting interfaces shared by every layer. |
| Helios.Networking | Real interface enumeration, ARP cache reads, ICMP subnet sweeps, Wi-Fi observatory - all via official .NET/Windows APIs. |
| Helios.Discovery | Orchestrates a discovery run and merges results into the live topology graph. |
| Helios.Topology | The graph engine, layout algorithms (force-directed, hierarchical, radial, grid), path analysis, dependency impact analysis, and snapshot diffing. |
| Helios.Graph | UI-facing graph view state: current view kind, selection, Focus Mode, and the Traffic/Health/Dependency/Incident lenses. |
| Helios.Traffic | Aggregates flow metadata into time-bucketed series for the Real-Time Traffic Graph. |
| Helios.Telemetry | Flow ingestion buffer, protocol statistics, traffic pattern analysis, baseline engine, anomaly engine. |
| Helios.Events | Event correlation engine and the Network Timeline store. |
| Helios.Alerts | Alert Center (with deduplication), Incident Workspace, Notification Service. |
| Helios.Rules | Threshold/condition Rule Engine, Monitoring Schedule ("open hours") evaluator, Maintenance Mode. |
| Helios.Analytics | Health Engine, Subnet Explorer, Availability/SLA Estimator. |
| Helios.Storage | EF Core + SQLite persistence and the Retention Manager. |
| Helios.Search | SQLite FTS5-backed Global Search / Command Palette index. |
| Helios.Reports | Markdown/JSON/CSV/PDF report generation and the Export Studio. |
| Helios.AI | Pluggable local (ONNX) and cloud AI provider interfaces plus the grounded Network Analyst service. |
| Helios.Security | DPAPI-backed Credential Vault, Audit Log, Security Center. |
| Helios.Agents | Windows Worker Service for future multi-host remote monitoring. |
| Helios.Simulation | Fully isolated "what-if" Scenario Simulator, always labeled SIMULATION. |
| Helios.Integrations | Plugin host (isolated AssemblyLoadContext) and Connector SDK. |
| Helios.Infrastructure | Background Job Center (Channels-based), event bus, self-metrics monitor. |
| Helios.Server | Optional ASP.NET Core Central Observatory Server for multi-agent deployments. |
| Helios.UI | The WinUI 3 desktop shell: canvas, panels, themes, localization, settings. |
| Helios.Tests | xUnit tests covering discovery, topology, rules, health, anomalies, storage, and simulation. |
