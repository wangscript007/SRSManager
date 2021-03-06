﻿using Microsoft.AspNetCore.Mvc;
using SrsApis.SrsManager.Apis;
using SrsConfFile.SRSConfClass;
using SrsManageCommon;
using SRSManageCommon.ManageStructs;
using SrsWebApi.Attributes;

namespace SrsWebApi.Controllers
{
    /// <summary>
    /// vhostingest接口类
    /// </summary>
    [ApiController]
    [Route("")]
    public class VhostIngestController
    {
        /// <summary>
        /// 获取IngestList，通过deviceId,vhostDomain
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="vhostDomain"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthVerify]
        [Log]
        [Route("/VhostIngest/GetVhostIngestList")]
        public JsonResult GetVhostIngestList(string deviceId, string vhostDomain)
        {
            ResponseStruct rss = CommonFunctions.CheckParams(new object[] {deviceId, vhostDomain});
            if (rss.Code != ErrorNumber.None)
            {
                return Program.CommonFunctions.DelApisResult(null!, rss);
            }

            var rt = VhostIngestApis.GetVhostIngestList(deviceId, vhostDomain,
                out ResponseStruct rs);
            return Program.CommonFunctions.DelApisResult(rt, rs);
        }


        /// <summary>
        /// 通过VhostDomain和IngestInstanceName删除一个Ingest
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="vhostDomain"></param>
        /// <param name="ingestInstanceName"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthVerify]
        [Log]
        [Route("/VhostIngest/DeleteVhostIngestByIngestInstanceName")]
        public JsonResult DeleteVhostIngestByIngestInstanceName(string deviceId, string vhostDomain,
            string ingestInstanceName)
        {
            ResponseStruct rss = CommonFunctions.CheckParams(new object[] {deviceId, vhostDomain, ingestInstanceName});
            if (rss.Code != ErrorNumber.None)
            {
                return Program.CommonFunctions.DelApisResult(null!, rss);
            }

            var rt = VhostIngestApis.DeleteVhostIngestByIngestInstanceName(deviceId, vhostDomain, ingestInstanceName,
                out ResponseStruct rs);
            return Program.CommonFunctions.DelApisResult(rt, rs);
        }

        /// <summary>
        /// 获取所有或者指定vhost中的ingest实例名称
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="vhostDomain"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthVerify]
        [Log]
        [Route("/VhostIngest/GetVhostIngestNameList")]
        public JsonResult GetVhostIngestNameList(string deviceId, string vhostDomain = "")
        {
            ResponseStruct rss = CommonFunctions.CheckParams(new object[] {deviceId, vhostDomain});
            if (rss.Code != ErrorNumber.None)
            {
                return Program.CommonFunctions.DelApisResult(null!, rss);
            }

            var rt = VhostIngestApis.GetVhostIngestNameList(deviceId, out ResponseStruct rs, vhostDomain);
            return Program.CommonFunctions.DelApisResult(rt, rs);
        }

        /// <summary>
        /// 获取一个Ingest配置
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="vhostDomain"></param>
        /// <param name="ingestInstanceName"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthVerify]
        [Log]
        [Route("/VhostIngest/GetVhostIngest")]
        public JsonResult GetVhostIngest(string deviceId, string vhostDomain, string ingestInstanceName)
        {
            ResponseStruct rss = CommonFunctions.CheckParams(new object[] {deviceId, vhostDomain, ingestInstanceName});
            if (rss.Code != ErrorNumber.None)
            {
                return Program.CommonFunctions.DelApisResult(null!, rss);
            }

            var rt = VhostIngestApis.GetVhostIngest(deviceId, vhostDomain, ingestInstanceName, out ResponseStruct rs);
            return Program.CommonFunctions.DelApisResult(rt, rs);
        }

        /// <summary>
        /// 设置或创建Ingest
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="vhostDomain"></param>
        /// <param name="ingest"></param>
        /// <param name="ingestInstanceName"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthVerify]
        [Log]
        [Route("/VhostIngest/SetVhostIngest")]
        public JsonResult SetVhostIngest(string deviceId, string vhostDomain, string ingestInstanceName, Ingest ingest)
        {
            ResponseStruct rss = CommonFunctions.CheckParams(new object[]
                {deviceId, vhostDomain, ingest, ingestInstanceName});
            if (rss.Code != ErrorNumber.None)
            {
                return Program.CommonFunctions.DelApisResult(null!, rss);
            }

            var rt = VhostIngestApis.SetVhostIngest(deviceId, vhostDomain, ingestInstanceName, ingest,
                out ResponseStruct rs);
            return Program.CommonFunctions.DelApisResult(rt, rs);
        }


        /// <summary>
        /// 启用或停用一个ingest
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="vhostDomain"></param>
        /// <param name="ingestInstanceName"></param>
        /// <param name="enable"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthVerify]
        [Log]
        [Route("/VhostIngest/OnOrOffIngest")]
        public JsonResult OnOrOffIngest(string deviceId, string vhostDomain, string ingestInstanceName, bool enable)
        {
            ResponseStruct rss = CommonFunctions.CheckParams(new object[]
                {deviceId, vhostDomain, enable, ingestInstanceName});
            if (rss.Code != ErrorNumber.None)
            {
                return Program.CommonFunctions.DelApisResult(null!, rss);
            }

            var rt = VhostIngestApis.OnOrOffIngest(deviceId, vhostDomain, ingestInstanceName, enable,
                out ResponseStruct rs);
            return Program.CommonFunctions.DelApisResult(rt, rs);
        }
    }
}