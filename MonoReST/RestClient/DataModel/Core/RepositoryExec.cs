using Emc.Documentum.Rest.Net;
using Emc.Documentum.Rest.Http.Utility;
using System;
using System.Collections.Generic;

namespace Emc.Documentum.Rest.DataModel
{
    public partial class Repository
    {
        /// <summary>
        /// Get current login user resource
        /// </summary>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public User GetCurrentUser(SingleGetOptions options)
        {
            return Client.GetSingleton<User>(
                this.Links,
                LinkRelations.CURRENT_USER.Rel,
                options);
        }


        /// <summary>
        /// Get cabinets feed in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public Feed<T> GetCabinets<T>(FeedGetOptions options) where T : Cabinet
        {
            return Client.GetFeed<T>(
                this.Links,
                LinkRelations.CABINETS.Rel,
                options);
        }

        /// <summary>
        /// Get users feed in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public Feed<T> GetUsers<T>(FeedGetOptions options) where T : User
        {
            return Client.GetFeed<T>(
                this.Links,
                LinkRelations.USERS.Rel,
                options);
        }

        /// <summary>
        /// Create a new user in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>User</returns>
        public T CreateUser<T>(T newUser, SingleGetOptions options) where T : User
        {
            return Client.Post<T>(
                this.Links,
                LinkRelations.USERS.Rel,
                newUser,
                options);
        }

        /// <summary>
        /// Get groups in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public Feed<T> GetGroups<T>(FeedGetOptions options) where T : Group
        {
            return Client.GetFeed<T>(
                this.Links,
                LinkRelations.GROUPS.Rel,
                options);
        }

        /// <summary>
        /// Create a new group in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>Group</returns>
        public T CreateGroup<T>(T newGroup, SingleGetOptions options) where T : Group
        {
            return Client.Post<T>(
                this.Links,
                LinkRelations.GROUPS.Rel,
                newGroup,
                options);
        }

        /// <summary>
        /// Remove the relation between group and it's user or group member
        /// </summary>      
        /// <param name="groupRelationUri">The relation resource uri between the group and its member</param>        
        public void RemoveRelationBetweenGroupAndMember(string groupRelationUri)
        {
            Client.Delete(groupRelationUri);
        }

        /// <summary>
        /// Get formats in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public Feed<T> GetFormats<T>(FeedGetOptions options) where T : Format
        {
            return Client.GetFeed<T>(
                this.Links,
                LinkRelations.FORMATS.Rel,
                options);
        }

        /// <summary>
        /// Get relations in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public Feed<T> GetRelations<T>(FeedGetOptions options) where T : Relation
        {
            return Client.GetFeed<T>(
                this.Links,
                LinkRelations.RELATIONS.Rel,
                options);
        }

        /// <summary>
        /// Get relation types in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public Feed<T> GetRelationTypes<T>(FeedGetOptions options) where T : RelationType
        {
            return Client.GetFeed<T>(
                this.Links,
                LinkRelations.RELATION_TYPES.Rel,
                options);
        }

        /// <summary>
        /// Create a new relation types in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>Relation</returns>
        public T CreateRelation<T>(T newRelation, SingleGetOptions options) where T : Relation
        {
            return Client.Post<T>(
                this.Links,
                LinkRelations.RELATIONS.Rel,
                newRelation,
                options);
        }

        /// <summary>
        /// Get network locations in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public Feed<T> GetNetworkLocations<T>(FeedGetOptions options) where T : NetworkLocation
        {
            return Client.GetFeed<T>(
                this.Links,
                Emc.Documentum.Rest.Http.Utility.LinkRelations.NETWORK_LOCATIONS.Rel,
                options);
        }

        /// <summary>
        /// Get types in this repository
        /// </summary>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public Feed<DmType> GetTypes(FeedGetOptions options)
        {
            return Client.GetFeed<DmType>(
                this.Links,
                LinkRelations.TYPES.Rel,
                options);
        }

        /// <summary>
        ///  Get checked out PersistentObjects in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public Feed<T> GetCheckedOutObjects<T>(FeedGetOptions options) where T : PersistentObject
        {
            return Client.GetFeed<T>(
                this.Links,
                LinkRelations.CHECKED_OUT_OBJECTS.Rel,
                options);
        }

        /// <summary>
        /// Execute a DQL query in this repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dql"></param>
        /// <param name="options"></param>
        /// <returns>Feed</returns>
        public Feed<T> ExecuteDQL<T>(string dql, FeedGetOptions options)
        {
            decimal count = 0;
            double pageCount = 0;

            if (options == null) options = new FeedGetOptions();
            string dqlUri = LinkRelations.FindLinkAsString(this.Links, LinkRelations.DQL.Rel);
            string dqlUriWithoutTemplateParams = dqlUri.Substring(0, dqlUri.IndexOf("{"));

            /******************** BEGIN GET TOTAL IF SPECIFIED *****************************/
            if (options != null && options.IncludeTotal)
            {
                String countDql = "select count(*) as total " + dql.Substring(dql.IndexOf("from"));
                List<KeyValuePair<string, object>> cl = options.ToQueryList();
                cl.Add(new KeyValuePair<string, object>("dql", countDql));
                Feed<PersistentObject> countFeed = Client.Get<Feed<PersistentObject>>(dqlUriWithoutTemplateParams, cl);
                List<Entry<PersistentObject>> res = (List<Entry<PersistentObject>>)countFeed.Entries;

                foreach (Entry<PersistentObject> obj in res) // There will only be one result
                {
                    count = decimal.Parse(obj.Content.GetPropertyValue("total").ToString());
                }
            }
            /********************* END GET TOTAL IF SPECIFIED ******************************/

            // Now execute the real query
            List<KeyValuePair<string, object>> pa = options.ToQueryList();
            pa.Add(new KeyValuePair<string, object>("dql", dql));
            Feed<T> feed = this.Client.Get<Feed<T>>(dqlUriWithoutTemplateParams, pa);
            if (count > 0)
            {
                feed.Total = (int)count;
            }
            return feed;
        }


        /// <summary>
        ///  Executes a Full Text Query against this repository with simple search language
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="search"></param>
        /// <returns>Feed</returns>
        public Feed<T> ExecuteSimpleSearch<T>(SearchOptions search) where T : PersistentObject
        {
            decimal count = 0;
            double pageCount = 0;

            if (search == null) search = new SearchOptions();
            string searchUri = LinkRelations.FindLinkAsString(this.Links, LinkRelations.SEARCH.Rel);
            List<KeyValuePair<string, object>> pa = search.ToQueryList();
            Feed<T> feed = this.Client.Get<Feed<T>>(searchUri, pa);
            return feed;
        }

        /// <summary>
        /// Create a synchronous batch
        /// </summary>
        /// <param name="options"></param>
        /// <returns>batch</returns>
        public Batch CreateBatch(Batch batch)
        {
            return Client.Post<Batch>(
                this.Links,
                LinkRelations.BATCHES.Rel,
                batch,
                null);
        }

        /// <summary>
        /// Get batch capabilities
        /// </summary>
        /// <param name="options"></param>
        /// <returns>batch capabilities</returns>
        public BatchCapabilities GetBatchCapabilities()
        {
            return Client.GetSingleton<BatchCapabilities>(
                this.Links,
                LinkRelations.BATCH_CAPABILITIES.Rel,
                null);
        }

        /// <summary>
        /// This gets a folder object from the feed then fetches the full folder object. 
        /// </summary>
        /// <param name="dql"></param>
        /// <param name="options"></param>
        /// <returns>Folder</returns>
        public Folder GetFolderByQualification(string dql, FeedGetOptions options)
        {
            return GetObjectByQualification<Folder>(dql, options);
        }

        /// <summary>
        /// Get a document by a Dql qualification 
        /// </summary>
        /// <param name="dql"></param>
        /// <param name="options"></param>
        /// <returns>RestDocument</returns>
        public Document GetDocumentByQualification(string dql, SingleGetOptions options)
        {
            return GetObjectByQualification<Document>(dql, options);
        }

        /// <summary>
        /// Get sysobject by ID.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public T GetSysObjectById<T>(string objectId) where T : PersistentObject
        {
            SingleGetOptions options = new SingleGetOptions { Links = true };
            return GetSysObjectById<T>(objectId, options);
        }

        /// <summary>
        /// Get sysobject by object ID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectId"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public T GetSysObjectById<T>(string objectId, SingleGetOptions options) where T : PersistentObject
        {
            return GetObjectByQualification<T>(String.Format("dm_sysobject where r_object_id='{0}'", objectId), options);
        }

        /// <summary>
        /// Get object by dql qualification
        /// </summary>
        /// <param name="dql"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public PersistentObject GetObjectByQualification(string dql, SingleGetOptions options)
        {
            return GetObjectByQualification<PersistentObject>(dql, options);
        }

        /// <summary>
        /// Get object by dql qualification
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dql"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public T GetObjectByQualification<T>(string dql, SingleGetOptions options)
        {
            dql = "select r_object_id from " + dql;
            Feed<PersistentObject> feed = ExecuteDQL<PersistentObject>(dql, new FeedGetOptions { Inline = false });
            if (feed.Entries == null || feed.Entries.Count == 0)
            {
                return default(T);
            }
            if (feed.Entries.Count > 1)
            {
                throw new Exception("The qualification '" + dql + "' has more than one object in repository.");
            }
            return Client.GetSingleton<T>(feed.Entries[0].Links, LinkRelations.EDIT.Rel, options);
        }

        private string _cabinetType = "dm_cabinet";
        /// <summary>
        /// When creating a new cabinet object, this will be the object type used. Defaults to 'dm_cabinet'.
        /// </summary>
        private string CabinetType
        {
            get { return _cabinetType; }
            set { _cabinetType = value; }
        }

        private string _documentType = "dm_document";
        /// <summary>
        /// When creating a new document object, this will be the object type used. Defaults to 'dm_document'.
        /// </summary>
        public string DocumentType
        {
            get { return _documentType; }
            set { _documentType = value; }
        }

        private string _folderType = "dm_folder";
        /// <summary>
        /// When creating a folder object, this will be the default type used. Defaults to 'dm_folder'.
        /// </summary>
        public string FolderType
        {
            get { return _folderType; }
            set { _folderType = value; }
        }

    } // End Repository Class
}
