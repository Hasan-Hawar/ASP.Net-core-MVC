using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.ViewModel
{
	public class ForgotPasswordViewModel
	{
		[Required]
		[EmailAddress]
		public String Email { get; set; }
	}
}
