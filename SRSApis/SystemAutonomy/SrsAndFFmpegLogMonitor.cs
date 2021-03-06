using System;
using System.IO;
using System.Threading;
using SrsApis.SrsManager.Apis;
using SrsManageCommon;

namespace SRSApis.SystemAutonomy
{
    public class SrsAndFFmpegLogMonitor
    {
        private int interval = SrsManageCommon.Common.SystemConfig.SrsAdnffmpegLogMonitorServiceinterval;

        private void processSrsFileMove(string srsFilePath)
        {
            string fileName = Path.GetFileName(srsFilePath);
            string dirPath = Path.GetDirectoryName(srsFilePath)!;
            if (!Directory.Exists(dirPath + "/logbak"))
            {
                Directory.CreateDirectory(dirPath + "/logbak");
            }

            fileName = dirPath + "/logbak/srslogback_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + fileName;
            File.Copy(srsFilePath, fileName);
            LinuxShell.Run("cat /dev/null >" + srsFilePath);
            LogWriter.WriteLog("转存srs日志,并清空现有日志", srsFilePath + "->" + fileName);
        }

        private void processFFmpegFileMove(string ffmpegFilePath)
        {
            string fileName = Path.GetFileName(ffmpegFilePath);
            string dirPath = Path.GetDirectoryName(ffmpegFilePath)!;
            if (!Directory.Exists(dirPath + "/logbak"))
            {
                Directory.CreateDirectory(dirPath + "/logbak");
            }

            fileName = dirPath + "/logbak/ffmpeglogback_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + fileName;

            File.Copy(ffmpegFilePath, fileName);
            LinuxShell.Run("cat /dev/null >" + ffmpegFilePath);
            LogWriter.WriteLog("转存ffmpeg日志,并清空现有日志", ffmpegFilePath + "->" + fileName);
        }

       
        private void Run()
        {
            while (true)
            {
                foreach (var srs in Common.SrsManagers)
                {
                    if (srs != null && srs.IsRunning)
                    {
                        string srsLogFile = srs.Srs.Srs_log_file!;
                        string ffmpegLogPath = srs.Srs.Ff_log_dir!;
                        FileInfo srsLog = new FileInfo(srsLogFile);
                        if (srsLog.Exists && srsLog.Length >= (1024 * 1000 * 10)) //10M
                        {
                            processSrsFileMove(srsLogFile);
                        }

                        DirectoryInfo ffmpegDir = new DirectoryInfo(ffmpegLogPath);
                        if (ffmpegDir.Exists)
                        {
                            foreach (var file in ffmpegDir.GetFiles())
                            {
                                if (file.FullName.Contains("ffmpeglogback")) continue;

                                if (file.Exists && file.Length >= 1024 * 1000 * 10) //10M
                                {
                                    processFFmpegFileMove(file.FullName);
                                }

                                Thread.Sleep(500);
                            }
                        }
                    }

                    Thread.Sleep(1000);
                }


                Thread.Sleep(interval);
            }
        }

        public SrsAndFFmpegLogMonitor()
        {
            new Thread(new ThreadStart(delegate

            {
                try
                {
                    LogWriter.WriteLog("启动日志转存服务...(循环间隔：" + interval + "ms)");
                    Run();
                }
                catch (Exception ex)
                {
                    LogWriter.WriteLog("启动日志转存服务失败...", ex.Message + "\r\n" + ex.StackTrace, ConsoleColor.Yellow);
                }
            })).Start();
        }
    }
}