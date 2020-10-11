using ABSAProject.Utility;
using System;

namespace ABSAProject.User
{
    public class UserFactory
    {
        public static UserModel CreateUser(string rawUserData)
        {
            var splitUserData = rawUserData.Split(Environment.GetEnvironmentVariable("Separator"));
            var hashedPass = PasswordHasher.HashPassword(splitUserData[2]);
            return new UserModel(splitUserData[0], splitUserData[1], hashedPass, splitUserData[3]);
        }
    }
}
