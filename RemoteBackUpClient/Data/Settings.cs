using System.Collections.Generic;
using System.Linq;

namespace RemoteBackUpClient.Data
{
    public class Settings
    {
        private Dictionary<string, string> _dictionaryList = new Dictionary<string, string>();
        public string DefaultPath { get; set; }
        public string SelectedDb { get; set; }
        public List<ListItem> List { get; set; } = new List<ListItem>();

        //public Dictionary<string, string> DictionaryList
        //{
        //    get
        //    {
        //        if (_dictionaryList.Count > 0)
        //        {
        //            return _dictionaryList;
        //        }

        //        _dictionaryList = List.ToDictionary(i => i.DbName, i => i.Url);

        //        return _dictionaryList;
        //    }
        //    set => _dictionaryList = value;
        //}
        
    }

    public class ListItem
    {
        public string DbName { get; set; }
        public string Url { get; set; }
    }
}