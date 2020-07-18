using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace EmployeeManagment.Secutity
{
	public class CanEditOnlyAdminRolesAndClaimsHandler : 
		AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, 
			ManageAdminRolesAndClaimsRequirement requirement)
		{
			var authFilterContext = context.Resource as AuthorizationFilterContext;
			if(authFilterContext == null)
			{
				// return complete task and deny access
				return Task.CompletedTask;
			}

			// in context Object we have the list of user claims and from the list of claims we need the name identifier 
			// claim because the contains the ID of the logged in user

			var loggedInAdminId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

			string adminIdBeingEdited = authFilterContext.HttpContext.Request.Query["userId"];

			if(context.User.IsInRole("Admin") && context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true")
			&& adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
			{
				context.Succeed(requirement);
			}
			return Task.CompletedTask;
		}
	}
}
