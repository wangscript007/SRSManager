﻿#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mictlanix.DotNet.Onvif.Common;
using SrsApis.SrsManager.Apis.ApiModules;
using SrsManageCommon.ApisStructs;
using DateTime = System.DateTime;

namespace Test_FreeSql
{
    class Program
    {
      

        static void Main(string[] args)
        {
            DBManager.fsql.Delete<StreamDvrPlan>().Where("1=1").ExecuteAffrows();
            DBManager.fsql.Delete<DvrDayTimeRange>().Where("1=1").ExecuteAffrows();
            var a = new StreamDvrPlan();
            a.App = "live";
            a.Enable = true;
            a.Stream = "stream";
            a.DeviceId = "device_id123";
            a.LimitDays = 10;
            a.LimitSpace = 99999999;
            a.VhostDomain = "vhost";
            a.OverStepPlan = OverStepPlan.DeleteFile;
            a.TimeRange = new List<DvrDayTimeRange>();
            DvrDayTimeRange b = new DvrDayTimeRange();
            b.EndTime = DateTime.Now.AddDays(10);
            b.StartTime = DateTime.Now;
            b.WeekDay = DateTime.Now.DayOfWeek;
            a.TimeRange.Add(b);
            DvrDayTimeRange c = new DvrDayTimeRange();
            c.EndTime = DateTime.Now.AddDays(15);
            c.StartTime = DateTime.Now.AddDays(-5);
            c.WeekDay = DayOfWeek.Monday;
            a.TimeRange.Add(c);
            /*联同子类一起插入*/
            var repo = DBManager.fsql.GetRepository<StreamDvrPlan>();
            repo.DbContextOptions.EnableAddOrUpdateNavigateList = true; //需要手工开启
            repo.Insert(a);
            /*联同子类一起插入*/
            /*联同子类一起查出*/
            var ret = DBManager.fsql.Select<StreamDvrPlan>().IncludeMany(a => a.TimeRange)
                .ToList();
             /*联同子类一起查出*/
             
            Console.WriteLine("Hello World!");
        }
    }
}