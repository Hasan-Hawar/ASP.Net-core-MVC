using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagment.Models
{
	public class Employee
	{
		public int Id { get; set; }
		[NotMapped]
		public string EncryptedId { get; set; }
		[Required]
		[MaxLength(50, ErrorMessage ="Name cannot exeed 50 characters")]
		public string Name { get; set; }
		[Required]
		[RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
			ErrorMessage = "Invalid Email format")]
		[Display(Name= "Office Email")]
		public string Email { get; set; }
		[Required]
		public Dept? Depratment { get; set; }
		public string PhotoPath { get; set; }

	}
}

