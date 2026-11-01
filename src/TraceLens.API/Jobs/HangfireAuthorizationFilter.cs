using Hangfire.Dashboard;

namespace TraceLens.API.Jobs;

public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var host = context.GetHttpContext().Request.Host.Host;
        return host == "localhost" || host == "127.0.0.1";
    }
}
