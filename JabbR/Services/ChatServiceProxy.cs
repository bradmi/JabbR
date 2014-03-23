using JabbR.Infrastructure;
using JabbR.Models;

namespace JabbR.Services
{
    public class ChatServiceProxy : IChatServiceProxy
    {
        private readonly IBackplaneChannel _backplaneChannel;

        private readonly IJabbrRepository _repository;

        private readonly ICache _cache;

        public ChatServiceProxy(IBackplaneChannel backplaneChannel, IJabbrRepository repository, ICache cache)
        {
            _backplaneChannel = backplaneChannel;
            _repository = repository;
            _cache = cache;

            _backplaneChannel.Subscribe<IChatServiceProxy>(this);
        }

        void IChatServiceProxy.RemoveUserInRoomRemote(ChatUser user, ChatRoom room)
        {
            _backplaneChannel.Invoke<IChatServiceProxy>("RemoveUserInRoom", new object[] { user.Id, room.Name });
        }

        void IChatServiceProxy.RemoveUserInRoom(string userId, string roomName)
        {
            var user = _repository.GetUserById(userId);
            var room = _repository.GetRoomByName(roomName);
            _cache.RemoveUserInRoom(user, room);
        }
    }
}