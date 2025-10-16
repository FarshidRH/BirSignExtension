using MapIdeaHub.BirSign.NetFrameworkExtension.Dtos;
using System;
using System.Threading.Tasks;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Events
{
    public class IdsEvents
    {
        public Func<UserInfo, Task<bool>> OnCheckUserExists { get; set; }

        public Func<UserInfo, Task> OnUserRegistered { get; set; }

        public Func<UserInfo, Task> OnManageUserAccess { get; set; }

        public Func<UserInfo, Task> OnUserAuthenticated { get; set; }
    }
}
