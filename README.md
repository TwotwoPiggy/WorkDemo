# WorkDemo

个人技术想法 Demo 仓库，包含各种前后端技术方案的实践代码。

## 当前项目

### OcrDemo - OCR JSON 存储方案 Demo

基于 .NET 9 + ASP.NET Core + EF Core + MySQL + Azure Blob + Vue 3 + TypeScript 的 OCR 数据处理与存储方案。

**核心特性：**
- MySQL 存储元数据 + Azure Blob 存储 JSON 文件（混合存储方案）
- 乐观锁版本控制，支持并发编辑冲突检测
- 防抖自动保存 + 手动保存
- 动态表单 + 序列化，支持 JSON 的增删改操作
- 支持 Azure Blob / 本地文件 两种存储模式切换
- 历史版本追踪

**项目结构：**
```
WorkDemo/
├── backend/          # .NET 9 Web API
│   ├── Models/       # 数据实体
│   ├── Data/         # EF Core DbContext
│   ├── Services/     # 业务服务
│   └── Controllers/  # API 控制器
└── frontend/         # Vue 3 + TypeScript
    ├── src/
    │   ├── types/        # 类型定义
    │   ├── api/          # API 调用
    │   ├── composables/  # 组合式逻辑
    │   ├── components/   # 通用组件
    │   └── views/        # 页面组件
    └── ...
```

## 快速开始

### 后端

```bash
cd backend
# 配置 appsettings.json 中的 MySQL 连接字符串
dotnet run
```

### 前端

```bash
cd frontend
npm install
npm run dev
```