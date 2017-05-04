using Emc.Documentum.Rest.Net;
using Emc.Documentum.Rest.Http.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Emc.Documentum.Rest.DataModel
{
    public partial class Group
    {
        /// <summary>
        /// Get groups feed of which this group is a member
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public Feed<T> GetParentGroups<T>(FeedGetOptions options)
        {
            return Client.GetFeed<T>(
                GetFullLinks(),
                LinkRelations.PARENT.Rel,
                options);
        }

        /// <summary>
        /// Get groups feed which are members of this group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public Feed<T> GetSubGroups<T>(FeedGetOptions options)
        {
            return Client.GetFeed<T>(
                GetFullLinks(),
                LinkRelations.GROUPS.Rel,
                options);
        }

        /// <summary>
        /// Get users feed which are members of this group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public Feed<T> GetGroupUsers<T>(FeedGetOptions options)
        {
            return Client.GetFeed<T>(
                GetFullLinks(),
                LinkRelations.USERS.Rel,
                options);
        }

        /// <summary>
        /// Get groups feed which are members of this group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public Feed<T> GetGroupGroups<T>(FeedGetOptions options)
        {
            return Client.GetFeed<T>(
                GetFullLinks(),
                LinkRelations.GROUPS.Rel,
                options);
        }

        /// <summary>
        /// Update a group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newGroup"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public T Update<T>(T newGroup, GenericOptions options) where T : Group
        {
            return Client.Post<T>(
                GetFullLinks(),
                LinkRelations.EDIT.Rel,
                newGroup,
                options);
        }

        /// <summary>
        ///  Delete a group       
        /// </summary>
        public void Delete()
        {
            Client.Delete(LinkRelations.FindLinkAsString(GetFullLinks(), LinkRelations.DELETE.Rel));
        }


        /// <summary>
        /// Add an existing user into this group as a user member
        /// </summary>       
        /// <param name="userHref">The specified user resource uri. This variable is mandatory.</param>
        /// <returns></returns>
        public void AddUserToGroup(User userHref)
        {           
            Client.Post(
                GetFullLinks(),
                LinkRelations.USERS.Rel,
                userHref,null);
        }

        /// <summary>
        /// Add an existing group into this group as a sub-group member
        /// </summary>       
        /// <param name="groupHref">The specified group resource uri. This variable is mandatory.</param>
        /// <returns></returns>
        public void AddSubGroup(Group groupHref)
        {
            Client.Post(
                GetFullLinks(),
                LinkRelations.GROUPS.Rel,
                groupHref, null);
        }
    }
}
