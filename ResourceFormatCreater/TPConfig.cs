using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public enum DICTYPE
{
    CocostudioType = 0,
    MapType = 1,
}

namespace ResourceFormatCreater
{
    public class TPConfig
    {
        public static string createPath = "";
        public static List<string> notBuildList = new List<string>();
        public static List<string> singleBuildList = new List<string>();
        public static List<string> cocoStudioList = new List<string>();
        public static List<string> plistBuildList = new List<string>();
        public static string tmxMap = "";
        public static string cmdStr = " --allow-free-size --trim --padding 1 --opt RGBA8888 --dither-fs-alpha --format cocos2d ";
        public static string singleCmdStr = " --allow-free-size --no-trim --padding 0 --disable-rotation --opt RGBA8888 --dither-fs-alpha --content-protection 10241024102410241024102410241024 --format cocos2d ";

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
        public static bool BeInPlistBuildList(string dicName)
        {
            return beInList(plistBuildList, dicName);
        }
        public static bool BeTmxMap(string dicName)
        {
            return tmxMap == dicName;
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
                    if (dic.ContainsKey("plistBuildList"))
                    {
                        List<object> lst = dic["plistBuildList"] as List<object>;
                        foreach (object o in lst)
                        {
                            plistBuildList.Add(o.ToString());
                        }
                    }
                    if (dic.ContainsKey("tmxMap"))
                    {
                        tmxMap = dic["tmxMap"].ToString();
                    }
                    if (dic.ContainsKey("cmdStr"))
                    {
                        cmdStr = dic["cmdStr"].ToString();
                    }
                    if (dic.ContainsKey("singleCmdStr"))
                    {
                        singleCmdStr = dic["singleCmdStr"].ToString();
                    }
                }
            }
            else
            {
                Dictionary<string, object> dicInfo = new Dictionary<string, object>();
                dicInfo["notBuildList"] = notBuildList;
                dicInfo["singleBuildList"] = singleBuildList;
                dicInfo["cocoStudioList"] = cocoStudioList;
                dicInfo["plistBuildList"] = cocoStudioList;
                dicInfo["tmxMap"] = tmxMap;
                dicInfo["cmdStr"] = cmdStr;
                dicInfo["singleCmdStr"] = singleCmdStr;
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
