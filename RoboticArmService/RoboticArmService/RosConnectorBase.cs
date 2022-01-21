using RosSharp.RosBridgeClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboticArmService
{
    public abstract class RosConnectorBase : IDisposable
    {
        public static RosSocket rosSocket = null;
        private static string ip = string.Empty;
        private static RosSocket _rosInstance = null;

        protected RosConnectorBase(string socketIpWithport)
        {
            ip = socketIpWithport;
        }



        public async Task<T> InitializeAsync<T>(T obj)
        {
            try
            {

                _rosInstance = await RoboticArmService.RosConnectorBase.GetRosInstance();
                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception("Connection Error");
            }

        }

        private async static Task<RosSocket> Connect()
        {


            rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol("ws://" + ip),
            RosSocket.SerializerEnum.Newtonsoft_JSON);


            if (rosSocket != null)
                return rosSocket;
            else
                throw new Exception("Ros Conenction Error");


        }
        private async static Task<RosSocket> ConnectRos()
        {
            try
            {

                rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol("ws://" + ip),
                RosSocket.SerializerEnum.Newtonsoft_JSON);
            }
            catch (Exception ex)
            {
            }




            throw new Exception("Ros Conenction Error");


        }


        public static async Task<RosSocket> GetRosInstance()
        {

            if (_rosInstance == null)
            {
                _rosInstance = await Connect();
            }
            return _rosInstance;

        }
        public void Dispose()
        {

            GC.SuppressFinalize(this);

        }


    }
}
