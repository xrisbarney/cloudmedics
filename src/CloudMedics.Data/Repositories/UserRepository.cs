﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudMedics.Domain;

namespace CloudMedics.Data.Repositories
{
    public interface IUserRepository{
        
        Task<AppUser> CreateUserAsync(AppUser user);
        Task<int> CreateUsersAsync(IList<AppUser> users);
        Task<AppUser> GetUserAsync(string userId);
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> UpdateUserAccountAsync(string userId, AppUser updatedUser);
        Task<bool> DeleteUserAccountAsync(string userId);
        Task<IEnumerable<AppUser>> FilterUsersAsync(Func<AppUser, bool> filterFn);

    }

    public class UserRepository:BaseRepository<AppUser>, IUserRepository
    {
        public UserRepository()
        {
        }

        public async Task<bool> DeleteUserAccountAsync(string userId){
            return await Delete(userId);
        }

        public async Task<AppUser> CreateUserAsync(AppUser user)
        {
            return await base.Add(user);
        }

        public async Task<int> CreateUsersAsync(IList<AppUser> users)
        {
            return await Add(users);
        }

        public async Task<IEnumerable<AppUser>> FilterUsersAsync(Func<AppUser, bool> filterFn)
        {
            return await Filter(filterFn);
        }

        public async Task<AppUser> GetUserAsync(string userId)
        {
            return await Get(userId);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await GetAll();
        }

        public async Task<AppUser> UpdateUserAccountAsync(string userId, AppUser updatedUser)
        {
            var userAccount = await GetUserAsync(userId);
            if (userAccount == null)
                return null;
            return await Update(updatedUser);
        }
    }
}
