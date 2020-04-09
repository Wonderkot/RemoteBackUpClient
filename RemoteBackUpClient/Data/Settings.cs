using System.Collections.Generic;

namespace RemoteBackUpClient.Data
{
    public class Settings
    {
        public string DefaultPath { get; set; }
        public string SelectedDb { get; set; }
        public List<ListItem> List { get; set; } = new List<ListItem>();
        public string Password { get; set; }
        public string Login { get; set; }
    }

    public class ListItem
    {
        public string DbName { get; set; }
        public string Url { get; set; }
        public bool UseSchedule { get; set; } = true;
    }
}