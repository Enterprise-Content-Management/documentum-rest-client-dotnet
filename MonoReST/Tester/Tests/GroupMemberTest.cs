using Emc.Documentum.Rest.DataModel;
using Emc.Documentum.Rest.Http.Utility;
using Emc.Documentum.Rest.Net;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Emc.Documentum.Rest.Test
{
    public class GroupMemberTest
    {
        public static void Run(RestController client, string RestHomeUri, string repositoryName, bool printResult,
            string groupName,string subGroupName,string userName)
        {
            // get repository resource
            HomeDocument home = client.Get<HomeDocument>(RestHomeUri, null);
            Feed<Repository> repositories = home.GetRepositories<Repository>(new FeedGetOptions { Inline = true, Links = true });
            Repository repository = repositories.FindInlineEntry(repositoryName);

            //create or get group
            Group testGroup = CreateOrGetGroup(repository, groupName, printResult);

            //create or get group
            Group testSubGroup = CreateOrGetGroup(repository, subGroupName, printResult);

            //create or get user
            User testUser = CreateOrGetUser(repository, userName, printResult);

            //add user/sub-group to the parent group
            AddMembersOperations(repository, testGroup,testSubGroup,testUser, printResult);   
            //remove user/sub-group from the parent group         
            RemoveMembersOperations(repository, testGroup, testSubGroup, testUser, printResult);           
        }
      

        private static void AddMembersOperations(Repository repository, Group group, Group subGroup, User user, bool printResult)
        {
            do
            {
                Console.WriteLine("\nPress 'u' to add user to the group, 'g' to add sub-group to the group,'a' to add user and sub-group to the parent group, or any other key to continue following tests..");
                Console.ForegroundColor = ConsoleColor.Yellow;
                ConsoleKeyInfo next = Console.ReadKey();
                Console.ForegroundColor = ConsoleColor.White;
                string keyword = next.KeyChar.ToString();

                switch (keyword)
                {
                    case "u":
                        AddUserToGroup(group, user, printResult);
                        break;
                    case "g":
                        AddGroupToGroup(group, subGroup, printResult);
                        break;
                    case "a":
                        AddUserToGroup(group, user, printResult);
                        AddGroupToGroup(group, subGroup, printResult);
                        return;                        
                    default:
                        Console.WriteLine("\n");
                        return;

                }
            } while (true);
            
        }

        private static void RemoveMembersOperations(Repository repository, Group group, Group subGroup, User user, bool printResult)
        {
            do
            {
                Console.WriteLine("\nPress 'u' to remove user from the group, 'g' to remove sub-group from the group,'q' to back to previous layer, or any other key run all tests..");
                Console.ForegroundColor = ConsoleColor.Yellow;
                ConsoleKeyInfo next = Console.ReadKey();
                Console.ForegroundColor = ConsoleColor.White;
                string keyword = next.KeyChar.ToString();

                switch (keyword)
                {
                    case "u":
                        RemoveUserFromGroup(repository, group, user, printResult);
                        break;
                    case "g":
                        RemoveGroupFromGroup(repository, group, subGroup, printResult);
                        break;
                    case "q":
                        Console.WriteLine("\n");
                        return;
                    default:
                        RemoveUserFromGroup(repository, group, user, printResult);
                        RemoveGroupFromGroup(repository, group, subGroup, printResult);
                        break;
                }
            } while (true);
        }              

        private static void AddUserToGroup(Group group, User user, bool printResult)
        {
            object groupName;
            object userName;
            group.Properties.TryGetValue("group_name", out groupName);
            user.Properties.TryGetValue("user_name", out userName);
            Console.WriteLine("\nAdding user <" + userName + "> to group <" + groupName + ">...");
            User memberUser = new User();
            memberUser.Href = LinkRelations.FindLinkAsString(user.Links, LinkRelations.SELF.Rel);  
            group.AddUserToGroup(memberUser);
            if (printResult)
            {
                Console.WriteLine("\nSuccessfully add the user to the group.");
            }
        }

        private static void AddGroupToGroup(Group group, Group subGroup, bool printResult)
        {
            object groupName;
            object subGroupName;
            group.Properties.TryGetValue("group_name", out groupName);
            subGroup.Properties.TryGetValue("group_name", out subGroupName);
            Console.WriteLine("\nAdding group <" + subGroupName + "> to group <" + groupName + ">...");
            Group memberGroup = new Group();
            memberGroup.Href = LinkRelations.FindLinkAsString(subGroup.Links, LinkRelations.SELF.Rel);
            group.AddSubGroup(memberGroup);
            if (printResult)
            {
                Console.WriteLine("\nSuccessfully add the sub-group to the group.");
            }
        }

        private static void RemoveUserFromGroup(Repository repository, Group group, User user, bool printResult)
        {
            object groupName;
            object userName;
            group.Properties.TryGetValue("group_name", out groupName);
            user.Properties.TryGetValue("user_name", out userName);
            string userRelationUri="";
            //find the user resouce in this group 
            string filter = "user_name='" + userName + "'";
            Feed<User> users = group.GetGroupUsers<User>(new FeedGetOptions { ItemsPerPage = 10, IncludeTotal = true, Filter = filter });
            int total = users.Total;            
            if (total==1)
            {
                //find the delete user group membership relation link in the entry links
                userRelationUri = FindRelationLinkInEntryLinks(users.Entries[0].Links);
            }
            if (userRelationUri.Equals(""))
            {
                Console.WriteLine("\nCannt remove the user <"+userName+"> from the group <"+groupName+">, as the user is not a member of this group now.");
            }else if (userRelationUri == null)
            {
                Console.WriteLine("\nFailed to get the relation uri from the group-users entry links.");
            }else
            {
                repository.RemoveRelationBetweenGroupAndMember(userRelationUri);
                Console.WriteLine("\nSuccessfully remove the user <" + userName + "> from the group <" + groupName + ">.");
            }

        }

        private static void RemoveGroupFromGroup(Repository repository, Group group, Group subGroup, bool printResult)
        {
            object groupName;
            object subGroupName;
            group.Properties.TryGetValue("group_name", out groupName);
            subGroup.Properties.TryGetValue("group_name", out subGroupName);
            string groupRelationUri = "";
            //find the user resouce in this group 
            string filter = "group_name='" + subGroupName + "'";
            Feed<Group> groups = group.GetGroupGroups<Group>(new FeedGetOptions { ItemsPerPage = 10, IncludeTotal = true, Filter = filter });
            int total = groups.Total;
            if (total == 1)
            {
                //find the delete group membership relation link in the entry links
                groupRelationUri = FindRelationLinkInEntryLinks(groups.Entries[0].Links);
            }
            if (groupRelationUri.Equals(""))
            {
                Console.WriteLine("\nCannt remove the sub-group <" + subGroupName + "> from the group <" + groupName + ">, as the user is not a member of this group now.");
            }
            else if (groupRelationUri == null)
            {
                Console.WriteLine("\nFailed to get the relation uri from the group-groups entry links.");
            }
            else
            {
                repository.RemoveRelationBetweenGroupAndMember(groupRelationUri);
                Console.WriteLine("\nSuccessfully remove the sub-group <" + subGroupName + "> from the group <" + groupName + ">.");
            }

        }

        private static string FindRelationLinkInEntryLinks(List<Link> links)
        {           
            foreach(Link link in links)
            {
                if (link.Rel.Equals(LinkRelations.DELETE.Rel))
                {
                    return link.Href;
                }
            }
            return null;
        }


        private static Group CreateOrGetGroup(Repository repository, string groupName, bool printResult)
        {
            Group group;
            if (string.IsNullOrWhiteSpace(groupName))
            {
                groupName = "Net_Sample_Test_Group" + new Random().Next();  
            }
            group = CreateGroupIfNeed(repository, printResult, groupName);
            return group;
        }

        private static User CreateOrGetUser(Repository repository, string userName, bool printResult)
        {
            User user;
            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = "Net_Sample_Test_User" + new Random().Next();
            }
            user = CreateUserIfNeed(repository, printResult, userName);
            return user;
        }

        private static User CreateUserIfNeed(Repository repository, bool printResult, string userNameToCreate)
        {
            string filter = "user_name='" + userNameToCreate + "' or user_login_name='" + userNameToCreate + "'";
            FeedGetOptions options = new FeedGetOptions() { ItemsPerPage = 10, Inline = true, IncludeTotal = true, Filter = filter };
            Feed<User> users = repository.GetUsers<User>(options);
            User user;
            int total = users.Total;
            if (total > 0)
            {
                user = GetUserFromFeed(users, userNameToCreate);
            }
            else
            {
                user = CreateUser(repository, userNameToCreate);
            }

            if (printResult)
            {
                Console.WriteLine(user.ToString());
            }
            return user;
        }

        private static User GetUserFromFeed(Feed<User> users, string userNameToCreate)
        {
            Console.WriteLine("\n\tSkip creating user, as there is already a user whose user_name or/and user_login_name is '" + userNameToCreate + "' in server");
            List<Entry<User>> entries = users.Entries;
            if (entries.Count == 1)
            {
                return entries.ElementAt(0).Content;
            }
            else
            {
                return users.FindInlineEntry(userNameToCreate);
            }
        }

        private static User CreateUser(Repository repository, string userNameToCreate)
        {
            Console.WriteLine("\n\t\tCreates a user with the same user_name and user_login_name: '" + userNameToCreate + "'");
            User toCreatedUser = new User();
            toCreatedUser.SetPropertyValue("user_name", userNameToCreate);
            toCreatedUser.SetPropertyValue("user_login_name", userNameToCreate);
            toCreatedUser.SetPropertyValue("default_folder", "/" + userNameToCreate);

            return repository.CreateUser(toCreatedUser, null);
        }

        private static Group CreateGroupIfNeed(Repository repository, bool printResult, string groupNameToCreate)
        {
            string filter = "group_name='" + groupNameToCreate + "'";
            FeedGetOptions options = new FeedGetOptions() { ItemsPerPage = 10, Inline = true, IncludeTotal = true, Filter = filter };
            Feed<Group> groups = repository.GetGroups<Group>(options);
            Group group;
            int total = groups.Total;
            if (total > 0)
            {                
                group = GetGroupFromFeed(groups, groupNameToCreate);
            }
            else
            {               
                group = CreateGroup(repository, groupNameToCreate);
            }

            if (printResult)
            {
                Console.WriteLine(group.ToString());
            }
            return group;
        }

        private static Group GetGroupFromFeed(Feed<Group> groups, string groupNameToCreate)
        {
            Console.WriteLine("\n\tSkip creating group, as there is already a group whose group_name is '" + groupNameToCreate + "' in server");
            List<Entry<Group>> entries = groups.Entries;
            if (entries.Count == 1)
            {
                return entries.ElementAt(0).Content;
            }
            else
            {
                return groups.FindInlineEntry(groupNameToCreate);
            }
        }

        private static Group CreateGroup(Repository repository, string groupNameToCreate)
        {
            Console.WriteLine("\n\t\tCreates a group: '" + groupNameToCreate + "'");
            Group toCreatedGroup = new Group();
            toCreatedGroup.SetPropertyValue("group_name", groupNameToCreate);

            return repository.CreateGroup(toCreatedGroup, null);
        }
             

        private static void DeleteGroup(Group group)
        {
            group.Delete();
        }

        private static void DeleteUser(User user)
        {
            
           user.Delete();           
        }       
              
    }
}
