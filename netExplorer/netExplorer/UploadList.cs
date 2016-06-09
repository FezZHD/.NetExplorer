namespace netExplorer
{
    public class UploadList:DownloadList
    {
        public string LocalFilePath { get; set; }
        public UploadList(string downloadPath, string fileName,string localFilePath) : base(downloadPath, fileName)
        {
            DownloadPath = downloadPath;
            FileName = fileName;
            LocalFilePath = localFilePath;
        }
    }
}
