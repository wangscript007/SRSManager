using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SrsApis.SrsManager.Apis.ApiModules;
using SrsConfFile.SRSConfClass;
using SrsManageCommon;
using SrsManageCommon.ApisStructs;
using Common = SRSApis.Common;
using Dvr = SrsApis.SrsManager.Apis.ApiModules.Dvr;

namespace SrsApis.SrsManager.Apis
{
    public static class FastUsefulApis
    {

        /// <summary>
        /// 返回Dvr列表BydeviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<Dvr> GetDvrList(string deviceId, out ResponseStruct rs)
        {
            
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return null!;
            }

           return  OrmService.Db.Select<Dvr>().Where(x => x.Device_Id!.Trim().ToLower().Equals(deviceId.Trim().ToLower()))
                .ToList();
           
        }
        /// <summary>
        /// 通过id删除一个录制计划
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool DeleteDvrPlanById(long id, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            if (id <= 0)
            {
                rs.Code = ErrorNumber.FunctionInputParamsError;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.FunctionInputParamsError];
                return false;
            }

            var retSdp = OrmService.Db.Select<StreamDvrPlan>().Where(
                x => x.Id == id).First();
            if (retSdp == null)
            {
                rs.Code = ErrorNumber.SrsDvrPlanNotExists;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsDvrPlanNotExists];
                return false;
            }

            var retDelete = OrmService.Db.Delete<StreamDvrPlan>().Where(x => x.Id == id).ExecuteAffrows();
            if (retDelete > 0)
            {
                OrmService.Db.Delete<DvrDayTimeRange>().Where(x => x.DvrDayTimeRangeStreamDvrPlanId == id)
                    .ExecuteAffrows();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 启用或停止一个录制计划
        /// </summary>
        /// <param name="id"></param>
        /// <param name="enable"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool OnOrOffDvrPlanById(long id, bool enable, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            if (id <= 0)
            {
                rs.Code = ErrorNumber.FunctionInputParamsError;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.FunctionInputParamsError];
                return false;
            }

            var retSdp = OrmService.Db.Select<StreamDvrPlan>().Where(
                x => x.Id == id).First();
            if (retSdp == null)
            {
                rs.Code = ErrorNumber.SrsDvrPlanNotExists;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsDvrPlanNotExists];
                return false;
            }

            var retUpdate = OrmService.Db.Update<StreamDvrPlan>().Set(x => x.Enable, enable).Where(x => x.Id == id)
                .ExecuteAffrows();
            if (retUpdate > 0)
                return true;
            return false;
        }

        /// <summary>
        /// 获取一个录制计划Byid
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static StreamDvrPlan GetDvrPlanById(long id, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return null!;
            }

            if (id <= 0)
            {
                rs.Code = ErrorNumber.FunctionInputParamsError;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.FunctionInputParamsError];
                return null!;
            }

            var retSdp = OrmService.Db.Select<StreamDvrPlan>().Where(
                x => x.Id == id).First();
            if (retSdp == null)
            {
                rs.Code = ErrorNumber.SrsDvrPlanNotExists;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsDvrPlanNotExists];
                return null!;
            }

            var rr = OrmService.Db.Select<DvrDayTimeRange>()
                .Where(x => x.DvrDayTimeRangeStreamDvrPlanId == retSdp.Id).ToList();
            if (rr != null)
            {
                retSdp.TimeRange = rr;
            }

            return retSdp;
        }

        /// <summary>
        /// 获取录制计划
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<StreamDvrPlan> GetDvrPlan(ReqGetDvrPlan obj, out ResponseStruct rs)
        {
            bool idFound = !string.IsNullOrEmpty(obj.DeviceId);
            bool vhostFound = !string.IsNullOrEmpty(obj.VhostDomain);
            bool streamFound = !string.IsNullOrEmpty(obj.Stream);
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return null!;
            }

            if (!string.IsNullOrEmpty(obj.DeviceId))
            {
                var retSrs = SystemApis.GetSrsManagerInstanceByDeviceId(obj.DeviceId);
                if (retSrs == null || retSrs.Srs == null)
                {
                    rs.Code = ErrorNumber.SrsObjectNotInit;
                    rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                    return null!;
                }

                if (!string.IsNullOrEmpty(obj.VhostDomain))
                {
                    if (retSrs.Srs.Vhosts == null || retSrs.Srs.Vhosts.Count == 0)
                    {
                        rs.Code = ErrorNumber.SrsObjectNotInit;
                        rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                        return null!;
                    }

                    var retVhost = VhostApis.GetVhostByDomain(obj.DeviceId, obj.VhostDomain, out rs);
                    if (retVhost == null)
                    {
                        return null!;
                    }

                    if (!string.IsNullOrEmpty(obj.Stream))
                    {
                        var onPublishList = GetOnPublishMonitorList(out rs);
                        if (onPublishList == null || onPublishList.Count == 0)
                        {
                            rs.Code = ErrorNumber.SrsStreamNotExists;
                            rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsStreamNotExists];
                            return null!;
                        }
                    }
                }
            }

            List<StreamDvrPlan> tmpList;
            if (idFound && !vhostFound && !streamFound)
            {
                tmpList = OrmService.Db.Select<StreamDvrPlan>().Where(
                    x => x.DeviceId.Trim().ToLower().Equals(obj.DeviceId!.Trim().ToLower())).ToList();
            }
            else if (idFound && vhostFound && !streamFound)
            {
                tmpList = OrmService.Db.Select<StreamDvrPlan>().Where(
                    x => x.DeviceId.Trim().ToLower().Equals(obj.DeviceId!.Trim().ToLower())
                         && x.VhostDomain.Trim().ToLower().Equals(obj.VhostDomain!.Trim().ToLower())).ToList();
            }
            else if (idFound && vhostFound && streamFound)
            {
                tmpList = OrmService.Db.Select<StreamDvrPlan>().Where(
                    x => x.DeviceId.Trim().ToLower().Equals(obj.DeviceId!.Trim().ToLower())
                         && x.VhostDomain.Trim().ToLower().Equals(obj.VhostDomain!.Trim().ToLower())
                         && x.Stream.Trim().ToLower().Equals(obj.Stream!.Trim().ToLower())).ToList();
            }
            else
            {
                tmpList = OrmService.Db.Select<StreamDvrPlan>().Where("1=1").ToList();
            }

            if (tmpList != null)
            {
                foreach (var r in tmpList)
                {
                    if (r != null)
                    {
                        var rr = OrmService.Db.Select<DvrDayTimeRange>()
                            .Where(x => x.DvrDayTimeRangeStreamDvrPlanId == r.Id).ToList();
                        if (rr != null)
                        {
                            r.TimeRange = rr;
                        }
                    }
                }

                return tmpList;
            }

            return null!;
        }

        /// <summary>
        /// 修改一个录制计划ByID
        /// </summary>
        /// <param name="sdp"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool SetDvrPlanById(StreamDvrPlan sdp, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            if (sdp.Id <= 0)
            {
                rs.Code = ErrorNumber.FunctionInputParamsError;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.FunctionInputParamsError];
                return false;
            }

            if (sdp.TimeRange != null)
            {
                foreach (var s in sdp.TimeRange)
                {
                    if (s.StartTime >= s.EndTime)
                    {
                        rs.Code = ErrorNumber.FunctionInputParamsError;
                        rs.Message = ErrorMessage.ErrorDic![ErrorNumber.FunctionInputParamsError];
                        return false;
                    }

                    if ((s.EndTime - s.StartTime).TotalSeconds <= 120)
                    {
                        rs.Code = ErrorNumber.SrsDvrPlanTimeLimitExcept;
                        rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsDvrPlanTimeLimitExcept];

                        return false;
                    }
                }
            }

            var retSdp = OrmService.Db.Select<StreamDvrPlan>()
                .Where(x => x.Id == sdp.Id)
                .First();
            if (retSdp != null)
            {
                Console.WriteLine("retSDp!=njull");
                var retUpdate = OrmService.Db.Update<StreamDvrPlan>(sdp).Set(x=>x.LimitDays,sdp.LimitDays).
                    Set(x=>x.LimitSpace!=sdp.LimitSpace).Set(x=>x.OverStepPlan!=sdp.OverStepPlan).
                    Set(x=>x.Enable!=sdp.Enable).Where(x => x.Id == retSdp.Id).ExecuteAffrows();
               Console.WriteLine("retUpdate:"+retUpdate);
                if (retUpdate > 0)
                {
                    if (sdp.TimeRange != null)
                    {
                     var retDelete=   OrmService.Db.Delete<DvrDayTimeRange>()
                            .Where(x => x.DvrDayTimeRangeStreamDvrPlanId == retSdp.Id)
                            .ExecuteAffrows();
                        Console.WriteLine("delete"+retDelete);
                        for (int i = 0; i <= sdp.TimeRange!.Count - 1; i++)
                        {
                            sdp.TimeRange[i].DvrDayTimeRangeStreamDvrPlanId = retSdp.Id;
                        }

                        var retInsert = OrmService.Db.Insert<List<DvrDayTimeRange>>(sdp.TimeRange)
                            .ExecuteAffrows();
                        Console.WriteLine("retInsert:" + retInsert);
                        if (retInsert > 0)
                            return true;
                    }
                    else
                    {
                        return true;
                    }

                    return false;
                }

                return false;
            }

            rs.Code = ErrorNumber.SrsDvrPlanNotExists;
            rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsDvrPlanNotExists];

            return false;
        }

        /// <summary>
        /// 修改或新建一个录制计划
        /// </summary>
        /// <param name="sdp"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool SetDvrPlan(StreamDvrPlan sdp, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            var ret = Common.SrsManagers.FindLast(x =>
                x.SrsDeviceId.Trim().ToLower().Equals(sdp.DeviceId.Trim().ToLower()));
            if (ret == null)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            if (ret.Srs.Vhosts == null || ret.Srs.Vhosts.Count == 0)
            {
                rs.Code = ErrorNumber.SrsSubInstanceNotFound;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound];
                return false;
            }

            if (sdp.TimeRange != null)
            {
                foreach (var s in sdp.TimeRange)
                {
                    if (s.StartTime >= s.EndTime)
                    {
                        rs.Code = ErrorNumber.FunctionInputParamsError;
                        rs.Message = ErrorMessage.ErrorDic![ErrorNumber.FunctionInputParamsError];
                        return false;
                    }

                    if ((s.EndTime - s.StartTime).TotalSeconds <= 120)
                    {
                        rs.Code = ErrorNumber.SrsDvrPlanTimeLimitExcept;
                        rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsDvrPlanTimeLimitExcept];

                        return false;
                    }
                }
            }

            var retVhost =
                ret.Srs.Vhosts.FindLast(x => x.VhostDomain!.Trim().ToLower().Equals(sdp.VhostDomain.Trim().ToLower()));
            if (retVhost == null)
            {
                rs.Code = ErrorNumber.SrsSubInstanceNotFound;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound];
                return false;
            }

            var retSdp = OrmService.Db.Select<StreamDvrPlan>()
                .Where(x => x.Stream == sdp.Stream && x.App == sdp.App && x.DeviceId == sdp.DeviceId &&
                            x.VhostDomain == sdp.VhostDomain)
                .First();
            if (retSdp != null)
            {
                var retUpdate = OrmService.Db.Update<StreamDvrPlan>(sdp).Set(x=>x.LimitDays,sdp.LimitDays).
                    Set(x=>x.LimitSpace!=sdp.LimitSpace).Set(x=>x.OverStepPlan!=sdp.OverStepPlan).
                    Set(x=>x.Enable!=sdp.Enable).Where(x => x.Id == retSdp.Id).ExecuteAffrows();
                if (retUpdate > 0)
                {
                    if (sdp.TimeRange != null)
                    {
                        var retDelete=OrmService.Db.Delete<DvrDayTimeRange>()
                            .Where(x => x.DvrDayTimeRangeStreamDvrPlanId == retSdp.Id)
                            .ExecuteAffrows();
                        Console.WriteLine("delete"+retDelete);
                        for (int i = 0; i <= sdp.TimeRange!.Count - 1; i++)
                        {
                            sdp.TimeRange[i].DvrDayTimeRangeStreamDvrPlanId = retSdp.Id;
                        }

                        var retInsert = OrmService.Db.Insert<List<DvrDayTimeRange>>(sdp.TimeRange)
                            .ExecuteAffrows();
                        if (retInsert > 0)
                            return true;
                    }
                    else
                    {
                        return true;
                    }

                    return false;
                }

                return false;
            }

            var repo = OrmService.Db.GetRepository<StreamDvrPlan>();
            repo.Insert(sdp);
            repo.SaveMany(sdp, "DvrDayTimeRange");
            return true;
        }

        /// <summary>
        /// 对某个vhost启用或停用低时延模式
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="vhostDomain"></param>
        /// <param name="enable"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool OnOrOffVhostMinDelay(string deviceId, string vhostDomain, bool enable, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            var ret = Common.SrsManagers.FindLast(x =>
                x.SrsDeviceId.Trim().ToLower().Equals(deviceId.Trim().ToLower()));
            if (ret == null)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            if (ret.Srs.Vhosts == null || ret.Srs.Vhosts.Count == 0)
            {
                rs.Code = ErrorNumber.SrsSubInstanceNotFound;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound];
                return false;
            }

            var retVhost =
                ret.Srs.Vhosts.FindLast(x => x.VhostDomain!.Trim().ToLower().Equals(vhostDomain.Trim().ToLower()));
            if (retVhost == null)
            {
                rs.Code = ErrorNumber.SrsSubInstanceNotFound;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound];
                return false;
            }

            retVhost.Tcp_nodelay = enable;
            retVhost.Min_latency = enable;
            if (enable)
            {
                if (retVhost.Vplay == null)
                {
                    retVhost.Vplay = new Play();
                }

                retVhost.Vplay.Gop_cache = !enable;
                retVhost.Vplay.Queue_length = 10;
                retVhost.Vplay.Mw_latency = 100;
                if (retVhost.Vpublish == null)
                {
                    retVhost.Vpublish = new SrsConfFile.SRSConfClass.Publish();
                }

                retVhost.Vpublish.Mr = !enable;
            }
            else
            {
                if (retVhost.Vplay != null)
                    retVhost.Vplay = null;
                if (retVhost.Vpublish != null)
                    retVhost.Vpublish = null;
            }

            return true;
        }

        /// <summary>
        /// 用于28181的ptz镜头控制
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="obj"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool PtzZoomForGb28181(string deviceId, SrsGBT28181PtzZoomModule obj, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            var ret = Common.SrsManagers.FindLast(x =>
                x.SrsDeviceId.Trim().ToLower().Equals(deviceId.Trim().ToLower()));
            if (ret == null)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            var retClient = FastUsefulApis.GetClientInfoByStreamValue(obj.Stream!, out rs);
            if (retClient == null)
            {
                return false;
            }

            if (retClient.MonitorType != MonitorType.GBT28181)
            {
                rs.Code = ErrorNumber.SrsClientNotGB28181;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsClientNotGB28181];
                return false;
            }

            string[] streams = obj.Stream!.Split('@', StringSplitOptions.RemoveEmptyEntries);
            if (streams.Length != 2)
            {
                rs.Code = ErrorNumber.FunctionInputParamsError;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.FunctionInputParamsError];
                return false;
            }

            if (ret.Srs == null || ret.Srs.Http_api == null || ret.Srs.Http_api.Enabled == false)
            {
                rs.Code = ErrorNumber.SrsSubInstanceNotFound;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound];
                return false;
            }

            if (obj.Speed > 255) obj.Speed = 255;
            if (obj.Speed <= 0) obj.Speed = 1;

            string cmd = "stop";
            switch (obj.PtzZoomDir)
            {
                case ZoomDir.MORE:
                    cmd = "zoomin";
                    break;
                case ZoomDir.LESS:
                    cmd = "zoomout";
                    break;
                default:
                    cmd = "stop";
                    break;
            }

            if (obj.Stop == true) cmd = "stop";
            string reqUrl = "http://127.0.0.1:" + ret.Srs.Http_api.Listen.ToString() + "/api/v1/gb28181?id=" +
                            streams[0] + "&action=sip_ptz&chid=" +
                            streams[1] + "&ptzcmd=" + cmd + "&speed=" + obj.Speed;
            try
            {
                string tmpStr = NetHelper.Get(reqUrl);
                var retReq = JsonHelper.FromJson<SrsGb28181PtzControlResponseModule>(tmpStr);
                if (retReq != null && retReq.Code == 0)
                {
                    return true;
                }
                else
                {
                    rs.Code = (ErrorNumber) retReq!.Code;
                    rs.Message = ErrorMessage.ErrorDic![rs.Code];
                    return false;
                }
            }
            catch (Exception ex)
            {
                rs.Code = ErrorNumber.Other;
                rs.Message = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 用于28181的ptz移动控制
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="obj"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool PtzMoveForGb28181(string deviceId, SrsGBT28181PtzMoveModule obj, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            var ret = Common.SrsManagers.FindLast(x =>
                x.SrsDeviceId.Trim().ToLower().Equals(deviceId.Trim().ToLower()));
            if (ret == null)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return false;
            }

            var retClient = FastUsefulApis.GetClientInfoByStreamValue(obj.Stream!, out rs);
            if (retClient == null)
            {
                return false;
            }

            if (retClient.MonitorType != MonitorType.GBT28181)
            {
                rs.Code = ErrorNumber.SrsClientNotGB28181;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsClientNotGB28181];
                return false;
            }

            string[] streams = obj.Stream!.Split('@', StringSplitOptions.RemoveEmptyEntries);
            if (streams.Length != 2)
            {
                rs.Code = ErrorNumber.FunctionInputParamsError;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.FunctionInputParamsError];
                return false;
            }

            if (ret.Srs == null || ret.Srs.Http_api == null || ret.Srs.Http_api.Enabled == false)
            {
                rs.Code = ErrorNumber.SrsSubInstanceNotFound;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound];
                return false;
            }

            if (obj.Speed > 255) obj.Speed = 255;
            if (obj.Speed <= 0) obj.Speed = 1;

            string cmd = "stop";
            switch (obj.PtzMoveDir)
            {
                case PtzMoveDir.LEFT:
                    cmd = "left";
                    break;
                case PtzMoveDir.RIGHT:
                    cmd = "right";
                    break;
                case PtzMoveDir.UP:
                    cmd = "up";
                    break;
                case PtzMoveDir.DOWN:
                    cmd = "down";
                    break;
                default:
                    cmd = "stop";
                    break;
            }


            if (obj.Stop == true) cmd = "stop";
            string reqUrl = "http://127.0.0.1:" + ret.Srs.Http_api.Listen.ToString() + "/api/v1/gb28181?id=" +
                            streams[0] + "&action=sip_ptz&chid=" +
                            streams[1] + "&ptzcmd=" + cmd + "&speed=" + obj.Speed;
            try
            {
                string tmpStr = NetHelper.Get(reqUrl);
                var retReq = JsonHelper.FromJson<SrsGb28181PtzControlResponseModule>(tmpStr);
                if (retReq != null && retReq.Code == 0)
                {
                    return true;
                }
                else
                {
                    rs.Code = (ErrorNumber) retReq!.Code;
                    rs.Message = ErrorMessage.ErrorDic![rs.Code];
                    return false;
                }
            }
            catch (Exception ex)
            {
                rs.Code = ErrorNumber.Other;
                rs.Message = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 通过stream的值返回monitor的信息
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static Client GetClientInfoByStreamValue(string stream, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            var ret = OrmService.Db.Select<Client>()
                .Where(x => x.ClientType == ClientType.Monitor && x.Stream!.Equals(stream.Trim())).First();
            return ret;
        }

        /// <summary>
        /// 获取所有正在运行中的srs信息
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<Self_Srs> GetRunningSrsInfoList(out ResponseStruct rs)
        {
            List<Self_Srs> result = null!;
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return null!;
            }

            if (Common.SrsManagers != null && Common.SrsManagers.Count > 0)
            {
                result = new List<Self_Srs>();
                foreach (var sm in Common.SrsManagers)
                {
                    if (sm.IsRunning && sm.Srs.Http_api != null && sm.Srs.Http_api.Enabled == true)
                    {
                        string reqUrl = "http://127.0.0.1:" + sm!.Srs.Http_api!.Listen + "/api/v1/summaries";
                        try
                        {
                            string tmpStr = NetHelper.Get(reqUrl);
                            var retReq = JsonHelper.FromJson<SrsSystemInfo>(tmpStr);
                            if (retReq != null && retReq.Data != null && retReq.Data.Self != null)
                            {
                                string filename = Path.GetFileName(retReq.Data.Self.Argv)!;
                                string ext = Path.GetExtension(filename);
                                retReq.Data.Self.Srs_DeviceId = filename.Replace(ext, "");
                                result.Add(retReq.Data.Self);
                            }
                        }
                        catch
                        {
                        }
                    }

                    Thread.Sleep(50);
                }
            }

            return result!;
        }

        /// <summary>
        /// 停止所有运行中的srs实例
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<SrsStartStatus> StopAllSrs(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return null!;
            }

            List<SrsStartStatus> result = new List<SrsStartStatus>();
            foreach (var sm in Common.SrsManagers)
            {
                if (sm.IsRunning == true)
                {
                    bool ret = sm.Stop(out rs);
                    SrsStartStatus sts = new SrsStartStatus();
                    sts.DeviceId = sm.SrsDeviceId;
                    sts.IsStarted = !ret;
                    sts.Message = JsonHelper.ToJson(rs);
                    result.Add(sts);
                }

                Thread.Sleep(50);
            }

            return result;
        }

        /// <summary>
        /// 初始化及启动所有未初始化或未启动的srs实例
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<SrsStartStatus> InitAndStartAllSrs(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.SrsManagers == null || Common.SrsManagers.Count == 0)
            {
                rs.Code = ErrorNumber.SrsObjectNotInit;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit];
                return null!;
            }

            List<SrsStartStatus> result = new List<SrsStartStatus>();
            foreach (var sm in Common.SrsManagers)
            {
                if (sm.IsInit == false || sm.IsRunning == false)
                {
                    bool ret = sm.SRS_Init(sm.SrsConfigPath, out rs);
                    if (ret)
                    {
                        ret = sm.Start(out rs);
                    }

                    SrsStartStatus sts = new SrsStartStatus();
                    sts.DeviceId = sm.SrsDeviceId;
                    sts.IsStarted = ret;
                    sts.Message = JsonHelper.ToJson(rs);
                    result.Add(sts);
                }

                Thread.Sleep(50);
            }

            return result;
        }

        /// <summary>
        /// 通过deviceId及clientId踢掉一个摄像头或踢掉一个播放者
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="clientId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool KickoffClient(string deviceId, string clientId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            var ret = Common.SrsManagers.FindLast(x =>
                x.SrsDeviceId.Trim().ToUpper().Equals(deviceId.Trim().ToUpper()));
            if (ret != null && ret.Srs != null)
            {
                if (ret.Srs.Http_api != null && ret.Srs.Http_api.Enabled == true)
                {
                    string reqUrl = "http://127.0.0.1:" + ret.Srs.Http_api!.Listen + "/api/v1/clients/" + clientId;
                    try
                    {
                        string tmpStr = NetHelper.Delete(reqUrl);
                        var retReq = JsonHelper.FromJson<SrsSimpleResponseModule>(tmpStr);
                        if (retReq.Code == 0)
                        {
                            return true!;
                        }

                        return false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return false;
                    }
                }

                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.SrsSubInstanceNotFound,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound],
                };
                return false;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.SrsObjectNotInit,
                Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit],
            };
            return false;
        }

        /// <summary>
        /// 获取Stream状态信息BySrsDeviceId,及streamId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="streamId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static SrsStreamSingleStatusModule GetStreamStatusByDeviceIdAndStreamId(string deviceId, string streamId,
            out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            var ret = Common.SrsManagers.FindLast(x =>
                x.SrsDeviceId.Trim().ToUpper().Equals(deviceId.Trim().ToUpper()));
            if (ret != null && ret.Srs != null)
            {
                if (ret.Srs.Http_api != null && ret.Srs.Http_api.Enabled == true)
                {
                    string reqUrl = "http://127.0.0.1:" + ret.Srs.Http_api!.Listen + "/api/v1/streams/" + streamId;
                    try
                    {
                        string tmpStr = NetHelper.Get(reqUrl);
                        var retReq = JsonHelper.FromJson<SrsStreamSingleStatusModule>(tmpStr);
                        if (retReq.Code == 0 && retReq.Stream != null)
                        {
                            return retReq!;
                        }

                        return null!;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null!;
                    }
                }

                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.SrsSubInstanceNotFound,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound],
                };
                return null!;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.SrsObjectNotInit,
                Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit],
            };
            return null!;
        }

        /// <summary>
        /// 获取StreamList状态信息BySrsDeviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static SrsStreamsStatusModule GetStreamListStatusByDeviceId(string deviceId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            var ret = Common.SrsManagers.FindLast(x =>
                x.SrsDeviceId.Trim().ToUpper().Equals(deviceId.Trim().ToUpper()));
            if (ret != null && ret.Srs != null)
            {
                if (ret.Srs.Http_api != null && ret.Srs.Http_api.Enabled == true)
                {
                    string reqUrl = "http://127.0.0.1:" + ret.Srs.Http_api!.Listen + "/api/v1/streams/";
                    try
                    {
                        string tmpStr = NetHelper.Get(reqUrl);
                        var retReq = JsonHelper.FromJson<SrsStreamsStatusModule>(tmpStr);
                        if (retReq.Code == 0 && retReq.Streams != null)
                        {
                            return retReq!;
                        }

                        return null!;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null!;
                    }
                }

                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.SrsSubInstanceNotFound,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound],
                };
                return null!;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.SrsObjectNotInit,
                Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit],
            };
            return null!;
        }


        /// <summary>
        /// 获取Vhost状态信息BySrsDeviceId,及vhostId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="vhostId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static SrsVhostSingleStatusModule GetVhostStatusByDeviceIdAndVhostId(string deviceId, string vhostId,
            out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            var ret = Common.SrsManagers.FindLast(x =>
                x.SrsDeviceId.Trim().ToUpper().Equals(deviceId.Trim().ToUpper()));
            if (ret != null && ret.Srs != null)
            {
                if (ret.Srs.Http_api != null && ret.Srs.Http_api.Enabled == true)
                {
                    string reqUrl = "http://127.0.0.1:" + ret.Srs.Http_api!.Listen + "/api/v1/vhosts/" + vhostId;
                    try
                    {
                        string tmpStr = NetHelper.Get(reqUrl);
                        var retReq = JsonHelper.FromJson<SrsVhostSingleStatusModule>(tmpStr);
                        if (retReq.Code == 0 && retReq.Vhost != null)
                        {
                            return retReq!;
                        }

                        return null!;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null!;
                    }
                }

                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.SrsSubInstanceNotFound,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound],
                };
                return null!;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.SrsObjectNotInit,
                Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit],
            };
            return null!;
        }


        /// <summary>
        /// 获取VhostList状态信息BySrsDeviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static SrsVhostsStatusModule GetVhostListStatusByDeviceId(string deviceId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            var ret = Common.SrsManagers.FindLast(x =>
                x.SrsDeviceId.Trim().ToUpper().Equals(deviceId.Trim().ToUpper()));
            if (ret != null && ret.Srs != null)
            {
                if (ret.Srs.Http_api != null && ret.Srs.Http_api.Enabled == true)
                {
                    string reqUrl = "http://127.0.0.1:" + ret.Srs.Http_api!.Listen + "/api/v1/vhosts/";
                    try
                    {
                        string tmpStr = NetHelper.Get(reqUrl);
                        var retReq = JsonHelper.FromJson<SrsVhostsStatusModule>(tmpStr);
                        if (retReq.Code == 0 && retReq.Vhosts != null)
                        {
                            return retReq!;
                        }

                        return null!;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null!;
                    }
                }

                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.SrsSubInstanceNotFound,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.SrsSubInstanceNotFound],
                };
                return null!;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.SrsObjectNotInit,
                Message = ErrorMessage.ErrorDic![ErrorNumber.SrsObjectNotInit],
            };
            return null!;
        }


        /// <summary>
        /// 获取所有播放中的用户
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<Client> GetOnlinePlayerByDeviceId(string deviceId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            List<Client> result = SrsManageCommon.OrmService.Db.Select<Client>()
                .Where(x => x.IsOnline == true && x.ClientType == ClientType.User && x.IsPlay == true &&
                            x.Device_Id!.Equals(deviceId)).ToList();
            return result;
        }

        /// <summary>
        /// 获取所有播放中的用户
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<Client> GetOnlinePlayer(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            List<Client> result = SrsManageCommon.OrmService.Db.Select<Client>()
                .Where(x => x.IsOnline == true && x.ClientType == ClientType.User && x.IsPlay == true).ToList();
            return result;
        }


        /// <summary>
        /// 获取所有发布中的摄像头
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<Client> GetOnPublishMonitorListByDeviceId(string deviceId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(deviceId))
            {
                rs.Code = ErrorNumber.FunctionInputParamsError;
                rs.Message = ErrorMessage.ErrorDic![ErrorNumber.FunctionInputParamsError];
                return null!;
            }

            List<Client> result = SrsManageCommon.OrmService.Db.Select<Client>()
                .Where(x => x.IsOnline == true && x.ClientType == ClientType.Monitor &&
                            x.Device_Id!.Equals(deviceId.Trim())).ToList();
            return result;
        }

        /// <summary>
        /// 获取所有发布中的摄像头
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<Client> GetOnPublishMonitorList(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            List<Client> result = SrsManageCommon.OrmService.Db.Select<Client>()
                .Where(x => x.IsOnline == true && x.ClientType == ClientType.Monitor).ToList();
            return result;
        }

        /// <summary>
        /// 通过rtsp地址获取一个ingest的配置
        /// </summary>
        /// <param name="rtspUrl"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static Ingest GetOnvifMonitorIngestTemplate(string rtspUrl, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            Uri url = new Uri(rtspUrl);
            string ip = url.Host;
            ushort port = (ushort) url.Port;
            string protocol = url.Scheme;
            string pathInfo = url.PathAndQuery;
            if (pathInfo.Contains('='))
            {
                int eqflagidx = pathInfo.LastIndexOf('=');
                pathInfo = pathInfo.Substring(eqflagidx + 1);
            }
            else
            {
                int flagidx = pathInfo.LastIndexOf('/');
                pathInfo = pathInfo.Substring(flagidx + 1);
            }

            Ingest result = new Ingest();
            result.IngestName = ip.Trim() + "_" + pathInfo.Trim().ToLower();
            result.Enabled = true;
            result.Input = new IngestInput();
            result.Input.Type = IngestInputType.stream;
            result.Input.Url = rtspUrl;
            result.Ffmpeg = "./ffmpeg";
            result.Engines = new List<IngestTranscodeEngine>();
            IngestTranscodeEngine eng = new IngestTranscodeEngine();
            eng.Enabled = true;
            eng.Perfile = new IngestEnginePerfile();
            eng.Perfile.Re = "re;";
            eng.Perfile.Rtsp_transport = "tcp";
            eng.Vcodec = "copy";
            eng.Acodec = "copy";
            eng.Output = "rtmp://127.0.0.1/live/" + result.IngestName;
            result.Engines.Add(eng);
            return result;
        }
    }
}