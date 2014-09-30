using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ResourceFormatCreater
{
    public enum TPFormat
    {
        PVR = 0,
        PNG = 1,
        PVRCCZ = 2,

        PLIST = 99,
    }

    public class TpCmd
    {
        public List<TpFileInfo> lstTpFile;
        public string cmd = "";
        public string outFormat = "";
        public string outPlist = "";

        public TpCmd()
        {
            lstTpFile = new List<TpFileInfo>();
        }
    }
    public class TPistInfo
    {
        public TpFileInfo pngFile;
        public TpFileInfo plistFile;

        public TPistInfo(TpFileInfo pngFile, TpFileInfo plistFile)
        {
            this.pngFile = pngFile;
            this.plistFile = plistFile;

            this.pngFile.bePng = true;
            this.pngFile.beInPlist = true;
            this.plistFile.beInPlist = true;
        }
    }
    public class TpDicInfo
    {
        public List<TpFileInfo> files = new List<TpFileInfo>();
        public List<TpDicInfo> directorys = new List<TpDicInfo>();
        public DirectoryInfo directoryInfo;
        public bool beNotBuild = false;
        public bool beSingleBuild = false;
        public bool beCocosStudioProject = false;
        public bool bePlistBuildList = false;
        public bool beTMXMapPath = false;
        public TpDicInfo parent;

        public TpDicInfo(DirectoryInfo directoryInfo, TpDicInfo parent)
        {
            this.directoryInfo = directoryInfo;
            this.parent = parent;

            if (parent == null)
            {
                this.beNotBuild = false;
                this.beSingleBuild = false;
                this.beCocosStudioProject = false;
                this.bePlistBuildList = false;
                this.beTMXMapPath = false;
            }
            else
            {
                this.beNotBuild = parent.beNotBuild;
                this.beSingleBuild = parent.beSingleBuild;
                this.beCocosStudioProject = parent.beCocosStudioProject;
                this.bePlistBuildList = parent.bePlistBuildList;
                this.beTMXMapPath = parent.beTMXMapPath;
            }

            if (!this.beNotBuild)
            {
                if (TPConfig.BeInNotBuildList(this.directoryInfo.Name))
                {
                    this.beNotBuild = true;
                }
            }
            if (!this.beSingleBuild)
            {
                if (TPConfig.BeInSingleBuildList(this.directoryInfo.Name))
                {
                    this.beSingleBuild = true;
                }
            }
            if (!this.beCocosStudioProject)
            {
                if (TPConfig.BeInCocoStudioList(this.directoryInfo.Name))
                {
                    this.beCocosStudioProject = true;
                }
            }
            if (!this.bePlistBuildList)
            {
                if (TPConfig.BeInPlistBuildList(this.directoryInfo.Name))
                {
                    this.bePlistBuildList = true;
                }
            }
            if (!this.beTMXMapPath)
            {
                if (TPConfig.BeTmxMap(this.directoryInfo.Name))
                {
                    this.beTMXMapPath = true;
                }
            }

            FileInfo []fs = directoryInfo.GetFiles();
            foreach (FileInfo info in fs)
            {
                files.Add(new TpFileInfo(info, this));
            }

            for (int index = 0; index < files.Count;index++ )
            {
                TpFileInfo info  = files[index];
                if (info.bePng)
                {
                    if (BePngInPlist(files, info.info.Name))
                    {
                        files[index].beInPlist = true;
                    }
                }
                else
                {
                    if (info.info.Name.EndsWith(".plist"))
                    {
                        if (BePlistInPlist(files, info.info.Name))
                        {
                            files[index].beInPlist = true;
                        }
                    }
                }
            }

            DirectoryInfo[] dicInfos = directoryInfo.GetDirectories();
            foreach (DirectoryInfo info in dicInfos)
            {
                directorys.Add(new TpDicInfo(info, this));
            }
            
        }

        bool BePngInPlist(List<TpFileInfo> infoList,string pngFileName)
        {
            string name = pngFileName.Replace(".png","");
            TpFileInfo result= infoList.Find(
            delegate(TpFileInfo content)
            {
                
                if (content.info.Name.EndsWith(".plist"))
                {
                    return content.info.Name.Replace(".plist", "") == name;
                    //contentName = 
                }
                else{
                    return false;
                }
                //return content == key;
            });
            if (result == null) return false;
            return true;
        }

        bool BePlistInPlist(List<TpFileInfo> infoList, string pngFileName)
        {
            string name = pngFileName.Replace(".plist", "");
            TpFileInfo result = infoList.Find(
            delegate(TpFileInfo content)
            {
                if (content.info.Name.EndsWith(".png"))
                {
                    return content.info.Name.Replace(".png", "") == name;
                    //contentName = 
                }
                else
                {
                    return false;
                }
                //return content == key;
            });
            if (result == null) return false;
            return true;
        }

        public List<TpFileInfo> GetPlistPngs()
        {
            List<TpFileInfo> lst = new List<TpFileInfo>();
            foreach (TpFileInfo info in files)
            {
                if (info.bePng && info.beInPlist)
                {
                    lst.Add(info);
                }
            }
            return lst;
        }

    }

    public class TpFileInfo
    {
        public FileInfo info;
        public bool bePng;
        public bool beInPlist;
        public TpDicInfo parent;
        public string tagFileName = "";

        public TpFileInfo(FileInfo info, TpDicInfo parent)
        {
            this.info = info;
            this.parent = parent;
            this.beInPlist = false;
            this.tagFileName = "";
            if (info.Name.EndsWith(".png") || info.Name.EndsWith(".PNG"))
            {
                this.bePng = true;
            }
        }
    }
}
