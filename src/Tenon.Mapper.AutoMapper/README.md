# Tenon.Mapper.AutoMapper

基于 AutoMapper 的对象映射实现，提供了简单且灵活的对象映射功能。

## 项目特点

- 基于 AutoMapper 实现
- 支持依赖注入
- 支持配置文件扫描
- 统一的映射接口

## 快速开始

1. 安装 NuGet 包：
```bash
dotnet add package Tenon.Mapper.AutoMapper
```

2. 添加服务：
```csharp
// 使用类型标记扫描配置
services.AddAutoMapperSetup(typeof(Startup));

// 或使用程序集扫描配置
services.AddAutoMapperSetup(Assembly.GetExecutingAssembly());
```

3. 创建映射配置：
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.FullName, 
                      opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
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
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // 基本映射
        CreateMap<Source, Destination>();

        // 自定义成员映射
        CreateMap<Source, Destination>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Age, opt => opt.Ignore());

        // 双向映射
        CreateMap<Source, Destination>().ReverseMap();
    }
}
```

### 高级配置

```csharp
public class AdvancedMappingProfile : Profile
{
    public AdvancedMappingProfile()
    {
        // 条件映射
        CreateMap<Source, Destination>()
            .ForMember(dest => dest.Status,
                      opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"));

        // 集合映射
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Items,
                      opt => opt.MapFrom(src => src.OrderItems));

        // 值转换器
        CreateMap<Source, Destination>()
            .ForMember(dest => dest.Date,
                      opt => opt.ConvertUsing(new DateTimeTypeConverter()));
    }
}
```

## 依赖项

- AutoMapper
- AutoMapper.Collection
- Tenon.Mapper.Abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions

## 项目结构

```
Tenon.Mapper.AutoMapper/
├── Extensions/
│   └── ServiceCollectionExtension.cs  # 服务注册扩展
├── AutoMapperObject.cs               # AutoMapper 实现
└── Tenon.Mapper.AutoMapper.csproj    # 项目文件
```

## 注意事项

1. 性能考虑：
   - AutoMapper 会在启动时编译映射
   - 建议在应用程序启动时注册所有映射

2. 最佳实践：
   - 将映射配置放在单独的 Profile 类中
   - 使用依赖注入管理映射器实例
   - 避免在运行时动态创建映射

3. 调试提示：
   - 启用 AutoMapper 的调试功能来诊断映射问题
   - 使用 `AssertConfigurationIsValid()` 验证映射配置
