using Registration.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Registration.Application.Contracts
{
    public interface IBotUserServices
    {
        Task AddAsync(BotUser entity);

        Task<BotUser> FindByChannelUserIdAsync(string channelId, string userId);

        Task<IList<BotUser>> GetListAsync(string name = null);
    }
}