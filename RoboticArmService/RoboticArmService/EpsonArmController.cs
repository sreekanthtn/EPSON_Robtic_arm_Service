using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using RosBridgeClient.MessageTypes.Device_Status.msg;

namespace RoboticArmService
{
    public partial class EpsonArmController : ServiceBase
    {
       
        private static string ip = RoboticArmService.Properties.ServiceSettings.Default.Ip;//  Properties.Settings
        private static string armPathInfoTopic = RoboticArmService.Properties.ServiceSettings.Default.ArmMovementInfoTopic;
        private static string armCommandTopic = RoboticArmService.Properties.ServiceSettings.Default.ArmCommandTopic;

        private static string arm_ack = RoboticArmService.Properties.ServiceSettings.Default.ArmAckTopic;
        private static string arm_service = RoboticArmService.Properties.ServiceSettings.Default.ArmPathInfoService;


        private static float HomeX = RoboticArmService.Properties.ServiceSettings.Default.HomeX;
        private static float HomeY = RoboticArmService.Properties.ServiceSettings.Default.HomeY;
        private static float HomeZ = RoboticArmService.Properties.ServiceSettings.Default.HomeZ;
        private static float HomeU = RoboticArmService.Properties.ServiceSettings.Default.HomeU;
        private static float HomeV = RoboticArmService.Properties.ServiceSettings.Default.HomeV;
        private static float HomeW = RoboticArmService.Properties.ServiceSettings.Default.HomeW;

        ROSConnector ObjectDeviceStatus;

        Controller objRobt;
        public EpsonArmController()
        {
            InitializeComponent();
        }

        internal void ListenAndSynch() {
            do
            {
                if (ROSConnector.PaintProcess == StatusInfo.Listen)
                {
                    TestStartupAndStop();
                }
            } while (true);
           

        }

        internal void TestStartupAndStop()
        {
            //this.RobotInit();

                SubscribeRos();
                while (ROSConnector.PaintProcess == StatusInfo.ReadyToAcceptPathinfo)
                {
                    this.RobotInit();
                    SubscribePathService();

                }     
        }
        public void RobotInit() {
            objRobt = Controller.Instance;
            if (objRobt != null)
            {
                objRobt.Switchon();
                objRobt.pulse();


            }

        }

        protected override void OnStart(string[] args)
        {
            try
            {

                objRobt = Controller.Instance;
                if (objRobt != null)
                {
                        objRobt.Switchon();
                      
                }
            }
            catch( Exception ex)
            {
                EventLog.WriteEntry("ArmService", "Exception at OnStartup " + ex.Message, EventLogEntryType.Error, 1, short.MaxValue);
            }
            
        }
        public async void PublishAck(string topicId, string message) {
            ObjectDeviceStatus = await ROSConnector.CreateAsync(ip);
        
            if (ObjectDeviceStatus != null) {
                ObjectDeviceStatus.PublishTopic(topicId, message);

            }

           
        }

        public async void SubscribeRos()
        {
             ObjectDeviceStatus = await ROSConnector.CreateAsync(ip);
            if (ObjectDeviceStatus != null)
            {

                ObjectDeviceStatus.SubscribeArmCommandTopic(armCommandTopic, ArmCommands);

            }
           
        }
        public async void SubscribePathService()
        {
            ROSConnector.PaintProcess = StatusInfo.Wait;
            ObjectDeviceStatus = await ROSConnector.CreateAsync(ip);
            if (ObjectDeviceStatus != null)
            {

                ObjectDeviceStatus.CallPathInfo(arm_service, ArmMovement);
             

            }

        }


        public void ArmMovement(RosBridgeClient.MessageTypes.Arm.msg.PathInfo pathInfo)
        {

            ROSConnector.PaintProcess = StatusInfo.PaintProcessInprogress;

            if (objRobt != null)
            {
                if (objRobt.HomePosition(HomeX, HomeY, HomeZ, HomeU, HomeV, HomeW))
                {
                    objRobt.TracePath(pathInfo);
                }
            }

            


            ROSConnector.PaintProcess = StatusInfo.Listen;

        }

        public async void ArmCommands(stats command) {
            if (command.ready == true) {

                ObjectDeviceStatus = await ROSConnector.CreateAsync(ip);
                if (ObjectDeviceStatus != null)
                {           
                    ObjectDeviceStatus.CallPathInfo(arm_service, ArmMovement);
                }
            }
                
            if (command.fatalerr ==true || command.running==false)
                ObjectDeviceStatus.UnSubscribeTopic(armPathInfoTopic);
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("ArmService", "ArmService  is Stopped ", EventLogEntryType.Information, 2, short.MaxValue);
        }
    }
}
