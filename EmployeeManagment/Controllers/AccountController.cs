using EmployeeManagment.Models;
using EmployeeManagment.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace EmployeeManagment.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly ILogger<AccountController> logger;

		public AccountController(UserManager<ApplicationUser> userManager, 
			SignInManager<ApplicationUser> signInManager,
			ILogger<AccountController> logger)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> AddPassword()
		{
			var user = await userManager.GetUserAsync(User);

			var userHashPassword = await userManager.HasPasswordAsync(user);

			if (userHashPassword)
			{
				return RedirectToAction("ChangePassword");
			}
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.GetUserAsync(User);

				var result = await userManager.AddPasswordAsync(user, model.NewPassword);

				if (!result.Succeeded)
				{
					foreach(var error in result.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
					return View();
				}

				await signInManager.RefreshSignInAsync(user);
				return View("AddPasswordConfirmation");
			}
			return View();
		}



		[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await signInManager.SignOutAsync();
			return RedirectToAction("index", "home");
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Login(string returnUrl)
		{
			LoginViewModel model = new LoginViewModel
			{
				ReturnUrl = returnUrl,
				ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()

			};
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
		{
			model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByEmailAsync(model.Email);

				if (user != null && !user.EmailConfirmed
					&& (await userManager.CheckPasswordAsync(user, model.Password)))
				{
					ModelState.AddModelError(string.Empty, "Email is not confirmed yet!");
					return View(model);
				}

				// The last boolean parameter lockoutOnFailure indicates if the account
				// should be locked on failed logon attempt. On every failed logon
				// attempt AccessFailedCount column value in AspNetUsers table is
				// incremented by 1. When the AccessFailedCount reaches the configured
				// MaxFailedAccessAttempts which in our case is 5, the account will be
				// locked and LockoutEnd column is populated. After the account is
				// lockedout, even if we provide the correct username and password,
				// PasswordSignInAsync() method returns Lockedout result and the login
				// will not be allowed for the duration the account is locked.

				var result = await signInManager.PasswordSignInAsync(model.Email, model.Password,
																		model.RememberMe, true);
				if (result.Succeeded)
				{
					if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
					{
						return Redirect(returnUrl);
					}
					else {
						return RedirectToAction("index", "home");
					}
				}
				if (result.IsLockedOut)
				{
					return View("AccountLocked");
				}
				ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
			}
			return View(model);
		}


		[HttpGet]
		[AllowAnonymous]
		public IActionResult Register()
		{
			return View();
		}

		[AcceptVerbs("Get", "Post")]
		[AllowAnonymous]
		public async Task<IActionResult> IsEmailInUse(string email)
		{
			var user = await userManager.FindByEmailAsync(email);

			if (user == null)
			{
				return Json(true);
			}
			else
			{
				return Json($"Email {email} is already in use.");
			}
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = model.Email,
					Email = model.Email,
					City = model.City
				};
				var result = await userManager.CreateAsync(user, model.Password);

				if (result.Succeeded)
				{

					var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
					var confirmationLink = Url.Action(
						"ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);
					logger.Log(LogLevel.Warning, confirmationLink);


					if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
					{
						return RedirectToAction("ListUsers", "Administration");
					}

					ViewBag.ErrorTitle = "Registration successful";
					ViewBag.ErrorMessage = "Before you can Login, please confirm your email with the " +
						"confirmation Email we have emailed you!";
					return View("Error");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(model);
		}

		[AllowAnonymous]
		public IActionResult ExternalLogin(string provider, string returnUrl)
		{
			var redirectUrl = Url.Action("ExternalLoginCallBack", "Account", new { ReturnUrl = returnUrl });

			var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		[AllowAnonymous]
		public async Task<IActionResult> ExternalLoginCallBack(string returnUrl = null, string remoteError = null)
		{
			returnUrl = returnUrl ?? Url.Content("~/");

			LoginViewModel loginViewModel = new LoginViewModel
			{
				ReturnUrl = returnUrl,
				ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
			};

			if (remoteError != null)
			{
				ModelState.AddModelError(string.Empty, $"Error from external providerL: {remoteError}");
				return View("Login", loginViewModel);
			}

			var info = await signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				ModelState.AddModelError(string.Empty, "Error loading external login infromation");
				return View("Login", loginViewModel);
			}


			var email = info.Principal.FindFirstValue(ClaimTypes.Email);
			ApplicationUser user = null;

			if (email != null)
			{
				user = await userManager.FindByEmailAsync(email);

				if (user != null && !user.EmailConfirmed)
				{
					ModelState.AddModelError(string.Empty, "Email is not confirmed yet!");
					return View("Login", loginViewModel);
				}
			}

			var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
														info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

			if (signInResult.Succeeded)
			{
				return LocalRedirect(returnUrl);
			}
			else
			{
				if (email != null)
				{
					if (user == null)
					{
						user = new ApplicationUser
						{
							UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
							Email = info.Principal.FindFirstValue(ClaimTypes.Email)
						};

						await userManager.CreateAsync(user);

						var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

						var confirmationLink = Url.Action("ConfirmEmail", "Account", 
															new { userId = user.Id, token = token }, Request.Scheme);

						logger.Log(LogLevel.Warning, confirmationLink);

						ViewBag.ErrorTitle = "Registration successfull";
						ViewBag.ErrorMessage = "Before you can Login, please confirm your email with the " +
						"confirmation Email we have emailed you!";
						return View("Error");
					}
					await userManager.AddLoginAsync(user, info);
					await signInManager.SignInAsync(user, isPersistent: false);

					return LocalRedirect(returnUrl);
				}
				ViewBag.ErrorTitle = $"Email claim does not received from: {info.LoginProvider}";
				ViewBag.ErrorMessage = "Please contact out suppot department on tech.support@hawar.com";

				return View("Error");
			}
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			if(userId == null || token == null)
			{
				return RedirectToAction("index", "home");
			}

			var user = await userManager.FindByIdAsync(userId);

			if(user == null)
			{
				ViewBag.ErrorMessage = $"The user with ID: {userId} is Invalid";
				return View("NotFound");
			}

			var result = await userManager.ConfirmEmailAsync(user, token);

			if (result.Succeeded)
			{
				return View();
			}

			ViewBag.ErrorTitel = "Email cannot be confirmed";
			return View("Error");
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model) 
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByEmailAsync(model.Email);

				if(user != null && await userManager.IsEmailConfirmedAsync(user))
				{
					var token = await userManager.GeneratePasswordResetTokenAsync(user);

					var passwordResetLink = Url.Action("ResetPassword", "Account",
						new { email = model.Email, token = token }, Request.Scheme);
					logger.Log(LogLevel.Warning, passwordResetLink);

					return View("ForgotPasswordConfirmation");
				}
				return View("ForgotPasswordConfirmation");

			}
			return View(model);
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult ResetPassword(string email, string token){

			if(email == null || token == null)
			{
				ModelState.AddModelError("", "Invalid password reset token");
			}

			
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{

			if (ModelState.IsValid)
			{
				var user = await userManager.FindByEmailAsync(model.Email);

				if (user != null)
				{
					var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);

					if (result.Succeeded)
					{
						if(await userManager.IsLockedOutAsync(user))
						{
							await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
						}

						return View("ResetPasswordConfirmation");
					}
					foreach(var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
						return View(model);
					}
					return View("ResetPasswordConfirmation"); 
				}
			}
				return View(model);
		}
		[HttpGet]
		public async Task<IActionResult> ChangePassword()
		{
			var user = await userManager.GetUserAsync(User);

			var userHashPassword = await userManager.HasPasswordAsync(user);

			if (!userHashPassword)
			{
				return RedirectToAction("AddPassword");
			}

			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.GetUserAsync(User);
				if (user == null)
				{
					return RedirectToAction("Login");
				}
				var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

				if (!result.Succeeded)
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
					return View(model);
				}

				// if change password succeeded, we want to refresh the sign-in cookies 
				// to set-up the new changes setting 
				await signInManager.RefreshSignInAsync(user);
				// send confirmation to the user ..
				return View("ChangePasswordConfirmation");
			}
			
			return View(model);
		}
	}
}
