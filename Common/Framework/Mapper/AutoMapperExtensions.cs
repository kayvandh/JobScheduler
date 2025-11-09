using Framework.Mapper.Interfaces;
using System.Reflection;

namespace Framework.Mapper
{
    public static class AutoMapperExtensions
    {
        public static void CreateMappingFromAssembly(this AutoMapper.Profile profile, Assembly assembly)
        {
            var types = assembly.GetExportedTypes();

            //----------
            var oneWayMap = types.Where(t => !t.IsAbstract && !t.IsInterface).SelectMany(t =>
                t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IOneWayMap<>))
                    .Select(
                        i =>
                            new
                            {
                                Source = t,
                                Destination = i.GetGenericArguments()[0]
                            })).ToList();
            foreach (var map in oneWayMap)
            {
                profile.CreateMap(map.Source, map.Destination);
            }

            //----------
            var oneWayReverseMap = types.Where(t => !t.IsAbstract && !t.IsInterface).SelectMany(t =>
                t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IOneWayReverseMap<>))
                    .Select(
                        i =>
                            new
                            {
                                Source = i.GetGenericArguments()[0],
                                Destination = t
                            })).ToList();
            foreach (var map in oneWayReverseMap)
            {
                profile.CreateMap(map.Source, map.Destination);
            }

            //----------
            var twoWayMap = types.Where(t => !t.IsAbstract && !t.IsInterface).SelectMany(t =>
                t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITwoWayMap<>))
                    .Select(
                        i =>
                            new
                            {
                                Source = t,
                                Destination = i.GetGenericArguments()[0]
                            })).ToList();
            foreach (var map in twoWayMap)
            {
                profile.CreateMap(map.Source, map.Destination)
                    .ReverseMap();
            }

            //----------
            var customMaps = types.Where(t => typeof(ICustomMap).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(t => (ICustomMap)Activator.CreateInstance(t)).ToList();

            if (customMaps != null && customMaps.Any())
            {
                foreach (var map in customMaps)
                {
                    map?.CustomMappings(profile);
                }
            }
        }
    }
}