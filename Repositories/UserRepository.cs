using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WPF_LoginForm.Model;

namespace WPF_LoginForm.Repositories
{
    public class UserRepository : RepositoryBase, IUserRepository
    {
        public void Add(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        public bool AuthenticateUser(NetworkCredential credential)
        {
            bool validUser;
            using (var connection = GetConnection())
            using (var command = new SQLiteCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "select * from [User] where username=@username and [password]=@password";
                command.Parameters.Add("@username", DbType.String).Value = credential.UserName;
                command.Parameters.Add("@password", DbType.String).Value = credential.Password;
                validUser = command.ExecuteScalar() == null ? false : true;
            }
            return validUser;
        }

        public void Edit(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserModel> GetByAll()
        {
            throw new NotImplementedException();
        }

        public UserModel GetbyId(int id)
        {
            throw new NotImplementedException();
        }

        public UserModel GetByUsername(string username)
        {
            UserModel user = null;
            using (var connection = GetConnection())
            using (var command = new SQLiteCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "select * from [User] where username=@username";
                command.Parameters.Add("@username", DbType.String).Value = username;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel()
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Password = string.Empty,
                            Name = reader.GetString(3),
                            LastName = reader.GetString(4),
                            Email = reader.GetString(5),
                        };
                    }
                }
            }
            return user;
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
