using JabbR.Models;

namespace JabbR.Services
{
    public interface IChatServiceProxy
    {
        void RemoveUserInRoomRemote(ChatUser user, ChatRoom room);

        void RemoveUserInRoom(string userId, string roomName);
    }
}
