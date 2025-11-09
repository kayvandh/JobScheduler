using Framework.Mapper;
using System.Reflection;

namespace JobScheduler.Application.Mapping
{
    public class ApplicationProfile : AutoMapper.Profile
    {
        public ApplicationProfile()
        {
            this.CreateMappingFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
