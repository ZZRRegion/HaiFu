using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaiFu.Project.Model
{
    public class Users:ModelBase
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int IsPass { get; set; }
        public string CloudToken { get; set; }
        public Users()
        {
            this.Name = Guid.NewGuid().ToString();
            this.Password = this.Name;
            this.Email = this.Name;
            this.IsPass = 1;
            this.CloudToken = this.Name;
        }
    }
}
