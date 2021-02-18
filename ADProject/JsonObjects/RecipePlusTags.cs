using ADProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADProject.JsonObjects
{
    public class RecipePlusTags
    {


        public RecipePlusTags() { }


        public Recipe recipe { get; set; }

        public string tags { get; set; }
    }
}
