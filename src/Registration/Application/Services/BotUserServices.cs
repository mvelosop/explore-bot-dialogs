using Microsoft.Extensions.Logging;
using Registration.Application.Contracts;
using Registration.Domain.Model;
using Registration.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Registration.Application.Services
{
    public class BotUserServices : IBotUserServices
    {
        private readonly RegistrationRepo _repo;
        private readonly ILogger<BotUserServices> _logger;

        public BotUserServices(
            ILogger<BotUserServices> logger,
            RegistrationRepo repo)
        {
            _logger = logger;

            _logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
            _repo = repo;
        }

        public async Task AddAsync(BotUser entity)
        {
            _repo.Users.Add(entity);
        }

        public async Task<BotUser> FindByChannelUserIdAsync(string channelId, string userId)
        {
            return _repo.Users
                .FirstOrDefault(c => c.ChannelId == channelId && c.UserId == userId);
        }

        public async Task<IList<BotUser>> GetListAsync(string name = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return new ReadOnlyCollection<BotUser>(_repo.Users);

            return new ReadOnlyCollection<BotUser>(
                _repo.Users
                .Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .ToList());
        }

    }
}