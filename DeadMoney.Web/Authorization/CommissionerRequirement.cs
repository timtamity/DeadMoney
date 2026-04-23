using Microsoft.AspNetCore.Authorization;

namespace DeadMoney.Web.Authorization;

public class CommissionerRequirement : IAuthorizationRequirement { }

public class CommissionerHandler : AuthorizationHandler<CommissionerRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CommissionerRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}