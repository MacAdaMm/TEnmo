using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IUserDao
    {
        User GetUser(string username);
        User AddUser(string username, string password);
        List<User> GetUsers();
        public decimal GetUserBalanceById(int id);
        public int GetAccountId(int UserId);
        public decimal GetBalanceByAccount(int _account);
        public string GetUsernameByAcount(int _account);
    }
}
