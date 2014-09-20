using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ResourceFormatCreater
{
    public class TpDicInfo
    {
        List<TpFileInfo> files = new List<TpFileInfo>();
        List<TpDicInfo> directorys = new List<TpDicInfo>();
        DirectoryInfo directoryInfo;
        bool beNotBuild = false;
        bool beSingleBuild = false;
        bool beCocosStudioProject = false;
        TpDicInfo parent;

        public TpDicInfo(DirectoryInfo directoryInfo, TpDicInfo parent)
        {
            this.directoryInfo = directoryInfo;
            this.parent = parent;

            if (parent == null)
            {
                this.beNotBuild = false;
                this.beSingleBuild = false;
                this.beCocosStudioProject = false;
            }
            else
            {
                this.beNotBuild = parent.beNotBuild;
                this.beSingleBuild = parent.beSingleBuild;
                this.beCocosStudioProject = parent.beCocosStudioProject;

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
                    if (BeInPlist(files, info.info.Name))
                    {
                        files[index].beInPlist = true;
                    }
                }
            }

            DirectoryInfo[] dicInfos = directoryInfo.GetDirectories();
            foreach (DirectoryInfo info in dicInfos)
            {
                directorys.Add(new TpDicInfo(info, this));
            }
            
        }

        bool BeInPlist(List<TpFileInfo> infoList,string pngFileName)
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
    }

    public class TpFileInfo
    {
        public FileInfo info;
        public bool bePng;
        public bool beInPlist;
        public TpDicInfo parent;

        public TpFileInfo(FileInfo info, TpDicInfo parent)
        {
            this.info = info;
            this.parent = parent;
            this.beInPlist = false;
            if (info.Name.EndsWith(".png") || info.Name.EndsWith(".PNG"))
            {
                this.bePng = true;
            }
        }
    }
}
