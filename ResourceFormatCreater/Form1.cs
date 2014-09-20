using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResourceFormatCreater
{
    public partial class rescreater : Form
    {
        FileConfig fileCongig = null;
        Dictionary<string, List<FileInfo>> m_dics = new Dictionary<string, List<FileInfo>>();
        public rescreater()
        {
            InitializeComponent();

            TPConfig.LoadConfig();

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
            
            return;
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
             
            DirectoryInfo dicInfo = new DirectoryInfo(fileCongig.filePath);
            TpDicInfo info = new TpDicInfo(dicInfo, null);

            string result = "";
            for (int i = 0; i < 10; i++)
            {
                string cmdStr = "TexturePacker E:/work/test/sys/loadingbg.png E:/work/test/sys/gateselectbg.png --allow-free-size --trim --padding 1 --opt RGBA4444 --dither-fs-alpha --format cocos2d  --data d://" + i+ "hd.plist --sheet  d://"+i+"hd.png";
                result = result + cmdStr + "\n";
            }
            
            Process p = new Process();  // 初始化新的进程
            p.StartInfo.FileName = "CMD.EXE"; //创建CMD.EXE 进程
            p.StartInfo.RedirectStandardInput = true; //重定向输入
            p.StartInfo.RedirectStandardOutput = true;//重定向输出
            p.StartInfo.UseShellExecute = false; // 不调用系统的Shell
            p.StartInfo.RedirectStandardError = true; // 重定向Error
            p.StartInfo.CreateNoWindow = false; //不创建窗口
            p.Start(); // 启动进程
            p.StandardInput.WriteLine(result); // Cmd 命令
            p.StandardInput.WriteLine("exit"); // 退出
            p.OutputDataReceived += p_OutputDataReceived;
            p.BeginOutputReadLine();
            //string s = p.StandardOutput.ReadToEnd(); //将输出赋值给 S
           // Console.WriteLine(s);
           // p.WaitForExit();  // 等待退出
           // richTextBox1.Text = s; // 在Richtextbox1 中显示 输出内容
            /*
            LoadRes();
            CreateListJson();
             * */
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
                //result.Append(e.Data);
            }
        }

        private void LoadRes()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(fileCongig.filePath);
            DirectoryInfo[] alldirectory = directoryInfo.GetDirectories();
            for (int index = 0; index < alldirectory.Length; index++)
            {
                List<FileInfo> files = new List<FileInfo>();
                GetAllFiles(alldirectory[index], ref files);
                m_dics[alldirectory[index].Name] = files;
            }

            List<FileInfo> jsons = new List<FileInfo>();
            m_dics["exportJson"] = jsons;
            FileInfo[] allFiles = directoryInfo.GetFiles();
            for (int index = 0; index < allFiles.Length; index++)
            {
                if (allFiles[index].Name.EndsWith(".json") || allFiles[index].Name.EndsWith(".ExportJson") || allFiles[index].Name.EndsWith(".csb"))
                {
                    jsons.Add(allFiles[index]);
                }
            }
        }

        private void CreateListJson()
        {
            string content = "";
            foreach (KeyValuePair<string, List<FileInfo>> each in m_dics)
            {
                content += CreateTxtResource(each.Key, each.Value);
            }

            content += "var list_cocosResource = [];\n";
            foreach (KeyValuePair<string, List<FileInfo>> each in m_dics)
            {
                content += "list_cocosResource.push( res_" + each.Key + " );\n";
                //content += CreateTxtResource(each.Key, each.Value);
            }

            string writePath = fileCongig.createPath + "/" + fileCongig.fileName;
            File.WriteAllText(writePath, content);
            //Console.Write(content);
            MessageBox.Show("生成成功");
        }

        private string CreateTxtResource(string name, List<FileInfo> lst)
        {
            string content = "var res_" + name + "={\n";
            for (int index = 0; index < lst.Count; index++ )
            {
                FileInfo each = lst[index];
                content += "\t" + GetResContent(each, index == (lst.Count - 1)) + "\n";
            }

            content += "};\n\n";

            return content;
        }

        private string GetResContent(FileInfo info,bool beLast)
        {
            string result = "";
            string last = "";
            string path = fileCongig.basePath + GetRelatePath(PathAlign(info.FullName));
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

            result = qianzhui + name + " : \"" + path + "\"";
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
    }
}
