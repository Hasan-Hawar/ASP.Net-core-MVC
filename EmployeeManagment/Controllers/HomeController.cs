using EmployeeManagment.Models;
using EmployeeManagment.Secutity;
using EmployeeManagment.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Controllers
{ 
	[Authorize]
	public class HomeController : Controller
	{
		private readonly IEmployeeRepository employeeReposiory;
		private readonly IHostingEnvironment hostingEnvironment;
		private readonly ILogger<HomeController> logger;
		private readonly IDataProtector protector;

		public HomeController(IEmployeeRepository employeeRepository, 
							  IHostingEnvironment hostingEnvironment, ILogger<HomeController> logger,
							  IDataProtectionProvider dataProtectionProvider,
							  DataProtectionPurposeStrings dataProtectionPurposeStrings)
		{
			
			this.employeeReposiory = employeeRepository;
			this.hostingEnvironment = hostingEnvironment;
			this.logger = logger;
			protector = dataProtectionProvider
				.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
		}


		[AllowAnonymous]
		public ViewResult Index()
		{
			var model = employeeReposiory.GetAllEmployees()
					.Select(e => 
					{
						e.EncryptedId = protector.Protect(e.Id.ToString());
						return e;
					});
			return View(model);
		}

		[AllowAnonymous]
		public ViewResult Details(string id)
		{
			//throw new Exception("Error in Details View")

			logger.LogTrace("Trace Log");
			logger.LogDebug("Debug Log");
			logger.LogInformation("Information Log");
			logger.LogWarning("Warning Log");
			logger.LogError("Error Log");
			logger.LogCritical("Critical Log");

			int employeeId = Convert.ToInt32(protector.Unprotect(id));

			Employee employee = employeeReposiory.GetEmployee(employeeId);

			if(employee == null)
			{
				Response.StatusCode = 404;
				return View("EmployeeNotFound", employeeId);
			}

			HomeDetailsViewModel model = new HomeDetailsViewModel()
			{
				Employee = employee,
				PageTitle = "Employee Details"
			};

			return View(model);
		}

		[HttpGet]
		public ViewResult Create()
		{
			return View();
		}
		[HttpGet]
		public ViewResult Edit(int id)
		{
			Employee employee = employeeReposiory.GetEmployee(id);
			EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
			{
				Id = employee.Id,
				Name = employee.Name,
				Email = employee.Email,
				Depratment = employee.Depratment,
				ExistingPhotoPath = employee.PhotoPath
			};
			return View(employeeEditViewModel);
		}
		[HttpPost]
		public IActionResult Edit(EmployeeEditViewModel model)
		{
			if (ModelState.IsValid)
			{
				Employee employee = employeeReposiory.GetEmployee(model.Id);
				employee.Name = model.Name;
				employee.Email = model.Email;
				employee.Depratment = model.Depratment;

				if(model.Photo != null)
				{
					if(model.ExistingPhotoPath != null)
					{
						string filePath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingPhotoPath);
						System.IO.File.Delete(filePath);
					}
					employee.PhotoPath = ProcessUploadedFile(model);
				}
				
				employeeReposiory.Update(employee);
				return RedirectToAction("index");
			}
			return View();
		}

		private string ProcessUploadedFile(EmployeeCreateViewModel model)
		{
			string uniqueFileName = null;
			if (model.Photo != null)
			{
				string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
				uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
				string filePath = Path.Combine(uploadsFolder, uniqueFileName);
				using(var fileStream = new FileStream(filePath, FileMode.Create))
				{

				model.Photo.CopyTo(fileStream);
				}
			}

			return uniqueFileName;
		}

		[HttpPost]
		public IActionResult Create(EmployeeCreateViewModel model)
		{
			if (ModelState.IsValid)
			{
				string uniqueFileName = ProcessUploadedFile(model);
				Employee newEmployee = new Employee
				{
					Name = model.Name,
					Email = model.Email,
					Depratment = model.Depratment,
					PhotoPath = uniqueFileName
				};
				employeeReposiory.Add(newEmployee);
				return RedirectToAction("details", new { id = newEmployee.Id });
			}
			return View();
		}
	}
}
