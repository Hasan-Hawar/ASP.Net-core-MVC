using EmployeeManagment.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.ViewModel
{
	public class EmployeeCreateViewModel
	{
		[Required]
		[MaxLength(50, ErrorMessage = "Name cannot exeed 50 characters")]
		public string Name { get; set; }
		[Required]
		[RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
			ErrorMessage = "Invalid Email format")]
		[Display(Name = "Office Email")]
		public string Email { get; set; }
		[Required]
		public Dept? Depratment { get; set; }
		public IFormFile Photo { get; set; }
	}
}
