using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResourceFormatCreater
{
    public class TPConfig
    {
        public static string createPath = "";
        public static List<string> notBuildList = new List<string>();
        public static List<string> singleBuildList = new List<string>();
        public static List<string> cocoStudioList = new List<string>();

        public static bool BeInNotBuildList(string dicName)
        {
            return beInList(notBuildList, dicName);
        }

        public static bool BeInSingleBuildList(string dicName)
        {
            return beInList(singleBuildList, dicName);
        }
        public static bool BeInCocoStudioList(string dicName)
        {
            return beInList(cocoStudioList, dicName);
        }

        public static void LoadConfig()
        {
            createPath = Application.StartupPath.ToString() + "/buildConfig.txt";

            if (File.Exists(createPath))
            {

                string text = File.ReadAllText(createPath);

                Dictionary<string, object> dic = MiniJSON.Json.Deserialize(text) as Dictionary<string, object>;

                if (dic != null)
                {
                    if (dic.ContainsKey("notBuildList"))
                    {
                        List<object> lst = dic["notBuildList"] as List<object>;
                        foreach (object o in lst)
                        {
                            notBuildList.Add(o.ToString());
                        }
                    }
                    if (dic.ContainsKey("singleBuildList"))
                    {
                        List<object> lst = dic["singleBuildList"] as List<object>;
                        foreach (object o in lst)
                        {
                            singleBuildList.Add(o.ToString());
                        }
                    }
                    if (dic.ContainsKey("cocoStudioList"))
                    {
                        List<object> lst = dic["cocoStudioList"] as List<object>;
                        foreach (object o in lst)
                        {
                            cocoStudioList.Add(o.ToString());
                        }
                    }
                }
            }
            else
            {
                Dictionary<string, object> dicInfo = new Dictionary<string, object>();
                dicInfo["notBuildList"] = notBuildList;
                dicInfo["singleBuildList"] = singleBuildList;
                dicInfo["cocoStudioList"] = cocoStudioList;

                string str = MiniJSON.Json.Serialize(dicInfo);
                File.WriteAllText(createPath, str);
            }
        }

        private static bool beInList(List<string> lst, string key)
        {
            string result = lst.Find(
            delegate(string content)
            {
                return content == key;
            });

            if (result == null) return false;
            if (result == "") return false;
            return true;
        }
    }
}
