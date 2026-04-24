using Microsoft.AspNetCore.Authorization;

namespace DeadMoney.Web.Authorization;

public class AdminRequirement : IAuthorizationRequirement { }

public class AdminHandler : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}