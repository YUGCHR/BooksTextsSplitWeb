using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CachingFramework.Redis;
using Microsoft.Extensions.Localization;
using BooksTextsSplit.Models;
using BooksTextsSplit.Helpers;

namespace BooksTextsSplit.Services
{
    public interface IResultDataService
    {
        Task<LoginAttemptResult> ResultData(int resultCode, string userEmail);
        LoginAttemptResult ResultDataWithToken(int resultCode, string newToken);
        Task<string> CreateToken(string emailKey);
    }
    public class ResultDataService : IResultDataService
    {
        private readonly RedisContext cache;
        private readonly IStringLocalizer<ResultDataService> _localizer;

        public ResultDataService(RedisContext c, IStringLocalizer<ResultDataService> localizer)
        {
            cache = c;
            _localizer = localizer;
            
        }

        public async Task<LoginAttemptResult> ResultData(int resultCode, string userEmail)
        {
            // resultCode = 1; // - to test
            User user = new User();
            if (resultCode == 0)
            {
                if (userEmail != null)
                {
                    user = (await cache.Cache.GetObjectAsync<User>(userEmail)).WithoutPassword();
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

        public LoginAttemptResult ResultDataWithToken(int resultCode, string newToken)
        {
            LoginAttemptResult resultData = new LoginAttemptResult();
            if (resultCode == 0)
            {
                resultData.IssuedToken = newToken;
            }
            resultData.ResultMessage = _localizer["ResultCode" + resultCode];
            resultData.ResultCode = resultCode;
            return resultData;
        }

        public async Task<string> CreateToken(string emailKey)
        {
            User user = await cache.Cache.GetObjectAsync<User>(emailKey);
            user.Token = "4db6A12C94kfv51qaxB2sdgf781xvf11dfnhsr3382gui914asc6A12C94acdfb51cbB2avs781db1";

            // Set Token to Redis                                          
            await cache.Cache.SetObjectAsync(user.Token, user, TimeSpan.FromDays(1));
            User getNewToken = await cache.Cache.GetObjectAsync<User>(user.Token);
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
