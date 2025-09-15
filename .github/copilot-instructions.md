```markdown
# HL7 Results Gateway – AI Working Guide (Concise)

## 1. Architecture (Clean, Layered)

Domain → Application (CQRS) → API (Azure Functions Isolated) → Client (Blazor WASM). Infrastructure implements external concerns (logging, converters) behind Domain interfaces. Never reference outward (no circular deps). Core directories:

- Domain: Entities (`HL7Result`, `Patient`), ValueObjects (`HL7MessageType`), Parser (`HL7MessageParser`), Exceptions (`HL7ParseException`).
- Application: Use case commands/handlers (`ProcessHL7MessageCommand` + handler) returning result DTOs.
- Infrastructure: Implementations (e.g. `JsonHL7Converter`), `ILoggingService` (Serilog default).
- API: Azure Function endpoints (`HealthCheck`, `ProcessHL7Message`, `ConvertJsonToHL7`).
- Client: Blazor feature‑based under `src/Client/Features/*` with required `{Feature}.razor.md` + `Extensions/*ServiceExtensions.cs`.

## 2. Critical Patterns

Dependency Injection: Register per layer in respective `Program.cs`; features add a single `builder.Services.Add{Feature}Services()` line. Logging always via `ILoggingService`. Parser access through `IHL7MessageParser` only.
Testing: All tests under `/tests` mirroring structure; use xUnit + FluentAssertions (`result.Success.Should().BeTrue();`). No `Assert.*`. One test project per layer.
HTTP Clients (Client): Named `AzureFunctionsApi` plus optional per‑feature named clients.
Component Isolation: Each component may have `.razor`, `.razor.cs`, `.razor.css`, optional `.razor.js` (scoped). Scoped CSS mandatory for local styles.

## 3. Typical HL7 Flow

Raw HL7 (POST `/api/hl7/process`) → API Function → Application handler → Domain parser → Domain result → API response → Client feature renders parsed segments. JSON conversion path: `/api/convert-json-to-hl7` uses Infrastructure converter.

## 4. Adding a Feature (Backend + Frontend)

1. Domain: Add entity/value object if truly domain (avoid anemic duplication).
2. Application: Create command/handler returning rich result object.
3. Infrastructure: Implement required interface / adapter.
4. API: Add Function (AuthorizationLevel.Function unless health). Bind request body (plain text for HL7, JSON where appropriate) and map to command.
5. Client: Create `Features/{Name}/` with `Models|Services|Components|Pages|Extensions` + `{Name}.razor.md` + DI extension.
6. Register DI (API + Client) with one liner.
7. Tests: Domain parser/logic first, then handler, then endpoint (.http file), then Client (if UI logic).

## 5. Key Files (Exemplars)

`ProcessHL7Message.cs` – Function shape & DI usage.
`HL7MessageParserTests.cs` – Parser test style.
`ConvertJsonToHL7Tests.cs` – End‑to‑end style for converter.
`Program.cs` (API & Client) – DI registration conventions.

## 6. Commands & Scripts

Build: `dotnet build` | Tests: `dotnet test`
Run Functions: `cd src/HL7ResultsGateway.API && func start`
Run Client: `cd src/Client && dotnet run`
Clean/Rebuild (fix file locks): `./scripts/clean-build.ps1`
HTTP Smoke: use `.http` files in API project (VS Code REST client or Thunder Client).

## 7. Conventions & Style

Target: .NET 9. File‑scoped namespaces, explicit access modifiers, nullable enabled. Use value objects over primitives when domain meaning matters. FluentAssertions only. One public DI extension per feature. Keep feature registration flat and explicit (no reflection scanning).

## 8. Error & Logging

Return 400 for parse/validation failures, 500 for unexpected. Include timestamp + message. Log through `ILoggingService`; never instantiate Serilog directly outside Infrastructure. Avoid leaking raw HL7 on error—log correlation details only.

## 9. Common Gotchas

Do NOT upgrade Functions to unsupported runtime (.NET >9). Ignore duplicate `WorkerExtensions` warnings. Always create documentation file + DI extension or feature PR is incomplete. Keep Azure Functions started from its project directory (path-sensitive). Parser changes require updating related tests across Domain + Application + API.

## 10. Minimal Examples

Assertion: `parsed.MessageType.Should().Be(HL7MessageType.ORU_R01);`
Feature DI Extension skeleton: see any existing `Extensions/*ServiceExtensions.cs` and replicate naming.
Endpoint test (health): `GET http://localhost:7071/api/health` expecting JSON with `status` & `timestamp`.

## 11. When Unsure

Trace from API inward (Function → Handler → Parser). Confirm DI registrations exist in both `Program.cs` files. Add tests before implementation (parser / handler). Keep instructions updated if introducing a new cross‑cutting concern.
```
