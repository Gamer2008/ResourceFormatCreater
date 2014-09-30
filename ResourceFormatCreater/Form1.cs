using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResourceFormatCreater
{
    public partial class rescreater : Form
    {
        FileConfig fileCongig = null;
        Dictionary<string, List<TpFileInfo>> m_dics = new Dictionary<string, List<TpFileInfo>>();
        Dictionary<string, List<TpFileInfo>> m_normaldics = new Dictionary<string, List<TpFileInfo>>();
        List<TpCmd> listCmd = new List<TpCmd>();
        private List<Thread> m_ThreadList;
        private TPFormat m_format = TPFormat.PVRCCZ;
        private Dictionary<string, TpFileInfo> m_tpInfos = new Dictionary<string, TpFileInfo>();
        TpDicInfo m_rootDic = null;
        TpDicInfo m_srcDic = null;
        TpDicInfo m_cocostudioDic = null;
        TpDicInfo m_mapDic = null;
        bool m_single = true;

        public rescreater()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            TPConfig.LoadConfig();
            this.AddLogContent("读取配置:");
            string content = "";
            if (TPConfig.cocoStudioList.Count > 0)
            {
                content += "cocostudio路径：  " + TPConfig.cocoStudioList[0];
            }
            else
            {
                content += "cocostudio路径：\r\n空";
            }

            this.AddLogContent(content);
            content = "单独文件夹：  ";
            foreach (string f in TPConfig.singleBuildList)
            {
                content += f + "  ";
            }
            this.AddLogContent(content);
            content = "TP不合并文件夹：  ";
            foreach (string f in TPConfig.notBuildList)
            {
                content += f + "  ";
            }
            this.AddLogContent(content);

            m_ThreadList = new List<Thread>();
            fileCongig = new FileConfig();
            fileCongig.ReadFileConfig();

            txt_work.Text = fileCongig.filePath;
            txt_create.Text = fileCongig.createPath;

        }

        private void btn_create_Click(object sender, EventArgs e)
        {
           
            fileCongig.filePath = PathAlign(txt_work.Text);
            fileCongig.createPath = PathAlign(txt_create.Text);
            if (fileCongig.filePath == "")
            {
                MessageBox.Show("请选择资源目录");
                return;
            }
            if (!Directory.Exists(fileCongig.filePath))
            {
                MessageBox.Show("请选择正确的择资源目录");
                return;
            }
            
            if (fileCongig.createPath == "")
            {
                MessageBox.Show("请选择保存目录");
                return;
            }

            if (!Directory.Exists(fileCongig.createPath))
            {
                MessageBox.Show("请选择正确保存目录");
                return;
            }

            fileCongig.SaveFileConfig();
            listCmd.Clear();
            m_tpInfos.Clear();
            DirectoryInfo dicInfo = new DirectoryInfo(fileCongig.filePath);
            this.m_rootDic = new TpDicInfo(dicInfo, null);

            DirectoryInfo srcdicInfo = new DirectoryInfo(fileCongig.filePath + "/../src");
            this.m_srcDic = new TpDicInfo(srcdicInfo, null);

            this.m_cocostudioDic = this.GetCocoStudioDir();
            this.m_mapDic = this.GetMapDir();

            string result = GetPngFormatCMDStr(this.m_rootDic);
            CopyFileAndInitTpInfos();
            DealTMXMAp();
            DealPlistBuild();
            StartCreate();
            CreatNormalListJson();
            CreateCocoStudioListJson();
            ModifyAllCocostudioJson();
            
        }

        private void DeleteUnUserPlist()
        {
            string path = GetSinglePlistDirectory();
            Directory.Delete(path,true);
        }


        void initPlistBuild(TpDicInfo dicInfo, ref List<TpDicInfo> listDicInfo)
        {
            if (dicInfo.bePlistBuildList)
            {
                listDicInfo.Add(dicInfo);
            }

            foreach (TpDicInfo dic in dicInfo.directorys)
            {
                initPlistBuild(dic, ref listDicInfo);
            }
        }

        void DealPlistBuild()
        {
            List<TpDicInfo> listDicInfo = new List<TpDicInfo>();
            initPlistBuild(m_rootDic, ref listDicInfo);

            foreach (TpDicInfo dicInfo in listDicInfo)
            {
                List<TpFileInfo> listPlist = dicInfo.GetPlistPngs();
                List<TpFileInfo> lst = dicInfo.files;

                foreach (TpFileInfo info in lst)
                {
                    if (info.bePng)
                    {
                        continue;
                    }

                    string content = "";
                    content = File.ReadAllText(info.info.FullName);
                    if (info.info.Name.EndsWith(".tmx"))
                    {
                        content = content.Replace(".png", GetTPFormatString(this.m_format));
                    }
                    else if (info.info.Name.EndsWith(".plist"))
                    {
                        string oldPng = info.info.Name.Replace(".plist", ".png");
                        string newFormat = info.info.Name.Replace(".plist", GetTPFormatString(this.m_format));
                        content = content.Replace(oldPng, newFormat);
                    }
                    else if (info.info.Name.EndsWith(".json") || info.info.Name.EndsWith(".ExportJson"))
                    {
                        foreach (TpFileInfo png in listPlist)
                        {
                            string oldPng = png.info.Name;
                            string newFormat = png.info.Name.Replace(".png", GetTPFormatString(this.m_format));
                            content = content.Replace(oldPng, newFormat);
                        }
                    }

                    string outputPath = PathAlign(GetOutputDirectory(info.parent.directoryInfo)) + "/" + info.info.Name;
                    System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding(false);
                    File.WriteAllText(outputPath, content, utf8);
                }
            }
        }

        void DealTMXMAp()
        {
            List<TpFileInfo> lst = new List<TpFileInfo>();
            GetAllNormalTPFiles(m_mapDic, ref lst);
            foreach (TpFileInfo info in lst)
            {
                if (info.bePng)
                {
                    continue;
                }
                string content = "";
                content = File.ReadAllText(info.info.FullName);
                if (info.info.Name.EndsWith(".tmx"))
                {
                    content = content.Replace(".png", GetTPFormatString(this.m_format));
                }
                else if (info.info.Name.EndsWith(".plist"))
                {
                    string oldPng = info.info.Name.Replace(".plist", ".png");
                    string newFormat = info.info.Name.Replace(".plist", GetTPFormatString(this.m_format));
                    content = content.Replace(oldPng, newFormat);
                }

                string outputPath = PathAlign(GetOutputDirectory(info.parent.directoryInfo)) + "/" + info.info.Name;
                System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding(false);
                File.WriteAllText(outputPath, content, utf8);
            }
        }

        void ProcessCmd(TpCmd cmd)
        {
            this.AddLogContent("处理文件夹：" + GetRelatePath(cmd.lstTpFile[0].parent.directoryInfo.FullName));
            this.AddLogContent("处理图片: ");
            string pngs = "";
            foreach (TpFileInfo tpInfo in cmd.lstTpFile)
            {
                pngs += tpInfo.info.Name + "  ";
            }
            this.AddLogContent(pngs);

            
            string result = cmd.cmd; 
            Process p = new Process();  // 初始化新的进程
            p.StartInfo.FileName = "CMD.EXE"; //创建CMD.EXE 进程
            p.StartInfo.RedirectStandardInput = true; //重定向输入
            p.StartInfo.RedirectStandardOutput = true;//重定向输出
            p.StartInfo.UseShellExecute = false; // 不调用系统的Shell
            p.StartInfo.RedirectStandardError = true; // 重定向Error
            p.StartInfo.CreateNoWindow = true; //不创建窗口
            p.Start(); // 启动进程
            p.StandardInput.WriteLine(result); // Cmd 命令
            p.StandardInput.WriteLine("exit"); // 退出
            p.WaitForExit();

            this.AddLogContent("生成：" + cmd.outFormat);
            this.AddLogContent("生成：" + cmd.outPlist);
            this.AddLogContent("******************************************生成成功******************************************\r\n");
           
        }

        void CreateTP()
        {
            foreach (TpCmd cmd in listCmd)
            {
                ProcessCmd(cmd);
            }

            DeleteUnUserPlist();
            if (this.ck_js.Checked)
            {
                JSCompile();
            }
            
            MessageBox.Show("加密成功");
        }

        void CopyFileAndInitTpInfos()
        {
            CIDictory(this.m_rootDic);
            FileCopy(this.m_srcDic.directoryInfo.FullName, this.fileCongig.createPath + "/src", true);
        }

        void JSCompile()
        {
            this.AddLogContent("编译JS文件");
            string result = "@echo off \n";
            string path = "";
            if (Directory.Exists(Application.StartupPath + fileCongig.cocos2dConsolePath))
            {
                path = Application.StartupPath + fileCongig.cocos2dConsolePath;
            }
            else
            {
                path = fileCongig.cocos2dConsolePath;
            }

            result += "python " + path + "/cocos.py jscompile -s " + this.fileCongig.createPath + "/src -d " + this.fileCongig.createPath + "/src";
            DirectoryInfo dicInfo = new DirectoryInfo(this.fileCongig.createPath + "/src");
            string root = dicInfo.Root.FullName;
            result += "\n" + root.Substring(0, root.Length - 1);
            result += "\ncd " + dicInfo.FullName;
            result += "\ndel /f/q/s *.js";
            Process p = new Process();  // 初始化新的进程
            p.StartInfo.FileName = "CMD.EXE"; //创建CMD.EXE 进程
            p.StartInfo.RedirectStandardInput = true; //重定向输入
            p.StartInfo.RedirectStandardOutput = true;//重定向输出
            p.StartInfo.UseShellExecute = false; // 不调用系统的Shell
            p.StartInfo.RedirectStandardError = true; // 重定向Error
            p.StartInfo.CreateNoWindow = true; //不创建窗口
            p.Start(); // 启动进程
            p.StandardInput.WriteLine(result); // Cmd 命令
            p.StandardInput.WriteLine("exit"); // 退出
            p.OutputDataReceived += new DataReceivedEventHandler(pro_OutputDataReceived);
            p.BeginOutputReadLine();
            p.WaitForExit();

            this.AddLogContent("******************************************js编译成功******************************************\r\n");
        }

        void pro_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.AddLogContent(e.Data);
            }
        }

        void CIDictory(TpDicInfo dicInfo)
        {
            this.AddLogContent("处理文件夹：" + dicInfo.directoryInfo.Name);
            if (dicInfo == this.m_mapDic)
            {
                return;
            }
            if (dicInfo.bePlistBuildList)
            {
                return;
            }
            if (!dicInfo.beCocosStudioProject || (dicInfo.beCocosStudioProject && dicInfo.beNotBuild))
            {
                foreach (TpFileInfo file in dicInfo.files)
                {
                    this.AddLogContent("移动文件:" + file.info.Name);
                    string outputPath = PathAlign(GetOutputDirectory(dicInfo.directoryInfo)) + "/" + file.info.Name;
                    System.IO.File.Copy(file.info.FullName, outputPath, true);
                }
            }

            else 
            {
                foreach (TpFileInfo file in dicInfo.files)
                {
                    if (file.beInPlist)
                    {
                        this.AddLogContent("移动文件:" + file.info.Name);
                        string outputPath = PathAlign(GetOutputDirectory(dicInfo.directoryInfo)) + "/" + file.info.Name;
                        System.IO.File.Copy(file.info.FullName, outputPath, true);
                    }
                    else if (!file.bePng)
                    {
                        this.AddLogContent("移动文件:" + file.info.Name);
                        string outputPath = PathAlign(GetOutputDirectory(dicInfo.directoryInfo)) + "/" + file.info.Name;
                        System.IO.File.Copy(file.info.FullName, outputPath, true);
                    }
                }
            }

            foreach (TpDicInfo info in dicInfo.directorys)
            {
                CIDictory(info);
            }
        }


        void StartCreate()
        {
            //创建一个新的线程
            Thread tWorkingThread = new Thread(CreateTP);

            //将新建的线程加入到自定义线程队列中，以便在窗体结束时关闭所有的线程
            m_ThreadList.Add(tWorkingThread);
            tWorkingThread.Start();
            this.btn_create.Enabled = false;

        }

        private void StopThread()
        {
            //编列自定义队列,将所有线程终止
            foreach (Thread tWorkingThread in m_ThreadList)
            {
                tWorkingThread.Abort();
            }
        }

        string GetPngFormatCMDStr(TpDicInfo dicInfo)
        {
            string cmdStr = "";

            if (dicInfo.beTMXMapPath || dicInfo.bePlistBuildList)
            {
                foreach (TpFileInfo file in dicInfo.files)
                {
                    if (file.bePng)
                    {
                        TpCmd cmd = new TpCmd();
                        cmdStr += GetEachPngFormatCMDStr(file, ref cmd);
                        this.listCmd.Add(cmd);
                    }
                }
            }

            //在CocosStudioProject项目文件夹
            else if (dicInfo.beCocosStudioProject)
            {
                if (!dicInfo.beNotBuild)
                {
                    if (dicInfo.beSingleBuild)
                    {
                        foreach (TpFileInfo file in dicInfo.files)
                        {
                            if (file.bePng && !file.beInPlist)
                            {
                                TpCmd cmd = new TpCmd();
                                cmdStr += GetEachPngFormatCMDStr(file, ref cmd);
                                this.listCmd.Add(cmd);
                            }
                        }
                        
                    }
                    else
                    {
                        TpCmd cmd = new TpCmd();
                        cmdStr += this.GetDirectoryFormatCMDStr(dicInfo,ref cmd);
                        if (cmd.cmd != "")
                        {
                            this.listCmd.Add(cmd);
                        }
                    }
                }
                else   //TODO:不打包png,直接复制
                {

                }

                
            }
            //TODO:未在CocosStudioProject项目文件夹,需要酌情处理
            else
            {
                
            }

            foreach (TpDicInfo subdicInfo in dicInfo.directorys)
            {
                cmdStr += GetPngFormatCMDStr(subdicInfo);
            }

            return cmdStr;
        }

        /// <summary>
        /// 获取单个文件的TexturePacker命令行
        /// </summary>
        /// <returns></returns>
        string GetEachPngFormatCMDStr(TpFileInfo file,ref TpCmd cmd)
        {
            cmd.lstTpFile.Add(file);
            string outputDic = PathAlign(GetOutputDirectory(file.parent.directoryInfo)) + "/";
            string outFormat = outputDic + GetFormatName(file.info.Name, this.m_format);
            string outFormatPlist = GetSinglePlistDirectory() + "/" + GetFormatName(file.info.Name,TPFormat.PLIST);
            file.tagFileName = GetCocostudioRelatePath(file.parent.directoryInfo.FullName + "/" + GetFormatName(file.info.Name, this.m_format));
            cmd.outFormat = outFormat;
            cmd.outPlist = outFormatPlist;
            string cmdStr = "TexturePacker " + file.info.FullName + TPConfig.singleCmdStr + " --data " + outFormatPlist + " --sheet " + outFormat;
            cmdStr = cmdStr + "\n";
            cmd.cmd = cmdStr;

            m_tpInfos[GetCocostudioRelatePath(file.parent.directoryInfo.FullName) + "/"+file.info.Name] = file;
            return cmdStr;
        }

        string GetDirectoryFormatCMDStr(TpDicInfo dicInfo,ref TpCmd cmd)
        {
            string outputDic = PathAlign(GetOutputDirectory(dicInfo.directoryInfo)) + "/";
            string outFormat = outputDic + GetFormatName(dicInfo.directoryInfo.Name, this.m_format);
            string outFormatPlist = outputDic + GetFormatName(dicInfo.directoryInfo.Name, TPFormat.PLIST);
            string cmdStr = "TexturePacker ";
            bool hasPng = false;
            foreach (TpFileInfo file in dicInfo.files)
            {
                if (file.bePng && !file.beInPlist)
                {
                    cmd.lstTpFile.Add(file);
                    file.tagFileName = GetRelatePath(dicInfo.directoryInfo.FullName) + "/" + GetFormatName(dicInfo.directoryInfo.Name, this.m_format);
                    cmdStr += file.info.FullName + " ";
                    hasPng = true;
                }
            }

            if (!hasPng)
            {
                return "";
            }

            cmdStr = cmdStr + TPConfig.cmdStr + " --data " + outFormatPlist + " --sheet " + outFormat + "\n";
            cmd.outFormat = outFormat;
            cmd.outPlist = outFormatPlist;
            cmd.cmd = cmdStr;
            return cmdStr;
        }

        string GetFormatName(string name, TPFormat format)
        {
            string resultFormat = GetTPFormatString(format);
            if (name.Contains(".png"))
            {
                return name.Replace(".png", resultFormat);
            }
            return name + resultFormat;
           
        }

        string GetTPFormatString(TPFormat format)
        {
            string result = ".png";
            switch (format)
            {
                case TPFormat.PNG:
                    {
                        result = ".png";
                        break;
                    }
                case TPFormat.PVR:
                    {
                        result = ".pvr";
                        break;
                    }
                case TPFormat.PVRCCZ:
                    {
                        result = ".pvr.ccz";
                        break;
                    }
                case TPFormat.PLIST:
                    {
                        result = ".plist";
                        break;
                    }
            }
            return result;
        }

        /// <summary>
        /// 根据文件夹获取输出目录
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        string GetOutputDirectory(DirectoryInfo dic)
        {
            string orgPtah = m_rootDic.directoryInfo.Name + "/"+ GetRelatePath(dic.FullName);
            string outputPath = fileCongig.createPath + "/" + orgPtah;
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            return outputPath;
        }

        string GetSinglePlistDirectory()
        {
            string plistPath = fileCongig.createPath + "/plist";

            if (!Directory.Exists(plistPath))
            {
                Directory.CreateDirectory(plistPath);
            }
            return plistPath;
        }

        void AddLogContent(string log)
        {
            this.txtContent.AppendText(log + "\r\n");
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.txtContent.AppendText(e.Data);
            }
        }

        private void LoadNormalRes() 
        {
            List<TpDicInfo> alldirectory = m_rootDic.directorys;
            for (int index = 0; index < alldirectory.Count; index++)
            {
                List<TpFileInfo> files = new List<TpFileInfo>();
                GetAllNormalTPFiles(alldirectory[index], ref files);
                m_normaldics[alldirectory[index].directoryInfo.Name] = files;
            }
        }


        private void GetAllNormalTPFiles(TpDicInfo dic, ref List<TpFileInfo> lst,bool beCocostudio = false)
        {
            if (beCocostudio == false && dic.beCocosStudioProject == true)
            {
                return;
            }

            List<TpFileInfo> allFiles = dic.files;
            for (int index = 0; index < allFiles.Count; index++)
            {
                List<string> files = new List<string>();
                lst.Add(allFiles[index]);
            }

            List<TpDicInfo> alldirectory = dic.directorys;
            for (int index = 0; index < alldirectory.Count; index++)
            {
                GetAllNormalTPFiles(alldirectory[index], ref lst, beCocostudio);
            }
        } 
        private void LoadCocoStudioRes(string dirPath)
        {
            List<TpDicInfo> alldirectory = GetCocoStudioDir().directorys;
            for (int index = 0; index < alldirectory.Count; index++)
            {
                List<TpFileInfo> files = new List<TpFileInfo>();
                GetAllNormalTPFiles(alldirectory[index], ref files,true);
                m_dics[alldirectory[index].directoryInfo.Name] = files;
            }

            List<TpFileInfo> jsons = new List<TpFileInfo>();
            m_dics["exportJson"] = jsons;
            List<TpFileInfo> allFiles = GetCocoStudioDir().files;
            for (int index = 0; index < allFiles.Count; index++)
            {
                if (allFiles[index].info.Name.EndsWith(".json") || allFiles[index].info.Name.EndsWith(".ExportJson") || allFiles[index].info.Name.EndsWith(".csb"))
                {
                    jsons.Add(allFiles[index]);
                }
            }
        }

        private void CreatNormalListJson() 
        {
            LoadNormalRes();
            string content = "";
            foreach (KeyValuePair<string, List<TpFileInfo>> each in m_normaldics)
            {
                content += CreateTxtResource(each.Key, each.Value);
            }

            content += "var list_normalResource = [];\n";
            foreach (KeyValuePair<string, List<TpFileInfo>> each in m_normaldics)
            {
                content += "list_normalResource.push( res_" + each.Key + " );\n";
            }
            string outPath = (fileCongig.createPath + "/src/");
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            string writePath = outPath + "NormalResource.js";
            File.WriteAllText(writePath, content);
        }

        private void CreateCocoStudioListJson()
        {
            var cocoStudioDic = GetCocoStudioDir();
            if (cocoStudioDic == null)
            {
                return;
            }

            LoadCocoStudioRes(cocoStudioDic.directoryInfo.FullName);

            string content = "var tagFormat = \"" + GetTPFormatString(this.m_format)+ "\"\n";
            foreach (KeyValuePair<string, List<TpFileInfo>> each in m_dics)
            {
                content += CreateTxtResource(each.Key, each.Value);
            }

            content += "var list_cocosResource = [];\n";
            foreach (KeyValuePair<string, List<TpFileInfo>> each in m_dics)
            {
                content += "list_cocosResource.push( res_" + each.Key + " );\n";
                //content += CreateTxtResource(each.Key, each.Value);
            }

            string outPath = (fileCongig.createPath + "/src/");
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            string writePath = outPath + fileCongig.fileName;
            File.WriteAllText(writePath, content);
            //Console.Write(content);
        }

        private string CreateTxtResource(string name, List<TpFileInfo> lst)
        {
            string content = "var res_" + name + "={\n";
            for (int index = 0; index < lst.Count; index++ )
            {
                TpFileInfo each = lst[index];
                content += "\t" + GetResContent(each, index == (lst.Count - 1)) + "\n";
            }

            content += "};\n\n";

            return content;
        }

        private void ModifyAllCocostudioJson()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(GetCocoStudioDir().directoryInfo.FullName);
            FileInfo[] allFiles = directoryInfo.GetFiles();
            for (int index = 0; index < allFiles.Length; index++)
            {
                if (allFiles[index].Name.EndsWith(".json") || allFiles[index].Name.EndsWith(".ExportJson") || allFiles[index].Name.EndsWith(".csb"))
                {
                    string outputPath = PathAlign(GetOutputDirectory(directoryInfo)) + "/" + allFiles[index].Name;
                    ModifyCocostudioJson(allFiles[index].FullName, outputPath);
                }
            }
        }

        private void ModifyCocostudioJson(string orgPath,string writePath)
        {
            string jsonS = File.ReadAllText(orgPath);
            Dictionary<string, object> dicObj = MiniJSON.Json.Deserialize(jsonS) as Dictionary<string, object>;
            if (dicObj.ContainsKey("texturesPng"))
            {
                List<object> lstObj = dicObj["texturesPng"] as List<object>;
                for (int index = 0; index < lstObj.Count ; index++)
                {
                    string key = lstObj[index].ToString();
                    if (this.m_tpInfos.ContainsKey(key))
                    {
                        lstObj[index] = this.m_tpInfos[key].tagFileName;
                    }
                }
            }
            if (dicObj.ContainsKey("widgetTree"))
            {
                replaceFilePng(dicObj["widgetTree"] as Dictionary<string, object>);
            }

            string jsonresult = MiniJSON.Json.Serialize(dicObj);
            System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding(false);
            File.WriteAllText(writePath, jsonresult, utf8);
        }

        private void replaceFilePng(Dictionary<string, object> dic) 
        {
            if (dic.ContainsKey("options"))
            {
                Dictionary<string, object> optionsDic = dic["options"] as Dictionary<string, object>;
                if (optionsDic.ContainsKey("fileNameData"))
                {
                    ReplacePng(optionsDic["fileNameData"] as Dictionary<string, object>);
                }
                if (optionsDic.ContainsKey("disabledData"))
                {
                    ReplacePng(optionsDic["disabledData"] as Dictionary<string, object>);
                }
                if (optionsDic.ContainsKey("normalData"))
                {
                    ReplacePng(optionsDic["normalData"] as Dictionary<string, object>);
                }
                if (optionsDic.ContainsKey("pressedData"))
                {
                    ReplacePng(optionsDic["pressedData"] as Dictionary<string, object>);
                }
                if (optionsDic.ContainsKey("textureData"))
                {
                    ReplacePng(optionsDic["textureData"] as Dictionary<string, object>);
                }
                if (optionsDic.ContainsKey("backGroundImageData"))
                {
                    ReplacePng(optionsDic["backGroundImageData"] as Dictionary<string, object>);
                }
                if (optionsDic.ContainsKey("backGroundScale9Enable"))
                {
                    ReplacePng(optionsDic["backGroundScale9Enable"] as Dictionary<string, object>);
                }
                if (optionsDic.ContainsKey("backGroundImage"))
                {
                    ReplacePng(optionsDic["backGroundImage"] as Dictionary<string, object>);
                }
                if (optionsDic.ContainsKey("charMapFileData"))
                {
                    ReplacePng(optionsDic["charMapFileData"] as Dictionary<string, object>);
                }
                
            }
            if (dic.ContainsKey("children"))
            {
                List<object> children = dic["children"] as List<object>;
                for (int index = 0; index < children.Count;index++ )
                {
                    Dictionary<string, object> sub = children[index] as Dictionary<string, object>;
                    replaceFilePng(sub);
                }
            }
        }

        private void ReplacePng(Dictionary<string, object> innerDic)
        {
            if (innerDic == null)
            {
                return;
            }
            if (innerDic.ContainsKey("path"))
            {
                if (innerDic["path"] == null)
                {
                    return;
                }
                string key = innerDic["path"].ToString();
                if (this.m_tpInfos.ContainsKey(key))
                {
                    innerDic["path"] = this.m_tpInfos[key].tagFileName;
                }
            }
        }

        private string GetResContent(TpFileInfo tpInfo,bool beLast)
        {
            string result = "";
            string last = "";
            bool beConcostudio = tpInfo.parent.beCocosStudioProject;
            FileInfo info = tpInfo.info;
            string path = GetCocostudioRelatePath(info.FullName);
            string name = info.Name.Replace(".", "_");
            string qianzhui = "";
            
            if (info.Name.EndsWith(".png") || info.Name.EndsWith(".plist"))
            {
                qianzhui = "p_";
            }
            else if (info.Name.EndsWith(".json") || info.Name.EndsWith(".ExportJson") || info.Name.EndsWith(".csb"))
            {
                 qianzhui = "j_";
            }
            else if (info.Name.EndsWith(".mp3") || info.Name.EndsWith(".ogg"))
            {
                qianzhui = "o_";
            }
            else if (info.Name.EndsWith(".tmx"))
            {
                qianzhui = "m_";
            }

            if (beConcostudio)
            {
                if (m_tpInfos.ContainsKey(path))
                {
                    TpFileInfo file = m_tpInfos[path];
                    string outputDic = this.m_rootDic.directoryInfo.Name + "/" + GetRelatePath(file.parent.directoryInfo.FullName) + "/";
                    string outFormat = outputDic + GetFormatName(file.info.Name, this.m_format);
                    path = outFormat;
                }
                else
                {
                    path = this.m_rootDic.directoryInfo.Name + "/" + GetRelatePath(PathAlign(info.FullName));
                }
            }
            else
            {
                if ((tpInfo.parent.beTMXMapPath || tpInfo.parent.bePlistBuildList) && tpInfo.bePng)
                {
                    string outputDic = this.m_rootDic.directoryInfo.Name + "/" + GetRelatePath(tpInfo.parent.directoryInfo.FullName) + "/";
                    string outFormat = outputDic + GetFormatName(tpInfo.info.Name, this.m_format);
                    path = outFormat;
                }
                else
                {
                    path = this.m_rootDic.directoryInfo.Name + "/" + GetRelatePath(PathAlign(info.FullName));
                }
                
            }
            
            result = qianzhui + name + " : \""  + path + "\"";
            if (!beLast)
            {
                result += ",";
            }
            return result;
 
        }

        private void GetAllFiles(DirectoryInfo dic, ref List<FileInfo> lst)
        {
            FileInfo[] allFiles = dic.GetFiles();
            for (int index = 0; index < allFiles.Length; index++)
            {
                List<string> files = new List<string>();
                lst.Add(allFiles[index]);
            }

            DirectoryInfo[] alldirectory = dic.GetDirectories();
            for (int index = 0; index < alldirectory.Length; index++)
            {
                GetAllFiles(alldirectory[index],ref lst);
            }
        }

        private  string PathAlign(string path)
        {
            return path.Replace("\\", "/");
        }

        private  string GetRelatePath(string path)
        {

            string result = PathAlign(path);

            if (result.StartsWith(fileCongig.filePath))
            {
                result = result.Replace(fileCongig.filePath, "");
            }

            if (result.StartsWith("//"))
            {
                result = result.Substring(2);
            }

            if (result.StartsWith("/"))
            {
                result = result.Substring(1);
            }

            return result;
        }

        private TpDicInfo GetTpDicInfoByType(TpDicInfo par, DICTYPE type)
        {
            foreach (TpDicInfo dic in par.directorys)
            {
                if (type == DICTYPE.CocostudioType)
                {
                    if (dic.beCocosStudioProject)
                    {
                        return dic;
                    }
                }
                else if (type == DICTYPE.MapType)
                {

                    if (TPConfig.BeTmxMap(dic.directoryInfo.Name))
                    {
                        return dic;
                    }
                }


                TpDicInfo sub = GetTpDicInfoByType(dic, type);
                if (sub != null)
                {
                    return sub;
                }
            }

            return null;
        }

        private TpDicInfo GetCocoStudioDir()
        {
            return GetTpDicInfoByType(this.m_rootDic, DICTYPE.CocostudioType);
        }

        private TpDicInfo GetMapDir()
        {
            return GetTpDicInfoByType(this.m_rootDic, DICTYPE.MapType);
        }

        private string GetCocoStudioPath()
        {
            return GetRelatePath(GetCocoStudioDir().directoryInfo.FullName);
        }
        private string GetCocostudioRelatePath(string path)
        {
            string result = PathAlign(path);
            string relate = PathAlign(m_rootDic.directoryInfo.FullName) + "/" + GetCocoStudioPath();
            if (result.StartsWith(relate))
            {
                result = result.Replace(relate, "");
            }

            if (result.StartsWith("//"))
            {
                result = result.Substring(2);
            }

            if (result.StartsWith("/"))
            {
                result = result.Substring(1);
            }

            return result;
        }

        private static void FileCopy(string srcdir, string destdir, bool recursive)
        {
            DirectoryInfo dir;
            FileInfo[] files;
            DirectoryInfo[] dirs;
            string tmppath;

            //determine     if     the     destination     directory     exists,     if     not     create     it       
            if (!Directory.Exists(destdir))
            {
                Directory.CreateDirectory(destdir);
            }

            dir = new DirectoryInfo(srcdir);

            //if     the     source     dir     doesn't     exist,     throw       
            if (!dir.Exists)
            {
                throw new ArgumentException("source     dir     doesn't     exist     ->     " + srcdir);
            }

            //get     all     files     in     the     current     dir       
            files = dir.GetFiles();

            //loop     through     each     file       
            foreach (FileInfo file in files)
            {
                //create     the     path     to     where     this     file     should     be     in     destdir       
                tmppath = Path.Combine(destdir, file.Name);

                //copy     file     to     dest     dir       
                file.CopyTo(tmppath, true);
            }

            //cleanup       
            files = null;

            //if     not     recursive,     all     work     is     done       
            if (!recursive)
            {
                return;
            }

            //otherwise,     get     dirs       
            dirs = dir.GetDirectories();

            //loop     through     each     sub     directory     in     the     current     dir       
            foreach (DirectoryInfo subdir in dirs)
            {
                //create     the     path     to     the     directory     in     destdir       
                tmppath = Path.Combine(destdir, subdir.Name);

                //recursively     call     this     function     over     and     over     again       
                //with     each     new     dir.       
                FileCopy(subdir.FullName, tmppath, recursive);
            }

            //cleanup       
            dirs = null;

            dir = null;
        }       
                

        private void rescreater_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.StopThread();
        }

        private void radioJS_CheckedChanged(object sender, EventArgs e)
        {
            
        }
    }
}
