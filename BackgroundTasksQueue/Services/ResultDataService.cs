using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using BooksTextsSplit.Library.Models;
using BackgroundTasksQueue.Helpers;
using CachingFramework.Redis.Contracts.Providers;

namespace BackgroundTasksQueue.Services
{
    public interface IResultDataService
    {
        Task<LoginAttemptResult> ResultData(int resultCode, string userEmail);
        Task<LoginAttemptResult> ResultDataWithToken(int resultCode, UserData user);
        //Task<string> CreateToken(string emailKey);
    }
    public class ResultDataService : IResultDataService
    {
        private readonly ICacheProviderAsync _cache;
        private readonly IStringLocalizer<ResultDataService> _localizer;

        public ResultDataService(ICacheProviderAsync cache, IStringLocalizer<ResultDataService> localizer)
        {
            _cache = cache;
            _localizer = localizer;
            
        }

        public async Task<LoginAttemptResult> ResultData(int resultCode, string userEmail)
        {
            // resultCode = 1; // - to test
            UserData user = new UserData();
            if (resultCode == 0)
            {
                if (userEmail != null)
                {
                    user = (await _cache.GetObjectAsync<UserData>(userEmail)).WithoutPassword();
                    if (user == null)
                    {
                        resultCode = 5;
                    };
                }
                else
                {
                    resultCode = 5;
                }
            };
            LoginAttemptResult resultData = new LoginAttemptResult()
            {
                AuthUser = user,
                ResultMessage = _localizer["ResultCode" + resultCode],
                ResultCode = resultCode
            };
            return resultData;
        }

        public async Task<LoginAttemptResult> ResultDataWithToken(int resultCode, UserData user)
        {
            LoginAttemptResult resultData = new LoginAttemptResult();
            if (resultCode == 0)
            {
                resultData.IssuedToken = await CreateToken(user.Email);                
            }            
            resultData.ResultMessage = _localizer["ResultCode" + resultCode];
            resultData.ResultCode = resultCode;
            return resultData;
        }

        private async Task<string> CreateToken(string emailKey)
        {
            UserData user = await _cache.GetObjectAsync<UserData>(emailKey);
            user.Token = "4db6A12C94kfv51qaxB2sdgf781xvf11dfnhsr3382gui914asc6A12C94acdfb51cbB2avs781db1";

            // Set Token to Redis                                          
            await _cache.SetObjectAsync(user.Token, user, TimeSpan.FromDays(1));
            UserData getNewToken = await _cache.GetObjectAsync<UserData>(user.Token);
            if (getNewToken.Token == user.Token)
            {
                return getNewToken.Token;
            }
            else
            {
                return null;
            }
        }
    }
}
