using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KidKountWebService
{
    public class ChildModel
    {
        public string name = "";
        public string notes = "";
        public string uuid = "";
        public string member = "";

        public ChildModel()
        {
            name = "";
            notes = "";
            uuid = "";
            member = "";
        }

        public ChildModel(string name, string notes, string uuid, string member)
        {
            this.name = name;
            this.notes = notes;
            this.uuid = uuid;
            this.member = member;
        }

        public ChildModel(string uuid)
        {
            this.uuid = uuid;
            PopulateFromUuid();
        }

        public void Populate()
        {
            PopulateFromUuid();
        }

        private void PopulateFromUuid(){
            var newChild = MemberHelper.Instance.GetChild(this.uuid);
            this.name = newChild.name;
            this.notes = newChild.notes;
            this.member = newChild.member;
        }
    }
}