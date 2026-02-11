using Microsoft.AspNetCore.Authorization;

namespace DeadMoney.Web.Authorization;

// This is a "Marker" class used to define the policy
public class CommissionerRequirement : IAuthorizationRequirement { }

// This is the logic that actually checks the user's "Identity Card" (Claims)
public class CommissionerHandler : AuthorizationHandler<CommissionerRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CommissionerRequirement requirement)
    {
        // We check for the "Commissioner" role claim assigned in your UserService
        if (context.User.IsInRole("Commissioner"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}