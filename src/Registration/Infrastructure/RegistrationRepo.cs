using Microsoft.Extensions.Logging;
using Registration.Domain.Model;
using System.Collections.Generic;

namespace Registration.Infrastructure
{
    public class RegistrationRepo
    {
        public RegistrationRepo(ILogger<RegistrationRepo> logger)
        {
            logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
        }

        public List<BotUser> Users { get; set; } = new List<BotUser>();
    }
}