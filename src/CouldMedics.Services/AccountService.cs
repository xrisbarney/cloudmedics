﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CloudMedics.Domain.Enumerations;
using CloudMedics.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using CloudMedics.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CouldMedics.Services.Abstractions
{
    public class AccountService : IAccountService
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountService> _logger;
        private readonly IPatientUserRepository _patientUserRepository;
        public AccountService(
                           UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole> roleManager,
                           IPasswordHasher<ApplicationUser> passwordHasher,
                           IConfiguration appConfigSettings,
                           ILogger<AccountService> logger,
                           IPatientUserRepository patientUserRepository
                          )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
            _configuration = appConfigSettings;
            _logger = logger;
            _patientUserRepository = patientUserRepository;
        }

        public async Task<Tuple<IdentityResult, ApplicationUser>> CreateUserAsync(ApplicationUser user, string password = "")
        {
            try
            {
                var validationResult = await ValidateAccountCreateRequest(user);
                if (!validationResult.Succeeded)
                    return Tuple.Create<IdentityResult, ApplicationUser>(validationResult, null);
                return await AddUserAccountAsync(user, password);

            }
            catch (Exception exception)
            {
                _logger.LogError("Error occured while creating user account - Location user service - Error -> {0}", exception);
                throw;
            }
        }

        public async Task<IEnumerable<ApplicationUser>> FilterUsersAsync(Func<ApplicationUser, bool> filterFn)
        {
            try
            {
                var allUsers = await _userManager.Users.ToListAsync();
                var users = allUsers.Where<ApplicationUser>(filterFn).ToList();
                return users;
            }
            catch (Exception exception)
            {
                var exce = exception;
                throw;
            }

        }

        public async Task<ApplicationUser> GetUserAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        private async Task<object> GenerateJWTSignInToken(ApplicationUser user)
        {
            try
            {

                var userClaims = await BuildUserClaims(user);
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Token:Key"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var securityToken = BuildAuthToken(credentials, userClaims);

                return new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                    expiresIn = securityToken.ValidTo
                };
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<bool> UserExist(string userIdentifier)
        {
            var userExistQueryResult = _userManager.Users.FirstOrDefault(user => user.Id.Equals(userIdentifier, StringComparison.OrdinalIgnoreCase) ||
                                                     user.Email.Equals(userIdentifier, StringComparison.OrdinalIgnoreCase) ||
                                                     user.PhoneNumber.Equals(userIdentifier, StringComparison.OrdinalIgnoreCase)) != null;
            return await Task.FromResult(userExistQueryResult);

        }


        public async Task<Tuple<IdentityResult, object>> SignInUserAsync(string userName, string password)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return Tuple.Create<IdentityResult, object>(IdentityResult.Failed(new IdentityError { Description = "username is invalid" }), null);
                var accountErrors = ValidateAccountStatus(user);
                if (accountErrors.Count > 0)
                    return Tuple.Create<IdentityResult, object>(IdentityResult.Failed(accountErrors[0]), null);
                if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success)
                {
                    var jwtToken = await GenerateJWTSignInToken(user);
                    return Tuple.Create(IdentityResult.Success, jwtToken);
                }
                return Tuple.Create<IdentityResult, object>(IdentityResult.Failed(new IdentityError { Description = "invalid username or password" }), null);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error occured while validating signing credentials  for user {0} -> {1}", userName, exception);
                throw;
            }
        }



        #region privates
        private async Task<Tuple<IdentityResult, ApplicationUser>> AddUserAccountAsync(ApplicationUser user, string password = "")
        {
            try
            {

                var userCreateResult = await (string.IsNullOrEmpty(password) ? _userManager.CreateAsync(user) :
                                              _userManager.CreateAsync(user, password));
                if (!userCreateResult.Succeeded)
                    return Tuple.Create<IdentityResult, ApplicationUser>(userCreateResult, null);
                var createdUser = await _userManager.FindByEmailAsync(user.Email);
                await AddUserToRole(createdUser);
                await SetUpRelatedAccount(user);
                return Tuple.Create(userCreateResult, createdUser);

                //user created. next we send confirmation email
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task AddUserToRole(ApplicationUser user)
        {
            var accountRole = MapRoleFromAccountType(user.AccountType);
            if (string.IsNullOrEmpty(accountRole))
                return;
            await _userManager.AddToRoleAsync(user, accountRole);
        }


        private string MapRoleFromAccountType(AccountType accountType)
        {
            var role = accountType == AccountType.Administrator ? Enum.GetName(typeof(RoleNames), RoleNames.Administrator) :
                           accountType == AccountType.Doctor ? Enum.GetName(typeof(RoleNames), RoleNames.Doctor) :
                                                 accountType == AccountType.Staff ? Enum.GetName(typeof(RoleNames), RoleNames.Staff) :
                                                 accountType == AccountType.Patient ? Enum.GetName(typeof(RoleNames), RoleNames.User) :
                                                 accountType == AccountType.System ? Enum.GetName(typeof(RoleNames), RoleNames.SuperAdministrator) : string.Empty;
            return role;
        }

        private IList<IdentityError> ValidateAccountStatus(ApplicationUser user)
        {
            List<IdentityError> authenticationErrors = new List<IdentityError>();
            if (!user.EmailConfirmed)
                authenticationErrors.Add(new IdentityError { Description = "You have not confirmed your email address" });
            if (user.AccountStatus != AccountStatus.Active)
                authenticationErrors.Add(new IdentityError { Description = "Your account is suspended. Contact admin to reolve issues " });
            return authenticationErrors;
        }

        private async Task<IdentityResult> ValidateAccountCreateRequest(ApplicationUser user)
        {
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Invalid user model.Cannot ceate user with null details" });
            var userAccountExist = await UserExist(user.Email);
            if (userAccountExist)
                return IdentityResult.Failed(new IdentityError { Description = $"User account with email address {user.Email} already exist" });
            return IdentityResult.Success;
        }

        private async Task<IList<Claim>> BuildUserClaims(ApplicationUser user)
        {
            var assignedRoles = (await _userManager.GetRolesAsync(user));
            var rolesAsClaims = new Claim("roles", string.Join(",", assignedRoles));

            var roleClaims = new List<Claim>();
            foreach (var role in assignedRoles)
            {
                roleClaims.AddRange((await _roleManager.GetClaimsAsync(new IdentityRole() { Name = role })));
            }
            roleClaims.Add(rolesAsClaims);
            roleClaims.Union(new Claim[]{
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Sid, user.Id)
                });

            return roleClaims;

        }

        private JwtSecurityToken BuildAuthToken(SigningCredentials credentials, IList<Claim> userClaims, int tokenExpiryInMinutes = 15)
        {
            return
                new JwtSecurityToken(
                    issuer: _configuration["Token:Issuer"],
                    audience: _configuration["Token:Audience"],
                    claims: userClaims,
                    expires: DateTime.Now.AddMinutes(tokenExpiryInMinutes),
                    signingCredentials: credentials
                );

        }

        private async Task<bool> SetUpRelatedAccount(ApplicationUser user)
        {
            switch (user.AccountType)
            {
                case AccountType.Patient:
                    {
                        var newPatient = new Patient { UserId = user.Id };
                        var patientAccountCreated = await _patientUserRepository.AddPatientAsync(newPatient) != null;
                        return patientAccountCreated;
                    };
                default:
                    {
                        return false;
                    }
            }
        }

        #endregion


    }
}
