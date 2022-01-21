using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCAPINet;
using RosBridgeClient.MessageTypes.Arm.msg;

namespace RoboticArmService
{
    public sealed class Controller :IDisposable
    {
        

        private  RCAPINet.Spel m_spel;
        private static Controller instance = null;

        private static bool SimulatorMode = RoboticArmService.Properties.ServiceSettings.Default.IsSimulatorMode;
        public static Controller Instance
        {
            
             get
            {
                //192.168.225.69 9090
                    if (instance == null)
                    {
                        instance = new Controller();
                    }
                    return instance;
                
            }
           
        }

       
        private void Inilitalize()
        {
            try
            {

                m_spel = new RCAPINet.Spel();
                m_spel.Initialize();
                m_spel.Project = "D:\\EpsonRC70\\projects\\RechabilityTest\\RechabilityTest.sprj";
                m_spel.AsyncMode = true;
                m_spel.EventReceived += new RCAPINet.Spel.EventReceivedEventHandler(Controller_Event);

                if (SimulatorMode)
                {
                    m_spel.ShowWindow(RCAPINet.SpelWindows.Simulator);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Switchon() {
            if (m_spel == null) {
                throw new Exception("Robot Initilization is not done !");
            }
            else
            {
              
                m_spel.ResetAbort();
                m_spel.Reset();
                m_spel.AvoidSingularity = false;
                m_spel.ResetAbortEnabled = true;
                m_spel.MotorsOn = true;
                m_spel.PowerHigh = true;
                m_spel.AsyncMode = false;
            }
           
        }

        public bool HomePosition(float X,float Y,float Z,float U,float V,float W ) {

            try
            {	
                m_spel.Go(string.Format("XY({0},{1},{2},{3},{4},{5}) /F",X,Y,Z,U,V,W));
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Robot Controller Class", ex.Message, EventLogEntryType.Error);
                return false;
            }
        }

        public bool TracePath(RosBridgeClient.MessageTypes.Arm.msg.PathInfo pathInfo) {

            try
            {
                string[] StrokeName;
                List<string> Strokesnames = new List<string>(); ;
                for (int i = 0; i <= pathInfo.path.Count(); i++)
                {
                    int StrokerPoint = i + 1;
                    string Strokename = "Stroke" + StrokerPoint;
                    string pointExpresison = GetCurvePoints(pathInfo.path[i]);
                    if (pointExpresison != string.Empty)
                    {
                        Strokesnames.Add(Strokename);

                        m_spel.Curve(Strokename, false, 0, 6, pointExpresison);
                        m_spel.CVMove(Strokename);
                    }
                   
                }
            }
            catch(Exception ex)
            {


            }

            return true;

        }

        private string  GetCurvePoints(PathStamped pathStamp)
        {


        string PointStringExpression = string.Empty;

            string points=string.Empty;
            if(pathStamp.path_msg.Count() >= 3) {
                for (int i = 0; i < pathStamp.path_msg.Count(); i++)
                {

                    int pointName = i + 1;

                    var X = (float)pathStamp.path_msg[i].x * -1;
                    var Y = (float)pathStamp.path_msg[i].y * -1;
                    var Z = (float)pathStamp.path_msg[i].z * -1;
                    var U = (float)180.000;//       (float)pathStamp.path_msg[i].u;
                    var V = (float)-90.000;// (float)pathStamp.path_msg[i].v;
                    var W = (float)90.000; //(float)pathStamp.path_msg[i].w;            
                    m_spel.SetPoint(pointName, X, Y, Z, U, V, W, 0, RCAPINet.SpelHand.Righty, RCAPINet.SpelElbow.Above, RCAPINet.SpelWrist.Flip, 0, 0);
                 
                        PointStringExpression = PointStringExpression + "P" + pointName + ",";

                }
            }
            return PointStringExpression.TrimEnd(',');


        }

       



        public bool MovePoint() {

            SpelPoint sp = new SpelPoint((float)-481.687, (float)-104.162, (float)-84.058, (float)176.821, (float)-81.566, (float)93.375);          
            m_spel.SetPoint(1, sp);
            SpelPoint sp2 = new SpelPoint((float)-481.687, (float)104.162, (float)-84.058, (float)176.821, (float)-81.566, (float)93.375);
            m_spel.SetPoint(2, sp2);

            
            return true;
        }

        public bool pulse() {

            try {
               
                m_spel.Pulse(0, 0, 0, 0, 0, 0);
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Robot Controller Class", ex.Message,EventLogEntryType.Error);
                return false;
            }
          
            
        } 
        private void Controller_Event(Object sender, SpelEventArgs e)
        {

        }


        private Controller()
        {
            Inilitalize();
        }
        public void Dispose()
        { 
            GC.SuppressFinalize(this);  
        }


    }
}
