---
Last verified: 2026-08-03
---

# Architecture

This document describes how named-pipe requests are processed within **HostManagement**, including the service layer, processing logic, script execution and system configuration persistence.

## Pipe Request Processing

### Simple Request

A simple request, e.g. getting network status information, involves the corresponding service, script execution and the persistence layer.

The general flow is shown below. Arrows indicate the invocation direction. Results are returned to the respective caller in the opposite direction.

- `RequestHandler` receives a named-pipe request, delegates it to a service and returns a serialized `OperationResult`.
- `Service` executes logic by calling scripts via `ScriptHandler` and returns `OperationResult`.
- `ScriptHandler` requests script execution from `ScriptRunner` and returns `Result` and `Data?`.
- `ScriptRunner` executes script with argument(s) and returns `ScriptResponse`.
- `Script` reads system files or executes tools and returns `StdOut`, `StdErr` and `Exit code`.

```mermaid

flowchart LR

  subgraph 1 ["Service layer"]
  RequestHandler --> Service
  end

  Service --> ScriptHandler

  subgraph 2 ["Script layer"]
  ScriptHandler --> ScriptRunner
  ScriptRunner --> Script
  end

  Script --> Persistence

  subgraph 3 ["Persistence layer"]
  Persistence
  end

  linkStyle default stroke:#000
  style 1 fill:#fff6d9
  style 2 fill:#ecf6ec
  style 3 fill:#e3f6f6

```

<details>
  <summary markdown="span">

  `Detailed Flowchart`

  </summary>

```mermaid

sequenceDiagram

    autonumber

    box rgb(255, 246, 217) Service layer
    participant A as RequestHandler
    participant B as Service
    end

    box rgb(236, 246, 236) Script layer
    participant C as ScriptHandler
    participant D as ScriptRunner
    participant E as Script
    end

    box rgb(227, 246, 246) Persistence layer
    participant F as Persistence
    end

    A->>B: Request
    B->>C: Request<br>script execution
    C->>D: Request<br>script execution
    D->>E: Execute script
    E->>F: Read system files<br>Execute tools

    F->>E: System values
    E->>D: StdOut<br>StdErr<br>Exit code
    D->>C: ScriptResponse
    C->>B: Result<br>Data?
    B->>A: OperationResult

```

</details>

### Complex Request (Get/Set Configuration)

For the complex pipe request to **get/set** the **system configuration**, an additional layer is involved between the corresponding `Service` and the `ScriptHandler`.\
This layer is based on `ConfigurationAdapter` and can call multiple **sub adapters** for specific logic, e.g. DNS, Proxy settings.

#### Get system configuration

```mermaid

flowchart LR

  subgraph 1 ["Service layer"]
  RequestHandler --> Service
  end

  Service --> Adapter

  subgraph 2 ["Processing layer"]
  Adapter --> SubAdapter["Sub adapter"]
  end

  SubAdapter --> ScriptHandler

  subgraph 3 ["Script layer"]
  ScriptHandler --> ScriptRunner
  ScriptRunner --> Script
  end

  Script --> Persistence

  subgraph 4 ["Persistence layer"]
  Persistence
  end

  linkStyle default stroke:#000
  style 1 fill:#fff6d9
  style 2 fill:#f6f6e3
  style 3 fill:#ecf6ec
  style 4 fill:#e3f6f6

```

> [!note]
> If the system configuration is retrieved, it is cached (default: 30 seconds). If it is requested within this time again, only the service layer gets involved and returns the cached configuration.

#### Set system configuration

```mermaid

flowchart LR

  subgraph 1 ["Service layer"]
  RequestHandler --> Service
  end

  Service --> Adapter

  subgraph 2 ["Processing layer"]
  Adapter --> SubAdapter["Sub adapter"]
  end

  SubAdapter --> ScriptHandler

  subgraph 3 ["Script layer"]
  ScriptHandler --> ScriptRunner
  ScriptRunner --> Script
  end

  Script --> Persistence
  Service -->|Create or update<br/>SystemConfiguration.json| Persistence

  subgraph 4 ["Persistence layer"]
  Persistence
  end

  linkStyle default stroke:#000
  style 1 fill:#fff6d9
  style 2 fill:#f6f6e3
  style 3 fill:#ecf6ec
  style 4 fill:#e3f6f6

```

> [!note]
> Every time the system configuration is changed, a still cached configuration is invalidated.
