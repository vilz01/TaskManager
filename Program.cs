using System;
using System.Data.SqlClient;
using System.IO;

namespace SqlHarj
{
    class Program
    {
        static void Main(string[] args)
        {                          //path Data Source tulee vaihtaa omalle koneelle oikeaksi
            string? choise, query, path = @"Data Source=xxxx\SQLEXPRESS;Initial Catalog=Taskma;Integrated Security=true;encrypt=false;";
            int? actId;
            do
            {
                Console.Clear();
                Console.Write("1. Print activities\n2. Manage activities\n3. Search by tag\nInput: ");
                choise = Console.ReadLine();
                switch (choise)
                {
                    case "1":
                        query = "SELECT * FROM Activity";
                        PrintActivities(path, true, query);
                        Console.Write("\nPress ENTER");
                        Console.ReadLine();
                        break;
                    case "2":
                        do
                        {
                            Console.Clear();
                            Console.Write("1. New activity\n2. Select activity\n3. Back\nInput: ");
                            choise = Console.ReadLine();
                            switch (choise)
                            {
                                case "1":
                                    AddActivity(path);
                                    break;
                                case "2":
                                    actId = SelectActivity(path);
                                    if(actId != null)
                                    {
                                        do
                                        {
                                            Console.Clear();
                                            Console.Write("1. Update status\n2. Add tasks\n3. Add tags\n4. Delete activity\n5. Back\nInput: ");
                                            choise = Console.ReadLine();
                                            switch (choise)
                                            {
                                                case "1":
                                                    UpdateStatus(path, actId);
                                                    break;
                                                case "2":
                                                    AddTask(path, actId);
                                                    break;
                                                case "3":
                                                    AddTag(path, actId);
                                                    break;
                                                case "4":
                                                    DeleteActivity(path, actId);
                                                    break;
                                            }
                                        } while (choise != "end" && choise != "exit" && choise != "5");
                                    }
                                    break;
                            }
                        } while (choise != "end" && choise != "exit" && choise != "3");
                        break;
                    case "3":
                        SearchByTag(path);
                        break;
                }
            } while (choise != "exit");
        }

        public static void SqlInsert(string path, string? addition)
        {
            using SqlConnection connection = new(path);
            connection.Open();
            SqlCommand cmd = new SqlCommand(addition, connection);
            IAsyncResult result = cmd.BeginExecuteNonQuery();
            cmd.EndExecuteNonQuery(result);
            connection.Close();
        }

        public static void AddTag(string path, int? actId)
        {
            using SqlConnection connection = new(path);
            string? tag, tag1, addition, query = "SELECT * FROM Tag";
            SqlCommand cmd;
            bool exists;
            List<string> tags = new List<string>(), actTags = new List<string>();
            Console.Write("Write tags one at a time , if you are done, just press ENTER\ntag: #");
            tag = Console.ReadLine();
            while (!String.IsNullOrEmpty(tag))
            {
                exists = false;
                tag1 = "#";
                foreach (char c in tag)
                {
                    if (String.IsNullOrWhiteSpace(c.ToString()))
                    {
                        break;
                    }
                    tag1 = tag1 + c;
                }
                if (tag1.Length > 1)
                {
                    cmd = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader[1].ToString() == tag1)
                            {
                                exists = true;
                            }
                        }
                        if (!exists)
                        {
                            tags.Add(tag1);
                        }
                        actTags.Add(tag1);
                    }
                    connection.Close();
                }
                Console.Write("tag: #");
                tag = Console.ReadLine();
            }
            if(tags.Count > 0)
            {
                foreach(string x in tags)
                {
                    addition = "USE [Taskma] INSERT INTO [dbo].[Tag]([Name]) VALUES('"+x+"')";
                    SqlInsert(path, addition);
                }
            }
            foreach (string x in actTags)
            {
                addition = "USE [Taskma] INSERT INTO [dbo].[TagAct]([TagId],[ActId]) VALUES('" + GetTagId(path, x) + "','" + actId + "')";
                SqlInsert(path, addition);
            }
        }

        public static void CheckTag(string path, string query)
        {
            using SqlConnection connection = new SqlConnection(path);
            SqlCommand cmd = new SqlCommand(query, connection);
            connection.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                
            }
        }

        public static void PrintActivities(string path, bool printall, string query)
        {
            using SqlConnection connection = new SqlConnection(path);
            
            SqlCommand cmd = new SqlCommand(query, connection);
            connection.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.Write("\nActivity: ");
                if(!printall)
                {
                    Console.Write($"Id: {reader[0]}, ");
                }
                Console.WriteLine(string.Format($"{reader[1]}; {reader[2]}"));
                if (printall)
                {
                    PrintTags(path, Convert.ToInt32(reader[0].ToString()));
                    PrintStatus(path, Convert.ToInt32(reader[6].ToString()));
                    PrintTasks(path, Convert.ToInt32(reader[0].ToString()));
                }
            }
        }

        public static void PrintTasks(string path, int? actId)
        {
            using SqlConnection connection = new SqlConnection(path);
            string query = "SELECT * FROM Task WHERE ActivityId = "+actId;
            SqlCommand cmd = new SqlCommand(query, connection);
            connection.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            Console.Write("Tasks: ");
            while (reader.Read())
            {
                Console.Write(string.Format($"\t{reader[1]}: {reader[2]}\n"));
            }
            Console.WriteLine();
        }

        public static void PrintStatus(string path, int statusId)
        {
            using SqlConnection connection = new SqlConnection(path);
            string query = "SELECT Title FROM Status WHERE Id like "+statusId;
            SqlCommand cmd = new SqlCommand(query, connection);
            connection.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine(string.Format($"Status: {reader[0]}"));
            }
        }

        public static void PrintTags(string path, int actId)
        {
            using SqlConnection connection = new SqlConnection(path);
            string query = "SELECT Name FROM Tag JOIN TagAct on Tag.Id = TagAct.TagId WHERE TagAct.ActId LIKE "+actId;
            SqlCommand cmd = new SqlCommand(query, connection);
            connection.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            Console.Write("Tags: ");
            while (reader.Read())
            {
                Console.Write(string.Format($"{reader[0]} "));
            }
            Console.WriteLine();
        }

        public static void AddTask(string path, int? actId)
        {
            string? name, cont, addition;
            DateTime? startD = null, endD = null;
            int status = 1;
            Console.Write("Give task name: ");
            name = Console.ReadLine();
            Console.Write("Give task content: ");
            cont = Console.ReadLine();
            addition = "USE [Taskma] INSERT INTO [dbo].[Task] ([Name],[Cont],[StartD],[EndD],[Status],[ActivityId])" +
                " VALUES('" + name + "','" + cont + "','" + startD + "','" + endD + "','" + status + "','" + actId + "')";
            SqlInsert(path, addition);
            Console.WriteLine("** Task added **");
        }

        public static void AddActivity(string path)
        {
            string? title, desc, url = null, addition, choise;
            DateTime? startD = null, endD = null;
            int status = 1, actType;
            List<string> tags = new List<string>();
            Console.Write("Give activity title: ");
            title = Console.ReadLine();
            Console.Write("Give activity description: ");
            desc = Console.ReadLine();
            Console.Write("Choose activity type\n1. Hobby\n2. School\n3. Job\n4. Other\nChoise: ");
            choise = Console.ReadLine();
            while (!Int32.TryParse(choise, out actType) && actType != 1 && actType != 2 && actType != 3 && actType != 4)
            {
                Console.Write("Try again, must choose 1, 2, 3 or 4\nChoise: ");
                choise = Console.ReadLine();
            }
            addition = "USE [Taskma] INSERT INTO [dbo].[Activity] ([Title],[Description],[Url],[StartD],[EndD],[Status],[ActivityType])" +
                " VALUES('" + title + "','" + desc + "','" + url + "','" + startD + "','" + endD + "','" + status + "','" + actType + "')";
            SqlInsert(path, addition);
            Console.WriteLine("** Activity added **\nDo you want to add tasks to your activity?\n 1. Yes, 2.No\nInput: ");
            choise = Console.ReadLine();
            while(choise == "1")
            {
                AddTask(path, GetActId(path, title));
                Console.Write("Do you want to add another task?\n 1. Yes, 2. No\nInput: ");
                choise = Console.ReadLine();
            }
            Console.Write("Do you want to add tags to you activity?\n1. Yes, 2. No\nInput: ");
            choise = Console.ReadLine();
            if(choise == "1")
            {
                AddTag(path, GetActId(path, title));
            }
        }

        public static int GetActId(string path, string? title)
        {
            string query = "SELECT Id FROM Activity WHERE Activity.Title like '"+title+"'";
            return GetId(path, query);
        }

        public static int GetTagId(string path, string? tag)
        {
            string query = "SELECT Id FROM Tag WHERE Tag.Name like '"+tag+"'";
            return GetId(path, query);
        }

        public static int GetId(string path, string query)
        {
            using SqlConnection connection = new SqlConnection(path);
            int Id = 0;
            SqlCommand cmd = new SqlCommand(query, connection);
            connection.Open();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Id = Convert.ToInt32(reader[0].ToString());
                }
            }
            connection.Close();
            return Id;
        }

        public static int? SelectActivity(string path)
        {
            string? choise, query = "SELECT Id FROM Activity", query2 = "SELECT * FROM Activity";
            int? actId = null;
            PrintActivities(path, false, query2);
            Console.Write("\nChoose Id of an activity you want to modify\nChoise: ");
            choise = Console.ReadLine();
            using SqlConnection connection = new SqlConnection(path);
            SqlCommand cmd = new SqlCommand(query, connection);
            connection.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader[0].ToString() == choise)
                {
                    actId = Convert.ToInt32(choise);
                }
            }
            if(actId == null)
            {
                Console.WriteLine("No activity found with given Id\n");
                Console.ReadLine();
                return null;
            }
            else
            {
                return actId;
            }
        }

        public static void UpdateStatus(string path, int? actId)
        {
            string? choise, query;
            int statusId;
            Console.Write("Select new status:\n1. New, 2. In Progress, 3. Done, 4. Cancelled\nInput: ");
            choise = Console.ReadLine();
            if (choise == "1" | choise == "2" | choise == "3" | choise == "4")
            {
                statusId = Convert.ToInt32(choise);
                query = "UPDATE Activity SET Status = " + statusId + " WHERE Id LIKE " + actId;
                SqlInsert(path, query);
            }
            else
            {
                Console.WriteLine("Invalid statusId");
            }
        }

        public static void DeleteActivity(string path, int? actId)
        {
            string deleteAct = "DELETE FROM Activity WHERE Id LIKE "+actId;
            string deleteTag = "DELETE FROM TagAct WHERE ActId LIKE "+actId;
            string deleteTask = "DELETE FROM Task WHERE ActivityId LIKE "+actId;
            SqlInsert(path, deleteTag);
            SqlInsert(path, deleteTask);
            SqlInsert(path, deleteAct);
        }

        public static void SearchByTag(string path)
        {
            string? tag , query;
            do
            {
                Console.WriteLine("Leave by just pressing ENTER with the line being empty");
                Console.Write("Tag: #");
                tag = Console.ReadLine();
                if (!String.IsNullOrWhiteSpace(tag))
                {
                    query = "SELECT * FROM Activity JOIN TagAct ON TagAct.ActId = Activity.Id JOIN Tag ON Tag.Id = TagAct.TagId WHERE Tag.Name LIKE '#"+tag+"'";
                    PrintActivities(path, true, query);
                }
            } while (!String.IsNullOrWhiteSpace(tag));
        }
    }
}