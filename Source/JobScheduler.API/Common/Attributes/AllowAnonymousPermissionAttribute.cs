namespace JobScheduler.API.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class AllowAnonymousPermissionAttribute : Attribute
    {
    }
}