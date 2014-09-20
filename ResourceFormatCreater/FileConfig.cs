using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Deployment;
using System.Windows.Forms;

namespace ResourceFormatCreater
{
    public class FileConfig
    {
        public string filePath = "";
        public string createPath = "";
        public string basePath = "res/cocostudio/IceUI/";
        public string fileName = "CocosResource.js";

        string m_configFilePath = "";

        public FileConfig()
        {
            string currentPath = Application.StartupPath.ToString();

             m_configFilePath = currentPath + "/config.txt";
        }
        public bool ReadFileConfig()
        {
            if (File.Exists(m_configFilePath))
            {
                
                string text = File.ReadAllText(m_configFilePath);

                Dictionary<string, object> dic = MiniJSON.Json.Deserialize(text) as Dictionary<string, object>;

                if (dic != null)
                {
                    if (dic.ContainsKey("filePath"))
                    {
                        filePath = dic["filePath"].ToString();
                        
                    }
                    if (dic.ContainsKey("createPath"))
                    {
                        createPath = dic["createPath"].ToString();
 
                    }
                    if (dic.ContainsKey("basePath"))
                    {
                        basePath = dic["basePath"].ToString();

                    }
                    if (dic.ContainsKey("fileName"))
                    {
                        fileName = dic["fileName"].ToString();

                    }
                    return true;
                }
            }
            return false;
        }

        public void SaveFileConfig()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["filePath"] = filePath;
            dic["createPath"] = createPath;
            dic["basePath"] = basePath;
            dic["fileName"] = fileName;
            
            string message = MiniJSON.Json.Serialize(dic);
            File.WriteAllText(m_configFilePath, message);
        }
    }

    
}
