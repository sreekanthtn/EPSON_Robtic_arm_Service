using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Nav.Arm;
using std = RosSharp.RosBridgeClient.MessageTypes.Std;
using RosBridgeClient.MessageTypes.Device_Status.msg;
using System.Threading;
using RosBridgeClient.MessageTypes.Arm.srv;
using RosBridgeClient.MessageTypes.Arm.msg;

namespace RoboticArmService
{
    public delegate void TriggerArmMovement(RosBridgeClient.MessageTypes.Arm.msg.PathInfo path);
    public delegate void TriggerArmCommand(stats command);
    // private ManualResetEvent OnMessageReceived = new ManualResetEvent(false);
 
    public class ROSConnector : RosConnectorBase
    {
        private ManualResetEvent OnMessageReceived = new ManualResetEvent(false);
        public TriggerArmMovement armMovementMethod;
        public TriggerArmCommand armCommandMovement;
        private ManualResetEvent OnServiceReceived = new ManualResetEvent(false);
        private ManualResetEvent OnTopicReceived = new ManualResetEvent(false);
        public static bool responceReceived = false;
        public static string Arm_Statis_subscription_id = string.Empty;
        public static StatusInfo PaintProcess = StatusInfo.Listen;
        private ROSConnector(string ip) : base(ip)
        {

           
           
        }

        public static Task<ROSConnector> CreateAsync(string ip)
        {
            var ret = new ROSConnector(ip);

            return ret.InitializeAsync(ret);
        }
        /// <summary>
        /// Arm Command Subscription 
        /// </summary>
        /// <param name="SubscriberID"></param>
        /// <param name="armCommand"></param>
        public void SubscribeArmCommandTopic(string SubscriberID, TriggerArmCommand armCommand)
        {
            try
            {
                armCommandMovement = armCommand;
                Arm_Statis_subscription_id = rosSocket.Subscribe<stats>("/" + SubscriberID, ArmCommandHandler);
                OnTopicReceived.WaitOne();
                rosSocket.Unsubscribe(Arm_Statis_subscription_id);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void PublishTopic(string SubscriberID, string AckMessage)
        {
            try
            {
                string id = rosSocket.Advertise<std.String>("/"+ SubscriberID);
                std.String msg = new std.String
                {
                    data = AckMessage
                }; 
                 rosSocket.Publish(id, msg);
               

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
      
        public  bool UnSubscribeTopic(string SubscriberID)
        {
            try
            {
                 if(SubscriberID == "arm_status") { 
                    rosSocket.Unsubscribe(Arm_Statis_subscription_id);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
       


        public  bool CallPathInfo(string ServiceId,TriggerArmMovement armMovement)
        {
            armMovementMethod = armMovement;


            rosSocket.CallService<PathMessageRequest, PathMessageResponse>("/" + ServiceId, PathInfoHandler, new PathMessageRequest());
            OnServiceReceived.WaitOne();
            OnServiceReceived.Reset();
            return true;
        }

        private  void PathInfoHandler(PathMessageResponse message)
        {
            OnServiceReceived.Set();

            armMovementMethod.Invoke(message.path_msg);

        }



        private void ArmCommandHandler(stats command)
        {

          
            if (command.ready && PaintProcess == StatusInfo.Listen)
            {
                PaintProcess = StatusInfo.ReadyToAcceptPathinfo;
                OnTopicReceived.Set();
            }
        

          

        }


    }
}
