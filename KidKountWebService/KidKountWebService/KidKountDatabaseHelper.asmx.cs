using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data.MySqlClient;

/*  SWIFT PROTOCOLS - Updated 9/8/2014
 * 
protocol DbClassHelper{

    func getChildrenInSession(name: String) -> [ChildModel]
    func addChildToSession(child: ChildModel, sessionName: String)
    func removeChildFromSession(child: ChildModel)
}
protocol DbMemberHelper{
    func addMember(member: MemberModel) 
    func getMemberParents() -> [MemberParentModel] -- DONE
    func getChildrenOfMemberParent(parent: ParentModel) -> [MemberChildModel] -- DONE
    func getMember(parent: ParentModel) -> MemberModel
    func updateChild(child: ChildModel) -- DONE 
    func removeChild(child: ChildModel) -- DONE
    func addChildForParent(child: MemberChildModel, parent: MemberParentModel) -- DONE
    func updateParent(parent: ParentModel) -- DONE
    func removeParent(parent: ParentModel) -- DONE
    func addParentForChild(parent: MemberParentModel, child: MemberChildModel) -- DONE
}
 */


namespace KidKountWebService
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://KidKount.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    
    

    public class KidKountDatabaseHelper : System.Web.Services.WebService
    {
        

        //WEB FUNCTIONS 
        [WebMethod]
        public bool TestConnection()
        {
            return DatabaseConnect.Instance.TestConnection();
        }

        //CHILD FUNCTIONS
        [WebMethod]
        public ChildModel GetChild(string uuid)
        {
            return MemberHelper.Instance.GetChild(uuid);
        }

        [WebMethod]
        public void AddChild(string name, string notes, string uuid, string member)
        {
            MemberHelper.Instance.AddChild(name, notes, uuid, member);
        } 

        [WebMethod]
        public void UpdateChild(string name, string notes, string uuid)
        {
            var child = new ChildModel();
            child.name = name;
            child.notes = notes;
            child.uuid = uuid;

            MemberHelper.Instance.UpdateChild(child);
        }

        [WebMethod]
        public void DeleteChild(string uuid)
        {
            MemberHelper.Instance.DeleteChild(uuid);
        }

        //PARENT FUNCTIONS
        [WebMethod]
        public ParentModel GetParent(string uuid){
            return MemberHelper.Instance.GetParent(uuid);
        }

        [WebMethod]
        public void AddParent(string name, string uuid, string member, string[] email, string[] phone)
        {
            var newParent = new ParentModel();
            newParent.name = name;
            newParent.uuid = uuid;
            newParent.member = member;
            newParent.email = email;
            newParent.phone = phone;

            MemberHelper.Instance.AddParent(newParent);
         }

        [WebMethod]
        public void AddParentTEST(string name, string uuid, string member)
        {
            string[] email = {"email1", "email2", "email3"};
            string[] phone = {"phone1", "phone2", "phone3"};
            AddParent(name, uuid, member, email, phone);
        }

        [WebMethod]
        public void UpdateParent(string name, string uuid, string[] email, string[] phone)
        {
            var updateParent = new ParentModel();
            updateParent.name = name;
            updateParent.uuid = uuid;
            updateParent.email = email;
            updateParent.phone = phone;

            MemberHelper.Instance.UpdateParent(updateParent);
        }

        [WebMethod]
        public void UpdateParentTEST(string name, string uuid)
        {
            string[] email = { "emailUpdate1", "emailUpdate2", "emailUpdate3" };
            string[] phone = { "phoneUpdate1", "phoneUpdate2", "phoneUpdate3" };
            UpdateParent(name, uuid, email, phone);
        }

        [WebMethod]
        public void DeleteParent(string uuid)
        {
            MemberHelper.Instance.DeleteParent(uuid);
        }

        [WebMethod]
        public void DeleteGuestParent(string uuid)
        {
            MemberHelper.Instance.TryDeleteGuestParent(uuid);
        }

        [WebMethod]
        public ParentModel[] GetParentsOfChild(string uuid)
        {
            return MemberHelper.Instance.GetParentsOf(uuid);
        }

        //MEMBER FUNCTIONS 
        [WebMethod]
        public ParentModel[] GetAllMemberParents()
        {
            return MemberHelper.Instance.GetAllMemberParents();
        }

        [WebMethod]
        public ChildModel[] GetAllMemberChildren()
        {
            return MemberHelper.Instance.GetAllMemberChildren();
        }

        [WebMethod]
        public ChildModel[] GetChildrenOfParent(string uuid)
        {
            var parent = new ParentModel(uuid);

            var children = MemberHelper.Instance.GetChildrenOf(parent);

            return children;
        }

        [WebMethod]
        public string GetMemberOfParent(string uuid)
        {
            return MemberHelper.Instance.GetMemberOfParent(uuid);
        }

        [WebMethod]
        public string GetMemberOfChild(string uuid)
        {
            return MemberHelper.Instance.GetMemberOfChild(uuid);
        }

        [WebMethod]
        public void AddMemberId(string uuid)
        {
            MemberHelper.Instance.AddUpdateMember(uuid);
        }

        //SESSION FUNCTIONS 
        [WebMethod]
        public void AddChildToSession(string session, string child, string parent)
        {
            MemberHelper.Instance.AddChildToSession(session, child, parent);
        }

        [WebMethod]
        public void RemoveChildFromSession(string child)
        {
            MemberHelper.Instance.RemoveChildFromSession(child);
        }

        [WebMethod]
        public ChildModel[] GetChildrenInSession(string session)
        {
            return MemberHelper.Instance.GetChildrenInSession(session);
        }

        //MODEL FUNCTIONS 
        [WebMethod]
        public ParentModel GetCheckInParentOf(string uuid)
        {
            return MemberHelper.Instance.GetCheckInParentOf(uuid); 
        }

        //DATABASE MANAGEMENT STUFF 
        [WebMethod]
        public void CreateTables()
        {
            CreateKidKountTables();
        }

        //CLASS FUNCTIONS
        private void CreateNewDatabase()
        {
            var query = "CREATE DATABASE " + Constants.DATABASE_NAME;

            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var cmd = new MySqlCommand(query, DatabaseConnect.Instance.connection);
                cmd.ExecuteNonQuery();
                DatabaseConnect.Instance.CloseConnection();
            }
        }

        private void CreateKidKountTables()
        {
            var sessionTableQuery =
                "CREATE TABLE " + Constants.SESSION_TABLE +
                "(" +
                "ID INT NOT NULL AUTO_INCREMENT," +
                "name VARCHAR(255)," +
                "uuid VARCHAR(255)," +
                "PRIMARY KEY(ID)" +
                ");";

            var childrenTableQuery =
                "CREATE TABLE " + Constants.CHILDREN_TABLE +
                "(" +
                "ID INT NOT NULL AUTO_INCREMENT," +
                "name VARCHAR(255)," +
                "notes VARCHAR(255)," +
                "uuid VARCHAR(255)," +
                "checkInParent VARCHAR(255)," +
                "member VARCHAR(255)," +
                "session VARCHAR(255)," +
                "PRIMARY KEY(ID)" +
                ");";

            var parentTableQuery =
                "CREATE TABLE " + Constants.PARENT_TABLE +
                "(" +
                "ID INT NOT NULL AUTO_INCREMENT," +
                "name VARCHAR(255)," +
                "uuid VARCHAR(255)," +
                "member VARCHAR(255)," +
                "PRIMARY KEY(ID)" +
                ");";

            var linkedParentTableQuery =
                "CREATE TABLE " + Constants.LINKED_PARENT_TABLE +
                "(" +
                "ID INT NOT NULL AUTO_INCREMENT," +
                "member VARCHAR(255)," +
                "parent VARCHAR(255)," +
                "PRIMARY KEY(ID)" +
                ");";

            var memberTableQuery =
                "CREATE TABLE " + Constants.MEMBER_TABLE +
                "(" +
                "ID INT NOT NULL AUTO_INCREMENT," +
                "uuid VARCHAR(255)," +
                "PRIMARY KEY(ID)" +
                ");";

            var emailTableQuery =
                "CREATE TABLE " + Constants.EMAIL_TABLE +
                "(" +
                "ID INT NOT NULL AUTO_INCREMENT," +
                "email VARCHAR(255)," +
                "parent VARCHAR(255)," +
                "PRIMARY KEY(ID)" +
                ");";

            var phoneTableQuery =
                "CREATE TABLE " + Constants.PHONE_TABLE +
                "(" +
                "ID INT NOT NULL AUTO_INCREMENT," +
                "phone VARCHAR(255)," +
                "parent VARCHAR(255)," +
                "PRIMARY KEY(ID)" +
                ");";

            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var sessionCmd = new MySqlCommand(sessionTableQuery, DatabaseConnect.Instance.connection);
                var childrenCmd = new MySqlCommand(childrenTableQuery, DatabaseConnect.Instance.connection);
                var parentCmd = new MySqlCommand(parentTableQuery, DatabaseConnect.Instance.connection);
                var linkedParentCmd = new MySqlCommand(linkedParentTableQuery, DatabaseConnect.Instance.connection);
                var memberCmd = new MySqlCommand(memberTableQuery, DatabaseConnect.Instance.connection);
                var emailCmd = new MySqlCommand(emailTableQuery, DatabaseConnect.Instance.connection);
                var phoneCmd = new MySqlCommand(phoneTableQuery, DatabaseConnect.Instance.connection);

                sessionCmd.ExecuteNonQuery();
                childrenCmd.ExecuteNonQuery();
                parentCmd.ExecuteNonQuery();
                linkedParentCmd.ExecuteNonQuery();
                memberCmd.ExecuteNonQuery();
                emailCmd.ExecuteNonQuery();
                phoneCmd.ExecuteNonQuery();

                DatabaseConnect.Instance.CloseConnection();
            }
        }
        
    }
}