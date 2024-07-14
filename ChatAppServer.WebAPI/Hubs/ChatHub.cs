using ChatAppServer.WebAPI.Context;
using ChatAppServer.WebAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppServer.WebAPI.Hubs
{
    public sealed class ChatHub(ApplicationDbContext context) : Hub
    {

        public static Dictionary<string, Guid> Users = new(); //kullanici listesi
        public async Task Connect(Guid userId)
        {
            Users.Add(Context.ConnectionId, userId);
            User? user = await context.Users.FindAsync(userId);  //baglanan useri bulma
            if(user is not null)                            
            {
                user.Status = "online";                         //eger kullanici varsa
                await context.SaveChangesAsync();

                await Clients.All.SendAsync("Users", user);    //kullanicilarin degisen durumunu gonderme islemi
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Guid userId;
            Users.TryGetValue(Context.ConnectionId, out userId);
            Users.Remove(Context.ConnectionId);  //connection id yi temizledik yoksa liste kabardığı için problem çıkıyor

            User? user = await context.Users.FindAsync(userId);  
             if (user is not null)
              {
                user.Status = "offline";                         
                await context.SaveChangesAsync();

                await Clients.All.SendAsync("Users", user);
              }
            

        }

    }
}
