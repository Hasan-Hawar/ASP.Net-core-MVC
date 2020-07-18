using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeeManagment.ViewModel
{
	public class EditUserViewModel
	{
		public EditUserViewModel()
		{
			Claims = new List<string>();
			Roles = new List<string>();
		}

		public string Id { get; set; }
		[Required]
		public string UserName { get; set; }
		[Required]
		[EmailAddress]
		public string  Email { get; set; }
		public string City { get; set; }
		public List<string> Claims { get; set; }
		public IList<string> Roles { get; set; }

	}
}
