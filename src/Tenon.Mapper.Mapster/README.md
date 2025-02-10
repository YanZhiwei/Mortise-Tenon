# Tenon.Mapper.Mapster

基于 Mapster 的对象映射实现，提供了高性能的对象映射功能。

## 项目特点

- 基于 Mapster 实现
- 超高性能
- 支持依赖注入
- 支持编译时类型映射
- 统一的映射接口

## 快速开始

1. 安装 NuGet 包：
```bash
dotnet add package Tenon.Mapper.Mapster
```

2. 添加服务：
```csharp
// 使用程序集扫描配置
services.AddMapsterSetup(Assembly.GetExecutingAssembly());
```

3. 配置映射规则：
```csharp
public class MappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UserDto, User>()
            .Map(dest => dest.FullName, 
                 src => $"{src.FirstName} {src.LastName}");
    }
}
```

4. 使用映射服务：
```csharp
public class UserService
{
    private readonly IObjectMapper _mapper;

    public UserService(IObjectMapper mapper)
    {
        _mapper = mapper;
    }

    public User MapToUser(UserDto dto)
    {
        return _mapper.Map<UserDto, User>(dto);
    }

    public List<User> MapToUsers(List<UserDto> dtos)
    {
        return _mapper.Map<List<UserDto>, List<User>>(dtos);
    }
}
```

## 配置说明

### 基本配置

```csharp
public class BasicMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 基本映射
        config.NewConfig<Source, Destination>();

        // 自定义成员映射
        config.NewConfig<Source, Destination>()
            .Map(dest => dest.FullName, src => src.Name)
            .Ignore(dest => dest.Age);

        // 双向映射
        config.NewConfig<Source, Destination>()
            .TwoWays();
    }
}
```

### 高级配置

```csharp
public class AdvancedMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 条件映射
        config.NewConfig<Source, Destination>()
            .Map(dest => dest.Status, 
                 src => src.IsActive ? "Active" : "Inactive");

        // 集合映射
        config.NewConfig<Order, OrderDto>()
            .Map(dest => dest.Items, src => src.OrderItems);

        // 自定义转换
        config.NewConfig<Source, Destination>()
            .MapWith(src => new Destination 
            { 
                Date = ConvertDate(src.Date) 
            });
    }
}
```

## 依赖项

- Mapster
- Mapster.DependencyInjection
- Tenon.Mapper.Abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions

## 项目结构

```
Tenon.Mapper.Mapster/
├── Extensions/
│   └── ServiceCollectionExtension.cs  # 服务注册扩展
├── MapsterObject.cs                  # Mapster 实现
└── Tenon.Mapper.Mapster.csproj       # 项目文件
```

## 注意事项

1. 性能优势：
   - Mapster 比 AutoMapper 性能更好
   - 支持编译时生成映射代码
   - 适合高性能要求的场景

2. 最佳实践：
   - 使用 IRegister 接口组织映射配置
   - 利用编译时类型检查
   - 使用依赖注入管理映射器实例

3. 编译优化：
   - 使用 Mapster.Tool 生成编译时映射代码
   - 配置 Source Generator 提升性能
   - 利用 ValueTask 支持异步映射
