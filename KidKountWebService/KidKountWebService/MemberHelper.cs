using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data;

namespace KidKountWebService
{
    public class MemberHelper
    {
        //SINGLETON 
        private static MemberHelper instance;

        public static MemberHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MemberHelper();
                }
                return instance;
            }
        }

        private MemberHelper() { }
        //SINLETON END 

        /*
         *  PUBLIC FUNCTIONS
         * 
         */

        //CHILD FUNCTIONS
        public ChildModel GetChild(string uuid)
        {
            var query = "SELECT * FROM " + Constants.CHILDREN_TABLE + " WHERE uuid='" + uuid + "';";
            var child = new ChildModel();

            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, DatabaseConnect.Instance.connection);
                var dataReader = cmd.ExecuteReader();

                if (dataReader.Read())
                {
                    child.name = dataReader["name"] as string;
                    child.notes = dataReader["notes"] as string;
                    child.uuid = dataReader["uuid"] as string;
                    child.member = dataReader["member"] as string;
                }

                dataReader.Close();
                DatabaseConnect.Instance.CloseConnection();
            }

            return child;
        }

        public void AddChild(string name, string notes, string uuid, string member)
        {
            var query = "INSERT INTO " + Constants.CHILDREN_TABLE + " (name, notes, uuid, member) " +
                "VALUES('" + name + "','" + notes + "','" + uuid + "','" + member + "')";
            DatabaseConnect.Instance.NonQueryQuickExecute(query);
        }

        public void DeleteChild(string uuid)
        {
            var memberId = GetMemberOfChild(uuid);
            var query = "DELETE FROM " + Constants.CHILDREN_TABLE + " WHERE uuid='" + uuid + "';";
            DatabaseConnect.Instance.NonQueryQuickExecute(query);

            TryRemoveMemberId(memberId);
        }

        public void UpdateChild(ChildModel child)
        {
            var query = "UPDATE " + Constants.CHILDREN_TABLE +
                " SET name='" + child.name + "'," +
                " notes='" + child.notes + "'" +
                " WHERE uuid='" + child.uuid + "';";
            DatabaseConnect.Instance.NonQueryQuickExecute(query);
        }
        

        //PARENT FUNCTIONS 
        public ParentModel GetParent(string uuid)
        {
            var query = "SELECT * FROM " + Constants.PARENT_TABLE + " WHERE uuid='"
                + uuid + "';";
            var emailQuery = "SELECT * FROM " + Constants.EMAIL_TABLE + " WHERE parent='"
                + uuid + "';";
            var phoneQuery = "SELECT * FROM " + Constants.PHONE_TABLE + " WHERE parent='"
                + uuid + "';";
            var parent = new ParentModel();

            

            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, DatabaseConnect.Instance.connection);
                var emailCmd = new MySql.Data.MySqlClient.MySqlCommand(emailQuery, DatabaseConnect.Instance.connection);
                var phoneCmd = new MySql.Data.MySqlClient.MySqlCommand(phoneQuery, DatabaseConnect.Instance.connection);
                
                var dataReader = cmd.ExecuteReader();
                dataReader.Read();
                parent.name = dataReader["name"] as string;
                parent.uuid = uuid;
                parent.member = dataReader["member"] as string;
                dataReader.Close();

                var emailReader = emailCmd.ExecuteReader();
                var emailList = new List<string>();
                while (emailReader.Read())
                {
                    emailList.Add(emailReader["email"] as string);
                }
                parent.email = emailList.ToArray<string>();
                emailReader.Close();

                var phoneReader = phoneCmd.ExecuteReader();
                var phoneList = new List<string>();
                while (phoneReader.Read())
                {
                    phoneList.Add(phoneReader["phone"] as string);
                }
                parent.phone = phoneList.ToArray<string>();
                phoneReader.Close();
                DatabaseConnect.Instance.CloseConnection();
            }

            return parent;
        }

        public void AddParent(ParentModel parent){
            var query = "INSERT INTO " + Constants.PARENT_TABLE + " (name, uuid, member) " 
                + "VALUES('" + parent.name + "','" + parent.uuid + "','" + parent.member + "');";
            DatabaseConnect.Instance.NonQueryQuickExecute(query);

            foreach (string email in parent.email){
                AddEmailFor(email, parent.uuid);
            }
            foreach (string phone in parent.phone){
                AddPhoneFor(phone, parent.uuid);
            }
        }

        public void UpdateParent(ParentModel parent)
        {
            DeleteAllEmailFor(parent.uuid);
            DeleteAllPhoneFor(parent.uuid);

            var query = "UPDATE " + Constants.PARENT_TABLE +
                " SET name='" + parent.name + "' " +
                "WHERE uuid='" + parent.uuid + "';";
            DatabaseConnect.Instance.NonQueryQuickExecute(query);

            foreach (string email in parent.email)
            {
                AddEmailFor(email, parent.uuid);
            }

            foreach (string phone in parent.phone)
            {
                AddPhoneFor(phone, parent.uuid);
            }
        }

        public void DeleteParent(string uuid)
        {
            var memberId = GetMemberOfParent(uuid);

            var parentQuery = "DELETE FROM " + Constants.PARENT_TABLE +
                " WHERE uuid='" + uuid + "';";
            var emailQuery = "DELETE FROM " + Constants.EMAIL_TABLE +
                " WHERE parent='" + uuid + "';";
            var phoneQuery = "DELETE FROM " + Constants.PHONE_TABLE +
                " WHERE parent='" + uuid + "';";

            DatabaseConnect.Instance.NonQueryQuickExecute(parentQuery);
            DatabaseConnect.Instance.NonQueryQuickExecute(emailQuery);
            DatabaseConnect.Instance.NonQueryQuickExecute(phoneQuery);

            TryRemoveMemberId(memberId);
        }

        public void TryDeleteGuestParent(string uuid)
        {
            var checkQuery = "SELECT * FROM " + Constants.CHILDREN_TABLE +
                " WHERE checkInParent='" + uuid + "';";

            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var checkCmd = new MySql.Data.MySqlClient.MySqlCommand(checkQuery, DatabaseConnect.Instance.connection);
                var checkReader = checkCmd.ExecuteReader();
                if (!checkReader.HasRows)
                {
                    checkReader.Close();
                    DatabaseConnect.Instance.CloseConnection();
                    MemberHelper.Instance.DeleteParent(uuid);
                }
                else
                {
                    checkReader.Close();
                }

                
            }
        }

        public ParentModel[] GetParentsOf(string childUuid)
        {
            var parents = new ParentModel[0];
            var memberId = GetMemberOfChild(childUuid);
            var query = "SELECT * FROM " + Constants.PARENT_TABLE +
                " WHERE member='" + memberId + "';";

            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, DatabaseConnect.Instance.connection);
                var reader = cmd.ExecuteReader();
                var parentList = new List<ParentModel>();

                while (reader.Read())
                {
                    var parent = new ParentModel();
                    parent.uuid = reader["uuid"] as string;
                    parentList.Add(parent);
                }
                reader.Close();
                parents = parentList.ToArray<ParentModel>();

                foreach (ParentModel parent in parents)
                {
                    parent.Populate();
                }

                DatabaseConnect.Instance.CloseConnection();
            }

            return parents;
        }

        public ParentModel GetCheckInParentOf(string uuid)
        {
            var parent = new ParentModel();
            var query = "SELECT * FROM " + Constants.CHILDREN_TABLE + " WHERE uuid='"
                + uuid + "';";
            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var childCmd = new MySql.Data.MySqlClient.MySqlCommand(query, DatabaseConnect.Instance.connection);
                var parentUuid = "";
                var childReader = childCmd.ExecuteReader();
                if (childReader.Read())
                {
                    parentUuid = childReader["checkInParent"] as string;
                }
                childReader.Close();

                parent.uuid = parentUuid;

                DatabaseConnect.Instance.CloseConnection();
            }

            parent.Populate();

            return parent;
        }

        //MEMBER FUNCTIONS 
        public void AddUpdateMember(string uuid)
        {
            var checkQuery = "SELECT * FROM " + Constants.MEMBER_TABLE + " WHERE uuid='"
                + uuid + "';";
            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(checkQuery, DatabaseConnect.Instance.connection);
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    var addQuery = "INSERT INTO " + Constants.MEMBER_TABLE + " (uuid) "
                        + "VALUES('" + uuid + "')";
                    DatabaseConnect.Instance.NonQueryQuickExecute(addQuery);
                }
                else
                {
                    reader.Close();
                }
                DatabaseConnect.Instance.CloseConnection();
            }
        }

        public ParentModel[] GetAllMemberParents()
        {
            var parents = new ParentModel[0];
            var query = "SELECT * FROM " + Constants.PARENT_TABLE +
                " WHERE member<>'';";
            if (DatabaseConnect.Instance.OpenConnection())
            {
                var parentList = new List<ParentModel>();
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, DatabaseConnect.Instance.connection);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var parent = new ParentModel();
                    parent.uuid = reader["uuid"] as string;
                    parentList.Add(parent);
                }
                reader.Close();

                parents = parentList.ToArray<ParentModel>();
                foreach (ParentModel parent in parents)
                {
                    parent.Populate();
                }

                DatabaseConnect.Instance.CloseConnection();
            }

            return parents;
        }

        public ChildModel[] GetAllMemberChildren()
        {
            var children = new ChildModel[0];
            var query = "SELECT * FROM " + Constants.CHILDREN_TABLE +
                " WHERE member<>'';";

            if (DatabaseConnect.Instance.OpenConnection())
            {
                var childrenList = new List<ChildModel>();
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, DatabaseConnect.Instance.connection);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var child = new ChildModel();
                    child.uuid = reader["uuid"] as string;
                    childrenList.Add(child);
                }
                reader.Close();

                children = childrenList.ToArray<ChildModel>();
                foreach (ChildModel child in children)
                {
                    child.Populate();
                }

                DatabaseConnect.Instance.CloseConnection();
            }

            return children;
        }

        public ChildModel[] GetChildrenOf(ParentModel parent)
        {
            var children = new ChildModel[0];
            var parentQuery = "SELECT * FROM " + Constants.PARENT_TABLE +
                " WHERE uuid='" + parent.uuid + "';";

            if (DatabaseConnect.Instance.OpenConnection()) 
            {
                var parentCmd = new MySql.Data.MySqlClient.MySqlCommand(parentQuery, DatabaseConnect.Instance.connection);
                var parentReader = parentCmd.ExecuteReader();
                parentReader.Read();

                var member = parentReader["member"] as string;
                parentReader.Close();

                var childQuery = "SELECT * FROM " + Constants.CHILDREN_TABLE +
                    " WHERE member='" + member + "';";
                var childCmd = new MySql.Data.MySqlClient.MySqlCommand(childQuery, DatabaseConnect.Instance.connection);
                var childReader = childCmd.ExecuteReader();
                var childList = new List<ChildModel>();

                while (childReader.Read())
                {
                    var child = new ChildModel();
                    child.uuid = childReader["uuid"] as string;

                    childList.Add(child);
                }
                childReader.Close();

                children = childList.ToArray<ChildModel>();

                foreach (ChildModel child in children)
                {
                    child.Populate();
                }

                DatabaseConnect.Instance.CloseConnection();
            }

            return children;
        }

        // SESSION FUNCTIONS 

        public void AddChildToSession(string sessionName, string childUuid, string parentUuid)
        {
            var query = "INSERT INTO " + Constants.SESSION_TABLE + " (name, uuid) " +
                "VALUES('" + sessionName + "','" + childUuid + "')";

            DatabaseConnect.Instance.NonQueryQuickExecute(query);

            var childQuery = "UPDATE " + Constants.CHILDREN_TABLE +
                " SET checkInParent='" + parentUuid + "' " +
                "WHERE uuid='" + childUuid + "';";

            DatabaseConnect.Instance.NonQueryQuickExecute(childQuery);
        }

        public ChildModel[] GetChildrenInSession(string sessionName)
        {
            var children = new ChildModel[0];
            var query = "SELECT * FROM " + Constants.SESSION_TABLE +
                " WHERE name='" + sessionName + "';";

            if (DatabaseConnect.Instance.OpenConnection())
            {
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, DatabaseConnect.Instance.connection);
                var reader = cmd.ExecuteReader();
                var childrenList = new List<ChildModel>();

                while (reader.Read())
                {
                    var child = new ChildModel();
                    child.uuid = reader["uuid"] as string;
                    childrenList.Add(child);
                }
                reader.Close();
                

                children = childrenList.ToArray<ChildModel>();
                foreach (ChildModel child in children)
                {
                    child.Populate();
                }

                DatabaseConnect.Instance.CloseConnection();
            }

            return children;
        }

        public void RemoveChildFromSession(string childUuid)
        {
            var query = "DELETE FROM " + Constants.SESSION_TABLE +
                " WHERE uuid='" + childUuid + "';";

            DatabaseConnect.Instance.NonQueryQuickExecute(query);

            var childQuery = "UPDATE " + Constants.CHILDREN_TABLE + 
                " SET checkInParent=NULL WHERE uuid='" + childUuid + "';";
            DatabaseConnect.Instance.NonQueryQuickExecute(childQuery);
         }

        // EMAIL AND PHONE FUNCTIONS 
        public void AddEmailFor(string email, string parentUuid)
        {
            var query = "INSERT INTO " + Constants.EMAIL_TABLE + " (email, parent) " +
                "VALUES('" + email + "','" + parentUuid + "')";

            DatabaseConnect.Instance.NonQueryQuickExecute(query);
        }

        public void AddPhoneFor(string phone, string parentUuid)
        {
            var query = "INSERT INTO " + Constants.PHONE_TABLE + " (phone, parent) " +
                "VALUES('" + phone + "','" + parentUuid + "')";

            DatabaseConnect.Instance.NonQueryQuickExecute(query);
        }

        public void DeleteAllEmailFor(string parentUuid)
        {
            var query = "DELETE FROM " + Constants.EMAIL_TABLE + " WHERE parent='" +
                parentUuid + "';";
            DatabaseConnect.Instance.NonQueryQuickExecute(query);
        }

        public void DeleteEmailFor(string email, string parentUuid)
        {
            var query = "DELETE FROM " + Constants.EMAIL_TABLE + " WHERE email='"
                + email + "' AND parent='" + parentUuid + "';";
            DatabaseConnect.Instance.NonQueryQuickExecute(query);
        }

        public void DeleteAllPhoneFor(string parentUuid)
        {
            var query = "DELETE FROM " + Constants.PHONE_TABLE + " WHERE parent='" +
                parentUuid + "';";
            DatabaseConnect.Instance.NonQueryQuickExecute(query);
        }

        public void DeletePhoneFor(string phone, string parentUuid)
        {
            var query = "DELETE FROM " + Constants.EMAIL_TABLE + " WHERE phone='"
                + phone + "' AND parent='" + parentUuid + "';";
            DatabaseConnect.Instance.NonQueryQuickExecute(query);
        }

        public string GetMemberOfParent(string uuid)
        {
            var query = "SELECT * FROM " + Constants.PARENT_TABLE + " WHERE uuid='"
                + uuid + "';";

            var value = "";

            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, DatabaseConnect.Instance.connection);
                var reader = cmd.ExecuteReader();
                reader.Read();

                value = reader["member"] as string;
                reader.Close();

                DatabaseConnect.Instance.CloseConnection();
            }

            return value;
        }

        public string GetMemberOfChild(string uuid)
        {
            var query = "SELECT * FROM " + Constants.CHILDREN_TABLE + " WHERE uuid='"
                + uuid + "';";

            var value = "";

            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, DatabaseConnect.Instance.connection);
                var reader = cmd.ExecuteReader();
                reader.Read();

                value = reader["member"] as string;
                reader.Close();

                DatabaseConnect.Instance.CloseConnection();
            }

            return value;
        }

        private void TryRemoveMemberId(string uuid)
        {
            var parentQuery = "SELECT * FROM " + Constants.PARENT_TABLE +
                " WHERE member='" + uuid + "';";
            var childQuery = "SELECT * FROM " + Constants.CHILDREN_TABLE +
                " WHERE member='" + uuid + "';";

            if (DatabaseConnect.Instance.OpenConnection() == true)
            {
                var parentCmd = new MySql.Data.MySqlClient.MySqlCommand(parentQuery);
                var parentReader = parentCmd.ExecuteReader();
                if (!parentReader.HasRows)
                {
                    parentReader.Close();
                    var childCmd = new MySql.Data.MySqlClient.MySqlCommand(childQuery);
                    var childReader = childCmd.ExecuteReader();

                    if (!childReader.HasRows)
                    {
                        childReader.Close();

                        var deleteQuery = "DELETE FROM " + Constants.MEMBER_TABLE +
                            " WHERE uuid='" + uuid + "';";
                        DatabaseConnect.Instance.NonQueryQuickExecute(deleteQuery);
                    }
                    else
                    {
                        childReader.Close();
                    }
                }
                else
                {
                    parentReader.Close();
                }

                DatabaseConnect.Instance.CloseConnection();
            }
        }
    }
}