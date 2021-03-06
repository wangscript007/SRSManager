# SRSManager

## 一、简介

- SRSManager用于管理和控制SRS流媒体服务器的配置文件，将配置文件进行结构化处理，使配置文件更容易控制。
- 对SRS进程进行管理，使之可以通过一系列API来实现启动，停止，重启，重新加载配置等操作。
- 提供WEB管理接口，实现WebApi方式下的SRS管理。
- 在SRS之外集成onvif设备的管理，包括onvif设备探测，onvif ptz控制，onvif meidaurl获取等。
- 开设此项目主要原因是在自己的项目中要使用到SRS，为了更方便的使用SRS以满足项目所需而开设此项目，也同时为开
  源社区做些力所能及的贡献。
- 项目采用.net core 3.1 编译，SRSWebApi采用Asp.net Core 3.1 的WebApi工程开发，集成了Swagger接口调试文档，
  Onvif相关功能采用了Mictlanix.DotNet.Onvif控制类库。
- 项目中不包含SRS进程内容，需要你自己编译SRS工程，SRS开源地址为：https://github.com/ossrs/srs     
  本项目基于SRS 4.0+ release版本进行编码。
- 本项目支持linux和macos,需要.net core 3.1运行库支持。
  
## 二、重要

- 此项目还在开发中，不能用于生产环境。
- 出于对项目的需要，对srs的源码进行了简单修改，使http_hook时带上device_id,device_id来源于心跳中的device_id
- 对于srs源码的修改已在官方git中与官方提出，希望官方可以考虑进。

## 三、组成部分
- OnvifClient onvif的控制模块，用于发现，ptz探测等
- SRSApis 封装对SRS进程的相关功能API
- SRSConfFile 封装对SRS配置文件的结构化处理，可以读取与重写SRS配置文件 
- SRSManageCommon 项目中用到的相对通用的一些类和方法
- SRSWebApi 将SRSApis项目中的各种接口用WebApi的方式开放出来
- SRSCallBackManager 用于处理SRS的各种回调数据(废弃，移到SRSManageCommon项目中)
- Test_ 开头的项目是针对于以上部分的功能测试项

## 四、设计考虑
- 由于SRS属于自定义配置文件格式，在其他语言或其他项目中对SRS的配置文件操作较为困难，出于对SRS的管理考虑需要对配置
  文件进行结构化配置，需要实现.conf文件的结构化读入，与结构化实例序列化成SRS的.conf文件。这样会使对SRS管理来得相
  对轻松。
- 考虑一般摄像头没有rtp推流能力，只拥有rtsp流暴露的特性，考虑融入onvif相关功能，自动探测发现摄像头的rtsp流地址，
  ptz云台的控制等功能，使之可以配合srs的ingest进行联动，使一般摄像头通过SRS的ingest实现视频流转rtmp输出。
- OnvifClient,SRSApis,SRSConfFile,SRSManageCommon,SRSWebApi相互依赖的工程组，这一套需要实现完整的Onvif+SRS
  的控制单元，其中SRS进程实例和OnvifClient控制实例为List<Object>的形式存在，因此一台服务器上允许多个SRS进程及多个
  Onvif设备同时存在，SRS进程以uuid来区分彼此，onvif设备以ip地址及profile中的uuid来区分不同设备及不同设备下的不同
  媒体流。
- 我将OnvifClient,SRSApis,SRSConfFile,SRSManageCommon,SRSWebApi工程的集成称之为一个StreamNode，在StreamNode
  中~~我尽可能不采用任何关系型数据库组件~~来实现所有功能，这样可以保证程序最大程度上的自由性，简化其安装部署的难度。
- 打脸了，随着开发深入，发现不使用数据库组件使很多问题变得复杂，因此引入了FreeSql开源数据库组件，来支持相关数据的存储与查询。
- 对SRS原有HTTP API进行封装与转发，实现风格统一，鉴权统一的webapi接口。

## 五、如何运行
+ 项目采用微软.net core 3.1环境进行编码，第一点，请确保你拥有.net core 3.1的执行环境(支持linux、macos)
### 配置文件
+ 系统配置文件为srswebapi.wconf
+ 运行系统前需要配置好这个配置文件，系统启动时会加载并检查各配置项
```
#这是注释，#开始是注释,每行配置必须以分号（;）结束
httpport::5800;
#Webapi的监听端口
password::password123!@#;
#控制访问权限的密码，在Allow接口中需要用到这个password,具体见allow接口内容
allowkey::0D906284-6801-4B84-AEC9-DCE07FAE81DA	*	192.168.2.*	;
#允许访问的key与返问ip，如果*号表示所有ip均能访问，ip地址或ip地址加掩码*表示ip或ip段可访问
db::Data Source=192.168.2.35;Port=3306;User ID=root;Password=thisispassword; Initial Catalog=srswebapi;Charset=utf8; SslMode=none;Min pool size=1;
#数据库连接串，数据库需要手动创建，数据库中表系统会自动创建
dbtype::mysql;
#数据库类型，支持mysql,sqlite,oracle等常见数据库服务，具体支持哪些数据库请见开源项目FreeSql
auto_cleintmanagerinterval::5000;
#自动客户端管理的监控运行间隔时间（毫秒）
auto_logmonitorinterval::300000;
#自动日志转存的运行间隔时间（毫秒）
auto_dvrplaninterval::60000;
#自动录制计划的间隔运行时间（毫秒）
#auto_keepingeinterval::30000; #不用这个方案了
#自动ingest保活的运行间隔时间（毫秒），这个可能有点问题，暂时弃用
#增加参数enableingestkeeper,是否启用ingest拉流监控，启用后会针对每 srs进程进行监控
enableingestkeeper::true;
#增加参数ffmpegpath,用于指定ffmpeg可执行文件的路径，不指定则默认为StreamNode目录下
ffmpegpath::./ffmpeg;
#增加参数ffmpegthreadcount，用于指定在使 ffmpeg进行视频合并时使用 ffmpeg线程数量,默认为2个线程，线程数量不宜过多，2-4个比较合适
ffmpegthreadcount::2;

```
### 启动命令
+ 调试启动建议直接 dotnet SRSWebApi.dll
+ 正式运行建议 nohup dotnet SRSWebapi.dll > ./logs/run.log &
+ 正式运行将会运行在后台模式

### 停止运行
+ 调式模式下运行的，退出终端或者在终端上Ctrl+C,将结束进程，停止运行
+ 正式模式下运行，使用命令查出pid 再 kill pid
+ linux下查pid
```
ps -aux |grep SRSWebApi.dll
```
+ macos下查pid
```
ps -A|grep SRSWebApi.dll
```
```
kill -9 pid  
```

### 注意事项
+ 因我本身项目需要，对srs服务的源码做了一些小修改，具体修改内容可见 https://github.com/ossrs/srs/issues/1789
  如果你使用本项目进行测试，需要对srs的源码做相同的修改，再编译srs,当然如果srs官方接受了我的建议（貌似已经接受），
  以后我将直接采用官方的功能而不再修改srs源码
+ 项目目录下需要有ffmpeg可执行文件，否则系统启动会报错
+ 项目止录下需要有srs可执行文件，否则系统启动会报错
+ 对于完全拿来主义的同志们要说声抱歉了，项目暂时不提供WEB管理模块，只有WebApi模块

### 其他
#### 自动化服务介绍
+ DvrPlanExec 自动执行录制计划，使录制计划中计划得以实施
+ IngestMonitor Ingest拉流保活，发现ffmpeg日志大量疯狂写入时，执行ingest拉流器重启，保证拉流正常
+ KeepIngestStream 老的拉流保活方案，已经废弃
+ SrsAndFFmpegLogMonitor 用于Srs日志和FFmpeg日志的转存，保证日志不要爆表，日志文件大于10M时自动转移到其他目录，并清空当前日志文件
+ SrsClientManager 用于对Srs的连接信息（客户端）进行维护，如补摄像头ip 址，被rtsp地址，维护在线列表等



## 六、Api接口说明
+ 接口采用HttpWebApi方式提供，提供方式为http://serverip:apiport/接口类型/API方法
+ 接口调用方式：HttpGet、HttpPost
+ 当传输入参数为简单参数时采用HttpGet方式调用，复杂对象参数时采用HttpPost方式调用
+ 接口的输入参数与输出结果均为json封装方式（部份接口输入输出为简单结果时采用基础类型做为输入输出 int,string,bool等）

```
例如调用检测Srs实例是否正在运行时，可以通过CURL发送以下http请求获得状态
curl -X GET "http://192.168.2.42:5800/GlobalSrs/IsRunning?deviceId=22364bc4-5134-494d-8249-51d06777fb7f" -H "accept: */*"
```
## 七、异常与正常
+ 当接口调用出现异常时，API返回HttpStatusCode为400，同时告知异常原因,返回结构如下：
```json
 {
 	"Code": 0,  //错误代码
 	"Message": "无错误" //错误原因描述
 }
```
+ 当出现系统级异常时，由asp.net core自动捕获（比如传入参数有格式问题等情况）,
asp.net core将返回HttpStatusCode为400，并给出异常原因，返回结构如下：
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "|1e26aa01-4d02465285d0af0c.",
  "errors": {
    "": [
      "A non-empty request body is required."
    ],
    "obj": [
      "The obj field is required."
    ]
  }
}
```
+ 当接口调用正常时,HttpStatusCode为200，返回数据可以根据输出参数要求进行进行接收，并返序列化json到相应的实体类类型

## 八、接口调用约定
+ 时区:+8区
+ 时间格式: yyyy-MM-dd HH:mm:ss
+ 调用方式:HttpGet|HttpPost
+ 耗时操作:采用http callback的方式进行，当某个操作是耗时操作时（如/DvrPlan/CutOrMergeVideoFile）,接口要求在请求时传入callback地址，在操作完成后通过callback地址来通知接口调用应用相关结果 
+ 所有对Srs配置进行写操作（Set,Delete,Update,Insert|Create）的接口，均不会在操作完成后重写配置文件，需要应用调用/System/RefreshSrsObject接口才会将最新的配置信息写入对应的Srs进程配置文件中，并且自动Reload配置文件来刷新Srs运行参数
## 九、接口说明

### 猜你需要的接口之-FastUseful
+ 你可能用得很频繁的接口都在这个接口类里面，因为我们自己开发Web管理的同学需要，所以我集成提供在这个模块下了
  接口都相关简单，不再详细描述输入输出了，遇到特殊的会着重说明
 
#### /FastUseful/GetOnvifMonitorInfoByIngest
+ 获取某个ingest下面的Onvif拉流设备信息
+ 因为是ingest下面的拉流设备，所以输入参数必须能详细指定到某个ingest
+ 所以，需要以deviceId&vhostDomian&ingestName三个输入条件才能定位准确一个ingest,从而准备获得这个Onvif设备的相关信息
+ 看个实例
```
curl -X GET "http://192.168.2.42:5800/FastUseful/GetOnvifMonitorInfoByIngest?deviceId=22364bc4-5134-494d-8249-51d06777fb7f&vhostDomain=__defaultvhost__&ingestName=192.168.2.164_Media1" -H "accept: */*"
```
```json
{
  "host": "192.168.2.164",
  "username": "",
  "password": "",
  "mediaSourceInfoList": [
    {
      "sourceToken": "VideoSource_1",
      "framerate": 25,
      "width": 1920,
      "height": 1080
    }
  ],
  "onvifProfileLimitList": [
    {
      "profileToken": "Profile_1",
      "mediaUrl": "rtsp://192.168.2.164:554/LiveMedia/ch1/Media1",
      "ptzMoveSupport": true,
      "absoluteMove": true,
      "relativeMove": true,
      "continuousMove": true
    },
    {
      "profileToken": "Profile_2",
      "mediaUrl": "rtsp://192.168.2.164:554/LiveMedia/ch1/Media2",
      "ptzMoveSupport": true,
      "absoluteMove": true,
      "relativeMove": true,
      "continuousMove": true
    }
  ],
  "isInited": true
}
```           
#### /FastUseful/GetStreamInfoByVhostIngestName
+ 通过IngestName来获取流信息
+ 看实例
```
curl -X GET "http://192.168.2.42:5800/FastUseful/GetStreamInfoByVhostIngestName?deviceId=22364bc4-5134-494d-8249-51d06777fb7f&vhostDomain=__defaultvhost__&ingestName=192.168.2.164_Media1" -H "accept: */*"
```
```json
{
  "deviceId": "22364bc4-5134-494d-8249-51d06777fb7f",
  "vhostDomain": "__defaultvhost__",
  "ingestName": "192.168.2.164_Media1",
  "liveStream": "/live/192.168.2.164_Media1",
  "app": "live",
  "stream": "192.168.2.164_Media1",
  "monitorType": "Onvif",
  "ipAddress": "192.168.2.164",
  "username": "",
  "password": ""
}
```
#### /FastUseful/GetAllIngestByDeviceId
+ 获取所有Ingest实例列表
+ 返回将是List<VhostIngestConfClass?>

#### /FastUseful/OnOrOffVhostMinDelay
+ 设置某个Vhost为低延迟模式，或者关掉某个Vhost的低延迟模式

#### /FastUseful/PtzZoomForGb28181
+ 控制GB28181设备的焦距大小（就是放大缩小控制）
+ 这个有点重要，说明一下
+ HttpPost调用方式输入一个类，如下
```json
{
  "deviceId": "string",
  "stream": "string",
  "ptzZoomDir": "MORE",
  "speed": 0,
  "stop": true
}
```
+ deviceId指定的是哪个SRS实例
+ Stream指定的是对哪个GB28181设备进行控制，这边直接使用流id来做为设备id
+ ptzZoomDir说明的是要放大还是要缩小，More放大，Less缩小
+ speed表示操作过程中的速度
+ stop，停止信息，如果为true,则不再动作，如果为false将执行上述参数的动作
+ 后面有个ptzMove的操作，与Zoom操作类似

#### /FastUseful/PtzMoveForGb28181
+ 控制GB28181设备云台移动
+ 和/FastUseful/PtzZoomForGb28181一样，也提供一个输入类
```json
{
  "deviceId": "string",
  "stream": "string",
  "ptzMoveDir": "UP",
  "speed": 0,
  "stop": true
}
```
+ 其他参数与zoom控制一样，不同的是这里需要传入ptzmovedir，支持up,down,left,right

#### /FastUseful/GetClientInfoByStreamValue
+ 获取客户端信息，StreamNode会维护一份当前在线的客户端列表，客户端指的是摄像头流、用户播放、推流设备等，这些都是srs的客户端
+ 通过stream标记来获取到流的相关信息
+ 看个实例
```
curl -X GET "http://192.168.2.42:5800/FastUseful/GetClientInfoByStreamValue?stream=192.168.2.164_Media1" -H "accept: */*"
```
```json
{
  "id": 1693,
  "device_Id": "22364bc4-5134-494d-8249-51d06777fb7f",
  "monitorIp": "192.168.2.164",
  "client_Id": 20237,
  "clientIp": "127.0.0.1",
  "clientType": "Monitor",
  "monitorType": "Onvif",
  "rtmpUrl": "rtmp://127.0.0.1:1935/live",
  "httpUrl": "",
  "rtspUrl": "rtsp://192.168.2.164:554/LiveMedia/ch1/Media1",
  "vhost": "__defaultVhost__",
  "app": "live",
  "stream": "192.168.2.164_Media1",
  "param": "",
  "isOnline": true,
  "updateTime": "2020-06-18 09:20:46",
  "isPlay": false,
  "pageUrl": null
}
```
+ client_id是srs提供的client_id,可用于踢掉某个客户端
+ clientType标记了是摄像头，还是播放用户，还是推流用户
+ monitorType如果是摄像头，则会用此字段标记是onvif设备还是gb28181设备
+ isOnline标记设备是否正在线
+ isPlay 标记用户是否正在观看播放
+ 其他字段不做解释

#### /FastUseful/GetRunningSrsInfoList
+ 获取正在运行的Srs实例列表，返回List<SrsManager?>

#### /FastUseful/StopAllSrs
+ 停止所有正在运行的Srs实例
+ 正常停止返回true,否则返回异常原因

#### /FastUseful/InitAndStartAllSrs
+ 初始化及开始运行所有Srs实例

#### /FastUseful/KickoffClient
+ 踢掉一个客户端，这里需要client_id来指定到踢哪个客户端

#### /FastUseful/GetStreamStatusById
+ 获取流状态
+ 看实例比较明白
```
curl -X GET "http://192.168.2.42:5800/FastUseful/GetStreamStatusById?deviceId=22364bc4-5134-494d-8249-51d06777fb7f&streamId=36408" -H "accept: */*"
```
```json
{
  "code": 0,
  "server": 36405,
  "stream": {
    "id": 36408,
    "name": "chid43590668",
    "vhost": 36406,
    "app": "live",
    "live_ms": 1592460310443,
    "clients": 1,
    "frames": 2188496,
    "send_bytes": 33635407400891344,
    "recv_bytes": 33495078562309496,
    "kbps": {
      "recv_30s": 4064,
      "send_30s": 0
    },
    "publish": {
      "active": "true",
      "cid": 429
    },
    "video": {
      "codec": "H264",
      "profile": "High",
      "level": "4",
      "width": 1920,
      "height": 1088
    },
    "audio": null
  }
}
```
+ streamid是来原于srs的一个id,可以通过获取
​/FastUseful​/GetStreamListStatusByDeviceId接口获得

#### /FastUseful​/GetStreamListStatusByDeviceId
+ 和上面的接口功能是一样的，上面那个接口是获取一个stream的状态信息，而这个是通过deviceId来获取某个srs里所有的stream状态

#### /FastUseful/GetVhostStatusById
+ 得到vhost的状态信息
+ 和得到stream的状态信息接口一样，这个是来源于srs内部的信息，因此需要用到srs内部的vhostid
+ 看实例
```
curl -X GET "http://192.168.2.42:5800/FastUseful/GetVhostStatusById?deviceId=22364bc4-5134-494d-8249-51d06777fb7f&vhostId=36406" -H "accept: */*"
```
```json
{
  "code": 0,
  "server": 36405,
  "vhost": {
    "id": 36406,
    "name": "__defaultVhost__",
    "enabled": "true",
    "clients": 8,
    "streams": 8,
    "send_bytes": 272742742452577120,
    "recv_bytes": 271760350059237470,
    "kbps": {
      "recv_30s": 25324,
      "send_30s": 0
    },
    "hls": {
      "enabled": "false"
    }
  }
}
```
#### /FastUseful/GetVhostListStatusByDeviceId
+ 与GetStreamListStatusByDeviceId和/FastUseful/GetStreamStatusById的关系一样，这个接口获取的是全部Vhost的状态信息

#### /FastUseful/GetOnOnlinePlayerByDeviceId
+ 获取正在在线播放（观看）的用户列表
+ 看实例
```
curl -X GET "http://192.168.2.42:5800/FastUseful/GetOnOnlinePlayerByDeviceId?deviceId=22364bc4-5134-494d-8249-51d06777fb7f" -H "accept: */*"
```
```json
[
  {
    "id": 1706,
    "device_Id": "22364bc4-5134-494d-8249-51d06777fb7f",
    "monitorIp": "192.168.2.164",
    "client_Id": 25211,
    "clientIp": "192.168.2.129",
    "clientType": "User",
    "monitorType": null,
    "rtmpUrl": null,
    "httpUrl": "",
    "rtspUrl": null,
    "vhost": "__defaultVhost__",
    "app": "live",
    "stream": "192.168.2.164_Media1",
    "param": null,
    "isOnline": true,
    "updateTime": "2020-06-18 14:10:52",
    "isPlay": true,
    "pageUrl": "http://localhost:9528/stream-node/controller/node/3/srs/22364bc4-5134-494d-8249-51d06777fb7f/stream"
  },
  {
    "id": 1707,
    "device_Id": "22364bc4-5134-494d-8249-51d06777fb7f",
    "monitorIp": "192.168.2.164",
    "client_Id": 25212,
    "clientIp": "192.168.2.129",
    "clientType": "User",
    "monitorType": null,
    "rtmpUrl": null,
    "httpUrl": "",
    "rtspUrl": null,
    "vhost": "__defaultVhost__",
    "app": "live",
    "stream": "34020000002220000001@34020000001360000002",
    "param": null,
    "isOnline": true,
    "updateTime": "2020-06-18 14:10:52",
    "isPlay": true,
    "pageUrl": "http://localhost:9528/stream-node/controller/node/3/srs/22364bc4-5134-494d-8249-51d06777fb7f/stream"
  }
]
```

#### /FastUseful/GetOnOnlinePlayer
+ 获取所有在线播放用户

#### /FastUseful/GetOnPublishMonitorListById
+ 获取所有正在推流的设备列表，需要DeviceId

#### /FastUseful/GetOnPublishMonitorList
+ 获取所有正在推流的设备列表，不需要DeviceId,取StreamNode管理的所有Srs实例中的内容

#### /FastUseful/GetOnPublishMonitorById
+ 获取一个正在推流的设备，通过它的ID，支持多个id,用空格隔开

#### /FastUseful/GetOnvifMonitorIngestTemplate
+ 获取一个Ingest的模板，用于添加onvif设备
+ 看个实例
```
curl -X GET "http://192.168.2.42:5800/FastUseful/GetOnvifMonitorIngestTemplate?username=user&password=%20password&rtspUrl=rtsp%3A%2F%2F192.168.2.164%3A554%2FLiveMedia%2Fch1%2FMedia1" -H "accept: */*"
```
```json
{
  "ingestName": "192.168.2.164_media1",
  "enabled": true,
  "input": {
    "type": "stream",
    "url": "rtsp://user: password@192.168.2.164:554/LiveMedia/ch1/Media1"
  },
  "ffmpeg": "./ffmpeg",
  "engines": [
    {
      "enabled": true,
      "perfile": {
        "re": "re;",
        "rtsp_transport": "tcp"
      },
      "iformat": null,
      "vfilter": null,
      "vcodec": "copy",
      "vbitrate": null,
      "vfps": null,
      "vwidth": null,
      "vheight": null,
      "vthreads": null,
      "vprofile": null,
      "vpreset": null,
      "vparams": null,
      "acodec": "copy",
      "abitrate": null,
      "asample_rate": null,
      "achannels": null,
      "aparams": null,
      "oformat": null,
      "output": "rtmp://127.0.0.1/live/192.168.2.164_media1",
      "engineName": null,
      "instanceName": null
    }
  ],
  "instanceName": "192.168.2.164_media1"
}

```
+ 系统自动生成一个ingest模板，将这个ingest模板用VhostIngest中的相关接口插入，即可得到一个拉流引擎

### 录制计划相关-DvrPlan
+ 提供与录制有关的相关接口

#### /DvrPlan/CutOrMergeVideoFile
+ 对录像文件进行裁剪或合并操作
+ 可以对某个摄像头（流）已经存在的录像文件进行按时间的合并及裁剪
+ 支持秒级的裁剪与合并，与srs录制的视频时长间隔无关
+ 请求为HttpPost
+ 请求结构:
```json
{
  "startTime": "2020-06-18T07:11:55",
  "endTime": "2020-06-18T07:11:55",
  "deviceId": "string",
  "app": "string",
  "vhostDomain": "string",
  "stream": "string",
  "callbackUrl": "string"
}
```
+ deviceId&app&vhostDomain&stream 唯一指定到一个流
+ startTime&endTime，合并或裁剪的开始和结束时间
+ callbackUrl 是回调地址，完成裁剪或合并服务后将结果通过callbackUrl回调给应用
+ 注意事项:
1. 这个接口可以同步返回结果也可以异步回调返回结果，不写callbackurl并且starttime与endtime的时间间隔小于10分钟，则同步返回结果
   否则接口会异步返回结果
2. 在异步返回结果前，同步请求会生成taskId,及涉及合并或裁剪的视频列表信息返回给调用方，在任务完成时也将相应的信息通过callbackUrl返回给调用方
3. 调用方通过taskId进行不同任务的区分
4. 由于裁剪与合并操作是一个非常耗时的任务，因此大需求时间小于10分钟的情况下才给予同步处理返回，建议全部走异步回调方式来进行操作
#### /DvrPlan/UndoSoftDelete
+ 恢复被软删除的视频文件
+ 视频文件删除有两种方式，硬删除和软删除
+ 硬删除将直接删除视频文件，在数据库中标记该文件已经被删除
+ 软删除只在数据库中标记该文件被删除，在24小时后真正删除视频文件
+ 因此软删除的视频文件有机会在24小时内做接口调用恢复
+ 恢复删除是将数据库删除标记置回正常，这样删除线程将不对此文件进行处理

#### /DvrPlan/HardDeleteDvrVideoById
+ 硬删除一个视频文件（立即删除）

#### /DvrPlan/SoftDeleteDvrVideoById
+ 软删除一个社频文件 （24小时后删除）

#### /DvrPlan/GetDvrVideoList
+ 获取录像文件列表
+ HttpPost请求
+ 请求结果如下
```json
{
  "pageIndex": 0,
  "pageSize": 0,
  "includeDeleted": true,
  "startTime": "2020-06-18T07:31:20.114Z",
  "endTime": "2020-06-18T07:31:20.114Z",
  "orderBy": [
    {
      "fieldName": "string",
      "orderByDir": "ASC"
    }
  ],
  "deviceId": "string",
  "vhostDomain": "string",
  "app": "string",
  "stream": "string"
}
```
+ pageIndex,pageSize为分页参数，置null为不分页，pageIndex要从1开始
+ 接口最多一次返回10000条数据
+ includeDeleted 表示是否在返回数据中包含已删除的文件记录
+ startTime&endTime,表示要获取视频文件的时间范围
+ orderBy 要针对哪个字段进行排序，以及排序方式，orderBy是一个List<Orderby?>,可以有多个字段
+ deviceId,vhostDomain,app,Stream是唯一指定到一个流的录像文件的条件
+ 看一个实例
```
curl -X POST "http://192.168.2.42:5800/DvrPlan/GetDvrVideoList" -H "accept: */*" -H "Content-Type: application/json" -d "{\"pageIndex\":1,\"pageSize\":2,\"includeDeleted\":true,\"orderBy\":[{\"fieldName\":\"starttime\",\"orderByDir\":\"ASC\"}],\"deviceId\":\"\",\"vhostDomain\":\"\",\"app\":\"\",\"stream\":\"\"}"
```
```json
{
  "dvrVideoList": [
    {
      "id": 1,
      "device_Id": "22364bc4-5134-494d-8249-51d06777fb7f",
      "client_Id": 801,
      "clientIp": "192.168.2.164",
      "clientType": "Monitor",
      "monitorType": "Onvif",
      "videoPath": "/root/StreamNode/22364bc4-5134-494d-8249-51d06777fb7f/wwwroot/dvr/20200613/__defaultVhost__/live/192.168.2.164_Media1/19/20200613192745.mp4",
      "fileSize": 22619964,
      "vhost": "__defaultVhost__",
      "dir": "/root/StreamNode/22364bc4-5134-494d-8249-51d06777fb7f/wwwroot/dvr/20200613/__defaultVhost__/live/192.168.2.164_Media1/19",
      "stream": "192.168.2.164_Media1",
      "app": "live",
      "duration": 121200,
      "startTime": "2020-06-13 19:27:55",
      "endTime": "2020-06-13 19:29:56",
      "param": "",
      "deleted": false,
      "updateTime": "2020-06-13 19:29:56",
      "recordDate": "2020-06-13"
    },
    {
      "id": 2,
      "device_Id": "22364bc4-5134-494d-8249-51d06777fb7f",
      "client_Id": 801,
      "clientIp": "192.168.2.164",
      "clientType": "Monitor",
      "monitorType": "Onvif",
      "videoPath": "/root/StreamNode/22364bc4-5134-494d-8249-51d06777fb7f/wwwroot/dvr/20200613/__defaultVhost__/live/192.168.2.164_Media1/19/20200613192956.mp4",
      "fileSize": 21536369,
      "vhost": "__defaultVhost__",
      "dir": "/root/StreamNode/22364bc4-5134-494d-8249-51d06777fb7f/wwwroot/dvr/20200613/__defaultVhost__/live/192.168.2.164_Media1/19",
      "stream": "192.168.2.164_Media1",
      "app": "live",
      "duration": 120330,
      "startTime": "2020-06-13 19:29:56",
      "endTime": "2020-06-13 19:31:56",
      "param": "",
      "deleted": false,
      "updateTime": "2020-06-13 19:31:56",
      "recordDate": "2020-06-13"
    }
  ],
  "request": {
    "pageIndex": 1,
    "pageSize": 2,
    "includeDeleted": true,
    "startTime": null,
    "endTime": null,
    "orderBy": [
      {
        "fieldName": "starttime",
        "orderByDir": "ASC"
      }
    ],
    "deviceId": "",
    "vhostDomain": "",
    "app": "",
    "stream": ""
  },
  "total": 3117
}
```

#### /DvrPlan/DeleteDvrPlanById
+ 删除一个录制计划

#### /DvrPlan/OnOrOffDvrPlanById
+ 启用或停用一个录制计划

#### /DvrPlan/SetDvrPlanById
+ 设置一个录制计划
+ 注意，每次设置都是将老的录制计划删除（根据ID），再把新的录制计划写入，因此请注意：数据库的自增ID会变

#### /DvrPlan/CreateDvrPlan
+ 创建一个录制计划
+ HttpPost，提交一个结构
+ 看个实例吧,以下是
```json
{
  "enable": true,
  "deviceId": "string",
  "vhostDomain": "string",
  "app": "string",
  "stream": "string",
  "limitSpace": 0,
  "limitDays": 0,
  "overStepPlan": "StopDvr",
  "timeRangeList": [
    {
      "streamDvrPlanId": 0,
      "weekDay": "Sunday",
      "startTime": "2020-06-18T07:41:31.578Z",
      "endTime": "2020-06-18T07:41:31.578Z"
    }
  ]
}
```
+ enable为此方案是否执行
+ deviceId&vhostDomain&app&stream 唯一指定一个流
+ limitSpace 此流录制后的空间限制（所有文件的大小）
+ limitDays 此流录制后的时间限制（天数）
+ overStepPlan 超过时间限制或超过空间限制怎么处理，（StopDvr:停止录制 DeleteFile:删除文件）
+ 当limitSapce超过限制时，将逐个文件删除，当limitDays超过限制时将一天一天删除视频文件（注意：这是硬删除）
+ timeRangeList 录制的启用时间段
+ timeRange的结构：
```json
{
      "streamDvrPlanId": 0,
      "weekDay": "Sunday",
      "startTime": "2020-06-18T07:41:31",
      "endTime": "2020-06-18T07:41:31"
    }
```
+ 在修改、添加、删除等操作时streamDvrPlanId留空即可
+ weekDay表示星期几
+ startTime&endTime表示从几点到几点，日期可以随便指定，接口最终只会取时间，可以精确到秒

#### /DvrPlan/GetDvrPlan
+ 获取一个录制计划

### 鉴权接口-Allow
+ 对webapi访问进行鉴权
#### /Allow/RefreshSession
+ 刷新Session 
+ 所有接口调用，除了各别接口外，都需要Session
+ 以下是请求结构，需要有allowkey,有refreshCode,有当前的sessionCode
+ expires 可以忽略
```json
{
  "allowKey": "string",
  "refreshCode": "string",
  "sessionCode": "string",
  "expires": 0
}
```

#### /Allow/GetSession
+ 获取一个Session
```json
{
  "allowKey": "string"
}
``` 
+ 需要通过AllowKey来得到一个Session，在session有效其内可以用来访问各种api,session过期前需要通过RefreshSession接口收新session,并使用新的session进行通讯

#### /Allow/SetAllowByKey
+ 设置一个allowKey的参数
```json
{
  "password": "string",
  "allowkey": {
    "key": "string",
    "ipArray": [
      "string"
    ]
  }
}
```
+ password是配置文件中的password

#### /Allow/DelAllowByKey
+ 删除一个AllowKey
+ password是配置文件中的password

#### /Allow/AddAllow
+ 添加一个allowKey
+ password是配置文件中的password

#### /Allow/GetAllows
+ 获取allowKey列表
+ password是配置文件中的password

### Onvif相关接口-Onvif
+ 提供onvif设备的探测发现控制等功能

#### /Onvif/InitAll
+ 初始化所有未初始化的onvif设备

#### /Onvif/InitByIpAddress
+ 通过ip地址来对onvif设备进行初始化

#### /Onvif/SetPtzZoom
+ 对onvif设备进行焦距调整（放大/缩小 就是zoomin|zoomout操作）
+ 请求结构:
```json
{
  "ipAddr": "string",
  "profileToken": "string",
  "zoomDir": "MORE"
}
```
+ zoomDir为More是放大，Less是缩小
+ profileToken是onvif设备的token,以下有接口可以获取到

#### /Onvif/GetPtzPosition
+ 获取onvif设备当前的x,y,z坐标位置 

#### /Onvif/PtzKeepMoveStop
+ 停止onvif设备的持续移动

#### /Onvif/PtzMove
+ 控制onvif设备的云台移动
+ 请求结构:
```json
{
  "ipAddr": "string",
  "profileToken": "string",
  "moveDir": "UP",
  "moveType": "RELATIVE"
}
```
+ moveDir是云台移动方向，有UP, DOWN, LEFT, RIGHT, UPLEFT, UPRIGHT, DOWNLEFT, DOWNRIGHT，比gb28181的多4个方向
+ moveType是云台移动的方式，有 RELATIVE, KEEP  相对位置移动和持续移动
#### /Onvif/InitMonitor
+ 探测并初始化onvif设备
+ 请求结构:
```
{
  "ipAddrs": "string",
  "username": "string",
  "password": "string"
}
```
+ ip,用户名，密码来用探测onvif摄像头，ipaddrs可以有多个 用空格隔开
+ 返回结构（探测结果）
```json
[
  {
    "host": "192.168.2.164",
    "username": "",
    "password": "",
    "mediaSourceInfoList": [
      {
        "sourceToken": "VideoSource_1",
        "framerate": 25,
        "width": 1920,
        "height": 1080
      }
    ],
    "onvifProfileLimitList": [
      {
        "profileToken": "Profile_1",
        "mediaUrl": "rtsp://192.168.2.164:554/LiveMedia/ch1/Media1",
        "ptzMoveSupport": true,
        "absoluteMove": true,
        "relativeMove": true,
        "continuousMove": true
      },
      {
        "profileToken": "Profile_2",
        "mediaUrl": "rtsp://192.168.2.164:554/LiveMedia/ch1/Media2",
        "ptzMoveSupport": true,
        "absoluteMove": true,
        "relativeMove": true,
        "continuousMove": true
      }
    ],
    "isInited": true
  },
  {
    "host": "192.168.2.163",
    "username": "",
    "password": "",
    "mediaSourceInfoList": null,
    "onvifProfileLimitList": null,
    "isInited": false
  }
]
```
#### /Onvif​/GetMonitorList
+ 获取onvif设备的列表（包含信息）
#### /Onvif/GetMonitor
+ 根据ip获取onvif设备实例
+ 看个例子
```
curl -X GET "http://192.168.2.42:5800/Onvif/GetMonitor?ipAddress=192.168.2.164" -H "accept: */*"
```
```json
{
  "host": "192.168.2.164",
  "username": "",
  "password": "",
  "mediaSourceInfoList": [
    {
      "sourceToken": "VideoSource_1",
      "framerate": 25,
      "width": 1920,
      "height": 1080
    }
  ],
  "onvifProfileLimitList": [
    {
      "profileToken": "Profile_1",
      "mediaUrl": "rtsp://192.168.2.164:554/LiveMedia/ch1/Media1",
      "ptzMoveSupport": true,
      "absoluteMove": true,
      "relativeMove": true,
      "continuousMove": true
    },
    {
      "profileToken": "Profile_2",
      "mediaUrl": "rtsp://192.168.2.164:554/LiveMedia/ch1/Media2",
      "ptzMoveSupport": true,
      "absoluteMove": true,
      "relativeMove": true,
      "continuousMove": true
    }
  ],
  "isInited": true
}
```

### SRS全局接口-GlobalSrs
+ 提供对srs控制及全局参数修改方面的接口
#### GlobalSrs/IsRunning
+ 调用方式:HttpGet
+ 接口作用:检测Srs实例是否正在运行.
+ 输入参数:deviceId:string
+ 输出参数:true|false:bool|ExceptStruct
#### GlobalSrs/IsInit
+ 调用方式:HttpGet
+ 接口作用:检测Srs实例配置文件是否被加载并且初始化.
+ 输入参数:deviceId:string
+ 输出参数:true|false:bool|ExceptStruct
#### GlobalSrs/StartSrs
+ 调用方式:HttpGet
+ 接口作用:用于启动一个Srs实例进程（启动srs程序   ./srs -c config.conf）
+ 输入参数:deviceId:string
+ 输出参数:true|false:bool|ExceptStruct
#### GlobalSrs/StopSrs
+ 调用方式:HttpGet
+ 接口作用:停止srs进程，结束掉srs的服务
+ 输入参数:deviceId:string
+ 输出参数:true|false:bool|ExceptStruct
#### GlobalSrs/RestartSrs
+ 调用方式:HttpGet
+ 接口作用:重新启动Srs实例进程，内部逻辑先SrsStop,再SrsStart
+ 输入参数:deviceId:string
+ 输出参数:true|false:bool|ExceptStruct
#### GlobalSrs/ReloadSrs
+ 调用方式:HttpGet
+ 接口作用:重新加载Srs配置文件（热加载，不用停止Srs进程服务）向进程发送 SIGHUP信号 kill -s SIGHUP pid
+ 输入参数:deviceId:string
+ 输出参数:true|false:bool|ExceptStruct
#### /GlobalSrs/ChangeGlobalParams
+ 调用方式:Post
+ 接口作用:修改Srs的全局参数
+ 输入参数:
```json
{
  "deviceId": "string", //Srs实例id
  "gm": {
    "heartbeatEnable": true, //是否启用srs心跳
    "heartbeatSummariesEnable": true, //是否在srs心跳时带上系统统计信息
    "heartbeatUrl": "string", //srs心跳发送url地址（应用可以接管这个地址，默认由StreamNode接管）
    "httpApiEnable": true, //是否启用srs的httpapi,这个必须要启用，StreamNode里需要用到它
    "httpApiListen": 0,//srs的httpapi监听接口
    "httpServerEnable": true,//是否启用srs的httpServer，建议启用
    "httpServerListen": 0,//srs的httpserver监听端口
    "httpServerPath": "string",//srs的httpserver发布目录相当于nginx的wwwroot
    "listen": 0,//srs的rtmp监听端口，默认1935
    "maxConnections": 0 //srs的最大连接数量,默认linux系统1000,macos系统 128
  }
}
```
+ 输出参数:true|false:bool|ExceptStruct
+ 注意：别随便乱改这个参数

### 系统接口-System
+ 提供系统及StremNode层面的各类接口
#### /System/RefreshSrsObject
+ 调用方式:HttpGet
+ 接口作用:将内存中的Srs配置信息写入到对应的Srs实例配置文件里，并向Srs发送配置刷新命令，使Srs运行在刷新后配置信息的环境下
+ 输入参数:deviceId:string
+ 输出参数:true|false:bool|ExceptStruct
#### /System/GetAllSrsManagerDeviceId
+ 调用方式:HttpGet
+ 接口作用:获取StreamNode管理下的所有Srs实例设备ID
+ 输入参数:无
+ 输出参数:List<string?>|ExceptStruct
```json
[
  "22364bc4-5134-494d-8249-51d06777fb7f"
]
```
#### /System/CreateNewSrsInstance
+ 调用方式:HttpPost
+ 接口作用:创建一个新的Srs实例
+ 输入参数:
<details>
<summary>展开查看</summary>
<pre><code>
{
	"srs": {
		"rtc_server": {
			"enabled": true,
			"listen": 0,
			"candidate": "string",
			"ecdsa": true,
			"sendmmsg": 0,
			"encrypt": true,
			"reuseport": 0,
			"merge_nalus": true,
			"gso": true,
			"padding": 0,
			"perf_stat": true,
			"queue_length": 0,
			"black_hole": {
				"enabled": true,
				"publisher": "string"
			}
		},
		"tcmalloc_release_rate": 0,
		"listen": 0,
		"pid": "string",
		"chunk_size": 0,
		"ff_log_dir": "string",
		"ff_log_level": "string",
		"srs_log_tank": "string",
		"srs_log_level": "string",
		"srs_log_file": "string",
		"max_connections": 0,
		"daemon": true,
		"utc_time": true,
		"pithy_print_ms": 0,
		"work_dir": "string",
		"asprocess": true,
		"empty_ip_ok": true,
		"grace_start_wait": 0,
		"grace_final_wait": 0,
		"force_grace_quit": true,
		"disable_daemon_for_docker": true,
		"inotify_auto_reload": true,
		"auto_reload_for_docker": true,
		"heartbeat": {
			"enabled": true,
			"interval": 0,
			"url": "string",
			"device_id": "string",
			"summaries": true,
			"instanceName": "string"
		},
		"stats": {
			"network": 0,
			"disk": "string"
		},
		"http_api": {
			"enabled": true,
			"listen": 0,
			"crossdomain": true,
			"raw_Api": {
				"enabled": true,
				"allow_reload": true,
				"allow_query": true,
				"allow_update": true
			},
			"instanceName": "string"
		},
		"http_server": {
			"enabled": true,
			"listen": 0,
			"dir": "string",
			"crossdomain": true,
			"instanceName": "string"
		},
		"stream_casters": [{
			"sip": {
				"enabled": true,
				"listen": 0,
				"serial": "string",
				"realm": "string",
				"ack_timeout": 0,
				"keepalive_timeout": 0,
				"auto_play": true,
				"invite_port_fixed": true,
				"query_catalog_interval": 0
			},
			"auto_create_channel": true,
			"enabled": true,
			"caster": "mpegts_over_udp",
			"output": "string",
			"listen": 0,
			"rtp_port_min": 0,
			"rtp_port_max": 0,
			"host": "string",
			"audio_enable": true,
			"wait_keyframe": true,
			"rtp_idle_timeout": 0,
			"instanceName": "string"
		}],
		"srt_server": {
			"default_app": "string",
			"enabled": true,
			"listen": 0,
			"maxbw": 0,
			"connect_timeout": 0,
			"peerlatency": 0,
			"recvlatency": 0,
			"instanceName": "string"
		},
		"kafka": {
			"enabled": true,
			"brokers": "string",
			"topic": "string",
			"instanceName": "string"
		},
		"vhosts": [{
			"vnack": {
				"enabled": true
			},
			"instanceName": "string",
			"vhostDomain": "string",
			"enabled": true,
			"min_latency": true,
			"tcp_nodelay": true,
			"chunk_size": 0,
			"in_ack_size": 0,
			"out_ack_size": 0,
			"rtc": {
				"enabled": true,
				"bframe": "string",
				"acc": "string",
				"stun_timeout": 0,
				"stun_strict_check": true
			},
			"vcluster": {
				"mode": "string",
				"origin": "string",
				"token_traverse": true,
				"vhost": "string",
				"debug_srs_upnode": true,
				"origin_cluster": true,
				"coworkers": "string",
				"instanceName": "string"
			},
			"vforward": {
				"enabled": true,
				"destination": "string"
			},
			"vplay": {
				"mw_msgs": 0,
				"gop_cache": true,
				"queue_length": 0,
				"time_jitter": "full",
				"atc": true,
				"mix_correct": true,
				"atc_auto": true,
				"mw_latency": 0,
				"send_min_interval": 0,
				"reduce_sequence_header": true
			},
			"vpublish": {
				"mr": true,
				"mr_latency": 0,
				"firstpkt_timeout": 0,
				"normal_timeout": 0,
				"parse_sps": true,
				"instanceName": "string"
			},
			"vrefer": {
				"enabled": true,
				"all": "string",
				"publish": "string",
				"play": "string",
				"instanceName": "string"
			},
			"vbandcheck": {
				"enabled": true,
				"key": "string",
				"interval": 0,
				"limit_kbps": 0
			},
			"vsecurity": {
				"enabled": true,
				"seo": [{
					"sem": "allow",
					"set": "publish",
					"rule": "string"
				}]
			},
			"vhttp_static": {
				"enabled": true,
				"mount": "string",
				"dir": "string"
			},
			"vhttp_remux": {
				"enabled": true,
				"fast_cache": 0,
				"mount": "string",
				"hstrs": true
			},
			"vhttp_hooks": {
				"enabled": true,
				"on_connect": "string",
				"on_close": "string",
				"on_publish": "string",
				"on_unpublish": "string",
				"on_play": "string",
				"on_stop": "string",
				"on_dvr": "string",
				"on_hls": "string",
				"on_hls_notify": "string"
			},
			"vexec": {
				"enabled": true,
				"publish": "string"
			},
			"vdash": {
				"enabled": true,
				"dash_fragment": 0,
				"dash_update_period": 0,
				"dash_timeshift": 0,
				"dash_path": "string",
				"dash_mpd_file": "string"
			},
			"vhls": {
				"enabled": true,
				"hls_fragment": 0,
				"hls_td_ratio": 0,
				"hls_aof_ratio": 0,
				"hls_window": 0,
				"hls_on_error": "string",
				"hls_path": "string",
				"hls_m3u8_file": "string",
				"hls_ts_file": "string",
				"hls_ts_floor": true,
				"hls_entry_prefix": "string",
				"hls_acodec": "string",
				"hls_vcodec": "string",
				"hls_cleanup": true,
				"hls_dispose": 0,
				"hls_nb_notify": 0,
				"hls_wait_keyframe": true,
				"hls_keys": true,
				"hls_fragments_per_key": 0,
				"hls_key_file": "string",
				"hls_key_file_path": "string",
				"hls_key_url": "string",
				"hls_dts_directly": true
			},
			"vhds": {
				"enabled": true,
				"hds_fragment": 0,
				"hds_window": 0,
				"hds_path": "string"
			},
			"vdvr": {
				"enabled": true,
				"dvr_apply": "string",
				"dvr_plan": "string",
				"dvr_path": "string",
				"dvr_duration": 0,
				"dvr_wait_keyframe": true,
				"time_Jitter": "full"
			},
			"vingests": [{
				"ingestName": "string",
				"enabled": true,
				"input": {
					"type": "file",
					"url": "string"
				},
				"ffmpeg": "string",
				"engines": [{
					"enabled": true,
					"perfile": {
						"re": "string",
						"rtsp_transport": "string"
					},
					"iformat": "off",
					"vfilter": {
						"i": "string",
						"vf": "string",
						"filter_Complex": "string"
					},
					"vcodec": "string",
					"vbitrate": 0,
					"vfps": 0,
					"vwidth": 0,
					"vheight": 0,
					"vthreads": 0,
					"vprofile": "high",
					"vpreset": "medium",
					"vparams": {
						"t": 0,
						"coder": 0,
						"b_strategy": 0,
						"bf": 0,
						"refs": 0
					},
					"acodec": "string",
					"abitrate": 0,
					"asample_rate": 0,
					"achannels": 0,
					"aparams": {
						"profile_a": "string",
						"bsf_a": "string"
					},
					"oformat": "off",
					"output": "string",
					"engineName": "string",
					"instanceName": "string"
				}],
				"instanceName": "string"
			}],
			"vtranscodes": [{
				"enabled": true,
				"ffmpeg": "string",
				"engines": [{
					"enabled": true,
					"perfile": {
						"re": "string",
						"rtsp_transport": "string"
					},
					"iformat": "off",
					"vfilter": {
						"i": "string",
						"vf": "string",
						"filter_Complex": "string"
					},
					"vcodec": "string",
					"vbitrate": 0,
					"vfps": 0,
					"vwidth": 0,
					"vheight": 0,
					"vthreads": 0,
					"vprofile": "high",
					"vpreset": "medium",
					"vparams": {
						"t": 0,
						"coder": 0,
						"b_strategy": 0,
						"bf": 0,
						"refs": 0
					},
					"acodec": "string",
					"abitrate": 0,
					"asample_rate": 0,
					"achannels": 0,
					"aparams": {
						"profile_a": "string",
						"bsf_a": "string"
					},
					"oformat": "off",
					"output": "string",
					"engineName": "string",
					"instanceName": "string"
				}],
				"instanceName": "string"
			}]
		}],
		"configLines": [
			"string"
		],
		"streamNodeIpAddr": "string",
		"streamNodPort": 0,
		"deviceId": "string",
		"configLinesTrim": [
			"string"
		],
		"confFilePath": "string"
	},
	"srsConfigPath": "string",
	"srsDeviceId": "string",
	"srsWorkPath": "string",
	"srsPidValue": "string",
	"isStopedByUser": true
}
</code></pre>
</details>

+ 输出参数:SrsManage|null|ExceptStruct
+ 注:如果正常新建，则返回SrsManager对象,基本与传入参数一致

#### /System/GetSrsInstanceTemplate
+ 调用方式:HttpGet
+ 接口作用:获取一个SrsManager对象的模板，可以用于新建，在模板里已经做好了基本的设置
+ 输入参数:无
+ 输出参数:object:SrsMansger|ExceptStruct

<details>
<summary>展开查看</summary>
<pre><code>
{
  "srs": {
    "rtc_server": null,
    "tcmalloc_release_rate": null,
    "listen": 1935,
    "pid": "/root/StreamNode/21629eba-3bcf-42b0-b37e-4502896dcbe1/srs.pid",
    "chunk_size": 6000,
    "ff_log_dir": "/root/StreamNode/21629eba-3bcf-42b0-b37e-4502896dcbe1/ffmpegLog/",
    "ff_log_level": "warning",
    "srs_log_tank": "file",
    "srs_log_level": "verbose",
    "srs_log_file": "/root/StreamNode/21629eba-3bcf-42b0-b37e-4502896dcbe1/srs.log",
    "max_connections": 1000,
    "daemon": true,
    "utc_time": false,
    "pithy_print_ms": null,
    "work_dir": "/root/StreamNode/",
    "asprocess": false,
    "empty_ip_ok": null,
    "grace_start_wait": 2300,
    "grace_final_wait": 3200,
    "force_grace_quit": false,
    "disable_daemon_for_docker": null,
    "inotify_auto_reload": false,
    "auto_reload_for_docker": null,
    "heartbeat": {
      "enabled": true,
      "interval": 5,
      "url": "http://127.0.0.1:5000/api/v1/heartbeat",
      "device_id": "\"21629eba-3bcf-42b0-b37e-4502896dcbe1\"", //系统自动生成device_id,所有关于这个srs实例的内容都与device_id有关系.
      "summaries": true,                                       //一个StreamNode里不能存在两个相同的device_id
      "instanceName": null
    },
    "stats": null,
    "http_api": {
      "enabled": true,
      "listen": 8000,
      "crossdomain": true,
      "raw_Api": null,
      "instanceName": ""
    },
    "http_server": {
      "enabled": true,
      "listen": 8001,
      "dir": "/root/StreamNode/21629eba-3bcf-42b0-b37e-4502896dcbe1/wwwroot",
      "crossdomain": true,
      "instanceName": null
    },
    "stream_casters": null,
    "srt_server": null,
    "kafka": null,
    "vhosts": [
      {
        "vnack": null,
        "instanceName": "__defaultVhost__",
        "vhostDomain": "__defaultVhost__",
        "enabled": null,
        "min_latency": null,
        "tcp_nodelay": null,
        "chunk_size": null,
        "in_ack_size": null,
        "out_ack_size": null,
        "rtc": null,
        "vcluster": null,
        "vforward": null,
        "vplay": null,
        "vpublish": null,
        "vrefer": null,
        "vbandcheck": null,
        "vsecurity": null,
        "vhttp_static": null,
        "vhttp_remux": null,
        "vhttp_hooks": null,
        "vexec": null,
        "vdash": null,
        "vhls": null,
        "vhds": null,
        "vdvr": null,
        "vingests": null,
        "vtranscodes": null
      }
    ],
    "configLines": null,
    "streamNodeIpAddr": null,
    "streamNodPort": null,
    "deviceId": null,
    "configLinesTrim": null,
    "confFilePath": null
  },
  "srsConfigPath": "",
  "srsDeviceId": "21629eba-3bcf-42b0-b37e-4502896dcbe1",
  "srsWorkPath": "/root/StreamNode/",
  "srsPidValue": "",
  "isInit": true,
  "isStopedByUser": false,
  "isRunning": false
}
</code></pre>
</details>

#### /System/DelSrsByDevId
+ 调用方式:HttpGet
+ 接口作用:删除一个srs实例，如果srs进程正在运行，系统会停止srs进程，并且删除srs对应的配置文件
+ 输入参数:deviceId:string
+ 输出参数:true|false:bool|ExceptStruct
#### /System/GetSrsInstanceByDeviceId
+ 调用方式:HttpGet
+ 接口作用:通过deviceId获取一个Srs实例的配置
+ 输入参数:deviceId:string
+ 输出参数:Object:SrsManager|ExceptStruct,SrsManager结构详见之前说明
#### /System/LoadOnvifConfig
+ 调用方式:HttpGet
+ 接口作用:加载Onvif摄像头的配置文件，配置文件主要存放的是ip地址，用户名，密码，rtsp流地址
+ 输入参数:无
+ 输出参数:true|false:bool|ExceptStruct
#### /System/WriteOnvifConfig
+ 调用方式:HttpGet
+ 接口作用:将内存中的Onvif设备详情写入到配置文件，配置文件主要存放的是ip地址，用户名，密码，rtsp流地址
+ 输入参数:无
+ 输出参数:true|false:bool|ExceptStruct
#### /System/DelOnvifConfigByIpAddress
+ 调用方式:HttpGet
+ 接口作用:通过Onvif设备的ip地址来删除一个Onvif设备，删除后会自动重新加载onvif设备列表，把已经删除的对象剔除
+ 输入参数:ipAddress:string
+ 输出参数:true|false:bool|ExceptStruct
#### /System/GetSystemInfo
+ 调用方式:HttpGet
+ 接口作用:获取系统信息
+ 输入参数:无
+ 输出参数:info:SystemInfoModule|ExceptStruct
```json
{
  "srsList": [
    {
      "version": "4.0.26",
      "pid": 12135,
      "ppid": 1,
      "argv": "/root/StreamNode/srs -c /root/StreamNode/22364bc4-5134-494d-8249-51d06777fb7f.conf",
      "cwd": "/root/StreamNode",
      "mem_kbyte": 81852,
      "mem_percent": 0,
      "cpu_percent": 0.09,
      "srs_uptime": 3540,
      "srs_DeviceId": "22364bc4-5134-494d-8249-51d06777fb7f"
    }
  ],
  "system": { //注意，如果Srs实例没有运行，则system段为null,此段内容获取来自srs实例
    "cpu_percent": 0.02,
    "disk_read_KBps": 0,
    "disk_write_KBps": 0,
    "disk_busy_percent": 0,
    "mem_ram_kbyte": 8008448,
    "mem_ram_percent": 0.18,
    "mem_swap_kbyte": 16781308,
    "mem_swap_percent": 0,
    "cpus": 8,
    "cpus_online": 8,
    "uptime": 1056007.4,
    "ilde_time": 8292082.5,
    "load_1m": 0.23,
    "load_5m": 0.17,
    "load_15m": 0.15,
    "net_sample_time": 1592376245278,
    "net_recv_bytes": 0,
    "net_send_bytes": 0,
    "net_recvi_bytes": 2600853815680,
    "net_sendi_bytes": 1415715429273,
    "srs_sample_time": 1592376245278,
    "srs_recv_bytes": 8577771876,
    "srs_send_bytes": 87975,
    "conn_sys": 73,
    "conn_sys_et": 26,
    "conn_sys_tw": 17,
    "conn_sys_udp": 9,
    "conn_srs": 8
  },
  "networkInterfaceList": [
    {
      "index": 0,
      "name": "ens160",
      "mac": "01-0C-20-01-1B-60",
      "type": "Ethernet",
      "ipaddr": "192.168.2.42"
    },
    {
      "index": 1,
      "name": "docker0",
      "mac": "0A-42-37-98-C4-0F",
      "type": "Ethernet",
      "ipaddr": "172.17.0.1"
    },
    {
      "index": 2,
      "name": "br-14a99bbbd2d9",
      "mac": "02-40-9B-04-FC-3E",
      "type": "Ethernet",
      "ipaddr": "172.20.0.1"
    }
  ],
  "disksInfo": [
    {
      "devicePath": null,
      "path": "/",
      "size": 325713,
      "free": 260258,
      "format": "xfs",
      "volumeLabel": "/",
      "rootDirectory": "/"
    },
    {
      "devicePath": null,
      "path": "/dev",
      "size": 4088,
      "free": 4088,
      "format": "tmpfs",
      "volumeLabel": "/dev",
      "rootDirectory": "/dev"
    },
    {
      "devicePath": null,
      "path": "/dev/shm",
      "size": 4100,
      "free": 4100,
      "format": "tmpfs",
      "volumeLabel": "/dev/shm",
      "rootDirectory": "/dev/shm"
    },
    {
      "devicePath": null,
      "path": "/run",
      "size": 4100,
      "free": 3715,
      "format": "tmpfs",
      "volumeLabel": "/run",
      "rootDirectory": "/run"
    },
    {
      "devicePath": null,
      "path": "/sys/fs/cgroup",
      "size": 4100,
      "free": 4100,
      "format": "tmpfs",
      "volumeLabel": "/sys/fs/cgroup",
      "rootDirectory": "/sys/fs/cgroup"
    },
    {
      "devicePath": null,
      "path": "/",
      "size": 325713,
      "free": 260258,
      "format": "xfs",
      "volumeLabel": "/",
      "rootDirectory": "/"
    },
    {
      "devicePath": null,
      "path": "/boot",
      "size": 533,
      "free": 337,
      "format": "xfs",
      "volumeLabel": "/boot",
      "rootDirectory": "/boot"
    },
    {
      "devicePath": null,
      "path": "/var/lib/docker/overlay2/ec79f5cb0c9cdc370d5fd5fe75e23905e9b761d2c6d8b691525eb42f8fd1cf73/merged",
      "size": 325713,
      "free": 260258,
      "format": "overlay",
      "volumeLabel": "/var/lib/docker/overlay2/ec79f5cb0c9cdc370d5fd5fe75e23905e9b761d2c6d8b691525eb42f8fd1cf73/merged",
      "rootDirectory": "/var/lib/docker/overlay2/ec79f5cb0c9cdc370d5fd5fe75e23905e9b761d2c6d8b691525eb42f8fd1cf73/merged"
    },
    {
      "devicePath": null,
      "path": "/var/lib/docker/overlay2/c2b4cfeec86dcd9016b34fce83a08b98d8a905bec93f3d69a667a18ce9878fe7/merged",
      "size": 325713,
      "free": 260258,
      "format": "overlay",
      "volumeLabel": "/var/lib/docker/overlay2/c2b4cfeec86dcd9016b34fce83a08b98d8a905bec93f3d69a667a18ce9878fe7/merged",
      "rootDirectory": "/var/lib/docker/overlay2/c2b4cfeec86dcd9016b34fce83a08b98d8a905bec93f3d69a667a18ce9878fe7/merged"
    },
    {
      "devicePath": null,
      "path": "/var/lib/docker/containers/99a5d03de3fbf073f5480ed71543328cdec3df2cb9cb464c86487b85354b00cb/mounts/shm",
      "size": 67,
      "free": 67,
      "format": "tmpfs",
      "volumeLabel": "/var/lib/docker/containers/99a5d03de3fbf073f5480ed71543328cdec3df2cb9cb464c86487b85354b00cb/mounts/shm",
      "rootDirectory": "/var/lib/docker/containers/99a5d03de3fbf073f5480ed71543328cdec3df2cb9cb464c86487b85354b00cb/mounts/shm"
    },
    {
      "devicePath": null,
      "path": "/var/lib/docker/containers/cbd47bd483651421431dd16f8471bdc25ef4c4dbd5dbb8143923534187c923cf/mounts/shm",
      "size": 67,
      "free": 67,
      "format": "tmpfs",
      "volumeLabel": "/var/lib/docker/containers/cbd47bd483651421431dd16f8471bdc25ef4c4dbd5dbb8143923534187c923cf/mounts/shm",
      "rootDirectory": "/var/lib/docker/containers/cbd47bd483651421431dd16f8471bdc25ef4c4dbd5dbb8143923534187c923cf/mounts/shm"
    },
    {
      "devicePath": null,
      "path": "/run/user/0",
      "size": 820,
      "free": 820,
      "format": "tmpfs",
      "volumeLabel": "/run/user/0",
      "rootDirectory": "/run/user/0"
    }
  ],
  "platform": "linux",
  "architecture": "X64",
  "x64": true,
  "hostName": "localhost",
  "cpuCoreSize": 8,
  "version": "Linux 3.10.0-1127.10.1.el7.x86_64 #1 SMP Wed Jun 3 14:28:03 UTC 2020 Unix 3.10.0.1127"
}
```
#### /System​/GetSrsInstanceList
+ 调用方式:HttpGet
+ 接口作用:获取Srs实例列表（简项信息）
+ 输入参数:无
+ 输出参数:object:List<SrsInstanceModule?>|ExceptStruct
```json
[
  {
    "deviceId": "22364bc4-5134-494d-8249-51d06777fb7f",
    "isInit": true,
    "isRunning": true,
    "configPath": "/root/StreamNode/22364bc4-5134-494d-8249-51d06777fb7f.conf",
    "pidValue": "12135",
    "srsProcessWorkPath": "/root/StreamNode/srs",
    "srsInstanceWorkPath": "/root/StreamNode/"
  }
]
```
### SRS配置文件操作API（不详细展开，类与配置文件是相互映射的，有需要可以直接看源码）
+ RtcServer 

接口名|功能|备注
--|:--:|--:
/RtcServer/GetSrsRtcServer|获取Srs中Rtc服务的相关配置|输入为DeviceId
/RtcServer/SetRtcServer|对Srs的Rtc服务进行配置|输入为DeviceId
/RtcServer/DelRtcServer|删除Srs中的Rtc服务|输入为DeviceId

+ SrtServer

接口名|功能|备注
--|:--:|--:
/SrtServer/GetSrtServer|获取Srs中Srt服务的相关配置|输入为DeviceId
/SrtServer/SetSrtServer|对Srs的Srt服务进行配置|输入为DeviceId
/SrtServer/DelSrtServer|删除Srs中的Srt服务|输入为DeviceId

+ Stats

接口名|功能|备注
--|:--:|--:
/Stats/GetSrsStats|获取Srs中Stats服务的相关配置|输入为DeviceId
/Stats/SetSrsStats|对Srs的Stats服务进行配置|输入为DeviceId
/Stats/DelStats|删除Srs中的Stats服务|输入为DeviceId

+ StreamCaster

接口名|功能|备注
--|:--:|--:
/StreamCaster/GetStreamCasterInstanceNameList|获取所有StreamCaster的实例名称列表|输入为DeviceId
/StreamCaster/GetStreamCasterInstanceList|获取所有StreamCaster实例|输入为DeviceId
/StreamCaster/CreateStreamCaster|创建一个StreamCaster|输入为DeviceId&StreamCasterConfClass
/StreamCaster/GetStreamCasterTemplate|获取一个StreamCaster的创建模板|输入为CasterType(mpegts_over_udp|rtsp|flv|gb28181)
/StreamCaster/DeleteStreamCasterByInstanceName|通过实例名称删除一个StreamCaster|输入为DeivceId&StreamCasterInstanceName
/StreamCaster/ChangeStreamCasterInstanceName|修改一个StreamCaster的实例名称|输入为DeviceId&InstanceName&NewInstanceName
/StreamCaster/OnOrOff|启用名停用一个StreamCaster|输入为DeviceId&InstanceName&enable:bool
/StreamCaster/SetStreamCaster|修改一个StreamCaster的参数|输入为DeviceId&StreamCasterConfCalss


```js
curl -X GET "http://192.168.2.42:5800/StreamCaster/GetStreamCasterInstanceNameList?deviceId=22364bc4-5134-494d-8249-51d06777fb7f" -H "accept: */*"
```
```json
[
  "gb28181",
  "streamcaster-gb28181-template",
  "streamcaster-gb28181-template2"
]
```
```js
curl -X GET "http://192.168.2.42:5800/StreamCaster/GetStreamCasterInstanceList?deviceId=22364bc4-5134-494d-8249-51d06777fb7f" -H "accept: */*"
```
```json
[
  {
    "sip": {
      "enabled": true,
      "listen": 5060,
      "serial": "34020000002000000001",
      "realm": "3402000000",
      "ack_timeout": 30,
      "keepalive_timeout": 120,
      "auto_play": true,
      "invite_port_fixed": true,
      "query_catalog_interval": 60
    },
    "auto_create_channel": true,
    "enabled": true,
    "caster": "gb28181",
    "output": "rtmp://127.0.0.1/live/[stream]",
    "listen": 9000,
    "rtp_port_min": 58200,
    "rtp_port_max": 58300,
    "host": "*",
    "audio_enable": false,
    "wait_keyframe": false,
    "rtp_idle_timeout": 30,
    "instanceName": "gb28181"
  },
  {
    "sip": {
      "enabled": true,
      "listen": 5060,
      "serial": "34020000002000000001",
      "realm": "3402000000",
      "ack_timeout": 30,
      "keepalive_timeout": 120,
      "auto_play": true,
      "invite_port_fixed": true,
      "query_catalog_interval": 60
    },
    "auto_create_channel": false,
    "enabled": true,
    "caster": "gb28181",
    "output": "rtmp://127.0.0.1/[vhost]/[app]/[stream]",
    "listen": 9001,
    "rtp_port_min": 58200,
    "rtp_port_max": 58300,
    "host": "*",
    "audio_enable": true,
    "wait_keyframe": false,
    "rtp_idle_timeout": 30,
    "instanceName": "streamcaster-gb28181-template"
  },
  {
    "sip": {
      "enabled": true,
      "listen": 5060,
      "serial": "34020000002000000001",
      "realm": "3402000000",
      "ack_timeout": 30,
      "keepalive_timeout": 120,
      "auto_play": true,
      "invite_port_fixed": true,
      "query_catalog_interval": 60
    },
    "auto_create_channel": false,
    "enabled": true,
    "caster": "gb28181",
    "output": "rtmp://127.0.0.1/[vhost]/[app]/[stream]",
    "listen": 9002,
    "rtp_port_min": 58200,
    "rtp_port_max": 58300,
    "host": "*",
    "audio_enable": true,
    "wait_keyframe": false,
    "rtp_idle_timeout": 30,
    "instanceName": "streamcaster-gb28181-template2"
  }
]

```

### 以下接口类不一一展开（不详细展开，类与配置文件是相互映射的，有需要可以直接看源码及Swagger接口）
<table border="1">
<tr>
<td>Vhost</td>
<td>Vhost相关功能</td>
<td>VhostBandcheck</td>
<td>VhostBandcheck相关功能</td>
<td>VhostCluster</td>
<td>VhostCluster相关功能</td>
</tr>
<tr>
<td>VhostDash</td>
<td>VhostDash相关功能</td>
<td>VhostDvr</td>
<td>VhostDvr相关功能</td>
<td>VhostExec</td>
<td>VhostExec相关功能</td>
</tr>
<tr>
<td>VhostForward</td>
<td>VhostForward相关功能</td>
<td>VhostHds</td>
<td>VhostHds相关功能</td>
<td>VhostHls</td>
<td>VhostHls相关功能</td>
</tr>
<tr>
<td>VhostHttpHooks</td>
<td>VhostHttpHooks相关功能</td>
<td>VhostHttpRemux</td>
<td>VhostHttpRemux相关功能</td>
<td>VhostHttpStatic</td>
<td>VhostHttpStatic相关功能</td>
</tr>
<tr>
<td>VhostIngest</td>
<td>VhostIngest相关功能</td>
<td>VhostPlay</td>
<td>VhostPlay相关功能</td>
<td>VhostPublish</td>
<td>VhostPublish相关功能</td>
</tr>
<tr>
<td>VhostRtc</td>
<td>VhostRtc相关功能</td>
<td>VhostSecurity</td>
<td>VhostSecurity相关功能</td>
<td>VhostTranscode</td>
<td>VhostTranscode相关功能</td>
</tr>
</table>

