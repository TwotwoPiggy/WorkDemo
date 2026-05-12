# WorkDemo

个人技术想法 Demo 仓库，包含各种前后端技术方案的实践代码。

---

## OcrDemo - OCR JSON 存储方案

基于 .NET 8 + ASP.NET Core + EF Core + MySQL + Azure Blob + Vue 3 + TypeScript 的 OCR 数据处理与存储方案。

---

## 系统架构图

```mermaid
graph TB
    subgraph Frontend["前端 (Vue 3 + TypeScript)"]
        A[OcrEditorView<br/>编辑器页面]
        B[DynamicForm<br/>动态表单]
        C[StringField<br/>字符串字段]
        D[ArrayField<br/>数组字段]
        E[useOcrEditor<br/>Composable 逻辑层]
        F[ocr.ts<br/>API 请求层]

        A --> B
        B --> C
        B --> D
        A --> E
        E --> F
    end

    subgraph Backend["后端 (.NET 8 Web API)"]
        G[OcrController<br/>API 控制器]
        H[OcrStorageService<br/>核心业务服务]
        I[AppDbContext<br/>EF Core 数据上下文]
        J[IBlobService<br/>Blob 存储接口]

        G --> H
        H --> I
        H --> J
    end

    subgraph Storage["存储层"]
        K[(MySQL<br/>元数据)]
        L[Azure Blob<br/>JSON 文件]
        M[本地文件<br/>开发模式]

        J --> L
        J --> M
        I --> K
    end

    F -->|HTTP REST API| G

    style Frontend fill:#e6f7ff,stroke:#1890ff
    style Backend fill:#f6ffed,stroke:#52c41a
    style Storage fill:#fff7e6,stroke:#fa8c16
```

---

## 数据流全景图

```mermaid
flowchart LR
    subgraph OCR["OCR 平台"]
        OCR_API[OCR WebAPI]
    end

    subgraph Server["后端处理"]
        RECEIVE[接收原始 JSON]
        SAVE_RAW[保存原始 JSON 到 Blob]
        DESERIALIZE[反序列化 → 业务对象]
        PROCESS[业务处理]
        SERIALIZE[序列化 → JSON]
        SAVE_RESULT[保存处理结果到 Blob]
    end

    subgraph Frontend_Edit["前端编辑"]
        LOAD[加载 JSON → 对象]
        FORM[动态表单编辑]
        STRINGIFY[对象 → JSON]
        SEND[发送更新请求]
    end

    subgraph Concurrency["并发控制"]
        LOCK[乐观锁版本检查]
        CONFLICT{版本冲突?}
        HISTORY[历史版本记录]
    end

    OCR_API -->|HTTP 响应 JSON| RECEIVE
    RECEIVE --> SAVE_RAW
    RECEIVE --> DESERIALIZE
    DESERIALIZE --> PROCESS
    PROCESS --> SERIALIZE
    SERIALIZE --> SAVE_RESULT

    SAVE_RESULT --> LOAD
    LOAD --> FORM
    FORM --> STRINGIFY
    STRINGIFY --> SEND
    SEND --> LOCK
    LOCK --> CONFLICT
    CONFLICT -->|否| SAVE_RESULT
    CONFLICT -->|否| HISTORY
    CONFLICT -->|是 409| LOAD
```

---

## 存储架构详解

```mermaid
graph TB
    subgraph MySQL["MySQL 数据库"]
        TASK["ocr_tasks 表<br/>────────────<br/>id: BIGINT<br/>raw_json_blob_path: VARCHAR(500)<br/>result_json_blob_path: VARCHAR(500)<br/>status: VARCHAR(50)<br/>version: INT<br/>created_at: DATETIME<br/>updated_at: DATETIME"]
        HISTORY_T["ocr_task_histories 表<br/>────────────<br/>id: BIGINT<br/>task_id: BIGINT<br/>version: INT<br/>blob_path: VARCHAR(500)<br/>modified_by: VARCHAR(100)<br/>modified_at: DATETIME"]
    end

    subgraph Blob["Azure Blob Storage / 本地文件"]
        RAW["raw/{id}_raw.json<br/>原始 OCR 响应"]
        V1["result/{id}_v1.json<br/>处理结果 v1"]
        V2["result/{id}_v2.json<br/>处理结果 v2"]
        V3["result/{id}_v3.json<br/>处理结果 v3"]
    end

    TASK -.->|raw_json_blob_path| RAW
    TASK -.->|result_json_blob_path| V3
    HISTORY_T -.->|blob_path| V1
    HISTORY_T -.->|blob_path| V2
    HISTORY_T -.->|blob_path| V3

    style MySQL fill:#e6f7ff,stroke:#1890ff
    style Blob fill:#fff7e6,stroke:#fa8c16
```

**设计原则：**

| 存储位置 | 存储内容 | 作用 |
|---------|---------|------|
| MySQL | 元数据（路径、状态、版本号） | 快速查询、索引、状态管理 |
| Azure Blob | 完整 JSON 文件 | 大文件存储、版本管理、高性能读写 |
| 本地文件 | 开发模式替代 Blob | 无需 Azure 账号即可本地开发 |

---

## 请求处理流程

```mermaid
sequenceDiagram
    participant User as 用户
    participant Vue as Vue 前端
    participant API as .NET API
    participant DB as MySQL
    participant Blob as Azure Blob

    Note over User,Blob: ① 创建 OCR 任务
    User->>Vue: 提交原始 JSON
    Vue->>API: POST /api/ocr { rawJson }
    API->>DB: INSERT ocr_tasks
    API->>Blob: 上传 raw/{id}_raw.json
    API->>DB: UPDATE blob_path + version
    API-->>Vue: 返回 task id + status
    Vue-->>User: 显示创建成功

    Note over User,Blob: ② 加载编辑
    User->>Vue: 打开编辑器
    Vue->>API: GET /api/ocr/{id}
    API->>DB: SELECT ocr_tasks WHERE id
    API->>Blob: 下载 result/{id}_vN.json
    API-->>Vue: { data, version }
    Vue->>Vue: JSON.parse → JS 对象
    Vue-->>User: 渲染动态表单

    Note over User,Blob: ③ 编辑保存 (防抖 500ms)
    User->>Vue: 修改字段
    Vue->>Vue: 更新本地对象
    Vue->>Vue: 防抖等待 500ms
    Vue->>API: PUT /api/ocr/{id} { data, version }
    API->>DB: 检查 version 是否匹配
    alt 版本匹配
        API->>Blob: 上传 result/{id}_vN+1.json
        API->>DB: INSERT ocr_task_histories
        API->>DB: UPDATE ocr_tasks version+1
        API-->>Vue: 200 { version: N+1 }
        Vue-->>User: 显示 "已保存"
    else 版本冲突
        API-->>Vue: 409 Conflict
        Vue-->>User: 提示 "数据已被修改，请刷新"
    end
```

---

## 乐观锁并发控制机制

```mermaid
flowchart TD
    A[用户A 加载数据<br/>version = 3] --> B[用户A 编辑...]
    C[用户B 加载数据<br/>version = 3] --> D[用户B 编辑...]

    D --> E[用户B 先保存<br/>PUT { version: 3 }]
    E --> F{version 匹配?}
    F -->|是 3 == 3| G[保存成功<br/>version → 4]

    B --> H[用户A 后保存<br/>PUT { version: 3 }]
    H --> I{version 匹配?}
    I -->|否 3 != 4| J[409 Conflict<br/>提示刷新]

    G --> K[用户A 刷新页面]
    K --> L[获取最新数据<br/>version = 4]
    L --> M[重新编辑保存]
```

---

## 前端组件架构

```mermaid
graph TB
    subgraph Page["OcrEditorView.vue 页面"]
        TOOLBAR["工具栏<br/>版本号 + 手动保存"]
        FORM["DynamicForm.vue<br/>动态表单"]
        PREVIEW["JSON 预览<br/>实时显示当前数据"]
    end

    subgraph Composable["useOcrEditor.ts 逻辑层"]
        STATE["响应式状态<br/>data / version / loading / saving / error"]
        LOAD_FN["load() 加载"]
        SAVE_FN["save() 保存"]
        DEBOUNCE["debouncedSave() 防抖"]
        UPDATE["updateData() 更新"]
    end

    subgraph Components["表单组件"]
        DYNAMIC["DynamicForm<br/>遍历 Schema 渲染"]
        STRING["StringField<br/>v-model 双向绑定"]
        ARRAY["ArrayField<br/>增删改数组项"]
    end

    subgraph API_Layer["api/ocr.ts API 层"]
        FETCH_GET["fetchOcr(id)"]
        FETCH_PUT["updateOcr(id, request)"]
    end

    subgraph Types["types/ocr.ts 类型定义"]
        SCHEMA["FORM_SCHEMA<br/>表单结构定义"]
        INTERFACE["OcrData / OcrItem<br/>TS 接口"]
    end

    Page --> Composable
    Page --> Components
    Composable --> API_Layer
    Components --> Types
    DYNAMIC --> STRING
    DYNAMIC --> ARRAY
```

---

## 项目结构

```
WorkDemo/
├── README.md
├── .gitignore
├── backend/                          # .NET 8 Web API
│   ├── Models/
│   │   ├── OcrTask.cs                # 任务实体（含乐观锁版本号）
│   │   └── OcrTaskHistory.cs         # 历史版本实体
│   ├── Data/
│   │   └── AppDbContext.cs           # EF Core DbContext
│   ├── Services/
│   │   ├── IBlobService.cs           # Blob 存储接口
│   │   ├── AzureBlobService.cs       # Azure Blob 实现
│   │   ├── LocalFileBlobService.cs   # 本地文件存储（开发用）
│   │   └── OcrStorageService.cs      # 核心业务逻辑
│   ├── Controllers/
│   │   └── OcrController.cs          # REST API 控制器
│   ├── Program.cs                    # 启动 + DI + Swagger 配置
│   └── appsettings.json              # 数据库 / Blob 配置
└── frontend/                         # Vue 3 + TypeScript
    └── src/
        ├── types/ocr.ts              # 类型定义 + 表单 Schema 配置
        ├── api/ocr.ts                # HTTP API 请求封装
        ├── composables/useOcrEditor.ts  # 编辑状态管理 Composable
        ├── components/
        │   ├── StringField.vue       # 字符串字段组件
        │   ├── ArrayField.vue        # 数组字段组件（增/删/改）
        │   └── DynamicForm.vue       # Schema 驱动的动态表单
        ├── views/
        │   └── OcrEditorView.vue     # 编辑器主页面
        ├── App.vue
        ├── main.ts                   # 入口 + Vue Router
        └── style.css
```

---

## API 接口

| 方法 | 路径 | 说明 | 请求体 | 响应 |
|------|------|------|--------|------|
| `POST` | `/api/ocr` | 创建任务 | `{ rawJson }` | `{ id, status, version }` |
| `GET` | `/api/ocr/{id}` | 获取任务 | - | `{ id, data, version, status }` |
| `PUT` | `/api/ocr/{id}` | 更新结果 | `{ data, version }` | `200 { version }` / `409` |
| `POST` | `/api/ocr/{id}/process` | 提交处理结果 | `{ result }` | `200` |

---

## 快速开始

### 前置条件

- .NET 8 SDK
- Node.js 18+
- MySQL 5.7+ (或 8.0+)

### 后端

```bash
cd backend

# 修改 appsettings.json 中的 MySQL 连接字符串
# "ConnectionStrings": {
#   "MySql": "Server=localhost;Port=3306;Database=ocr_demo;User=root;Password=your_password;"
# }

dotnet run
# 启动后访问 http://localhost:5084/swagger 查看 API 文档
```

### 前端

```bash
cd frontend
npm install
npm run dev
# 启动后访问 http://localhost:5173
```

### 存储模式切换

默认使用**本地文件存储**（无需 Azure 账号）：

```json
// appsettings.json - 本地模式（默认，ConnectionStrings.AzureBlob 留空即可）
{
  "ConnectionStrings": {
    "AzureBlob": ""
  }
}
```

切换到 Azure Blob：

```json
{
  "ConnectionStrings": {
    "AzureBlob": "DefaultEndpointsProtocol=https;AccountName=xxx;AccountKey=xxx;EndpointSuffix=core.windows.net"
  }
}
```

---

## 技术栈

| 层级 | 技术 | 版本 |
|------|------|------|
| 后端框架 | ASP.NET Core | .NET 8 |
| ORM | EF Core + Pomelo.MySql | 8.0.x |
| 数据库 | MySQL | 5.7+ / 8.0+ |
| 文件存储 | Azure Blob / 本地文件 | - |
| API 文档 | Swashbuckle (Swagger) | 6.9 |
| 前端框架 | Vue 3 + TypeScript | 3.x |
| 构建工具 | Vite | 6.x |
| 路由 | Vue Router | 4.x |