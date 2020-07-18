
using EmployeeManagment.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.ViewModel
{
	public class RegisterViewModel
	{
		[Required]
		[EmailAddress]
		[Remote(action: "IsEmailInUse", controller: "Account")]
		[ValidEmailDomain(allowedDomain: "test.com", 
			ErrorMessage = "The Domain name must be test.com")]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name ="Confirm Password")]
		[Compare("Password", ErrorMessage ="Password and confirmation password do not match")]
		public string ConfirmPassword { get; set; }
		[Required]
		public string City { get; set; }

	}
}
