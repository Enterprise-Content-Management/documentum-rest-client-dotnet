using Emc.Documentum.Rest.Net;
using System.Collections.Generic;

namespace Emc.Documentum.Rest.DataModel
{
    public enum DuplicateType
    {
        /// <summary>
        /// EMAIL was found in the same folder it was being imported into
        /// </summary>
        FOLDER,
        /// <summary>
        /// EMAIL was found somewhere else in the system.
        /// </summary>
        SYSTEM,
    }

    /// <summary>
    /// Email Package entry
    /// </summary>
    public class EmailPackage
    {
        #region Constructors

        //TODO: Add param descriptions for IntelliSense
        /// <summary>
        /// Email Package entry constructor
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="isDuplicate"></param>
        public EmailPackage(Document doc, bool isDuplicate)
        {
            Email = doc;
            Attachments = new List<Document>();
            _duplicate = isDuplicate;
        }

        #endregion

        #region Properties

        private bool _duplicate = false;
        /// <summary>
        /// Returns whether the email already exists in the system
        /// </summary>
        public bool IsDuplicate
        {
            get { return _duplicate; }
            set { _duplicate = value; }
        }

        /// <remarks>
        /// If the email is a duplicate, was found in the same FOLDER or somewhere else in the SYSTEM
        /// </remarks>
        public DuplicateType DuplicateType { get; set; }

        public Document Email { get; set; }

        public List<Document> Attachments { get; set; }

        #endregion               

        #region Methods

        public override string ToString()
        {
            JsonDotnetJsonSerializer serializer = new JsonDotnetJsonSerializer();
            return serializer.Serialize(this);
        }

        #endregion
    }
}
