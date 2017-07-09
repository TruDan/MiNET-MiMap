using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace BiomeMap.Http.Modules
{
    public class CoreModule : NancyModule
    {

        private string _index;


        public CoreModule()
        {
            _index = WebResources.index;

            Get[""] = GetIndex;
        }

        private dynamic GetIndex(dynamic o)
        {
            return _index;
        }
    }
}
