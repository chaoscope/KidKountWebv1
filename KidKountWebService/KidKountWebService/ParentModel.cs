using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KidKountWebService
{
    public class ParentModel
    {
        public string name = "";
        public string uuid = "";
        public string member = "";

        public string[] email;
        public string[] phone;

        public ParentModel()
        {
            name = "";
            uuid = "";
            member = "";
        }

        public ParentModel(string name, string uuid, string member, string[] email, string[] phone)
        {
            this.name = name;
            this.uuid = uuid;
            this.member = member;
            this.email = email;
            this.phone = phone;
        }

        public ParentModel(string uuid)
        {
            this.uuid = uuid;
            PopulateFromUuid();
        }

        public void Populate()
        {
            PopulateFromUuid();
        }

        private void PopulateFromUuid()
        {
            var newParent = MemberHelper.Instance.GetParent(this.uuid);
            this.name = newParent.name;
            this.member = newParent.member;
            this.email = newParent.email;
            this.phone = newParent.phone;
        }

    }
}