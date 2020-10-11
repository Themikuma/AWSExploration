using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABSAProject.User
{
    public class UserModel
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Password { get; }
        public string Created { get; }

        public UserModel(string id, string name, string password, string created)
        {
            Id = new Guid(id);
            Name = name;
            Password = password;
            Created = created;
        }

        public Dictionary<string, AttributeValue> GetUserDocument(string Path)
        {
            var userDocument = new Dictionary<string, AttributeValue>();
            userDocument[nameof(Id)] = new AttributeValue(Id.ToString());
            userDocument[nameof(Name)] = new AttributeValue(Name);
            userDocument[nameof(Password)] = new AttributeValue(Password);
            userDocument[nameof(Created)] = new AttributeValue(Created);
            userDocument[nameof(Path)] = new AttributeValue(Path);
            return userDocument;
        }
    }
}
