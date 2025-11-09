using System.Reflection;

namespace Framework.Mapper.Interfaces;

public interface IMapperConfigurator
{
    void RegisterMaps(Assembly assembly);
}